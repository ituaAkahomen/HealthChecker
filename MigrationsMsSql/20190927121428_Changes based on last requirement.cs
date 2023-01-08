using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class Changesbasedonlastrequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailsToCC",
                table: "HMOs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GenerateAuthCodeOnSignUpComplete",
                table: "HMOs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailsToCC",
                table: "HMOs");

            migrationBuilder.DropColumn(
                name: "GenerateAuthCodeOnSignUpComplete",
                table: "HMOs");
        }
    }
}
