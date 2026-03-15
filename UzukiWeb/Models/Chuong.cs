using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace UzukiWeb.Models
{
    [Table("Chuong")] // Ép C# tìm đúng bảng tên "Chuong" trong SQL
    public class Chuong
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chương/tập không được để trống")]
        [MaxLength(200)]
        [Display(Name = "Tên Chương / Volume")]
        public string TenChuong { get; set; } = string.Empty;

        [Display(Name = "Thứ Tự")]
        public int SoThuTu { get; set; }

        [Display(Name = "Giá Mở Khóa (Coin)")]
        public int GiaCoin { get; set; } = 0;

        [Display(Name = "Ngày Đăng")]
        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Display(Name = "Lượt Xem")]
        public int LuotXem { get; set; } = 0;

        [Required]
        public int TruyenId { get; set; }

        [ForeignKey("TruyenId")]
        public Truyen? Truyen { get; set; }

        public string? NoiDung { get; set; }
    }
}