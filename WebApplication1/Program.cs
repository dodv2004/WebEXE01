using BLL.Services;
using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ??ng ký d?ch v? Database (DbContext)
builder.Services.AddDbContext<ProjectEXE01Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectEXE01Context")));

// ??ng ký DI (Dependency Injection) cho BLL và DAL
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// ??ng ký d?ch v? cho c? MVC (Controller/View) và Razor Pages (Admin Area)
builder.Services.AddControllersWithViews(); // C?n thi?t cho HomeController
builder.Services.AddRazorPages();          // C?n thi?t cho Admin Area


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
