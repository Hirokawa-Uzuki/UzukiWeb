using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UzukiWeb.Data;
using UzukiWeb.Models;
using UzukiWeb.Helpers;

namespace UzukiWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly UzukiDbContext _context;

        public HomeController(UzukiDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. HÀM XỬ LÝ TRANG CHỦ
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // 1. Truyện Mới Cập Nhật (Gửi làm Model chính)
            var danhSachTruyen = await _context.Truyens
                                    .Include(t => t.DanhMuc)
                                    .Include(t => t.Chuongs)
                                    .OrderByDescending(t => t.NgayCapNhat)
                                    .Take(8)
                                    .ToListAsync();

            // 2. Ranking Top 10 (Gửi qua ViewBag)
            ViewBag.Ranking = await _context.Truyens
                                    .OrderByDescending(t => t.LuotXem)
                                    .Take(10)
                                    .ToListAsync();

            // 3. Góc Đề Cử Sidebar (Gửi qua ViewBag)
            ViewBag.Recommend = await _context.Truyens
                                    .Include(t => t.DanhMuc)
                                    .Where(t => t.IsRecommend == true)
                                    .OrderByDescending(t => t.NgayCapNhat)
                                    .Take(4)
                                    .ToListAsync();

            // 4. Hot Sale Sidebar (Gửi qua ViewBag)
            ViewBag.HotSale = await _context.Truyens
                                    .Where(t => t.IsHotSale == true && t.GiaCoin > 0)
                                    .OrderByDescending(t => t.PhanTramGiam)
                                    .Take(3)
                                    .ToListAsync();

            // 5. Light Novel Nổi bật (Gửi qua ViewBag)
            ViewBag.LightNovel = await _context.Truyens
                                    .Where(t => t.DanhMucId == 3)
                                    .OrderByDescending(t => t.NgayCapNhat)
                                    .Take(4)
                                    .ToListAsync();

            // 6. Sách Bán Chạy (Sidebar Shop)
            ViewBag.SachBanChay = await _context.SanPhams
                                    .OrderByDescending(s => s.Id)
                                    .Take(3)
                                    .ToListAsync();

            // ==========================================
            // 7. LẤY DỮ LIỆU BANNER ĐỘNG TỪ BẢNG BANNERS
            // ==========================================
            var banners = await _context.Banners.Where(b => b.TrangThai).ToListAsync();

            ViewBag.Slides = banners.Where(b => b.ViTri == "Slide").ToList();
            ViewBag.KhuyenMaiBanner = banners.FirstOrDefault(b => b.ViTri == "KhuyenMai");

            return View(danhSachTruyen);
        }

        // ==========================================
        // HÀM XỬ LÝ TRANG CHI TIẾT & HIỂN THỊ CHƯƠNG
        // ==========================================
        public async Task<IActionResult> Detail(int id)
        {
            var truyen = await _context.Truyens
                                    .Include(t => t.DanhMuc)
                                    .Include(t => t.Chuongs)
                                    .Include(t => t.TheLoais)
                                    .FirstOrDefaultAsync(t => t.Id == id);
            if (truyen == null) return NotFound();

            truyen.Chuongs = truyen.Chuongs?.OrderBy(c => c.SoThuTu).ToList();

            var danhSachDaMua = new List<int>();
            bool daMuaFull = false; // Mặc định là chưa mua

            if (User.Identity!.IsAuthenticated)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // 1. Lấy danh sách chương lẻ đã mua
                danhSachDaMua = await _context.LichSuMuas
                                    .Where(l => l.TaiKhoanId == userId)
                                    .Select(l => l.ChuongId)
                                    .ToListAsync();

                // 2. KIỂM TRA XEM ĐÃ MUA TRỌN BỘ CHƯA (Bổ sung dòng này)
                daMuaFull = await _context.LichSuMuaTruyens
                                    .AnyAsync(l => l.TaiKhoanId == userId && l.TruyenId == id);
            }

            ViewBag.DanhSachDaMua = danhSachDaMua;
            ViewBag.DaMuaFull = daMuaFull; // Gửi cái này ra để View ẩn nút mua

            // --- CODE ĐẾM VIEW ---
            truyen.LuotXem += 1;
            _context.Update(truyen);
            await _context.SaveChangesAsync();

            return View(truyen);
        }

        // ==========================================
        // API AJAX: MỞ KHÓA CHƯƠNG (TRỪ COIN CÓ TÍNH KHUYẾN MÃI)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UnlockChapter(int chuongId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập để mở khóa chương này!" });

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Ép EF Core phải Join thêm bảng Truyện để lấy được thông tin % Giảm giá
            var chuong = await _context.Chuongs.Include(c => c.Truyen).FirstOrDefaultAsync(c => c.Id == chuongId);
            var user = await _context.TaiKhoans.FindAsync(userId);

            if (chuong == null || user == null) return Json(new { success = false, message = "Lỗi hệ thống!" });

            var daMua = await _context.LichSuMuas.AnyAsync(l => l.TaiKhoanId == userId && l.ChuongId == chuongId);
            if (daMua) return Json(new { success = true, message = "Bạn đã mở khóa chương này rồi!" });

            // LOGIC TÍNH GIÁ KHUYẾN MÃI Ở BACKEND (Chống hacker sửa mã JS)
            int giaThucTe = chuong.GiaCoin;
            if (chuong.Truyen?.IsHotSale == true && chuong.Truyen.PhanTramGiam > 0)
            {
                giaThucTe = chuong.GiaCoin - (chuong.GiaCoin * chuong.Truyen.PhanTramGiam / 100);
            }

            if (user.SoDuCoin < giaThucTe)
                return Json(new { success = false, message = $"Bạn còn {user.SoDuCoin} Coin. Không đủ {giaThucTe} Coin để mở khóa. Vui lòng nạp thêm!" });

            // TRỪ ĐÚNG SỐ TIỀN THỰC TẾ
            user.SoDuCoin -= giaThucTe;
            _context.LichSuMuas.Add(new LichSuMua
            {
                TaiKhoanId = userId,
                ChuongId = chuongId,
                SoCoinDaTru = giaThucTe,
                NgayMua = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Cập nhật Cookie...
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("Coin", user.SoDuCoin.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Json(new { success = true, message = "Mở khóa thành công! Bắt đầu đọc thôi." });
        }

        public async Task<IActionResult> Manga(string keyword, int? genreId, string sortBy = "new", int page = 1)
        {
            int pageSize = 12;
            var query = _context.Truyens.Include(t => t.Chuongs).Include(t => t.TheLoais).Where(t => t.DanhMucId == 1);
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(t => t.TenTruyen.Contains(keyword) || t.TacGia.Contains(keyword));
            if (genreId.HasValue) query = query.Where(t => t.TheLoais!.Any(tl => tl.Id == genreId.Value));
            switch (sortBy)
            {
                case "view": query = query.OrderByDescending(t => t.LuotXem); break;
                case "rating": query = query.OrderByDescending(t => t.DiemDanhGia); break;
                case "name": query = query.OrderBy(t => t.TenTruyen); break;
                case "new": default: query = query.OrderByDescending(t => t.NgayCapNhat); break;
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var danhSachManga = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page; ViewBag.TotalPages = totalPages; ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword; ViewBag.CurrentGenre = genreId; ViewBag.CurrentSort = sortBy;
            ViewBag.ListTheLoai = await _context.TheLoais.ToListAsync();
            // Lấy 1 bộ truyện tiêu điểm (Ví dụ: Lấy bộ mới nhất có IsRecommend = true)
            ViewBag.FeaturedManga = await _context.Truyens
                                        .Include(t => t.DanhMuc)
                                        .Where(t => t.DanhMucId == 1 && t.IsRecommend == true)
                                        .OrderByDescending(t => t.NgayCapNhat)
                                        .FirstOrDefaultAsync();
            return View(danhSachManga);
        }

        public async Task<IActionResult> Manhwa(string keyword, int? genreId, string sortBy = "new", int page = 1)
        {
            int pageSize = 12;
            var query = _context.Truyens.Include(t => t.Chuongs).Include(t => t.TheLoais).Where(t => t.DanhMucId == 2);
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(t => t.TenTruyen.Contains(keyword) || t.TacGia.Contains(keyword));
            if (genreId.HasValue) query = query.Where(t => t.TheLoais!.Any(tl => tl.Id == genreId.Value));
            switch (sortBy)
            {
                case "view": query = query.OrderByDescending(t => t.LuotXem); break;
                case "rating": query = query.OrderByDescending(t => t.DiemDanhGia); break;
                case "name": query = query.OrderBy(t => t.TenTruyen); break;
                case "new": default: query = query.OrderByDescending(t => t.NgayCapNhat); break;
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var danhSachManhwa = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page; ViewBag.TotalPages = totalPages; ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword; ViewBag.CurrentGenre = genreId; ViewBag.CurrentSort = sortBy;
            ViewBag.ListTheLoai = await _context.TheLoais.ToListAsync();
            // Lấy 1 bộ truyện tiêu điểm cho Manhwa (DanhMucId = 2 và có IsRecommend = true)
            ViewBag.FeaturedManhwa = await _context.Truyens
                                        .Include(t => t.DanhMuc)
                                        .Where(t => t.DanhMucId == 2 && t.IsRecommend == true)
                                        .OrderByDescending(t => t.NgayCapNhat)
                                        .FirstOrDefaultAsync();
            return View(danhSachManhwa);
        }

        public async Task<IActionResult> LightNovel(string keyword, int? genreId, string sortBy = "new", int page = 1)
        {
            int pageSize = 12;
            var query = _context.Truyens.Include(t => t.Chuongs).Include(t => t.TheLoais).Where(t => t.DanhMucId == 3);
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(t => t.TenTruyen.Contains(keyword) || t.TacGia.Contains(keyword));
            if (genreId.HasValue) query = query.Where(t => t.TheLoais!.Any(tl => tl.Id == genreId.Value));
            switch (sortBy)
            {
                case "view": query = query.OrderByDescending(t => t.LuotXem); break;
                case "rating": query = query.OrderByDescending(t => t.DiemDanhGia); break;
                case "name": query = query.OrderBy(t => t.TenTruyen); break;
                case "new": default: query = query.OrderByDescending(t => t.NgayCapNhat); break;
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var danhSachLN = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page; ViewBag.TotalPages = totalPages; ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword; ViewBag.CurrentGenre = genreId; ViewBag.CurrentSort = sortBy;
            ViewBag.ListTheLoai = await _context.TheLoais.ToListAsync();
            // Lấy 1 bộ truyện tiêu điểm cho Light Novel (DanhMucId = 3 và có IsRecommend = true)
            ViewBag.FeaturedLN = await _context.Truyens
                                        .Include(t => t.DanhMuc)
                                        .Where(t => t.DanhMucId == 3 && t.IsRecommend == true)
                                        .OrderByDescending(t => t.NgayCapNhat)
                                        .FirstOrDefaultAsync();
            return View(danhSachLN);
        }

        // ==========================================
        // TRANG GÓC ĐỀ CỬ (RECOMMEND)
        // ==========================================
        public async Task<IActionResult> Recommend()
        {
            var danhSach = await _context.Truyens
                .Include(t => t.Chuongs)
                .Include(t => t.DanhMuc)
                .Where(t => t.IsRecommend == true)
                .OrderByDescending(t => t.NgayCapNhat)
                .ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // TRANG HOT SALE (ĐANG KHUYẾN MÃI)
        // ==========================================
        public async Task<IActionResult> HotSale()
        {
            var danhSach = await _context.Truyens
                .Include(t => t.Chuongs)
                .Include(t => t.DanhMuc)
                .Where(t => t.IsHotSale == true && t.GiaCoin > 0)
                .OrderByDescending(t => t.PhanTramGiam)
                .ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // TRANG BẢNG XẾP HẠNG (RANKING - THEO LƯỢT XEM)
        // ==========================================
        public async Task<IActionResult> Ranking()
        {
            var danhSach = await _context.Truyens
                .Include(t => t.Chuongs)
                .Include(t => t.DanhMuc)
                .OrderByDescending(t => t.LuotXem)
                .Take(20)
                .ToListAsync();
            return View(danhSach);
        }

        // ==========================================
        // 1. TRANG PREMIUM (DANH SÁCH GÓI NẠP)
        // ==========================================
        public async Task<IActionResult> Premium()
        {
            // Lấy dữ liệu THẬT từ bảng GoiNaps trong Database, sắp xếp theo giá tiền từ thấp đến cao
            var cacGoiNap = await _context.GoiNaps.OrderBy(g => g.Gia).ToListAsync();

            ViewBag.CacGoiNap = cacGoiNap;
            return View();
        }

        // ==========================================
        // 2. XỬ LÝ NÚT XÁC NHẬN MUA TRONG POPUP
        // ==========================================
        [HttpPost]
        [Authorize] // Phải đăng nhập mới được thanh toán
        public IActionResult Checkout(int packageId, string packageName, int coinAmount, int price)
        {
            // Lưu thông tin gói nạp vào TempData để mang sang trang Thanh Toán (Cổng thanh toán)
            TempData["ThanhToan_TenGoi"] = packageName;
            TempData["ThanhToan_Coin"] = coinAmount;
            TempData["ThanhToan_Gia"] = price;

            return RedirectToAction("Payment");
        }

        // ==========================================
        // 3. TRANG CHỌN PHƯƠNG THỨC THANH TOÁN
        // ==========================================
        [Authorize]
        public IActionResult Payment()
        {
            // Nếu khách vào thẳng trang này mà không qua bước chọn gói, đá về trang Premium
            if (TempData["ThanhToan_Gia"] == null)
            {
                return RedirectToAction("Premium");
            }

            // Giữ lại TempData để load lên View (vì TempData tự xóa sau 1 lần đọc)
            TempData.Keep();
            return View();
        }

        // ==========================================
        // 1. TRANG HIỂN THỊ CỬA HÀNG (SHOP)
        // ==========================================
        public async Task<IActionResult> Shop()
        {
            // Lấy toàn bộ sách/merch từ DB
            var sanPhams = await _context.SanPhams.ToListAsync();
            return View(sanPhams);
        }

        // ==========================================
        // 2. THÊM VÀO GIỎ HÀNG (LƯU SESSION)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.SanPhams.FindAsync(productId);
            if (product == null) return NotFound();

            // Lấy giỏ hàng hiện tại ra (Nếu chưa có thì tạo giỏ trống)
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang") ?? new List<CartItem>();

            // Kiểm tra xem món này đã có trong giỏ chưa
            var item = cart.FirstOrDefault(c => c.SanPhamId == productId);

            if (item != null)
            {
                item.SoLuong++; // Có rồi thì cộng dồn số lượng
            }
            else
            {
                // Chưa có thì thêm mới vào
                cart.Add(new CartItem
                {
                    SanPhamId = product.Id,
                    TenSanPham = product.TenSanPham,
                    Gia = product.Gia,
                    AnhBia = product.AnhBia,
                    SoLuong = 1
                });
            }

            // Cất giỏ hàng vào lại Session
            HttpContext.Session.Set("GioHang", cart);

            TempData["Success"] = $"Đã thêm '{product.TenSanPham}' vào giỏ hàng!";
            return RedirectToAction("Shop");
        }

        // ==========================================
        // 3. XEM GIỎ HÀNG 
        // ==========================================
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang") ?? new List<CartItem>();
            return View(cart);
        }
        // ==========================================
        // 4. XÓA MÓN HÀNG KHỎI GIỎ
        // ==========================================
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(c => c.SanPhamId == productId);
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.Set("GioHang", cart); // Cập nhật lại giỏ
                }
            }
            return RedirectToAction("Cart");
        }

        // ==========================================
        // 5. TẠO ĐƠN HÀNG "CHỜ THANH TOÁN"
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckoutCart(string DiaChiGiaoHang, string SoDienThoai)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            if (cart == null || !cart.Any()) return RedirectToAction("Cart");

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            int tongTien = cart.Sum(c => c.ThanhTien);

            // Tạo đơn hàng nháp
            var donHang = new DonHang
            {
                TaiKhoanId = userId,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThai = "Chờ thanh toán", // Trạng thái chưa trả tiền
                DiaChiGiaoHang = DiaChiGiaoHang,
                SoDienThoai = SoDienThoai
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    DonHangId = donHang.Id,
                    SanPhamId = item.SanPhamId,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                });
            }
            await _context.SaveChangesAsync();

            // Truyền ID đơn hàng sang trang Thanh Toán
            TempData["ShopThanhToan_OrderId"] = donHang.Id;
            TempData["ShopThanhToan_TongTien"] = tongTien;

            return RedirectToAction("PaymentShop");
        }

        // ==========================================
        // 6. GIAO DIỆN CHỌN PHƯƠNG THỨC THANH TOÁN SHOP
        // ==========================================
        [Authorize]
        public IActionResult PaymentShop()
        {
            if (TempData["ShopThanhToan_OrderId"] == null) return RedirectToAction("Shop");
            TempData.Keep(); // Giữ Data để View xài
            return View();
        }

        // ==========================================
        // 7. XÁC NHẬN ĐÃ CHUYỂN KHOẢN (HOÀN TẤT)
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessPaymentShop(int orderId)
        {
            // Cập nhật trạng thái đơn hàng thành "Đã thanh toán"
            var order = await _context.DonHangs.FindAsync(orderId);
            if (order != null)
            {
                order.TrangThai = "Đã thanh toán (Chờ giao)";
                await _context.SaveChangesAsync();
            }

            // Xóa giỏ hàng
            HttpContext.Session.Remove("GioHang");

            TempData["Success"] = $"Thanh toán thành công đơn hàng #{orderId}! Chúng tôi sẽ giao hàng sớm nhất.";
            return RedirectToAction("Profile", "Account");
        }
        // ==========================================
        // TRANG ĐỌC TRUYỆN (HIỂN THỊ NỘI DUNG)
        // ==========================================
        public async Task<IActionResult> Read(int id)
        {
            var chuong = await _context.Chuongs
                .Include(c => c.Truyen)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chuong == null) return NotFound();

            // 1. KIỂM TRA BẢO MẬT: Nếu là chương VIP (Có phí)
            if (chuong.GiaCoin > 0)
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để đọc chương có phí!";
                    return RedirectToAction("Detail", new { id = chuong.TruyenId });
                }

                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                // KIỂM TRA 2 ĐIỀU KIỆN: Mua lẻ chương HOẶC Mua trọn bộ truyện
                bool daMuaChuong = await _context.LichSuMuas
                                        .AnyAsync(l => l.TaiKhoanId == userId && l.ChuongId == id);

                bool daMuaFull = await _context.LichSuMuaTruyens
                                        .AnyAsync(l => l.TaiKhoanId == userId && l.TruyenId == chuong.TruyenId);

                if (!daMuaChuong && !daMuaFull)
                {
                    TempData["Error"] = "Bạn cần mở khóa chương này hoặc mua trọn bộ để đọc!";
                    return RedirectToAction("Detail", new { id = chuong.TruyenId });
                }
            }

            // 2. Lấy toàn bộ chương của truyện này để làm Nút Chuyển Chương
            var tatCaChuong = await _context.Chuongs
                .Where(c => c.TruyenId == chuong.TruyenId)
                .OrderBy(c => c.SoThuTu)
                .ToListAsync();

            ViewBag.TatCaChuong = tatCaChuong;

            // Tìm vị trí chương hiện tại
            var currentIdx = tatCaChuong.FindIndex(c => c.Id == id);
            ViewBag.ChuongTruoc = currentIdx > 0 ? tatCaChuong[currentIdx - 1].Id : (int?)null;
            ViewBag.ChuongSau = currentIdx < tatCaChuong.Count - 1 ? tatCaChuong[currentIdx + 1].Id : (int?)null;

            return View(chuong);
        }

        // ==========================================
        // TÌM KIẾM TỔNG HỢP TOÀN TRANG (GLOBAL SEARCH)
        // ==========================================
        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            int pageSize = 12; // Số truyện trên 1 trang
            // Quét TẤT CẢ truyện, không phân biệt DanhMucId
            var query = _context.Truyens
                .Include(t => t.Chuongs)
                .Include(t => t.DanhMuc)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                // Tìm theo tên truyện hoặc tên tác giả
                query = query.Where(t => t.TenTruyen.Contains(keyword) || t.TacGia.Contains(keyword));
            }

            // Sắp xếp mới nhất lên đầu
            query = query.OrderByDescending(t => t.NgayCapNhat);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var danhSach = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword;

            return View(danhSach);
        }
        // ==========================================
        // TRANG HỖ TRỢ / LIÊN HỆ
        // ==========================================
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitContact(string hoTen, string email, string noiDung)
        {
            // Đây là phần điểm cộng Xử lý Form Liên Hệ
            // Nếu làm thêm hoàn chỉnh Web, có thể lưu vào DB bảng "LienHe" hoặc dùng SMTP gửi Email thật.

            TempData["Success"] = $"Cảm ơn {hoTen}! Lời nhắn của bạn đã được gửi. Chúng tôi sẽ phản hồi qua email {email} sớm nhất.";

            return RedirectToAction("Contact");
        }

        [HttpPost]
        public async Task<IActionResult> BuyFullTruyen(int truyenId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện giao dịch!" });

            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // Lấy thông tin truyện và user
            var truyen = await _context.Truyens.FindAsync(truyenId);
            var user = await _context.TaiKhoans.FindAsync(userId);

            if (truyen == null || user == null)
                return Json(new { success = false, message = "Thông tin không hợp lệ!" });

            // 1. Kiểm tra đã mua trọn bộ chưa
            var daMuaFull = await _context.LichSuMuaTruyens
                                .AnyAsync(l => l.TaiKhoanId == userId && l.TruyenId == truyenId);
            if (daMuaFull)
                return Json(new { success = false, message = "Bạn đã sở hữu trọn bộ truyện này rồi!" });

            // 2. Kiểm tra số dư (Sử dụng cột GiaCoin của truyện làm giá bán trọn bộ)
            if (user.SoDuCoin < truyen.GiaCoin)
                return Json(new { success = false, message = $"Bạn không đủ Coin (Cần {truyen.GiaCoin} Coin)!" });

            // 3. Thực hiện trừ tiền và lưu lịch sử
            user.SoDuCoin -= truyen.GiaCoin;
            _context.LichSuMuaTruyens.Add(new LichSuMuaTruyen
            {
                TaiKhoanId = userId,
                TruyenId = truyenId,
                SoCoinDaTru = truyen.GiaCoin,
                NgayMua = DateTime.Now
            });

            await _context.SaveChangesAsync();

            // CẬP NHẬT LẠI COIN TRÊN HEADER NGAY LẬP TỨC
            var identity = (ClaimsIdentity)User.Identity!;
            var coinClaim = identity.FindFirst("Coin");
            if (coinClaim != null) identity.RemoveClaim(coinClaim);
            identity.AddClaim(new Claim("Coin", user.SoDuCoin.ToString()));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // Cập nhật lại Session/Cookie nếu cần (để hiện số dư mới ngay lập tức)

            return Json(new { success = true, message = "Chúc mừng! Bạn đã mua trọn bộ truyện thành công." });
        }
    }
}