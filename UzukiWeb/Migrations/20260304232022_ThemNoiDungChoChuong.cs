using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzukiWeb.Migrations
{
    /// <inheritdoc />
    public partial class ThemNoiDungChoChuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoiDung",
                table: "Chuong",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoiDung",
                table: "Chuong");
        }
    }
}
