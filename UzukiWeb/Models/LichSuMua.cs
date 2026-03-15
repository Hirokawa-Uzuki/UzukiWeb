using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UzukiWeb.Models
{
    [Table("LichSuMua")]
    public class LichSuMua
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaiKhoanId { get; set; }

        [Required]
        public int ChuongId { get; set; }

        [Display(Name = "Số Coin Đã Trừ")]
        public int SoCoinDaTru { get; set; }

        [Display(Name = "Ngày Mua")]
        public DateTime NgayMua { get; set; } = DateTime.Now;

        [ForeignKey("TaiKhoanId")]
        public TaiKhoan? TaiKhoan { get; set; }

        [ForeignKey("ChuongId")]
        public Chuong? Chuong { get; set; }
    }
}