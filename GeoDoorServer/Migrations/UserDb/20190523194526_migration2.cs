using Microsoft.EntityFrameworkCore.Migrations;

namespace GeoDoorServer3.Migrations.UserDb
{
    public partial class migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneId",
                table: "Users",
                column: "PhoneId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneId",
                table: "Users");
        }
    }
}
