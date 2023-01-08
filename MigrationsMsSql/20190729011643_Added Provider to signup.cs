using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class AddedProvidertosignup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProviderID",
                table: "SignUps",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignUps_ProviderID",
                table: "SignUps",
                column: "ProviderID");

            migrationBuilder.AddForeignKey(
                name: "FK_SignUps_Providers_ProviderID",
                table: "SignUps",
                column: "ProviderID",
                principalTable: "Providers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignUps_Providers_ProviderID",
                table: "SignUps");

            migrationBuilder.DropIndex(
                name: "IX_SignUps_ProviderID",
                table: "SignUps");

            migrationBuilder.DropColumn(
                name: "ProviderID",
                table: "SignUps");
        }
    }
}
