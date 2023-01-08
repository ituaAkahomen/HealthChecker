﻿// <auto-generated />
using System;
using AnnualHealthCheckJs.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AnnualHealthCheckJs.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<DateTime?>("AccountExpires")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid>("Guid")
                        .HasColumnType("char(36)");

                    b.Property<int?>("HMOID")
                        .HasColumnType("int");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("ProfileType")
                        .HasColumnType("int");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("HMOID");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUserProvider", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("HMOID")
                        .HasColumnType("int");

                    b.Property<int>("ProviderID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("HMOID");

                    b.HasIndex("ProviderID");

                    b.HasIndex("UserID");

                    b.ToTable("ApplicationUserProviders");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Country", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ISO_Code_2")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("ISO_Code_3")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("ID");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Enrollee", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClientPlan")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("DOB")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("EmployeeID")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("EnrollmentID")
                        .HasColumnType("longtext");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<int>("HMOID")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("OtherNames")
                        .HasColumnType("longtext");

                    b.Property<string>("PIN")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("TmpAuthCode")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("HMOID");

                    b.ToTable("Enrollees");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ExcludedEnrollee", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("EnrolleeID")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("EnrolleeID");

                    b.ToTable("ExcludedEnrollees");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.HMO", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AuthCodeTemplate")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("EmailsToCC")
                        .HasColumnType("longtext");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool?>("GenerateAuthCodeOnSignUpComplete")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid>("Guid")
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("PinRegex")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SignatoryDesignation")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SignatoryName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("HMOs");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportEnrolleeSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Settings")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("ImportEnrolleeSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportExcludedEnrolleeSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Settings")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("ImportExcludedEnrolleeSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportProviderSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Settings")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("ImportProviderSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ImportServiceSettings", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Settings")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("ImportServiceSettings");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Location", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("StateID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("StateID");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ProjectConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("DefaultPIN")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Param1")
                        .HasColumnType("longtext");

                    b.Property<string>("Param10")
                        .HasColumnType("longtext");

                    b.Property<string>("Param2")
                        .HasColumnType("longtext");

                    b.Property<string>("Param3")
                        .HasColumnType("longtext");

                    b.Property<string>("Param4")
                        .HasColumnType("longtext");

                    b.Property<string>("Param5")
                        .HasColumnType("longtext");

                    b.Property<string>("Param6")
                        .HasColumnType("longtext");

                    b.Property<string>("Param7")
                        .HasColumnType("longtext");

                    b.Property<string>("Param8")
                        .HasColumnType("longtext");

                    b.Property<string>("Param9")
                        .HasColumnType("longtext");

                    b.Property<string>("ProjectName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("ProjectConfig");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Provider", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<bool?>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("HMOID")
                        .HasColumnType("int");

                    b.Property<int>("LocationID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<int>("StateID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("HMOID");

                    b.HasIndex("LocationID");

                    b.HasIndex("StateID");

                    b.ToTable("Providers");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ResetPin", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Code")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("DateExpired")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EnrolleeID")
                        .HasColumnType("int");

                    b.Property<string>("LinkID")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("EnrolleeID");

                    b.ToTable("ResetPins");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Service", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool?>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<decimal?>("GTE_Age")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<int>("HMOID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("HMOID");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUp", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("AlternateAppointmentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("AppointmentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("AuthorizationCode")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("CheckedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("CheckedOn_ByAdmin")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("CheckedOn_ByProvider")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("CheckedOn_UserID")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DateUpdated")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EnrolleeID")
                        .HasColumnType("int");

                    b.Property<int?>("LocationID")
                        .HasColumnType("int");

                    b.Property<int?>("ProviderID")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<string>("RatingGuid")
                        .HasColumnType("longtext");

                    b.Property<Guid?>("RefGuid")
                        .HasColumnType("char(36)");

                    b.Property<int?>("Stage")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("EnrolleeID");

                    b.HasIndex("LocationID");

                    b.HasIndex("ProviderID");

                    b.ToTable("SignUps");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUpReschedule", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("NewAppointmentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NewAuthorizationCode")
                        .HasColumnType("longtext");

                    b.Property<int>("NewProviderID")
                        .HasColumnType("int");

                    b.Property<DateTime>("OldAppointmentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("OldAuthorizationCode")
                        .HasColumnType("longtext");

                    b.Property<int>("OldProviderID")
                        .HasColumnType("int");

                    b.Property<int>("SignUpID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("NewProviderID");

                    b.HasIndex("OldProviderID");

                    b.HasIndex("SignUpID");

                    b.ToTable("SignUpReschedules");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.State", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("CountryID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(70)
                        .HasColumnType("varchar(70)");

                    b.HasKey("ID");

                    b.HasIndex("CountryID");

                    b.ToTable("States");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.ViewModels.AdminVM", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("HMO")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("Provider")
                        .HasColumnType("longtext");

                    b.Property<int>("hmoId")
                        .HasColumnType("int");

                    b.Property<int>("provId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("AdminVM");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUser", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.HMO", "HMO")
                        .WithMany()
                        .HasForeignKey("HMOID");

                    b.Navigation("HMO");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUserProvider", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.HMO", "HMO")
                        .WithMany()
                        .HasForeignKey("HMOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.Provider", "Provider")
                        .WithMany()
                        .HasForeignKey("ProviderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser", "User")
                        .WithMany("Providers")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HMO");

                    b.Navigation("Provider");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Enrollee", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.HMO", "HMO")
                        .WithMany()
                        .HasForeignKey("HMOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HMO");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ExcludedEnrollee", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Enrollee", "Enrollee")
                        .WithMany()
                        .HasForeignKey("EnrolleeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enrollee");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Location", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.State", "State")
                        .WithMany("Locations")
                        .HasForeignKey("StateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("State");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Provider", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.HMO", "HMO")
                        .WithMany()
                        .HasForeignKey("HMOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.State", "State")
                        .WithMany()
                        .HasForeignKey("StateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HMO");

                    b.Navigation("Location");

                    b.Navigation("State");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ResetPin", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Enrollee", "Enrollee")
                        .WithMany()
                        .HasForeignKey("EnrolleeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enrollee");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Service", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.HMO", "HMO")
                        .WithMany()
                        .HasForeignKey("HMOID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HMO");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUp", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Enrollee", "Enrollee")
                        .WithMany()
                        .HasForeignKey("EnrolleeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationID");

                    b.HasOne("AnnualHealthCheckJs.Models.Provider", "Provider")
                        .WithMany()
                        .HasForeignKey("ProviderID");

                    b.Navigation("Enrollee");

                    b.Navigation("Location");

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.SignUpReschedule", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Provider", "NewProvider")
                        .WithMany()
                        .HasForeignKey("NewProviderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.Provider", "OldProvider")
                        .WithMany()
                        .HasForeignKey("OldProviderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.SignUp", "SignUp")
                        .WithMany()
                        .HasForeignKey("SignUpID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NewProvider");

                    b.Navigation("OldProvider");

                    b.Navigation("SignUp");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.State", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.Country", "Country")
                        .WithMany("States")
                        .HasForeignKey("CountryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("AnnualHealthCheckJs.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.ApplicationUser", b =>
                {
                    b.Navigation("Providers");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.Country", b =>
                {
                    b.Navigation("States");
                });

            modelBuilder.Entity("AnnualHealthCheckJs.Models.State", b =>
                {
                    b.Navigation("Locations");
                });
#pragma warning restore 612, 618
        }
    }
}
