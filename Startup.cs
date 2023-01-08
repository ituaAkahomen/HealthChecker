using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnnualHealthCheckJs.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NToastNotify;
using RazorLight;
using DinkToPdf;
using DinkToPdf.Contracts;

using Hangfire;
using reCAPTCHA.AspNetCore;


namespace AnnualHealthCheckJs
{
    using Data;
    using Hangfire.MySql.Core;
    using Microsoft.AspNetCore.HttpOverrides;
    using Models;
    using Results;
    using Services;
    using Services.Core;
    using System.Reflection;

    public class Startup
    {

        public readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            string mySqlConnectionStr = string.Empty;
            if (_env.IsDevelopment())
                mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            else
                mySqlConnectionStr = "server=localhost;port=3306;database=multiannualhealthcheckdb;user=zinor;password=u8QxtkTLa4;Persist Security Info=False;Connect Timeout=300";

            services.AddDbContextPool<ApplicationDbContext>(options => options
                                            .UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            //services.AddIdentity<ApplicationUser, ApplicationRole>()
            //        .AddRoles<ApplicationRole>()
            //        //.AddDefaultUI(UIFramework.Bootstrap4)
            //        .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                //options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
             .AddRoles<ApplicationRole>()
             .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));

            services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmsSender, SmsSender>();
            services.AddTransient<IPdfService, PdfService>();

            services.AddTransient(typeof(AdminResult));
            services.AddTransient(typeof(AdminResult2));
            services.AddTransient(typeof(EnrolleeResult));
            services.AddTransient(typeof(EnrolleeResult2));
            services.AddTransient(typeof(ExEnrolleeResult));
            services.AddTransient(typeof(ExEnrolleeResult2));
            services.AddTransient(typeof(LocationResult));
            services.AddTransient(typeof(ProviderResult));
            services.AddTransient(typeof(ProviderResult2));
            services.AddTransient(typeof(ServiceResult));
            services.AddTransient(typeof(ServiceResult2));
            services.AddTransient(typeof(SignupResult));
            services.AddTransient(typeof(SignupResult2));
            services.AddTransient(typeof(PendingSignupResult));
            services.AddTransient(typeof(StateResult));
            services.AddTransient(typeof(HMOResult));

            services.AddTransient(typeof(GenderUtilizationReportResult));
            services.AddTransient(typeof(UtilizationByAgeRangeReportResult));
            services.AddTransient(typeof(RatingReportResult));

            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IImportProcessorService, ImportProcessorService>();

            services.AddSingleton<IRazorLightEngine>(f =>
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                return new RazorLightEngineBuilder()
                                .UseEmbeddedResourcesProject(assembly, "AnnualHealthCheckJs.Templates")              //(System.Reflection.Assembly.GetEntryAssembly())
                                .UseMemoryCachingProvider()
                                .SetOperatingAssembly(assembly)
                                .Build();
            });
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection")));
            services.AddHangfireServer();

            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation()
                    .AddNToastNotifyToastr(new ToastrOptions()
                    {
                        ProgressBar = false,
                        PositionClass = ToastPositions.TopRight
                    })
                    .AddNewtonsoftJson();

            services.AddHttpContextAccessor();

            string hangfireConnectionStr = string.Empty;
            if (_env.IsDevelopment())
                hangfireConnectionStr = Configuration.GetConnectionString("HangfireConnection");
            else
                hangfireConnectionStr = "server=localhost;port=3306;user=zinor;password=u8QxtkTLa4;database=multiannualhealthcheckhangfiredb;SslMode=none;Allow User Variables=True";

            services.AddHangfire(x => x.UseStorage(new MySqlStorage(hangfireConnectionStr, new MySqlStorageOptions()
            {
                TransactionIsolationLevel = System.Data.IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
            })));

            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            if (env.IsProduction())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseHangfireDashboard();

            app.UseNToastNotify();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "areas",
            //        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });


            var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                new HangfireInit(scope.ServiceProvider.GetService<INotificationService>()).Initialize();
            }
            //(new HangfireInit(app.ApplicationServices.GetService<INotificationService>())).Initialize();
        }
    }
}
