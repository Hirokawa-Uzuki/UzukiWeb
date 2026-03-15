using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class BannerController : Controller
    {
        private readonly UzukiDbContext _context;

        public BannerController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH BANNER
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Sắp xếp banner mới nhất lên đầu
            var danhSach = await _context.Banners.OrderByDescending(b => b.Id).ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // 2. THÊM BANNER MỚI
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile? AnhUpload)
        {
            //BỎ QUA LỖI THIẾU CHỮ Ở HINHANH
            ModelState.Remove("HinhAnh");
            if (ModelState.IsValid)
            {
                if (AnhUpload != null && AnhUpload.Length > 0)
                {
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUpload.CopyToAsync(stream);
                    }
                    banner.HinhAnh = fileName;
                }
                else
                {
                    ModelState.AddModelError("HinhAnh", "Vui lòng chọn một tệp ảnh cho Banner.");
                    return View(banner);
                }

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm Banner mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // ==========================================
        // 3. SỬA BANNER
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? AnhUpload)
        {
            if (id != banner.Id) return NotFound();
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                if (AnhUpload != null && AnhUpload.Length > 0)
                {
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUpload.CopyToAsync(stream);
                    }
                    banner.HinhAnh = fileName;
                }

                _context.Update(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật Banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // ==========================================
        // 4. XÓA BANNER
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa Banner thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}