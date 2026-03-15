using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] //(đã mở)
    public class ChuongController : Controller
    {
        private readonly UzukiDbContext _context;

        public ChuongController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. XEM DANH SÁCH CHƯƠNG CỦA 1 BỘ TRUYỆN
        // ==========================================
        public async Task<IActionResult> Index(int truyenId)
        {
            var truyen = await _context.Truyens.FindAsync(truyenId);
            if (truyen == null) return NotFound();

            // Gửi thông tin Truyện ra View để hiển thị tiêu đề
            ViewBag.Truyen = truyen;

            var danhSachChuong = await _context.Chuongs
                .Where(c => c.TruyenId == truyenId)
                .OrderBy(c => c.SoThuTu) // Sắp xếp từ chương 1 -> n
                .ToListAsync();

            return View(danhSachChuong);
        }

        // ==========================================
        // 2. THÊM CHƯƠNG MỚI
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create(int truyenId)
        {
            var truyen = await _context.Truyens.FindAsync(truyenId);
            if (truyen == null) return NotFound();

            ViewBag.Truyen = truyen;

            // Tự động tính Số Thứ Tự tiếp theo (VD: Đã có chương 5 thì tự gợi ý chương 6)
            var maxStt = await _context.Chuongs.Where(c => c.TruyenId == truyenId).MaxAsync(c => (int?)c.SoThuTu) ?? 0;

            var chuongMoi = new Chuong
            {
                TruyenId = truyenId,
                SoThuTu = maxStt + 1,
                GiaCoin = truyen.GiaCoin // Mặc định lấy theo giá của bộ truyện
            };

            return View(chuongMoi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chuong chuong)
        {
            if (ModelState.IsValid)
            {
                _context.Chuongs.Add(chuong);

                // Cập nhật ngày mới nhất cho Truyện để nó leo Top Trang Chủ
                var truyen = await _context.Truyens.FindAsync(chuong.TruyenId);
                if (truyen != null)
                {
                    truyen.NgayCapNhat = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đăng thành công: {chuong.TenChuong}!";

                // Thêm xong thì quay lại Danh sách chương của truyện đó
                return RedirectToAction("Index", new { truyenId = chuong.TruyenId });
            }

            ViewBag.Truyen = await _context.Truyens.FindAsync(chuong.TruyenId);
            return View(chuong);
        }

        // ==========================================
        // 3. XÓA CHƯƠNG
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var chuong = await _context.Chuongs.FindAsync(id);
            if (chuong != null)
            {
                int truyenId = chuong.TruyenId;
                _context.Chuongs.Remove(chuong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa chương thành công!";
                return RedirectToAction("Index", new { truyenId = truyenId });
            }
            return RedirectToAction("Index", "Truyen");
        }

        // ==========================================
        // 4. SỬA CHƯƠNG (CẬP NHẬT NỘI DUNG/ẢNH)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var chuong = await _context.Chuongs.FindAsync(id);
            if (chuong == null) return NotFound();

            // Gửi thông tin bộ truyện ra ngoài View để hiển thị tiêu đề cho đẹp
            ViewBag.Truyen = await _context.Truyens.FindAsync(chuong.TruyenId);
            return View(chuong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Chuong chuong)
        {
            if (id != chuong.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuong);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật chương thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Chuongs.Any(e => e.Id == chuong.Id)) return NotFound();
                    else throw;
                }
                // Sửa xong thì quay lại danh sách chương của đúng bộ truyện đó
                return RedirectToAction("Index", new { truyenId = chuong.TruyenId });
            }

            ViewBag.Truyen = await _context.Truyens.FindAsync(chuong.TruyenId);
            return View(chuong);
        }
    }
}
