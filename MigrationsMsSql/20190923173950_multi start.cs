using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class multistart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HMOID",
                schema: "Security",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOn",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOn_ByAdmin",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOn_ByProvider",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckedOn_UserID",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "SignUps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RatingGuid",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HMOID",
                table: "Services",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HMOID",
                table: "Providers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPIN",
                table: "ProjectConfig",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "ProjectConfig",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HMOID",
                table: "Enrollees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdminVM",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    hmoId = table.Column<int>(nullable: false),
                    HMO = table.Column<string>(nullable: true),
                    provId = table.Column<int>(nullable: false),
                    Provider = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminVM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HMOs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    SignatoryName = table.Column<string>(nullable: false),
                    SignatoryDesignation = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    AuthCodeTemplate = table.Column<string>(nullable: false),
                    PinRegex = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HMOs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ResetPins",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LinkID = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    EnrolleeID = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateExpired = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPins", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ResetPins_Enrollees_EnrolleeID",
                        column: x => x.EnrolleeID,
                        principalTable: "Enrollees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserProviders",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(nullable: false),
                    ProviderID = table.Column<int>(nullable: false),
                    HMOID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProviders", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProviders_HMOs_HMOID",
                        column: x => x.HMOID,
                        principalTable: "HMOs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProviders_Providers_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "Providers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProviders_User_UserID",
                        column: x => x.UserID,
                        principalSchema: "Security",
                        principalTable: "User",
                        principalColumn: "AspNetUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_HMOID",
                schema: "Security",
                table: "User",
                column: "HMOID");

            migrationBuilder.CreateIndex(
                name: "IX_Services_HMOID",
                table: "Services",
                column: "HMOID");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_HMOID",
                table: "Providers",
                column: "HMOID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollees_HMOID",
                table: "Enrollees",
                column: "HMOID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProviders_HMOID",
                table: "ApplicationUserProviders",
                column: "HMOID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProviders_ProviderID",
                table: "ApplicationUserProviders",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProviders_UserID",
                table: "ApplicationUserProviders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPins_EnrolleeID",
                table: "ResetPins",
                column: "EnrolleeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollees_HMOs_HMOID",
                table: "Enrollees",
                column: "HMOID",
                principalTable: "HMOs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_HMOs_HMOID",
                table: "Providers",
                column: "HMOID",
                principalTable: "HMOs",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_HMOs_HMOID",
                table: "Services",
                column: "HMOID",
                principalTable: "HMOs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_HMOs_HMOID",
                schema: "Security",
                table: "User",
                column: "HMOID",
                principalTable: "HMOs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollees_HMOs_HMOID",
                table: "Enrollees");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_HMOs_HMOID",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_HMOs_HMOID",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_User_HMOs_HMOID",
                schema: "Security",
                table: "User");

            migrationBuilder.DropTable(
                name: "AdminVM");

            migrationBuilder.DropTable(
                name: "ApplicationUserProviders");

            migrationBuilder.DropTable(
                name: "ResetPins");

            migrationBuilder.DropTable(
                name: "HMOs");

            migrationBuilder.DropIndex(
                name: "IX_User_HMOID",
                schema: "Security",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Services_HMOID",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Providers_HMOID",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Enrollees_HMOID",
                table: "Enrollees");

            migrationBuilder.DropColumn(
                name: "HMOID",
                schema: "Security",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CheckedOn",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "CheckedOn_ByAdmin",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "CheckedOn_ByProvider",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "CheckedOn_UserID",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "RatingGuid",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "HMOID",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "HMOID",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "DefaultPIN",
                table: "ProjectConfig");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "ProjectConfig");

            migrationBuilder.DropColumn(
                name: "HMOID",
                table: "Enrollees");
        }
    }
}
