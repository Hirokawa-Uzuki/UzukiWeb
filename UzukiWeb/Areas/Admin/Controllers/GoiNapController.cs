using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class GoiNapController : Controller
    {
        private readonly UzukiDbContext _context;

        public GoiNapController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH GÓI NẠP
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.GoiNaps.OrderBy(g => g.Gia).ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // 2. THÊM GÓI NẠP MỚI
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số IFormFile AnhUpload để hứng file từ View gửi lên
        public async Task<IActionResult> Create(GoiNap goiNap, IFormFile? AnhUpload)
        {
            if (ModelState.IsValid)
            {
                // KIỂM TRA VÀ XỬ LÝ LƯU FILE ẢNH
                if (AnhUpload != null && AnhUpload.Length > 0)
                {
                    // Lấy tên file gốc do người dùng up lên
                    string fileName = Path.GetFileName(AnhUpload.FileName);

                    // Trỏ đường dẫn lưu vào thư mục wwwroot/images
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    // Copy file lưu thẳng vào ổ cứng server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUpload.CopyToAsync(stream);
                    }

                    // Lưu tên file vào Database
                    goiNap.Anh = fileName;
                }
                else
                {
                    // Nếu Admin lười không chọn ảnh, gán cho nó 1 cái ảnh mặc định 
                    // Hoặc cứ để trống nếu Database cho phép Null
                    goiNap.Anh = "pack_default.png";
                }

                _context.GoiNaps.Add(goiNap);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã thêm gói nạp mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(goiNap);
        }

        // ==========================================
        // 3. SỬA GÓI NẠP
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var goiNap = await _context.GoiNaps.FindAsync(id);
            if (goiNap == null) return NotFound();
            return View(goiNap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm IFormFile AnhUpload vào tham số
        public async Task<IActionResult> Edit(int id, GoiNap goiNap, IFormFile? AnhUpload)
        {
            if (id != goiNap.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // XỬ LÝ UPLOAD ẢNH NẾU CÓ CHỌN FILE
                if (AnhUpload != null && AnhUpload.Length > 0)
                {
                    // Lấy tên file gốc
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    // Đường dẫn lưu vào thư mục wwwroot/images
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    // Copy file vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUpload.CopyToAsync(stream);
                    }

                    // Cập nhật tên file mới vào DB
                    goiNap.Anh = fileName;
                }

                _context.Update(goiNap);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật gói nạp thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(goiNap);
        }

        // ==========================================
        // 4. XÓA GÓI NẠP (TRANG CẢNH BÁO ĐỎ)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var goiNap = await _context.GoiNaps.FindAsync(id);
            if (goiNap == null) return NotFound();
            return View(goiNap);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var goiNap = await _context.GoiNaps.FindAsync(id);
            if (goiNap != null)
            {
                _context.GoiNaps.Remove(goiNap);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa gói nạp thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}