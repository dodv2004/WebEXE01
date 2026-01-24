using BLL.Services;
using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ DỊCH VỤ (SERVICES) ---
builder.Services.AddControllersWithViews();

// Kết nối database
builder.Services.AddDbContext<ProjectEXE01Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectEXE01Context")));

// Đăng ký Dependency Injection (DI)
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// Bổ sung: Giúp các Class ở tầng BLL hoặc Repository có thể truy cập HttpContext/Session
builder.Services.AddHttpContextAccessor();

// Cấu hình Session
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true; // Bảo mật: Tránh bị script bên thứ 3 đọc Cookie
    options.Cookie.IsEssential = true; // Bắt buộc phải có để ứng dụng chạy đúng
});

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Bổ sung HSTS để tăng cường bảo mật HTTPS khi chạy thực tế
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự cực kỳ quan trọng: Authentication -> Authorization -> Session
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();