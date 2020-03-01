using Microsoft.EntityFrameworkCore.Migrations;

namespace GeoDoorServer3.Migrations.UserDb
{
    public partial class migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserConnectionPort",
                table: "Settings",
                newName: "MaxErrorLogRows");

            migrationBuilder.AddColumn<string>(
                name: "DoorOpenHabLink",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GateOpenHabLink",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GateTimeout",
                table: "Settings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusOpenHabLink",
                table: "Settings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoorOpenHabLink",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GateOpenHabLink",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GateTimeout",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "StatusOpenHabLink",
                table: "Settings");

            migrationBuilder.RenameColumn(
                name: "MaxErrorLogRows",
                table: "Settings",
                newName: "UserConnectionPort");
        }
    }
}
