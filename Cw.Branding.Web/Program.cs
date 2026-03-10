using Cw.Branding.Web.Data;
using Cw.Branding.Web.Services;
using Cw.Branding.Web.Services.Implementations;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog (Logging)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services to the container.
builder.Services.AddControllersWithViews();

// 3. DbContext (Kết nối SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. Auth (Cookie Authentication cho Admin)
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.Cookie.Name = "CwAdminAuth";
        options.LoginPath = "/en/admin/auth/login"; // Đổi path login có /en/
        options.LogoutPath = "/en/admin/auth/logout";
        options.AccessDeniedPath = "/en/admin/auth/access-denied";
    });

builder.Services.AddAuthorization();

// Service registration will be added here when business logic is extracted from controllers
// Example: builder.Services.AddScoped<ICategoryService, CategoryService>();
// Example: builder.Services.AddScoped<IProductService, ProductService>();

// Register Services
builder.Services.AddScoped<INewsService, NewsService>();
// Đăng ký Services
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IMachineTypeService, MachineTypeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
var app = builder.Build();

// 5. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// 6. CẤU HÌNH ROUTING (QUAN TRỌNG: Thứ tự Admin -> Default)
// =========================================================

// 1. Route cho Admin Area (Phải dùng MapAreaControllerRoute)
// Cấu trúc: /en/admin/dashboard
// Đặt đoạn này TRƯỚC app.MapControllerRoute("default", ...)

app.MapAreaControllerRoute(
    name: "MyAreaAdmin",
    areaName: "Admin",
    pattern: "{lang=en}/admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{lang=vi}/{controller=Home}/{action=Index}/{id?}");
// 2. Route cho Client (Mặc định)
// Cấu trúc: /en/medical/index
app.MapControllerRoute(
    name: "default",
    pattern: "{lang=en}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "MyAreas",
    pattern: "{lang=vi}/{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


app.Run();