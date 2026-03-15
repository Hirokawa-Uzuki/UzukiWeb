using System.ComponentModel.DataAnnotations;

namespace UzukiWeb.Models
{
    public class GoiNap
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên gói")]
        [StringLength(100)]
        public string TenGoi { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số Coin")]
        public int Coin { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá tiền")]
        public int Gia { get; set; }

        [StringLength(255)]
        public string? Anh { get; set; } // Ảnh minh họa (pack1.png...)

        [StringLength(50)]
        public string? Tag { get; set; } // Nhãn dán: HOT, NEW... (có thể bỏ trống)

        [StringLength(500)]
        public string? MoTa { get; set; } // Mô tả ngắn gọn ưu đãi
    }
}