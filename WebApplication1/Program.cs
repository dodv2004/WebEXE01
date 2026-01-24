using BLL.Services;
using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ DỊCH VỤ (SERVICES) ---
builder.Services.AddControllersWithViews(); // Cho MVC (User UI)
builder.Services.AddRazorPages();           // Cho Razor Pages (Admin Area)

// Kết nối database
builder.Services.AddDbContext<ProjectEXE01Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectEXE01Context")));

// Đăng ký Dependency Injection (DI)
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// BẮT BUỘC: Để BLL truy cập được Session người dùng
builder.Services.AddHttpContextAccessor();

// Cấu hình Session (BẮT BUỘC cho hệ thống Login của bạn)
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Chuyển hướng lỗi về MVC Home
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// BẮT BUỘC: Phải đặt UseSession sau UseRouting và trước Map
app.UseSession();

// --- 3. ĐIỀU HƯỚNG (MAPPING) ---

// Map cho Controller (Trang chủ, Tra cứu, Login...)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map cho Razor Pages (Trang Admin)
app.MapRazorPages();

app.Run();