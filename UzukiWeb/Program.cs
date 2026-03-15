using Microsoft.EntityFrameworkCore;
using UzukiWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// --- BẬT SESSION CHO GIỎ HÀNG ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giữ giỏ hàng trong 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// --- KÍCH HOẠT ĐĂNG NHẬP COOKIE ---
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn khi chưa đăng nhập bị đá về đây
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi cấm truy cập
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Giữ đăng nhập 30 ngày
    });
// ----------------------------------------------------

// Đăng ký UzukiDbContext lấy chuỗi kết nối từ appsettings.json
builder.Services.AddDbContext<UzukiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UzukiDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication(); //  (Kiểm tra thẻ căn cước)
app.UseAuthorization();  // BẮT BUỘC CÓ SẴN (Kiểm tra quyền)

// Cấu hình xử lý file tĩnh (CSS, JS, Images)
app.UseAuthorization();
app.MapStaticAssets();

//Route cho Area Admin này vào trước
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
