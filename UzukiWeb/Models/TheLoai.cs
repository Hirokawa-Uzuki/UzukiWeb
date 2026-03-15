using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này

namespace UzukiWeb.Models
{
    [Table("TheLoai")] // Ép C# tìm đúng bảng tên "TheLoai" trong SQL
    public class TheLoai
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [MaxLength(100)]
        [Display(Name = "Tên Thể Loại")]
        public string TenTheLoai { get; set; } = string.Empty;

        public ICollection<Truyen>? Truyens { get; set; }
    }
}