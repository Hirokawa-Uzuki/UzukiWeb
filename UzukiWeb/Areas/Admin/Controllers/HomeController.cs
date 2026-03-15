using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Tạm tắt để test (đã mở)

    public class HomeController : Controller
    {
        private readonly UzukiDbContext _context;

        public HomeController(UzukiDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.TongTruyen = await _context.Truyens.CountAsync();
            ViewBag.TongNguoiDung = await _context.TaiKhoans.CountAsync();
            ViewBag.TongDonHang = await _context.DonHangs.CountAsync();

            // Tính tổng doanh thu từ các đơn hàng "Đã thanh toán" hoặc "Hoàn thành"
            ViewBag.TongDoanhThu = await _context.DonHangs
                .Where(d => d.TrangThai.Contains("Đã thanh toán") || d.TrangThai.Contains("Hoàn thành"))
                .SumAsync(d => d.TongTien);

            return View();
        }
    }
}