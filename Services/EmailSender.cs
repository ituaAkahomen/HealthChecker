using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using System.IO;
using RazorLight;
using Hangfire;
using Humanizer;

namespace AnnualHealthCheckJs.Services
{
    using Core;
    using Models;
    using Tools;
    using ViewModels;


    public class EmailSender : IEmailSender
    {
        private readonly IHostingEnvironment _environment;
        private readonly IRazorLightEngine _engine;
        private IPdfService _pdfService;

        public EmailSender(IHostingEnvironment env, IRazorLightEngine engine, IPdfService pdfService)
        {
            _environment = env;
            _engine = engine;
            _pdfService = pdfService;
        }

        public void SendEmail(EmailModel model)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Annual Health Check", "noreply@annualhealthcheck.com"));
            emailMessage.To.Add(new MailboxAddress("", model.Email));
            emailMessage.Subject = model.Subject;
            emailMessage.Importance = MessageImportance.High;

            BodyBuilder bodyBuilder = new BodyBuilder();
            if (model.HasPlainText)
                bodyBuilder.TextBody = model.Message; //plainTextMessage;

            if (model.HasHtml)
                bodyBuilder.HtmlBody = model.Message; //htmlMessage;

            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                foreach (var attachment in model.Attachments)
                    bodyBuilder.Attachments.Add(attachment.Key, attachment.Value);
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            //emailMessage.Body = new TextPart("plain") { Text = message };

