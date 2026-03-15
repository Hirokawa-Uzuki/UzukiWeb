using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class NguoiDungController : Controller
    {
        private readonly UzukiDbContext _context;

        public NguoiDungController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. XEM DANH SÁCH TÀI KHOẢN
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách tài khoản, xếp người mới đăng ký lên đầu
            var users = await _context.TaiKhoans.OrderByDescending(u => u.NgayTao).ToListAsync();
            return View(users);
        }

        // ==========================================
        // 2. CHỈNH SỬA (PHÂN QUYỀN & BƠM COIN)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.Id) return NotFound();

            // 1. Lấy thông tin tài khoản từ Database lên
            var userCu = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Id == id);
            if (userCu == null) return NotFound();

            // 2. CHỈ CẬP NHẬT 2 TRƯỜNG MÀ ADMIN ĐƯỢC PHÉP SỬA
            userCu.SoDuCoin = taiKhoan.SoDuCoin;
            userCu.VaiTro = taiKhoan.VaiTro;

            // 3. LƯU THẲNG VÀO DATABASE (Bỏ qua đoạn if ModelState.IsValid rườm rà)
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. XÓA TÀI KHOẢN (TRANG CẢNH BÁO)
        // ==========================================

        // Hiện trang Form cảnh báo đỏ
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null) return NotFound();

            return View(user); // Mở file Delete.cshtml
        }

        // Thực thi lệnh xóa khi bấm nút Xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user != null)
            {
                _context.TaiKhoans.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài khoản thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}