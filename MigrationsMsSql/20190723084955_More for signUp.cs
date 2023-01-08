using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class MoreforsignUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AlternateAppointmentDate",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "SignUps",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationID",
                table: "SignUps",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignUps_LocationID",
                table: "SignUps",
                column: "LocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_SignUps_Locations_LocationID",
                table: "SignUps",
                column: "LocationID",
                principalTable: "Locations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignUps_Locations_LocationID",
                table: "SignUps");

            migrationBuilder.DropIndex(
                name: "IX_SignUps_LocationID",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "AlternateAppointmentDate",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "LocationID",
                table: "SignUps");
        }
    }
}
