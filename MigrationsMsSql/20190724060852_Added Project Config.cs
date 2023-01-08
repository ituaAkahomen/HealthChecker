using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnualHealthCheckJs.Migrations
{
    public partial class AddedProjectConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectConfig",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProjectName = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Param1 = table.Column<string>(nullable: true),
                    Param2 = table.Column<string>(nullable: true),
                    Param3 = table.Column<string>(nullable: true),
                    Param4 = table.Column<string>(nullable: true),
                    Param5 = table.Column<string>(nullable: true),
                    Param6 = table.Column<string>(nullable: true),
                    Param7 = table.Column<string>(nullable: true),
                    Param8 = table.Column<string>(nullable: true),
                    Param9 = table.Column<string>(nullable: true),
                    Param10 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectConfig", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectConfig");
        }
    }
}
