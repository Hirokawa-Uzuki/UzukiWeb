using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UzukiWeb.Data;
using UzukiWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace UzukiWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UzukiDbContext _context;

        public AccountController(UzukiDbContext context)
        {
            _context = context;
        }

        // --- HÀM MÃ HÓA MẬT KHẨU SHA256 (Chống mất điểm bảo mật ngớ ngẩn) ---
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // ================= ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng User/Email
                var checkUser = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap || u.Email == model.Email);
                if (checkUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập hoặc Email đã tồn tại!";
                    return View(model);
                }

                // Mã hóa mật khẩu trước khi lưu xuống DB
                model.MatKhau = HashPassword(model.MatKhau);
                model.VaiTro = "KhachHang";
                model.SoDuCoin = 0; // Mới tạo acc cho 0 coin
                model.NgayTao = DateTime.Now;

                _context.TaiKhoans.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ================= ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string TenDangNhap, string MatKhau)
        {
            // Băm mật khẩu người dùng nhập vào để so sánh với DB
            var hashedPw = HashPassword(MatKhau);

            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == TenDangNhap && u.MatKhau == hashedPw);

            if (user != null)
            {
                // Tạo "Căn cước công dân" (Claims) cho phiên đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.TenDangNhap),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.VaiTro),
                    new Claim("Coin", user.SoDuCoin.ToString()) // Lưu trữ số dư Coin vào Cookie để hiện lên web
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng nhập chính thức (Lưu Cookie)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        // ================= ĐĂNG XUẤT =================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // TRANG CÁ NHÂN (PROFILE DASHBOARD)
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.TaiKhoans.FindAsync(userId);

            if (user == null) return RedirectToAction("Logout");

            // --- A. LẤY TOÀN BỘ DỮ LIỆU TỪ DATABASE TRƯỚC ---
            var danhSachMuaLe = await _context.LichSuMuas
                .Include(l => l.Chuong)
                .ThenInclude(c => c!.Truyen)
                .Where(l => l.TaiKhoanId == userId)
                .ToListAsync();

            var danhSachMuaFull = await _context.LichSuMuaTruyens
                .Include(l => l.Truyen)
                .Where(l => l.TaiKhoanId == userId)
                .ToListAsync();

            // --- B. TỔNG HỢP TAB: LỊCH SỬ GIAO DỊCH ---
            var lichSuLe = danhSachMuaLe.Select(l => new GiaoDichHistory
            {
                TenGiaoDich = l.Chuong!.Truyen!.TenTruyen + " - " + l.Chuong.TenChuong,
                SoCoin = l.SoCoinDaTru,
                NgayGiaoDich = l.NgayMua,
                LoaiGiaoDich = "Mua Chap Lẻ"
            });

            var lichSuFull = danhSachMuaFull.Select(l => new GiaoDichHistory
            {
                TenGiaoDich = l.Truyen!.TenTruyen + " (MUA TRỌN BỘ)",
                SoCoin = l.SoCoinDaTru,
                NgayGiaoDich = l.NgayMua,
                LoaiGiaoDich = "Mua Trọn Bộ"
            });

            // Gộp và gửi ra View
            ViewBag.LichSuGiaoDich = lichSuLe.Concat(lichSuFull)
                                             .OrderByDescending(x => x.NgayGiaoDich)
                                             .ToList();


            // --- C. TỔNG HỢP TAB: TỦ SÁCH CỦA TÔI ---
            var tuSachMuaLe = danhSachMuaLe.GroupBy(l => l.Chuong!.Truyen!.Id)
                                           .Select(g => g.First().Chuong!.Truyen).ToList();

            var tuSachMuaFull = danhSachMuaFull.Select(l => l.Truyen).ToList();

            // Gộp tủ sách (loại bỏ trùng lặp) và gửi ra View
            ViewBag.TuSach = tuSachMuaLe.Concat(tuSachMuaFull)
                                        .GroupBy(t => t!.Id)
                                        .Select(g => g.First())
                                        .ToList();

            // --- D. LẤY TAB: ĐƠN HÀNG ---
            ViewBag.DonHangs = await _context.DonHangs
                .Where(d => d.TaiKhoanId == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(user);
        }

        // ==========================================
        //  ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.TaiKhoans.FindAsync(userId);

            // Kiểm tra mật khẩu cũ có khớp với mã Hash trong DB không
            if (user!.MatKhau != HashPassword(oldPassword))
            {
                TempData["Error"] = "Mật khẩu cũ không chính xác!";
                return RedirectToAction("Profile");
            }

            // Mã hóa và lưu mật khẩu mới
            user.MatKhau = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }

        // ==========================================
        //  NẠP COIN (MÔ PHỎNG THANH TOÁN)
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TopUp(int coinAmount)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.TaiKhoans.FindAsync(userId);

            // Cộng tiền vào DB
            user!.SoDuCoin += coinAmount;
            await _context.SaveChangesAsync();

            // Cập nhật lại Cookie để số dư trên Header thay đổi ngay lập tức
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("Coin", user.SoDuCoin.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Success"] = $"Nạp thành công {coinAmount} Coin vào tài khoản!";
            return RedirectToAction("Profile");
        }
        
    }
}