using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class StringLengthbugforPIN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PIN",
                table: "Enrollees",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 5,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PIN",
                table: "Enrollees",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
