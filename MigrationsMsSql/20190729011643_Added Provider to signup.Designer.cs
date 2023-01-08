﻿// <auto-generated />
using System;
using AnnualHealthCheckJs.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AnnualHealthCheckJs.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190729011643_Added Provider to signup")]
    partial class AddedProvidertosignup
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AspNetRoleId")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("Role","Security");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AspNetUserId")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount");

                    b.Property<DateTime?>("AccountExpires");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("Enabled");

                    b.Property<Guid>("Guid");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<int>("ProfileType");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("User","Security");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Country", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ISO_Code_2")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.Property<string>("ISO_Code_3")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("ID");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Enrollee", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClientPlan")
                        .IsRequired();

                    b.Property<DateTime?>("DOB");

                    b.Property<string>("Email");

                    b.Property<string>("EmployeeID")
                        .IsRequired();

                    b.Property<bool>("Enabled");

                    b.Property<string>("EnrollmentID");

                    b.Property<int>("Gender");

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<string>("MobileNumber");

                    b.Property<string>("OtherNames");

                    b.Property<string>("PIN")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.Property<string>("TmpAuthCode");

                    b.HasKey("ID");

                    b.ToTable("Enrollees");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ExcludedEnrollee", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EnrolleeID");

                    b.Property<string>("Reason");

                    b.Property<int>("Year");

                    b.HasKey("ID");

                    b.HasIndex("EnrolleeID");

                    b.ToTable("ExcludedEnrollees");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportEnrolleeSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Settings");

                    b.HasKey("ID");

                    b.ToTable("ImportEnrolleeSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportExcludedEnrolleeSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Settings");

                    b.HasKey("ID");

                    b.ToTable("ImportExcludedEnrolleeSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportProviderSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Settings");

                    b.HasKey("ID");

                    b.ToTable("ImportProviderSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportServiceSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Settings");

                    b.HasKey("ID");

                    b.ToTable("ImportServiceSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Location", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("StateID");

                    b.HasKey("ID");

                    b.HasIndex("StateID");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ProjectConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("EndDate");

                    b.Property<string>("Param1");

                    b.Property<string>("Param10");

                    b.Property<string>("Param2");

                    b.Property<string>("Param3");

                    b.Property<string>("Param4");

                    b.Property<string>("Param5");

                    b.Property<string>("Param6");

                    b.Property<string>("Param7");

                    b.Property<string>("Param8");

                    b.Property<string>("Param9");

                    b.Property<string>("ProjectName")
                        .IsRequired();

                    b.Property<DateTime>("StartDate");

                    b.HasKey("ID");

                    b.ToTable("ProjectConfig");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Provider", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .IsRequired();

                    b.Property<int>("LocationID");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("StateID");

                    b.HasKey("ID");

                    b.HasIndex("LocationID");

                    b.HasIndex("StateID");

                    b.ToTable("Providers");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Service", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal?>("GTE_Age")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Gender");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUp", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("AlternateAppointmentDate");

                    b.Property<DateTime?>("AppointmentDate");

                    b.Property<string>("AuthorizationCode");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime?>("DateUpdated");

                    b.Property<int>("EnrolleeID");

                    b.Property<int?>("LocationID");

                    b.Property<int?>("ProviderID");

                    b.Property<Guid?>("RefGuid");

                    b.Property<int?>("Stage");

                    b.Property<int>("Year");

                    b.HasKey("ID");

                    b.HasIndex("EnrolleeID");

                    b.HasIndex("LocationID");

                    b.HasIndex("ProviderID");

                    b.ToTable("SignUps");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.State", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code")
                        .HasMaxLength(50);

                    b.Property<int>("CountryID");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(70);

                    b.HasKey("ID");

                    b.HasIndex("CountryID");

                    b.ToTable("States");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AspNetRoleClaimId")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("RoleId")
                        .HasColumnName("AspNetRoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaim","Security");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AspNetUserClaimId")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("UserId")
                        .HasColumnName("AspNetUserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaim","Security");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<int>("UserId")
                        .HasColumnName("AspNetUserId");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogin","Security");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnName("AspNetUserId");

                    b.Property<int>("RoleId")
                        .HasColumnName("AspNetRoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRole","Security");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnName("AspNetUserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("tUserToken","Security");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ExcludedEnrollee", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Enrollee", "Enrollee")
                        .WithMany()
                        .HasForeignKey("EnrolleeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Location", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.State", "State")
                        .WithMany("Locations")
                        .HasForeignKey("StateID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Provider", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AnnualHealthCheckJs.Models.State", "State")
                        .WithMany()
                        .HasForeignKey("StateID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUp", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Enrollee", "Enrollee")
                        .WithMany()
                        .HasForeignKey("EnrolleeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AnnualHealthCheckJs.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationID");

                    b.HasOne("AnnualHealthCheckJs.Models.Provider", "Provider")
                        .WithMany()
                        .HasForeignKey("ProviderID");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.State", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Country", "Country")
                        .WithMany("States")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
