using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzukiWeb.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangLichSuMua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichSuMua",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    ChuongId = table.Column<int>(type: "int", nullable: false),
                    SoCoinDaTru = table.Column<int>(type: "int", nullable: false),
                    NgayMua = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuMua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuMua_Chuong_ChuongId",
                        column: x => x.ChuongId,
                        principalTable: "Chuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuMua_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuMua_ChuongId",
                table: "LichSuMua",
                column: "ChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuMua_TaiKhoanId",
                table: "LichSuMua",
                column: "TaiKhoanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuMua");
        }
    }
}
