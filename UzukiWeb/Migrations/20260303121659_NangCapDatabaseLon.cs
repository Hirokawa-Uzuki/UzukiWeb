using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UzukiWeb.Migrations
{
    /// <inheritdoc />
    public partial class NangCapDatabaseLon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemDanhGia",
                table: "Truyens",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LuotXem",
                table: "Truyens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "Truyens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Chuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChuong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoThuTu = table.Column<int>(type: "int", nullable: false),
                    GiaCoin = table.Column<int>(type: "int", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    TruyenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chuong_Truyens_TruyenId",
                        column: x => x.TruyenId,
                        principalTable: "Truyens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TheLoai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTheLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheLoai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TheLoaiTruyen",
                columns: table => new
                {
                    TheLoaisId = table.Column<int>(type: "int", nullable: false),
                    TruyensId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheLoaiTruyen", x => new { x.TheLoaisId, x.TruyensId });
                    table.ForeignKey(
                        name: "FK_TheLoaiTruyen_TheLoai_TheLoaisId",
                        column: x => x.TheLoaisId,
                        principalTable: "TheLoai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TheLoaiTruyen_Truyens_TruyensId",
                        column: x => x.TruyensId,
                        principalTable: "Truyens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chuong_TruyenId",
                table: "Chuong",
                column: "TruyenId");

            migrationBuilder.CreateIndex(
                name: "IX_TheLoaiTruyen_TruyensId",
                table: "TheLoaiTruyen",
                column: "TruyensId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chuong");

            migrationBuilder.DropTable(
                name: "TheLoaiTruyen");

            migrationBuilder.DropTable(
                name: "TheLoai");

            migrationBuilder.DropColumn(
                name: "DiemDanhGia",
                table: "Truyens");

            migrationBuilder.DropColumn(
                name: "LuotXem",
                table: "Truyens");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "Truyens");
        }
    }
}
