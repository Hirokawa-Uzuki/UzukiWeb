using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UzukiWeb.Models
{
    // 1. BẢNG SẢN PHẨM (Sách vật lý, Merch...)
    [Table("SanPham")]
    public class SanPham
    {
        [Key] public int Id { get; set; }
        [Required] public string TenSanPham { get; set; } = string.Empty;
        public int Gia { get; set; }
        public string AnhBia { get; set; } = string.Empty;
    }

    // 2. BẢNG ĐƠN HÀNG (Lưu hóa đơn)
    [Table("DonHang")]
    public class DonHang
    {
        [Key] public int Id { get; set; }
        public int TaiKhoanId { get; set; } // ID người mua
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public int TongTien { get; set; }
        public string TrangThai { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đang giao, Hoàn thành

        [Required] public string DiaChiGiaoHang { get; set; } = string.Empty;
        [Required] public string SoDienThoai { get; set; } = string.Empty;
    }

    // 3. BẢNG CHI TIẾT ĐƠN HÀNG (Mua những cuốn sách nào)
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key] public int Id { get; set; }
        public int DonHangId { get; set; }
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
    }

    // 4. CLASS GIỎ HÀNG TẠM (Chỉ lưu trong Session, không lưu DB)
    public class CartItem
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public int Gia { get; set; }
        public string AnhBia { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public int ThanhTien => Gia * SoLuong;
    }
}
