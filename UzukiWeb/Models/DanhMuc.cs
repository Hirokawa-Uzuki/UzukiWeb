using System.ComponentModel.DataAnnotations;

namespace UzukiWeb.Models
{
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự")]
        [Display(Name = "Tên Danh Mục")]
        public string TenDanhMuc { get; set; } = string.Empty;

        // Khóa ngoại: 1 Danh mục có nhiều Truyện
        public ICollection<Truyen>? Truyens { get; set; }
    }
}
