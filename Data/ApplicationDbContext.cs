using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AnnualHealthCheckJs.ViewModels;

namespace AnnualHealthCheckJs.Data
{
    using Models;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUserProvider> ApplicationUserProviders { get; set; }
        public DbSet<ProjectConfig> ProjectConfig { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Enrollee> Enrollees { get; set; }
        public DbSet<ExcludedEnrollee> ExcludedEnrollees { get; set; }
        public DbSet<ResetPin> ResetPins { get; set; }
        public DbSet<HMO> HMOs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<SignUp> SignUps { get; set; }
        public DbSet<SignUpReschedule> SignUpReschedules { get; set; }
        public DbSet<State> States { get; set; }

        public DbSet<ImportEnrolleeSettings> ImportEnrolleeSettings { get; set; }
        public DbSet<ImportExcludedEnrolleeSettings> ImportExcludedEnrolleeSettings { get; set; }
        public DbSet<ImportProviderSettings> ImportProviderSettings { get; set; }
        public DbSet<ImportServiceSettings> ImportServiceSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity => {
                entity.Property(m => m.Email).HasMaxLength(127);
                entity.Property(m => m.NormalizedEmail).HasMaxLength(127);
                entity.Property(m => m.NormalizedUserName).HasMaxLength(127);
                entity.Property(m => m.UserName).HasMaxLength(127);
            });

            builder.Entity<ApplicationRole>(entity => {
                entity.Property(m => m.Name).HasMaxLength(127);
                entity.Property(m => m.NormalizedName).HasMaxLength(127);
            });

            //builder.Entity<ApplicationUser>(entity =>
            //{
            //    entity.ToTable(name: "User", schema: "Security");
            //    entity.Property(e => e.Id).HasColumnName("AspNetUserId");

            //});

            //builder.Entity<ApplicationRole>(entity =>
            //{
            //    entity.ToTable(name: "Role", schema: "Security");
            //    entity.Property(e => e.Id).HasColumnName("AspNetRoleId");

            //});

            //builder.Entity<IdentityUserClaim<int>>(entity =>
            //{
            //    entity.ToTable("UserClaim", "Security");
            //    entity.Property(e => e.UserId).HasColumnName("AspNetUserId");
            //    entity.Property(e => e.Id).HasColumnName("AspNetUserClaimId");

            //});

            //builder.Entity<IdentityUserLogin<int>>(entity =>
            //{
            //    entity.ToTable("UserLogin", "Security");
            //    entity.Property(e => e.UserId).HasColumnName("AspNetUserId");

            //});

            //builder.Entity<IdentityRoleClaim<int>>(entity =>
            //{
            //    entity.ToTable("RoleClaim", "Security");
            //    entity.Property(e => e.Id).HasColumnName("AspNetRoleClaimId");
            //    entity.Property(e => e.RoleId).HasColumnName("AspNetRoleId");
            //});

            //builder.Entity<IdentityUserRole<int>>(entity =>
            //{
            //    entity.ToTable("UserRole", "Security");
            //    entity.Property(e => e.UserId).HasColumnName("AspNetUserId");
            //    entity.Property(e => e.RoleId).HasColumnName("AspNetRoleId");

            //});

            //builder.Entity<IdentityUserToken<int>>(entity =>
            //{
            //    entity.ToTable("tUserToken", "Security");
            //    entity.Property(e => e.UserId).HasColumnName("AspNetUserId");
            //});
        }

        public DbSet<AnnualHealthCheckJs.ViewModels.AdminVM> AdminVM { get; set; }
    }
}
