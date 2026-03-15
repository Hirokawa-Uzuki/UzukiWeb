using System.ComponentModel.DataAnnotations;

namespace UzukiWeb.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng tải ảnh lên")]
        [StringLength(255)]
        public string HinhAnh { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác định vị trí")]
        [StringLength(50)]
        public string ViTri { get; set; } = null!; // Dùng để phân biệt: "Slide" hoặc "KhuyenMai"

        [StringLength(255)]
        public string? Link { get; set; } // Đường dẫn khi người dùng click vào ảnh (Có thể bỏ trống)

        public bool TrangThai { get; set; } = true; // Trạng thái: Bật (True) / Tắt (False)
    }
}