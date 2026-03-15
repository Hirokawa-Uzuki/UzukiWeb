using Microsoft.EntityFrameworkCore;
using UzukiWeb.Models;

namespace UzukiWeb.Data
{
    public class UzukiDbContext : DbContext
    {
        public UzukiDbContext(DbContextOptions<UzukiDbContext> options) : base(options)
        {
        }

        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<Truyen> Truyens { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LichSuMua> LichSuMuas { get; set; }
        public DbSet<LichSuMuaTruyen> LichSuMuaTruyens { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<GoiNap> GoiNaps { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        // BẮT BUỘC PHẢI CÓ 2 DÒNG NÀY ĐỂ KẾT NỐI BẢNG MỚI
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Chuong> Chuongs { get; set; }
    }
}
