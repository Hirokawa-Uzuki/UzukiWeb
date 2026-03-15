using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzukiWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBangMuaTronBo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichSuMuaTruyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    TruyenId = table.Column<int>(type: "int", nullable: false),
                    SoCoinDaTru = table.Column<int>(type: "int", nullable: false),
                    NgayMua = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuMuaTruyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuMuaTruyen_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuMuaTruyen_Truyens_TruyenId",
                        column: x => x.TruyenId,
                        principalTable: "Truyens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuMuaTruyen_TaiKhoanId",
                table: "LichSuMuaTruyen",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuMuaTruyen_TruyenId",
                table: "LichSuMuaTruyen",
                column: "TruyenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuMuaTruyen");
        }
    }
}
