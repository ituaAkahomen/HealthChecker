using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class rescheduleability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignUpReschedules",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SignUpID = table.Column<int>(nullable: false),
                    OldAppointmentDate = table.Column<DateTime>(nullable: false),
                    OldProviderID = table.Column<int>(nullable: false),
                    OldAuthorizationCode = table.Column<string>(nullable: true),
                    NewAppointmentDate = table.Column<DateTime>(nullable: false),
                    NewProviderID = table.Column<int>(nullable: false),
                    NewAuthorizationCode = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignUpReschedules", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SignUpReschedules_Providers_NewProviderID",
                        column: x => x.NewProviderID,
                        principalTable: "Providers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SignUpReschedules_Providers_OldProviderID",
                        column: x => x.OldProviderID,
                        principalTable: "Providers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SignUpReschedules_SignUps_SignUpID",
                        column: x => x.SignUpID,
                        principalTable: "SignUps",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignUpReschedules_NewProviderID",
                table: "SignUpReschedules",
                column: "NewProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_SignUpReschedules_OldProviderID",
                table: "SignUpReschedules",
                column: "OldProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_SignUpReschedules_SignUpID",
                table: "SignUpReschedules",
                column: "SignUpID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignUpReschedules");
        }
    }
}
