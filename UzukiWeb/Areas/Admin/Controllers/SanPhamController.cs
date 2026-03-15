using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SanPhamController : Controller
    {
        private readonly UzukiDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SanPhamController(UzukiDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. DANH SÁCH SẢN PHẨM
        public async Task<IActionResult> Index()
        {
            return View(await _context.SanPhams.ToListAsync());
        }

        // 2. THÊM MỚI SẢN PHẨM
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sp, IFormFile? AnhBiaFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (AnhBiaFile != null && AnhBiaFile.Length > 0)
                {
                    string fileName = DateTime.Now.Ticks.ToString() + "_" + AnhBiaFile.FileName;
                    string filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhBiaFile.CopyToAsync(stream);
                    }
                    sp.AnhBia = fileName;
                }
                else
                {
                    sp.AnhBia = "default-product.jpg"; // Ảnh mặc định nếu không up
                }

                _context.Add(sp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(sp);
        }

        // 3. SỬA SẢN PHẨM
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sp, IFormFile? AnhBiaFile)
        {
            if (id != sp.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var spCu = await _context.SanPhams.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

                    if (AnhBiaFile != null && AnhBiaFile.Length > 0)
                    {
                        string fileName = DateTime.Now.Ticks.ToString() + "_" + AnhBiaFile.FileName;
                        string filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AnhBiaFile.CopyToAsync(stream);
                        }
                        sp.AnhBia = fileName;
                    }
                    else
                    {
                        sp.AnhBia = spCu?.AnhBia ?? "001.jpg"; // Giữ nguyên ảnh cũ
                    }

                    _context.Update(sp);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanPhams.Any(e => e.Id == sp.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sp);
        }

        // 4. XÓA SẢN PHẨM
        // 1. GET: Mở form Cảnh báo xóa (Hiện cái khung đỏ đỏ)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();
            return View(sp); // Trả về giao diện cảnh báo
        }

        // 2. POST: Khi Admin bấm nút "Xác Nhận Xóa Thật"
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}