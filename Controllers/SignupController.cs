using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using reCAPTCHA.AspNetCore;

using Humanizer;

namespace AnnualHealthCheckJs.Controllers
{
    using Data;
    using Models;
    using Services.Core;
    using Tools;
    using ViewModels;

    public class SignupController : Controller
    {
        private IRecaptchaService _recaptcha;
        private IPdfService _pdfService;
        private IEmailSender _emailSender;
        private ISmsSender _smsSender;
        private ApplicationDbContext _context;

        public SignupController(IRecaptchaService recaptcha, IPdfService pdfService,
            IEmailSender emailSender, ISmsSender smsSender, ApplicationDbContext context)
        {
            _recaptcha = recaptcha;
            _pdfService = pdfService;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Verify()           // Step 1
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(VerifyIdentityVM model)
        {
            RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(model);
            }

            var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(r =>
                               (!string.IsNullOrEmpty(model.ID.Trim()) && (r.EmployeeID.ToLower().Trim() == model.ID.ToLower().Trim() || r.EnrollmentID.ToLower().Trim() == model.ID.ToLower().Trim())));   // && r.PIN.Trim() == model.PIN.Trim());

            if (enrollee == null)
            {
                ModelState.AddModelError("", "Wrong ID or PIN!");
                return View(model);
            }

            if (await _context.ExcludedEnrollees.AnyAsync(r => r.EnrolleeID == enrollee.ID))
            {
                ModelState.AddModelError("", "You have already undergone your annual health check!");
                return View(model);
            }

            if (!string.IsNullOrEmpty(enrollee.Email) || !string.IsNullOrEmpty(enrollee.MobileNumber))
            {
                // check for default PIN
                string defaultPin = (await _context.ProjectConfig.FirstOrDefaultAsync()).DefaultPIN;
                if (string.IsNullOrEmpty(enrollee.PIN) && model.PIN.Trim() == defaultPin)
                {
                    // change pin
                    return View(nameof(SignupController.PinChange), getChangePin(enrollee));
                }
                else if (!string.IsNullOrEmpty(enrollee.PIN) && enrollee.PIN.Trim() == model.PIN.Trim())
                {
                    // do nothing
                }
                else
                {
                    ModelState.AddModelError("", "Wrong ID or PIN!");
                    return View(model);
                }
            }

            if (await _context.SignUps.AnyAsync(r => r.EnrolleeID == enrollee.ID))
            {
                // find where he stopped and redirect him there
                var signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.EnrolleeID == enrollee.ID);
                switch (signup.Stage)
                {
                    case Steps.OnLocationandAvailability:
                        return View(nameof(SignupController.Location), await getLoc4SignupAsync(signup));

                    case Steps.OnConfirmation:
                        return View(nameof(SignupController.Confirm), await getLoc4SignupAsync(signup));

                    case Steps.GetRef:
                        return View(nameof(SignupController.GetRef), await getLoc4SignupAsync(signup));

                    case Steps.Rating:
                        return View(nameof(SignupController.Rating), await getLoc4SignupAsync(signup, false));

                    case Steps.Completed:
                        return View(nameof(SignupController.Completed), await getLoc4SignupAsync(signup, false));

                    default:
                        break;
                }
            }
            else
            {
                var config = await _context.ProjectConfig.FirstOrDefaultAsync();
                // Create new Signup entry
                SignUp newSignup = new SignUp()
                {
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    EnrolleeID = enrollee.ID,
                    Year = config.Year,
                    RefGuid = Guid.NewGuid(),
                    Stage = Steps.OnLocationandAvailability
                };

                await _context.AddAsync(newSignup);
                await _context.SaveChangesAsync();

                var signUp = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(s => s.ID == newSignup.ID);

                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signUp));
                //return RedirectToAction(nameof(SignupController.Location));
            }

            // something went wrong
            return View(model);
        }

        public async Task<IActionResult> PinChange(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(SignupController.Verify));

            var resetpin = await _context.ResetPins.Include(r => r.Enrollee).ThenInclude(r => r.HMO).FirstOrDefaultAsync(r => r.LinkID.ToLower().Trim() == id.ToLower().Trim());
            if (resetpin == null)
            {
                return View("ForgotPinFailed");
            }

            if (DateTime.Now > resetpin.DateExpired)
            {
                return View("ForgotPinFailed");
            }

            // change pin
            var vm = getChangePin(resetpin.Enrollee);
            vm.Link = id;
            return View(nameof(SignupController.PinChange), vm);
        }

        [HttpPost]
        public async Task<IActionResult> PinChange(ChangePinVM model)
        {
            RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            //    var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(e => e.ID == model.Id);
            //    model.enrollee = enrollee;
            //    model.hmoGuid = enrollee.HMO.Guid.ToString();

            //    return View(model);
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            //    var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(e => e.ID == model.Id);
            //    model.enrollee = enrollee;
            //    model.hmoGuid = enrollee.HMO.Guid.ToString();

            //    return View(model);
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");

                var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(e => e.ID == model.Id);
                model.enrollee = enrollee;
                model.hmoGuid = enrollee.HMO.Guid.ToString();

                return View(model);
                //return RedirectToAction(nameof(SignupController.Verify));
            }

            if (string.IsNullOrEmpty(model.Link))
            {
                var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(e => e.ID == model.Id);
                if (enrollee == null)
                {
                    var vm = getChangePin(enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                    return View(vm);
                }

                if (model.OldPin.Trim() == model.NewPin.Trim())
                {
                    var vm = getChangePin(enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "The new PIN must be different from the old PIN!");
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                    return View(vm);
                }
                if (model.NewPin.Trim() != model.ConfirmNewPin.Trim())
                {
                    var vm = getChangePin(enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "The new PIN and confirmation PIN do not match!");
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                    return View(vm);
                }


                var signUp = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(s => s.EnrolleeID == enrollee.ID);
                if (signUp == null)
                {
                    enrollee.PIN = model.NewPin.Trim();
                    // Create new Signup entry
                    SignUp newSignup = new SignUp()
                    {
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now,
                        EnrolleeID = enrollee.ID,
                        Year = DateTime.Now.Year,
                        RefGuid = Guid.NewGuid(),
                        Stage = Steps.OnLocationandAvailability
                    };

                    await _context.AddAsync(newSignup);
                    await _context.SaveChangesAsync();

                    signUp = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(s => s.ID == newSignup.ID);
                }

                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signUp));
            }
            else
            {
                var resetpin = await _context.ResetPins.Include(r => r.Enrollee).ThenInclude(r => r.HMO).FirstOrDefaultAsync(r => r.LinkID.ToLower() == model.Link);
                if (resetpin == null)
                {
                    return View("ForgotPinFailed");
                }

                if (resetpin.Code.Trim() != model.OldPin.Trim())
                {
                    var vm = getChangePin(resetpin.Enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "Enter the PIN you were sent as Old PIN!");
                    return View(vm);
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                }

                if (resetpin.Code.Trim() == model.NewPin.Trim())
                {
                    var vm = getChangePin(resetpin.Enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "The new PIN must be different from the old PIN!");
                    return View(vm);
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                }
                if (model.NewPin.Trim() != model.ConfirmNewPin.Trim())
                {
                    var vm = getChangePin(resetpin.Enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "The new PIN and confirmation PIN do not match!");
                    return View(vm);
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                }

                var enrollee = await _context.Enrollees.FirstOrDefaultAsync(e => e.ID == model.Id);
                if (enrollee == null)
                {
                    var vm = getChangePin(resetpin.Enrollee);
                    vm.Link = model.Link;

                    ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                    return View(vm);
                    //return RedirectToAction(nameof(SignupController.PinChange), model);
                }

                enrollee.PIN = model.NewPin.Trim();

                await _context.SaveChangesAsync();

                return View("ForgotPinSuccess");
            }
        }

        public IActionResult ForgotPin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPin(ForgotPinVM model)
        {
            RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.ForgotPin));
            }

            var enrollee = await _context.Enrollees.Include(e => e.HMO).FirstOrDefaultAsync(r =>
                               (!string.IsNullOrEmpty(model.ID.Trim()) && (r.EmployeeID.ToLower().Trim() == model.ID.ToLower().Trim() || r.EnrollmentID.ToLower().Trim() == model.ID.ToLower().Trim())));
            if (enrollee == null)
            {
                //ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                //return RedirectToAction(nameof(SignupController.ForgotPin), model);
                return View("ForgotPinFailed");
            }

            RandomStringGenerator RNG = new RandomStringGenerator(true, false, true, false);
            if (!string.IsNullOrEmpty(enrollee.Email))
            {
                string rndpin = RNG.Generate("nnnn");

                var resetpin = new ResetPin()
                {
                    Code = rndpin,
                    EnrolleeID = enrollee.ID,
                    LinkID = Guid.NewGuid().ToString() + Guid.NewGuid().ToString()
                };
                resetpin.DateCreated = DateTime.Now;
                resetpin.DateExpired = resetpin.DateCreated.AddHours(6);

                await _context.AddAsync(resetpin);
                await _context.SaveChangesAsync();

                var basePath = $"{this.Request.Scheme}://{this.Request.Host}";
                // Send Change PIN link via Email alongside pin.
                tForgotPinVM fpvm = new tForgotPinVM()
                {
                    HMO = enrollee.HMO.Name.Humanize(LetterCasing.Title),
                    HMOLogo = $"{basePath}/Logo/{enrollee.HMO.Guid.ToString()}",
                    FullNames = $"{enrollee.LastName.Humanize(LetterCasing.Title)} {enrollee.OtherNames.Humanize(LetterCasing.Title)}",
                    PIN = rndpin,
                    Link = $"{basePath}/signup/pinchange/{resetpin.LinkID}"
                };
                _emailSender.SendForgetPINBackground(enrollee.Email, "Annual Health Check: Reset PIN", fpvm);

                return View("ForgotPinSent");
            }

            if (!string.IsNullOrEmpty(enrollee.MobileNumber))
            {
                string rndpin = RNG.Generate("nnnn");

                var resetpin = new ResetPin()
                {
                    Code = rndpin,
                    EnrolleeID = enrollee.ID,
                    LinkID = Guid.NewGuid().ToString() + Guid.NewGuid().ToString()
                };
                resetpin.DateCreated = DateTime.Now;
                resetpin.DateExpired = resetpin.DateCreated.AddMinutes(30);

                await _context.AddAsync(resetpin);
                await _context.SaveChangesAsync();

                // Send SMS change pin
                _smsSender.SendForgetPINBackground(enrollee.MobileNumber, "HEALTH CHECK", new tForgotPinVM() { PIN = rndpin });

                return RedirectToAction(nameof(SignupController.PinChange), new { id = resetpin.LinkID });
            }

            return View(model);
        }

        public IActionResult Location()         // Step 2
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        [HttpPost]
        public async Task<IActionResult> Location(LocationAndAvailabilityVM model)
        {
            //RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }

            var signup = await _context.SignUps
                .Include(s => s.Enrollee).ThenInclude(l => l.HMO)
                .Include(s => s.Location).ThenInclude(l => l.State)
                .FirstOrDefaultAsync(r => r.ID == model.SignupID);
            if (signup == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }
            else
            {
                signup.DateUpdated = DateTime.Now;
                signup.AppointmentDate = model.Appointment;
                signup.LocationID = model.LocationID;
                signup.ProviderID = model.ProviderID;

                if (signup.Stage < Steps.OnConfirmation)
                    signup.Stage = Steps.OnConfirmation;

                _context.Update(signup);

                if (signup.Stage >= Steps.GetRef)
                {
                    var signupResh = new SignUpReschedule()
                    {
                        SignUpID = signup.ID,
                        OldAppointmentDate = signup.AppointmentDate.Value,
                        OldProviderID = signup.ProviderID.Value,
                        OldAuthorizationCode = signup.AuthorizationCode,
                        NewAppointmentDate = model.Appointment,
                        NewProviderID = model.ProviderID,

                        DateCreated = DateTime.Now
                    };

                    _context.Add(signupResh);
                }

                await _context.SaveChangesAsync();

                // refetch with location this time
                signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == model.SignupID);
                return View(nameof(SignupController.Confirm), await getLoc4SignupAsync(signup));
            }
            //return View(nameof(SignupController.Verify));
        }

        public async Task<IActionResult> StateLocations(string stateId, string hmoId)
        {
            var locs = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Location --" } };
            if (string.IsNullOrEmpty(stateId))
                return Json(locs);
            if (string.IsNullOrEmpty(hmoId))
                return Json(locs);

            int id = 0;
            bool isValid = int.TryParse(stateId, out id);
            if (!isValid)
                return Json(locs);

            Guid hmoGuid = Guid.Empty;
            isValid = Guid.TryParse(hmoId, out hmoGuid);
            if (!isValid)
                return Json(locs);


            var locations = /*_context.Locations.Where(l => l.StateID == id)*/_context.Providers.Include(p => p.HMO).Include(p => p.Location).Where(p => p.Enabled != false && p.Location.StateID == id && p.HMO.Guid == hmoGuid).Select(p => p.Location).Distinct()
                                        .Select(cp => new SelectListItem
                                        {
                                            Value = cp.ID.ToString(),
                                            Text = cp.Name.ToLower().Humanize(LetterCasing.Title)
                                        });
            var providers = _context.Providers.Where(p => p.Enabled != false && p.LocationID == int.Parse(locations.FirstOrDefault().Value) && p.HMO.Guid == hmoGuid)
                                        .Select(cp => new SelectListItem
                                        {
                                            Value = cp.ID.ToString(),
                                            Text = cp.Name.ToLower().Humanize(LetterCasing.Title)
                                        });

            locs = await locations.ToListAsync();
            var provs = await providers.ToListAsync();

            return Json(new { locations = locs, providers = provs });
        }

        public async Task<IActionResult> LocationProviders(string locationId, string hmoId)
        {
            var rtn = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Select a Provider --" } };
            if (string.IsNullOrEmpty(locationId))
                return Json(rtn);
            if (string.IsNullOrEmpty(hmoId))
                return Json(rtn);

            int id = 0;
            bool isValid = int.TryParse(locationId, out id);
            if (!isValid)
                return Json(rtn);

            Guid hmoGuid = Guid.Empty;
            isValid = Guid.TryParse(hmoId, out hmoGuid);
            if (!isValid)
                return Json(rtn);

            var locations = _context.Providers.Include(p => p.HMO).Where(p => p.Enabled != false && p.LocationID == id && p.HMO.Guid == hmoGuid)
                                        .Select(cp => new SelectListItem
                                        {
                                            Value = cp.ID.ToString(),
                                            Text = cp.Name.ToLower().Humanize(LetterCasing.Title)
                                        });
            rtn = await locations.ToListAsync();
            return Json(rtn);
        }

        public IActionResult Availability()     // Step 3
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Availability(string id)
        {
            var recaptcha = await _recaptcha.Validate(Request);
            if (!recaptcha.success)
                ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            return RedirectToAction(nameof(SignupController.Confirm));
            //return View();
        }
        [HttpPost]
        public async Task<IActionResult> AvailabilityBack(string id)
        {
            var recaptcha = await _recaptcha.Validate(Request);
            if (!recaptcha.success)
                ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            return RedirectToAction(nameof(SignupController.Location));
            //return View();
        }

        public IActionResult Confirm()          // Step 4
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int? id, string act)
        {
            //RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }

            if (id == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }

            var signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                                .Include(s => s.Location).ThenInclude(s => s.State)
                                                .Include(s => s.Provider).ThenInclude(s => s.Location)
                                                .FirstOrDefaultAsync(r => r.ID == id);
            if (act.ToLower() == "continue")
            {
                if (signup == null)
                {
                    ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                    return RedirectToAction(nameof(SignupController.Verify));
                }
                else
                {
                    signup.DateUpdated = DateTime.Now;

                    if (string.IsNullOrEmpty(signup.AuthorizationCode))
                    {
                        if (signup.Enrollee.HMO.GenerateAuthCodeOnSignUpComplete == true)
                        {
                            do
                            {
                                signup.Enrollee.TmpAuthCode = signup.Enrollee.HMO.AuthCodeTemplate.GenerateAuthCodeFromTemplate();
                            } while (_context.Enrollees.Any(r => r.TmpAuthCode == signup.Enrollee.TmpAuthCode));
                        }
                        signup.AuthorizationCode = signup.Enrollee.TmpAuthCode;
                    }

                    signup.RatingGuid = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();

                    if (signup.Stage >= Steps.GetRef)
                    {
                        do
                        {
                            signup.Enrollee.TmpAuthCode = signup.Enrollee.HMO.AuthCodeTemplate.GenerateAuthCodeFromTemplate();
                        } while (_context.Enrollees.Any(r => r.TmpAuthCode == signup.Enrollee.TmpAuthCode));
                        signup.AuthorizationCode = signup.Enrollee.TmpAuthCode;

                        var reshedule = _context.SignUpReschedules.OrderByDescending(s => s.DateCreated).Where(s => s.SignUpID == signup.ID).FirstOrDefault();
                        if (reshedule != null)
                        {
                            reshedule.NewAuthorizationCode = signup.AuthorizationCode;
                            _context.Update(reshedule);
                        }
                    }

                    signup.Stage = Steps.GetRef;
                    _context.Update(signup);
                    await _context.SaveChangesAsync();

                    // send email with the reference letter attached.
                    string schemeHost = string.Format("{0}://{1}", Request.Scheme, Request.Host);
                    _emailSender.SendReferenceLetterViaEmailCCInBackground(signup, schemeHost);

                    //if (!string.IsNullOrEmpty(signup.Enrollee.Email))
                    //{
                    //    // send email with the reference letter attached.
                    //    string schemeHost = string.Format("{0}://{1}", Request.Scheme, Request.Host);
                    //    _emailSender.SendReferenceLetterViaEmailInBackground(signup, schemeHost);
                    //}
                    return View(nameof(SignupController.GetRef), await getLoc4SignupAsync(signup));
                }
            }
            else
            {
                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signup));
            }
        }


        public IActionResult ConfirmBack()          // Step 4
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBack(int? id)
        {
            //RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            ////var recaptcha = await _recaptcha.Validate(Request);
            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }

            if (id == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }

            var signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).FirstOrDefaultAsync(r => r.ID == id);
            if (signup == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }
            else
            {
                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signup));
            }
        }


        public IActionResult GetRef()          // Step 5
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        public IActionResult GetRefBack()
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        [HttpPost]
        public async Task<IActionResult> GetRefBack(int? id)
        {
            //RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            ////var recaptcha = await _recaptcha.Validate(Request);
            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            //    var tignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(r => r.ID == id);
            //    if (tignup != null)
            //        return View(await getLoc4SignupAsync(tignup));
            //    else
            //        return View();
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }

            if (id == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }

            var signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).FirstOrDefaultAsync(r => r.ID == id);
            if (signup == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return View(nameof(SignupController.Verify));
            }
            else
            {
                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signup));
            }
        }

        public async Task<IActionResult> ReferenceLetter(Guid? id)
        {
            var signUp = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).FirstOrDefaultAsync(s => s.RefGuid == id);
            if (signUp == null)
                return NotFound();

            ReferenceLetterVM rvm = new ReferenceLetterVM();
            rvm.SignUp = signUp;
            //rvm.Providers = await _context.Providers.Where(p => p.LocationID == signUp.LocationID).ToListAsync();
            rvm.Provider = await _context.Providers.FirstOrDefaultAsync(p => p.ID == signUp.ProviderID);
            rvm.HasDOB = signUp.Enrollee.DOB.HasValue;
            rvm.GenderIsKnown = signUp.Enrollee.Gender != Gender.UNKNOWN;

            if (rvm.HasDOB)
            {
                var age = Convert.ToDecimal((DateTime.Now.Date - signUp.Enrollee.DOB.Value).TotalDays / 365.2425);
                if (signUp.Enrollee.Gender != Gender.UNKNOWN)
                    rvm.Services = (await _context.Services.Where(s => s.HMOID == signUp.Enrollee.HMOID).ToListAsync()).Where(s => s.Enabled != false && s.Gender.IsCorrectGender(signUp.Enrollee.Gender) && (!s.GTE_Age.HasValue || age >= s.GTE_Age.Value)).ToList();
                else
                    rvm.Services = (await _context.Services.Where(s => s.HMOID == signUp.Enrollee.HMOID).ToListAsync()).Where(s => s.Enabled != false && (!s.GTE_Age.HasValue || age >= s.GTE_Age.Value)).ToList();
            }
            else
            {
                if (signUp.Enrollee.Gender != Gender.UNKNOWN)
                {
                    rvm.Services = (await _context.Services.ToListAsync()).Where(s => s.Enabled != false && s.HMOID == signUp.Enrollee.HMOID && s.Gender.IsCorrectGender(signUp.Enrollee.Gender) && !s.GTE_Age.HasValue).ToList();
                    rvm.Over40Services = (await _context.Services.Where(s => s.HMOID == signUp.Enrollee.HMOID).ToListAsync()).Where(s => s.Enabled != false && s.Gender.IsCorrectGender(signUp.Enrollee.Gender) && s.GTE_Age.HasValue).ToList();
                }
                else
                {
                    rvm.Services = (await _context.Services.ToListAsync()).Where(s => s.Enabled != false && s.HMOID == signUp.Enrollee.HMOID && !s.GTE_Age.HasValue).ToList();
                    rvm.Over40Services = (await _context.Services.Where(s => s.HMOID == signUp.Enrollee.HMOID).ToListAsync()).Where(s => s.Enabled != false && s.GTE_Age.HasValue).ToList();
                }
            }

            if (signUp.Enrollee.HMO.Name.ToLower().Contains("roding"))
                return View("RodingReferenceLetter", rvm);
            else if (signUp.Enrollee.HMO.Name.ToLower().Contains("anchor"))
                return View("AnchorNewReferenceLetter", rvm);
            else if (signUp.Enrollee.HMO.Name.ToLower().Contains("united"))
                return View("UnitedNewReferenceLetter", rvm);
            else
                return View(rvm);
        }

        public IActionResult testr()
        {
            return View();
        }


        public IActionResult DownloadTestr()
        {
            string @controller = "signup", action = "testr";
            string URL = string.Format("{0}://{1}/{2}/{3}", Request.Scheme, Request.Host, @controller, action);

            var result = _pdfService.CreateFromUrl(URL);

            return File(result, "application/pdf", "myFile.pdf");
        }

        public IActionResult DownloadRef()          // Step 4
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }

        [HttpPost]
        public async Task<IActionResult> DownloadRef(string id, string act)
        {
            Guid guid = Guid.Empty;
            if (!Guid.TryParse(id, out guid))
                return BadRequest();

            var signUp = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO)
                                                .Include(s => s.Location).ThenInclude(s => s.State)
                                                .Include(s => s.Provider).ThenInclude(s => s.Location)
                                                .FirstOrDefaultAsync(s => s.RefGuid == guid);
            if (signUp == null)
                return BadRequest();

            if (act.ToLower() == "continue")
            {
                string @controller = "signup", action = "referenceletter";
                string URL = string.Format("{0}://{1}/{2}/{3}/{4}", Request.Scheme, Request.Host, @controller, action, id);

                var result = _pdfService.CreateFromUrl(URL);

                return File(result, "application/pdf", $"Reference_Letter_for_{signUp.Enrollee.EmployeeID}.pdf");
            }
            else
            {
                return View(nameof(SignupController.Location), await getLoc4SignupAsync(signUp));
            }
        }


        public async Task<IActionResult> Rating(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(SignupController.Verify));

            var signup = await _context.SignUps.FirstOrDefaultAsync(s => s.RatingGuid == id);
            if (signup == null)
            {
                return RedirectToAction(nameof(SignupController.Verify));
            }

            if (DateTime.Now.Date < signup.AppointmentDate.Value.Date)
            {
                return RedirectToAction(nameof(SignupController.Verify));
            }

            if (signup.Stage < Steps.Rating)
            {
                signup.DateUpdated = DateTime.Now;
                signup.Stage = Steps.Rating;

                _context.Update(signup);
                await _context.SaveChangesAsync();

                // refetch with location this time
                var rfsignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).Include(s => s.Provider).FirstOrDefaultAsync(r => r.ID == signup.ID);
                return View(nameof(SignupController.Rating), await getLoc4SignupAsync(rfsignup, false));
            }
            else if (signup.Stage == Steps.Rating)
            {
                // refetch with location this time
                var rfsignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).Include(s => s.Provider).FirstOrDefaultAsync(r => r.ID == signup.ID);
                return View(nameof(SignupController.Rating), await getLoc4SignupAsync(rfsignup, false));
            }
            else
            {
                // completed
                var rfsignup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).Include(s => s.Provider).FirstOrDefaultAsync(r => r.ID == signup.ID);
                return View(nameof(SignupController.Completed), await getLoc4SignupAsync(rfsignup, false));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rating(LocationAndAvailabilityVM model)
        {
            RecaptchaResponse recaptcha = null;
            //try
            //{
            //    recaptcha = await _recaptcha.Validate(Request);
            //}
            //catch (Exception)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            //if (!recaptcha.success)
            //{
            //    ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
            //    return View(model);
            //}

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }

            var signup = await _context.SignUps
                .Include(s => s.Enrollee).ThenInclude(l => l.HMO)
                .Include(s => s.Location).ThenInclude(l => l.State)
                .FirstOrDefaultAsync(r => r.ID == model.SignupID);
            if (signup == null)
            {
                ModelState.AddModelError("", "There was an error with your inputs. Please try again!");
                return RedirectToAction(nameof(SignupController.Verify));
            }
            else
            {
                signup.DateUpdated = DateTime.Now;
                signup.Stage = Steps.Completed;
                signup.CheckedOn = model.AppointmentDate;
                signup.Rating = (Rating)model.Rating;

                _context.Update(signup);
                await _context.SaveChangesAsync();

                // refetch with location this time
                signup = await _context.SignUps.Include(s => s.Enrollee).ThenInclude(s => s.HMO).Include(s => s.Location).ThenInclude(l => l.State).Include(s => s.Provider).FirstOrDefaultAsync(r => r.ID == model.SignupID);
                return View(nameof(SignupController.Completed), await getLoc4SignupAsync(signup));
            }
        }

        public IActionResult Completed()
        {
            return RedirectToAction(nameof(SignupController.Verify));
        }


        private ChangePinVM getChangePin(Enrollee enrollee)
        {
            var vm = new ChangePinVM();
            vm.enrollee = enrollee;
            vm.hmoGuid = enrollee.HMO.Guid.ToString();

            return vm;
        }

        private async Task<LocationAndAvailabilityVM> getLoc4SignupAsync(SignUp signup, bool offsetDate = true)
        {
            var config = await _context.ProjectConfig.FirstOrDefaultAsync();

            var vm = new LocationAndAvailabilityVM();
            vm.SignUp = signup;
            vm.hmoGuid = signup.Enrollee.HMO.Guid.ToString();

            if (signup.Stage != Steps.Rating)
            {
                if (offsetDate)
                {
                    vm.StartDate = DateTime.Now.Date > config.StartDate ? DateTime.Now.Date.AddDays(1) : config.StartDate;
                    vm.EndDate = DateTime.Now.Date > config.EndDate ? DateTime.Now.Date.AddDays(1) : config.EndDate;
                }
                else
                {
                    vm.StartDate = DateTime.Now.Date > config.StartDate ? DateTime.Now.Date : config.StartDate;
                    vm.EndDate = DateTime.Now.Date > config.EndDate ? DateTime.Now.Date : config.EndDate;
                }
            }
            else
            {
                vm.StartDate = config.StartDate;
                vm.EndDate = DateTime.Now.Date > config.EndDate ? config.EndDate : DateTime.Now.Date;
            }

            vm.Year = config.Year;

            switch (signup.Stage)
            {
                case Steps.OnLocationandAvailability:

                    vm.StatesList = new SelectList(/*(await _context.States.ToListAsync())*/_context.Providers.Include(p => p.State).Where(p => p.HMOID == signup.Enrollee.HMOID).Select(p => p.State).Distinct()
                            .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text");
                    //vm.LocationsList = new SelectList(new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "-- Pick a Location --" } });
                    break;

                case Steps.Rating:
                    vm.Rating = (int)signup.Rating;
                    vm.AppointmentDate = signup.AppointmentDate;
                    break;
                default:

                    vm.SignupID = signup.ID;
                    vm.Location = signup.Location;
                    vm.LocationID = signup.LocationID.Value;
                    vm.StateID = signup.Location.StateID;
                    vm.ProviderID = signup.ProviderID.Value;
                    vm.AppointmentDate = signup.AppointmentDate;

                    vm.Provider = await _context.Providers.FirstOrDefaultAsync(p => p.ID == vm.ProviderID);
                    //vm.Providers = await _context.Providers.Where(p => p.LocationID == signup.LocationID).ToListAsync();

                    vm.StatesList = new SelectList(/*(await _context.States.ToListAsync())*/_context.Providers.Include(p => p.State).Where(p => p.HMOID == signup.Enrollee.HMOID).Select(p => p.State).Distinct()
                            .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", signup.Location.StateID);
                    vm.LocationsList = new SelectList(/*(await _context.Locations.Where(l => l.StateID == signup.Location.StateID).ToListAsync())*/
                        _context.Providers.Include(p => p.Location).Where(p => p.Location.StateID == signup.Location.StateID && p.HMOID == signup.Enrollee.HMOID).Select(p => p.Location).Distinct()
                        .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", signup.LocationID);
                    vm.ProvidersList = new SelectList((await _context.Providers.Where(l => l.LocationID == signup.LocationID).ToListAsync())
                        .Select(r => new SelectListItem() { Value = r.ID.ToString(), Text = r.Name }), "Value", "Text", signup.ProviderID);
                    break;
            }

            return vm;
        }
    }
}