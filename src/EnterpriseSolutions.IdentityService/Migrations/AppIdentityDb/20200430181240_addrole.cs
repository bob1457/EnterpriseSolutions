using Microsoft.EntityFrameworkCore.Migrations;

namespace EnterpriseSolutions.IdentityService.Migrations.AppIdentityDb
{
    public partial class addrole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripton",
                table: "AspNetRoles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripton",
                table: "AspNetRoles");
        }
    }
}
