using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

namespace AnnualHealthCheckJs.Data
{
    using Models;

    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            await InitializeRoles(context, roleManager);
            await InitializeSUs(context, userManager);
            await InitializeCountries(context);
            await InitializeStates(context);
            await InitializeProjectConfig(context);
        }

        public static async Task InitializeRoles(ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            var roles = Enum.GetNames(typeof(ProfileTypes));

            foreach (var role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole(role));
                    if (result.Succeeded)
                    {
                        // do nothing.
                    }
                }
            }
        }

        public static async Task InitializeSUs(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            foreach (var email in SUs.Get())
            {
                var tmpUser = await userManager.FindByEmailAsync(email);
                if (tmpUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        DateCreated = DateTime.UtcNow,
                        Enabled = true,
                        ProfileType = ProfileTypes.SU,
                        Guid = Guid.NewGuid()
                    };
                    var result = await userManager.CreateAsync(user, "Itua123456&");
                    if (result.Succeeded)
                    {
                        result = await userManager.AddToRolesAsync(user, new List<string>() { Enum.GetName(typeof(ProfileTypes), ProfileTypes.SU) });
                    }
                }
            }
        }

        public static async Task InitializeProjectConfig(ApplicationDbContext context)
        {
            if (context.ProjectConfig.Any())
                return;

            await context.ProjectConfig.AddAsync(new ProjectConfig() { ProjectName = "Annual Health Check", StartDate = new DateTime(2019, 9, 30), EndDate = new DateTime(2019, 12, 21), Year = 2019, DefaultPIN = "2340" });
            await context.SaveChangesAsync();
        }

        public static async Task InitializeCountries(ApplicationDbContext context)
        {
            if (context.Countries.Any())
                return;

            //var Countries = new List<Country>();
            //Countries.Add(new Country() { ID = 156, Name = "Nigeria", ISO_Code_2 = "NG", ISO_Code_3 = "NGA" });

            //context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Countries] ON");
            //foreach (var country in Countries)
            //{
            //    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Countries] ON;INSERT [dbo].[Countries] (ID, Name, ISO_Code_2, ISO_Code_3) " +
            //                                   "VALUES (@0, @1, @2, @3)",
            //                                   new SqlParameter("@0", country.ID),
            //                                   new SqlParameter("@1", country.Name),
            //                                   new SqlParameter("@2", country.ISO_Code_2),
            //                                   new SqlParameter("@3", country.ISO_Code_3));
            //}
            //context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Countries] OFF");

            //await context.Countries.LoadAsync();      //.Load();

            context.Countries.Add(new Country()
            {
                Name = "Nigeria",
                ISO_Code_2 = "NG",
                ISO_Code_3 = "NGA",
                States = new List<State>()
                {
                    new State() { Name = "Abia", Code = "AB" },
                    new State() { Name = "Abuja Federal Capital Territory", Code = "CT" },
                    new State() { Name = "Adamawa", Code = "AD" },
                    new State() { Name = "Akwa Ibom", Code = "AK" },
                    new State() { Name = "Anambra", Code = "AN" },
                    new State() { Name = "Bauchi", Code = "BC" },
                    new State() { Name = "Bayelsa", Code = "BY" },
                    new State() { Name = "Benue", Code = "BN" },
                    new State() { Name = "Borno", Code = "BO" },
                    new State() { Name = "Cross River", Code = "CR" },
                    new State() { Name = "Delta", Code = "DE" },
                    new State() { Name = "Ebonyi", Code = "EB" },
                    new State() { Name = "Edo", Code = "ED" },
                    new State() { Name = "Ekiti", Code = "EK" },
                    new State() { Name = "Enugu", Code = "EN" },
                    new State() { Name = "Gombe", Code = "GO" },
                    new State() { Name = "Imo", Code = "IM" },
                    new State() { Name = "Jigawa", Code = "JI" },
                    new State() { Name = "Kaduna", Code = "KD" },
                    new State() { Name = "Kano", Code = "KN" },
                    new State() { Name = "Katsina", Code = "KT" },
                    new State() { Name = "Kebbi", Code = "KE" },
                    new State() { Name = "Kogi", Code = "KO" },
                    new State() { Name = "Kwara", Code = "KW" },
                    new State() { Name = "Lagos", Code = "LA" },
                    new State() { Name = "Nasarawa", Code = "NA" },
                    new State() { Name = "Niger", Code = "NI" },
                    new State() { Name = "Ogun", Code = "OG" },
                    new State() { Name = "Ondo", Code = "OD" },
                    new State() { Name = "Osun", Code = "OS" },
                    new State() { Name = "Oyo", Code = "OY" },
                    new State() { Name = "Plateau", Code = "PL" },
                    new State() { Name = "Rivers", Code = "RI" },
                    new State() { Name = "Sokoto", Code = "SO" },
                    new State() { Name = "Taraba", Code = "TA" },
                    new State() { Name = "Yobe", Code = "YO" },
                    new State() { Name = "Zamfara", Code = "ZA" },
                }
            });

            await context.SaveChangesAsync();
        }

        public static async Task InitializeStates(ApplicationDbContext context)
        {
            if (context.States.Any())
                return;

            //#region States
            //var States = new List<State>();
            //States.Add(new State() { ID = 2388, Name = "Abia", Code = "AB", CountryID = 156 });
            //States.Add(new State() { ID = 2389, Name = "Abuja Federal Capital Territory", Code = "CT", CountryID = 156 });
            //States.Add(new State() { ID = 2390, Name = "Adamawa", Code = "AD", CountryID = 156 });
            //States.Add(new State() { ID = 2391, Name = "Akwa Ibom", Code = "AK", CountryID = 156 });
            //States.Add(new State() { ID = 2392, Name = "Anambra", Code = "AN", CountryID = 156 });
            //States.Add(new State() { ID = 2393, Name = "Bauchi", Code = "BC", CountryID = 156 });
            //States.Add(new State() { ID = 2394, Name = "Bayelsa", Code = "BY", CountryID = 156 });
            //States.Add(new State() { ID = 2395, Name = "Benue", Code = "BN", CountryID = 156 });
            //States.Add(new State() { ID = 2396, Name = "Borno", Code = "BO", CountryID = 156 });
            //States.Add(new State() { ID = 2397, Name = "Cross River", Code = "CR", CountryID = 156 });
            //States.Add(new State() { ID = 2398, Name = "Delta", Code = "DE", CountryID = 156 });
            //States.Add(new State() { ID = 2399, Name = "Ebonyi", Code = "EB", CountryID = 156 });
            //States.Add(new State() { ID = 2400, Name = "Edo", Code = "ED", CountryID = 156 });
            //States.Add(new State() { ID = 2401, Name = "Ekiti", Code = "EK", CountryID = 156 });
            //States.Add(new State() { ID = 2402, Name = "Enugu", Code = "EN", CountryID = 156 });
            //States.Add(new State() { ID = 2403, Name = "Gombe", Code = "GO", CountryID = 156 });
            //States.Add(new State() { ID = 2404, Name = "Imo", Code = "IM", CountryID = 156 });
            //States.Add(new State() { ID = 2405, Name = "Jigawa", Code = "JI", CountryID = 156 });
            //States.Add(new State() { ID = 2406, Name = "Kaduna", Code = "KD", CountryID = 156 });
            //States.Add(new State() { ID = 2407, Name = "Kano", Code = "KN", CountryID = 156 });
            //States.Add(new State() { ID = 2408, Name = "Katsina", Code = "KT", CountryID = 156 });
            //States.Add(new State() { ID = 2409, Name = "Kebbi", Code = "KE", CountryID = 156 });
            //States.Add(new State() { ID = 2410, Name = "Kogi", Code = "KO", CountryID = 156 });
            //States.Add(new State() { ID = 2411, Name = "Kwara", Code = "KW", CountryID = 156 });
            //States.Add(new State() { ID = 2412, Name = "Lagos", Code = "LA", CountryID = 156 });
            //States.Add(new State() { ID = 2413, Name = "Nasarawa", Code = "NA", CountryID = 156 });
            //States.Add(new State() { ID = 2414, Name = "Niger", Code = "NI", CountryID = 156 });
            //States.Add(new State() { ID = 2415, Name = "Ogun", Code = "OG", CountryID = 156 });
            //States.Add(new State() { ID = 2416, Name = "Ondo", Code = "ONG", CountryID = 156 });
            //States.Add(new State() { ID = 2417, Name = "Osun", Code = "OS", CountryID = 156 });
            //States.Add(new State() { ID = 2418, Name = "Oyo", Code = "OY", CountryID = 156 });
            //States.Add(new State() { ID = 2419, Name = "Plateau", Code = "PL", CountryID = 156 });
            //States.Add(new State() { ID = 2420, Name = "Rivers", Code = "RI", CountryID = 156 });
            //States.Add(new State() { ID = 2421, Name = "Sokoto", Code = "SO", CountryID = 156 });
            //States.Add(new State() { ID = 2422, Name = "Taraba", Code = "TA", CountryID = 156 });
            //States.Add(new State() { ID = 2423, Name = "Yobe", Code = "YO", CountryID = 156 });
            //States.Add(new State() { ID = 2424, Name = "Zamfara", Code = "ZA", CountryID = 156 });
            //#endregion

            //context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[States] ON");
            //foreach (var zon in States)
            //{
            //    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[States] ON;INSERT [dbo].[States] (ID, Name, Code, CountryID) " +
            //                                   "VALUES (@0, @1, @2, @3)",
            //                                   new SqlParameter("@0", zon.ID),
            //                                   new SqlParameter("@1", zon.Name),
            //                                   new SqlParameter("@2", zon.Code),
            //                                   new SqlParameter("@3", zon.CountryID));
            //}
            //context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[States] OFF");

            //await context.States.LoadAsync();
        }
    }
}