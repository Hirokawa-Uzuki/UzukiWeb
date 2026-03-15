using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UzukiWeb.Models
{
    [Table("LichSuMuaTruyen")]
    public class LichSuMuaTruyen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaiKhoanId { get; set; }

        [Required]
        public int TruyenId { get; set; }

        [Display(Name = "Số Coin Đã Trừ")]
        public int SoCoinDaTru { get; set; }

        [Display(Name = "Ngày Mua")]
        public DateTime NgayMua { get; set; } = DateTime.Now;

        [ForeignKey("TaiKhoanId")]
        public TaiKhoan? TaiKhoan { get; set; }

        [ForeignKey("TruyenId")]
        public Truyen? Truyen { get; set; }
    }
}