using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;
using UzukiWeb.Models;

namespace UzukiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TruyenController : Controller
    {
        private readonly UzukiDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TruyenController(UzukiDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. XEM DANH SÁCH (Read)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var danhSachTruyen = await _context.Truyens.Include(t => t.DanhMuc).ToListAsync();
            return View(danhSachTruyen);
        }

        // ==========================================
        // 2. THÊM MỚI (Create)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.DanhMucId = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm IFormFile AnhUpload vào tham số 
        public async Task<IActionResult> Create(Truyen truyen, IFormFile? AnhUpload)
        {
            if (ModelState.IsValid)
            {
                // KIỂM TRA VÀ LƯU FILE ẢNH NẾU ADMIN CÓ CHỌN FILE
                if (AnhUpload != null && AnhUpload.Length > 0)
                {
                    // Lấy tên file gốc (VD: meo-con.jpg)
                    string fileName = Path.GetFileName(AnhUpload.FileName);

                    // Tạo đường dẫn lưu vào thư mục wwwroot/images
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    // Copy file từ bộ nhớ tạm vào ổ cứng server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUpload.CopyToAsync(stream);
                    }

                    // Gán tên file vào thuộc tính AnhBia để lưu xuống Database
                    truyen.AnhBia = fileName;
                }
                else
                {
                    // Nếu quên không chọn ảnh thì cho 1 cái ảnh mặc định chống cháy
                    truyen.AnhBia = "002.jpg";
                }

                _context.Add(truyen);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm truyện mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu code chạy đến đây nghĩa là Form bị lỗi (nhập thiếu thông tin), load lại dropdown Danh Mục
            ViewBag.DanhMucId = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", truyen.DanhMucId);
            return View(truyen);
        }

        // ==========================================
        // 3. SỬA (Update) - TÍCH HỢP CHỌN THỂ LOẠI
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Phải Include(t => t.TheLoais) để biết truyện này đang có những thể loại nào
            var truyen = await _context.Truyens
                .Include(t => t.TheLoais)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (truyen == null) return NotFound();

            ViewBag.DanhMucId = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", truyen.DanhMucId);

            // Gửi toàn bộ danh sách Thể Loại có trong DB ra View để làm Checkbox
            ViewBag.AllTheLoai = await _context.TheLoais.ToListAsync();

            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số List<int> selectedGenres để hứng các ô Checkbox được tích từ Giao diện
        public async Task<IActionResult> Edit(int id, Truyen truyen, IFormFile? AnhBiaFile, List<int> selectedGenres)
        {
            if (id != truyen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lấy truyện cũ từ DB ra (Phải Include TheLoais thì mới sửa được)
                    var truyenToUpdate = await _context.Truyens
                        .Include(t => t.TheLoais)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (truyenToUpdate == null) return NotFound();

                    // 2. XỬ LÝ ẢNH BÌA
                    if (AnhBiaFile != null && AnhBiaFile.Length > 0)
                    {
                        string fileName = DateTime.Now.Ticks.ToString() + "_" + AnhBiaFile.FileName;
                        string filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AnhBiaFile.CopyToAsync(stream);
                        }
                        truyenToUpdate.AnhBia = fileName;
                    }

                    // 3. CẬP NHẬT CÁC THÔNG TIN CHỮ
                    truyenToUpdate.TenTruyen = truyen.TenTruyen;
                    truyenToUpdate.LuotXem = truyen.LuotXem;
                    truyenToUpdate.TacGia = truyen.TacGia;
                    truyenToUpdate.DanhMucId = truyen.DanhMucId;
                    truyenToUpdate.MoTa = truyen.MoTa;
                    truyenToUpdate.GiaCoin = truyen.GiaCoin;
                    truyenToUpdate.TrangThai = truyen.TrangThai;
                    truyenToUpdate.IsRecommend = truyen.IsRecommend;
                    truyenToUpdate.IsHotSale = truyen.IsHotSale;
                    truyenToUpdate.PhanTramGiam = truyen.PhanTramGiam;
                    truyenToUpdate.NgayCapNhat = DateTime.Now;

                    // 4. CẬP NHẬT THỂ LOẠI (MANY-TO-MANY)
                    if (truyenToUpdate.TheLoais == null)
                    {
                        truyenToUpdate.TheLoais = new List<TheLoai>(); // Nếu chưa có thì khởi tạo danh sách trống
                    }
                    truyenToUpdate.TheLoais.Clear(); // An tâm quét sạch các thể loại cũ
                    if (selectedGenres != null && selectedGenres.Any())
                    {
                        // Lấy các Thể Loại mới dựa trên ID mà Admin vừa tích chọn
                        var newGenres = await _context.TheLoais
                            .Where(tl => selectedGenres.Contains(tl.Id))
                            .ToListAsync();

                        foreach (var genre in newGenres)
                        {
                            truyenToUpdate.TheLoais.Add(genre); // Nhồi thể loại mới vào
                        }
                    }

                    _context.Update(truyenToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật truyện thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Truyens.Any(e => e.Id == truyen.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DanhMucId = new SelectList(_context.DanhMucs, "Id", "TenDanhMuc", truyen.DanhMucId);
            ViewBag.AllTheLoai = await _context.TheLoais.ToListAsync();
            return View(truyen);
        }

        // ==========================================
        // 4. XÓA (Delete)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var truyen = await _context.Truyens
                .Include(t => t.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (truyen == null) return NotFound();

            return View(truyen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen != null)
            {
                _context.Truyens.Remove(truyen);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
