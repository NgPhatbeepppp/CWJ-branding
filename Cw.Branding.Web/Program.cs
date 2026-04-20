using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services;
using Cw.Branding.Web.Services.Implementations;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// 2. Add Services & Localization
builder.Services.AddControllersWithViews()
    .AddViewLocalization(); // Hỗ trợ dịch thuật trong View
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Tên Header mà script JS sẽ gửi lên
});
// Cấu hình danh sách ngôn ngữ hỗ trợ
// Tìm đoạn cấu hình RequestLocalizationOptions và cập nhật:
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("vi") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Quan trọng: Xóa các provider khác, chỉ giữ lại RouteData để ép hệ thống đi theo URL
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new RouteDataRequestCultureProvider { RouteDataStringKey = "lang", UIRouteDataStringKey = "lang" }
    };
});
// 3. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. Auth (Cập nhật đường dẫn Login động theo lang)
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.Cookie.Name = "CwAdminAuth";
        options.LoginPath = "/en/admin/auth/login";
        options.LogoutPath = "/en/admin/auth/logout";
    });

builder.Services.AddAuthorization();
// Nâng giới hạn dung lượng request (Ví dụ: 100MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.MemoryBufferThreshold = int.MaxValue;
});

// Nếu chạy trên Kestrel (mặc định của .NET Core)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});
// 5. DI Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IMachineTypeService, MachineTypeService>();
builder.Services.AddScoped<IProductImportService, ProductImportService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Auth/Login";
        options.AccessDeniedPath = "/Admin/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    }); 

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


var locOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// 7. CẤU HÌNH ROUTING ĐỒNG BỘ
// Sử dụng regex constraint để chỉ chấp nhận 'en' hoặc 'vi'

// Route Admin Area
app.MapAreaControllerRoute(
    name: "MyAreaAdmin",
    areaName: "Admin",
    pattern: "{lang:regex(^(en|vi)$)}/admin/{controller=Dashboard}/{action=Index}/{id?}");

// Route Client mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{lang:regex(^(en|vi)$)}/{controller=Home}/{action=Index}/{id?}");

// Route Fallback: Nếu user vào "/" không có lang -> Redirect về "/en"
app.MapGet("/", context => {
    context.Response.Redirect("/en");
    return Task.CompletedTask;
});

app.Run();