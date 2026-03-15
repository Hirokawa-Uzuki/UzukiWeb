using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UzukiWeb.Models
{
    public class Truyen
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên truyện không được để trống")]
        [MaxLength(200, ErrorMessage = "Tên truyện không quá 200 ký tự")]
        [Display(Name = "Tên Truyện")]
        public string TenTruyen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        [MaxLength(100)]
        [Display(Name = "Tác Giả")]
        public string TacGia { get; set; } = string.Empty;

        [Display(Name = "Mô Tả Nội Dung")]
        public string? MoTa { get; set; }

        [Display(Name = "Ảnh Bìa")]
        public string? AnhBia { get; set; }

        [Range(0, 100000, ErrorMessage = "Giá Coin phải từ 0 đến 100,000")]
        [Display(Name = "Giá (Coin)")]
        public int GiaCoin { get; set; }

        [Display(Name = "Ngày Cập Nhật")]
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // --- CÁC TRƯỜNG MỚI THÊM CHO DB LỚN ---
        [MaxLength(50)]
        [Display(Name = "Trạng Thái")]
        public string TrangThai { get; set; } = "Đang tiến hành"; // Đang tiến hành, Hoàn thành

        [Display(Name = "Lượt Xem")]
        public int LuotXem { get; set; } = 0;

        [Display(Name = "Điểm Đánh Giá")]
        public double DiemDanhGia { get; set; } = 5.0;
        // --------------------------------------

        // --- CÁC TRƯỜNG THÊM CHO HOT SALE & RECOMMEND ---
        [Display(Name = "Góc Đề Cử")]
        public bool IsRecommend { get; set; } = false; // Đánh dấu truyện được biên tập viên đề cử

        [Display(Name = "Đang Khuyến Mãi")]
        public bool IsHotSale { get; set; } = false; // Đánh dấu truyện đang Sale

        [Display(Name = "% Giảm Giá")]
        public int PhanTramGiam { get; set; } = 0; // Ví dụ: 30 (nghĩa là giảm 30%)
        // --------------------------------------

        [Required(ErrorMessage = "Vui lòng chọn Thể loại")]
        [Display(Name = "Danh Mục")]
        public int DanhMucId { get; set; }

        [ForeignKey("DanhMucId")]
        public DanhMuc? DanhMuc { get; set; }

        // Khóa ngoại Many-to-Many với Bảng TheLoai
        public ICollection<TheLoai>? TheLoais { get; set; }

        // Khóa ngoại One-to-Many với Bảng Chuong
        public ICollection<Chuong>? Chuongs { get; set; }
    }
}