            if (_environment.IsDevelopment())
            {
                var filename = Guid.NewGuid().ToString() + ".eml";
                using (StreamWriter data = System.IO.File.CreateText("c:\\mailbox\\" + filename))
                {
                    emailMessage.WriteTo(data.BaseStream);
                }
            }
            else
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.sendgrid.net", 587, false);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    //client.Authenticate("Rodinghc", "Rodinghc@123");
                    client.Authenticate("nehunx64", "Provider123$");

                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
            }
        }

        public void SendEmails(EmailModel2 model)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Annual Health Check", "noreply@annualhealthcheck.com"));
            emailMessage.To.Add(new MailboxAddress("", model.Emails.FirstOrDefault()));
            if (model.Emails.Count() > 1)
            {
                for (int i = 1; i < model.Emails.Count(); i++)
                    emailMessage.Cc.Add(new MailboxAddress("", model.Emails.ElementAt(i)));
            }

            emailMessage.Subject = model.Subject;
            emailMessage.Importance = MessageImportance.High;

            BodyBuilder bodyBuilder = new BodyBuilder();
            if (model.HasPlainText)
                bodyBuilder.TextBody = model.Message; //plainTextMessage;

            if (model.HasHtml)
                bodyBuilder.HtmlBody = model.Message; //htmlMessage;

            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                foreach (var attachment in model.Attachments)
                    bodyBuilder.Attachments.Add(attachment.Key, attachment.Value);
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            //emailMessage.Body = new TextPart("plain") { Text = message };

            if (_environment.IsDevelopment())
            {
                var filename = Guid.NewGuid().ToString() + ".eml";
                using (StreamWriter data = System.IO.File.CreateText("c:\\mailbox\\" + filename))
                {
                    emailMessage.WriteTo(data.BaseStream);
                }
            }
            else
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.sendgrid.net", 587, false);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    //client.Authenticate("Rodinghc", "Rodinghc@123");
                    client.Authenticate("nehunx64", "Provider123$");

                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
            }
        }

        public void SendEmailInBackground(EmailModel model)
        {
            var jobId = BackgroundJob.Enqueue(() => SendEmail(model));
        }

        public void SendEmailWitheCCInBackground(EmailModel2 model)
        {
            var jobId = BackgroundJob.Enqueue(() => SendEmails(model));
        }

        public async Task SendEmailAsync(string email, string subject, string message,
            bool hasPlainText = true, bool hasHtml = false,
            List<KeyValuePair<string, byte[]>> attachments = null)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Annual Health Check", "noreply@annualhealthcheck.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Importance = MessageImportance.High;

            BodyBuilder bodyBuilder = new BodyBuilder();
            if (hasPlainText)
                bodyBuilder.TextBody = message; //plainTextMessage;

            if (hasHtml)
                bodyBuilder.HtmlBody = message; //htmlMessage;

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var attachment in attachments)
                    bodyBuilder.Attachments.Add(attachment.Key, attachment.Value);
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            //emailMessage.Body = new TextPart("plain") { Text = message };

            await Task.Run(() =>
            {
                if (_environment.IsDevelopment())
                {
                    var filename = Guid.NewGuid().ToString() + ".eml";
                    using (StreamWriter data = System.IO.File.CreateText("c:\\mailbox\\" + filename))
                    {
                        emailMessage.WriteTo(data.BaseStream);
                    }
                }
                else
                {
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.sendgrid.net", 587, false);

                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        client.AuthenticationMechanisms.Remove("XOAUTH2");

                        // Note: only needed if the SMTP server requires authentication
                        client.Authenticate("nehunx64", "Provider123$");

                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }
                }
            });
        }

        public async Task SendEmailConfirmationAsync(string email, string callbackUrl)
        {
            string result = await _engine.CompileRenderAsync("EmailConfirmation.cshtml", new EntityValidationVM() { Link = callbackUrl });

            await SendEmailAsync(email, "Confirm your email", result, false, true);
        }

        public async Task SendNewAccountWithRandomPassword(string email, string subject, NewAccountVM model)
        {
            string result = await _engine.CompileRenderAsync("EmailNewAccountWithRandomPassword.cshtml", model);

            await SendEmailAsync(email, subject, result, false, true);
        }

        public async Task SendForgetPinAsync(string email, string subject, tForgotPinVM tvm)
        {
            string result = await _engine.CompileRenderAsync("ForgotPin_Email.cshtml", tvm);

            await SendEmailAsync(email, subject, result, false, true);
        }

        public void SendNewAccoundWithRandomPasswordInBackground(string email, string subject, NewAccountVM nvm)
        {
            string result = _engine.CompileRenderAsync("EmailNewAccountWithRandomPassword.cshtml", nvm).GetAwaiter().GetResult();

            SendEmailInBackground(new EmailModel() { Email = email, Subject = subject, Message = result, HasPlainText = false, HasHtml = true });
        }

        public void SendEmailConfirmationInBackground(string email, string callbackUrl)
        {
            string result = _engine.CompileRenderAsync("EmailConfirmation.cshtml", new EntityValidationVM() { Link = callbackUrl }).GetAwaiter().GetResult();

            SendEmailInBackground(new EmailModel() { Email = email, Subject = "Confirm your email", Message = result, HasPlainText = false, HasHtml = true });
        }

        public void SendForgetPINBackground(string email, string subject, tForgotPinVM tvm)
        {
            string result = _engine.CompileRenderAsync("ForgotPin_Email.cshtml", tvm).GetAwaiter().GetResult();

            SendEmailInBackground(new EmailModel() { Email = email, Subject = subject, Message = result, HasPlainText = false, HasHtml = true });
        }

        public void SendReferenceLetterViaEmailInBackground(SignUp signUp, string schemeHost)
        {
            string result = _engine.CompileRenderAsync("ReferenceLetter.cshtml", new ReferenceLetterEmailVM() { HMOName = signUp.Enrollee.HMO.Name, FullName = $"{signUp.Enrollee.LastName} {signUp.Enrollee.OtherNames}".ToLower().Humanize(LetterCasing.Title) }).GetAwaiter().GetResult();

            string @controller = "signup", action = "referenceletter";
            string URL = string.Format("{0}/{1}/{2}/{3}", schemeHost, @controller, action, signUp.RefGuid.ToString());

            var attachment = _pdfService.CreateFromUrl(URL);
            var attachmentName = $"Reference_Letter_for_{signUp.Enrollee.EmployeeID}.pdf";

            SendEmailInBackground(new EmailModel()
            {
                Email = signUp.Enrollee.Email,
                Subject = $"{signUp.Enrollee.HMO.Name.ToLower().Humanize(LetterCasing.Title)}: Annual Health Check Reference Letter with Authorization Code {signUp.AuthorizationCode}",
                Message = result,
                HasPlainText = false,
                HasHtml = true,
                Attachments = new List<KeyValuePair<string, byte[]>>() { new KeyValuePair<string, byte[]>(attachmentName, attachment) }
            });
        }

        public void SendReferenceLetterViaEmailCCInBackground(SignUp signUp, string schemeHost)
        {
            List<string> emails = new List<string>();
            // collect all emails.

            if (!string.IsNullOrEmpty(signUp.Enrollee.Email))
                emails.Add(signUp.Enrollee.Email);

            if (!string.IsNullOrEmpty(signUp.Provider.Email))
                emails.Add(signUp.Provider.Email);

            if (!string.IsNullOrEmpty(signUp.Enrollee.HMO.EmailsToCC))
            {
                var tmp = signUp.Enrollee.HMO.EmailsToCC.Split(';');
                emails.AddRange(tmp.Where(r => !string.IsNullOrEmpty(r)));
            }

            var mailAddresses = emails.Where(e => RegexUtilities.IsValidEmail(e));

            if (!mailAddresses.Any())
                return;

            string result = _engine.CompileRenderAsync("ReferenceLetter.cshtml", new ReferenceLetterEmailVM() { HMOName = signUp.Enrollee.HMO.Name, FullName = $"{signUp.Enrollee.LastName} {signUp.Enrollee.OtherNames}".ToLower().Humanize(LetterCasing.Title) }).GetAwaiter().GetResult();

            string @controller = "signup", action = "referenceletter";
            string URL = string.Format("{0}/{1}/{2}/{3}", schemeHost, @controller, action, signUp.RefGuid.ToString());

            var attachment = _pdfService.CreateFromUrl(URL);
            var attachmentName = $"Reference_Letter_for_{signUp.Enrollee.EmployeeID}.pdf";

            SendEmailWitheCCInBackground(new EmailModel2()
            {
                Emails = mailAddresses,
                Subject = $"{signUp.Enrollee.HMO.Name.ToLower().Humanize(LetterCasing.Title)}: Annual Health Check Reference Letter with Authorization Code {signUp.AuthorizationCode}",
                Message = result,
                HasPlainText = false,
                HasHtml = true,
                Attachments = new List<KeyValuePair<string, byte[]>>() { new KeyValuePair<string, byte[]>(attachmentName, attachment) }
            });
        }


        public void SendPreCheckupEmailReminder(SignUp signUp)
        {
            if (string.IsNullOrEmpty(signUp.Enrollee.Email))
                return;

            var model = new PreCheckReminderModel()
            {
                ProviderName = signUp.Provider.Name.ToLower().Humanize(LetterCasing.Title),
                FullNames = $"{signUp.Enrollee.LastName} {signUp.Enrollee.OtherNames}".Trim().ToLower().Humanize(LetterCasing.Title),
                Address = $"{signUp.Provider.Address.TrimEnd(',', ';', ' ', '.')}, {signUp.Provider.State}".ToLower().Humanize(LetterCasing.Title)
            };
            string result = _engine.CompileRenderAsync("PreCheckReminder_Email.cshtml", model).GetAwaiter().GetResult();

            SendEmailInBackground(new EmailModel() { Email = signUp.Enrollee.Email, Subject = "Annual Health Check: Appointment Reminder", Message = result, HasPlainText = false, HasHtml = true });
        }

        public void SendPostCheckupEmailReminder(SignUp signUp, string schemeHost)
        {
            if (string.IsNullOrEmpty(signUp.Enrollee.Email))
                return;

            var model = new PostCheckReminderModel()
            {
                ProviderName = signUp.Provider.Name.ToLower().Humanize(LetterCasing.Title),
                FullNames = $"{signUp.Enrollee.LastName} {signUp.Enrollee.OtherNames}".Trim().ToLower().Humanize(LetterCasing.Title),
                Address = $"{signUp.Provider.Address.TrimEnd(',', ';', ' ', '.')}, {signUp.Provider.State}".ToLower().Humanize(LetterCasing.Title),
                Link = $"{schemeHost}/signup/rating/{signUp.RatingGuid}",
                Day = (signUp.AppointmentDate.Value.Date == DateTime.Now.Date ? "today" : $"{(int)DateTime.Now.Date.Subtract(signUp.AppointmentDate.Value.Date).TotalDays} day(s) ago." )
            };
            string result = _engine.CompileRenderAsync("PostCheckReminder_Email.cshtml", model).GetAwaiter().GetResult();

            SendEmailInBackground(new EmailModel() { Email = signUp.Enrollee.Email, Subject = "Annual Health Check: Appointment Reminder and Rating", Message = result, HasPlainText = false, HasHtml = true });
        }
    }
}
