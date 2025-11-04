using BLL.Services;
using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Database (DbContext)
builder.Services.AddDbContext<ProjectEXE01Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectEXE01Context")));

// Đăng ký DI (Dependency Injection) cho BLL và DAL
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// Đăng ký dịch vụ cho cả MVC (Controller/View) và Razor Pages (Admin Area)
builder.Services.AddControllersWithViews(); // Cần thiết cho HomeController
builder.Services.AddRazorPages();          // Cần thiết cho Admin Area

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
