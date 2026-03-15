using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Tạm tắt để test (đã mở)
    public class DanhMucController : Controller
    {
        private readonly UzukiDbContext _context;

        public DanhMucController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. XEM DANH SÁCH DANH MỤC
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.DanhMucs.ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // 2. THÊM MỚI DANH MỤC
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // ==========================================
        // 3. SỬA DANH MỤC
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound();
            return View(danhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMuc danhMuc)
        {
            if (id != danhMuc.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DanhMucs.Any(e => e.Id == danhMuc.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // ==========================================
        // 4. XÓA DANH MỤC
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                // Lưu ý: Nếu danh mục này đang có truyện, việc xóa có thể bị lỗi do dính khóa ngoại (Foreign Key). 
                // Ở mức độ cơ bản, cứ gọi hàm Remove.
                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}