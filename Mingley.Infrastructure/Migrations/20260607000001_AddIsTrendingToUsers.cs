using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mingley.Infrastructure.Migrations
{
    [Microsoft.EntityFrameworkCore.Migrations.Migration("20260607000001_AddIsTrendingToUsers")]
    public partial class AddIsTrendingToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrending",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTrending",
                table: "Users");
        }
    }
}