using Microsoft.EntityFrameworkCore.Migrations;

namespace GeoDoorServer.Migrations.UserDb
{
    public partial class migration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoGateTimeout",
                table: "Settings",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoGateTimeout",
                table: "Settings");
        }
    }
}
