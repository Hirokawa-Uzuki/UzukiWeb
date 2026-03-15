using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Tạm tắt để test, (đã mở)
    public class DonHangController : Controller
    {
        private readonly UzukiDbContext _context;

        public DonHangController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. XEM DANH SÁCH ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách đơn hàng, sắp xếp mới nhất lên đầu
            var donHangs = await _context.DonHangs
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // ==========================================
        // 2. XEM CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            // Lấy tên người mua từ bảng TaiKhoan
            var nguoiMua = await _context.TaiKhoans.FindAsync(donHang.TaiKhoanId);
            ViewBag.TenNguoiMua = nguoiMua?.TenDangNhap ?? "Khách ẩn danh";

            // Dùng LINQ Join để lấy chi tiết sản phẩm khách đã mua
            var chiTiet = await (from c in _context.ChiTietDonHangs
                                 join s in _context.SanPhams on c.SanPhamId equals s.Id
                                 where c.DonHangId == id
                                 select new
                                 {
                                     TenSanPham = s.TenSanPham,
                                     AnhBia = s.AnhBia,
                                     SoLuong = c.SoLuong,
                                     DonGia = c.DonGia,
                                     ThanhTien = c.SoLuong * c.DonGia
                                 }).ToListAsync();

            ViewBag.ChiTietDonHang = chiTiet;

            return View(donHang);
        }

        // ==========================================
        // 3. CẬP NHẬT TRẠNG THÁI GIAO HÀNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string TrangThai)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                donHang.TrangThai = TrangThai;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}