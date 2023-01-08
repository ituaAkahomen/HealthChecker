using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;


namespace AnnualHealthCheckJs
{
    using Data;
    using Microsoft.Extensions.Hosting;
    using Models;
    using Tools;

    public static class Extensions
    {
        public static TypeCode ConvertToTypeCode(this AttributeTypes attribtype)
        {
            switch (attribtype)
            {
                case AttributeTypes.STRING:
                    return TypeCode.String;
                case AttributeTypes.INT:
                    return TypeCode.Int32;
                case AttributeTypes.LONG:
                    return TypeCode.Int64;
                case AttributeTypes.MONEY:
                    return TypeCode.Decimal;
                case AttributeTypes.NUMBER:
                    return TypeCode.Double;
                case AttributeTypes.BOOL:
                    return TypeCode.Boolean;
                case AttributeTypes.DATETIME:
                    return TypeCode.DateTime;
                default:
                    return TypeCode.Object;
            }
        }

        public static bool IsCorrectGender(this GenderX genderx, Gender sex)
        {
            switch (genderx)
            {
                case GenderX.MALE:
                    if (sex == Gender.FEMALE)
                        return false;
                    else
                        return true;

                case GenderX.FEMALE:
                    if (sex == Gender.MALE)
                        return false;
                    else
                        return true;

                default:
                    return true;
            }
        }

        public static string GenerateAuthCodeFromTemplate(this string template)
        {
            RandomStringGenerator RNG = new RandomStringGenerator(true, false, true, false);
            var regex = new Regex("{(.*?)}");
            var matches = regex.Matches(template);
            foreach (Match match in matches)
            {
                var value = match.Value;
                var val = match.Groups[1].Value;
                switch (val.ToLower())
                {
                    case "d":
                        template = template.Replace(value, DateTime.Now.ToString("d"));
                        break;
                    case "dd":
                        template = template.Replace(value, DateTime.Now.ToString("dd"));
                        break;
                    case "m":
                        template = template.Replace(value, DateTime.Now.ToString("M"));
                        break;
                    case "mm":
                        template = template.Replace(value, DateTime.Now.ToString("MM"));
                        break;
                    case "yy":
                        template = template.Replace(value, DateTime.Now.ToString("yy"));
                        break;
                    case "yyy":
                        template = template.Replace(value, DateTime.Now.ToString("yyyy"));
                        break;
                    case "yyyy":
                        template = template.Replace(value, DateTime.Now.ToString("yyyy"));
                        break;
                    default:
                        string rnds = RNG.Generate(val.ToLower());
                        template = template.Replace(value, rnds);
                        break;
                }
            }
            return template;
        }

        public static string UnformatPhoneNumber(this string phone)
        {
            string tmp = string.Empty;

            if (phone.Length >= 10 && phone.Length <= 14)
            {
                // process correct number here.
                if (phone.Substring(0, 1) == "+")
                {
                    tmp = phone.Substring(1, phone.Length - 1);

                    if (tmp.Length == 13)
                    {
                        if (tmp.Substring(0, 3) == "234")
                        {
                            tmp = RemoveInternationalization(tmp);
                            return AddLeadingZero(tmp);
                        }
                        else
                            return string.Empty;
                    }
                    else if (tmp.Length == 11)
                    {
                        return tmp;
                    }
                    else if (tmp.Length == 10)
                    {
                        return AddLeadingZero(tmp);
                    }
                    else
                        return string.Empty;
                }
                else
                {
                    tmp = phone;

                    if (tmp.Length == 13)
                    {
                        if (tmp.Substring(0, 3) == "234")
                        {
                            tmp = RemoveInternationalization(tmp);
                            return AddLeadingZero(tmp);
                        }
                        else
                            return string.Empty;
                    }
                    else if (tmp.Length == 11)
                    {
                        return tmp;
                    }
                    else if (tmp.Length == 10)
                    {
                        return AddLeadingZero(tmp);
                    }
                    else
                        return string.Empty;
                }
            }
            else
            {
                // wrong number here. not possible for this project
                return string.Empty;
            }
        }

        /// <summary>
        /// Format phone number.
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(this string phone)
        {
            string tmp = string.Empty;

            if (phone.Length >= 10 && phone.Length <= 14)
            {
                // process correct number here.
                if (phone.Substring(0, 1) == "+")
                {
                    tmp = phone.Substring(1, phone.Length - 1);

                    if (tmp.Length == 13)
                    {
                        if (tmp.Substring(0, 3) == "234")
                            return tmp;
                        else
                            return string.Empty;
                    }
                    else if (tmp.Length == 11)
                    {
                        tmp = StripLeadingZero(tmp);
                        return InternationalizeNumber(tmp);
                    }
                    else if (tmp.Length == 10)
                    {
                        return InternationalizeNumber(tmp);
                    }
                    else
                        return string.Empty;
                }
                else
                {
                    tmp = phone;

                    if (tmp.Length == 13)
                    {
                        if (tmp.Substring(0, 3) == "234")
                            return tmp;
                        else
                            return string.Empty;
                    }
                    else if (tmp.Length == 11)
                    {
                        tmp = StripLeadingZero(tmp);
                        return InternationalizeNumber(tmp);
                    }
                    else if (tmp.Length == 10)
                    {
                        return InternationalizeNumber(tmp);
                    }
                    else
                        return string.Empty;
                }
            }
            else
            {
                // wrong number here. not possible for this project
                return string.Empty;
            }
        }

        private static string StripLeadingZero(string number)
        {
            if (number.Substring(0, 1) == "0")
                return number.Substring(1, number.Length - 1);
            else
                return number;
        }

        private static string AddLeadingZero(string number)
        {
            if (number.Substring(0, 1) != "0")
            {
                return number.Insert(0, "0");
            }
            else
                return number;
        }

        private static string InternationalizeNumber(string number)
        {
            // remove any internal spaces.
            number = number.Replace(" ", "");
            return "234" + number;
        }

        private static string RemoveInternationalization(string number)
        {
            if (number.Substring(0, 3) == "234")
            {
                return number.Substring(3, number.Length - 3);
            }
            else
                return number;
        }

    }

    public static class WebHostExtensions
    {
        public static IHost Seed(this IHost webhost)
        {
            using (var scope = webhost.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                // alternatively resolve UserManager instead and pass that if only think you want to seed are the users     
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                SeedData.SeedAsync(dbContext, roleManager, userManager).GetAwaiter().GetResult();

                dbContext.Dispose();
                roleManager.Dispose();
                userManager.Dispose();
            }
            return webhost;
        }
    }

    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o;
            tempData.TryGetValue(key, out o);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }
    }
}
