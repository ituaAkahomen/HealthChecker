using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class AddedTmpAuthCodeandothernamestoEnrollerModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Enrollees",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "OtherNames",
                table: "Enrollees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TmpAuthCode",
                table: "Enrollees",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherNames",
                table: "Enrollees");

            migrationBuilder.DropColumn(
                name: "TmpAuthCode",
                table: "Enrollees");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Enrollees",
                newName: "FullName");
        }
    }
}
