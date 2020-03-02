using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GeoDoorServer.Migrations
{
    public partial class CustomizedIdentityUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessRights",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConnection",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessRights",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastConnection",
                table: "AspNetUsers");
        }
    }
}
