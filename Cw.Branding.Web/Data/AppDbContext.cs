using Cw.Branding.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<News> News => Set<News>();
    public DbSet<ContactFormEntry> ContactFormEntries => Set<ContactFormEntry>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<MachineType> MachineTypes => Set<MachineType>();
    public DbSet<HeroSection> HeroSections => Set<HeroSection>();
    public DbSet<HomeSlide> HomeSlides => Set<HomeSlide>();
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AppRole> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. MAPPING ---
        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<Brand>().ToTable("Brand");
        modelBuilder.Entity<MachineType>().ToTable("MachineType");
        modelBuilder.Entity<News>().ToTable("News");
        modelBuilder.Entity<ContactFormEntry>().ToTable("ContactFormEntry");
        modelBuilder.Entity<ProductImage>().ToTable("ProductImage");

        // --- 2. CẤU HÌNH CATEGORY ---
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);
        });

        // --- 3. CẤU HÌNH PRODUCT (Gộp chung các quan hệ) ---
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);

            // Quan hệ với Category (Bắt buộc - Không cho xóa Category nếu còn Product)
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với Brand (Tùy chọn - Set Null khi xóa Brand)
            entity.HasOne(p => p.Brand)
                  .WithMany()
                  .HasForeignKey(p => p.BrandId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ với MachineType (Tùy chọn - Set Null khi xóa MachineType)
            entity.HasOne(p => p.MachineType)
                  .WithMany()
                  .HasForeignKey(p => p.MachineTypeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // --- 4. CẤU HÌNH PRODUCT IMAGE (Xóa Product thì xóa luôn ảnh) ---
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- 5. CẤU HÌNH NEWS & CONTACT ---
        modelBuilder.Entity<News>(entity =>
        {
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);
        });

        modelBuilder.Entity<ContactFormEntry>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
        });
        base.OnModelCreating(modelBuilder);

        // Cấu hình tên bảng (để db gọn gàng)
        modelBuilder.Entity<HeroSection>().ToTable("HeroSection");
        modelBuilder.Entity<HomeSlide>().ToTable("HomeSlide");

        // (Optional) Seeding dữ liệu mặc định để trang chủ không bị trống sau khi update
        modelBuilder.Entity<HeroSection>().HasData(new HeroSection
        {
            Id = 1,
            BackgroundImage = "/images/Hero illstration.png",
            TitleEn = "Trusted Medical Solutions For Modern",
            TitleVi = "Giải pháp Y tế Tin cậy cho",
            HighlightEn = "Healthcare",
            HighlightVi = "Y tế Hiện đại"
          
        });
        base.OnModelCreating(modelBuilder);

        // 1. Seed Roles (Bắt buộc seed trước vì User có FK đến Role)
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin", Description = "Toàn quyền hệ thống" },
            new AppRole { Id = 2, Name = "User", Description = "Quản lý nội dung" }
        );

        // 2. Seed Admin Account
        // Khởi tạo hasher để tạo chuỗi mã hóa mật khẩu chuẩn .NET Identity
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>();

        modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = 1,
                Username = "admin",
                Email = "admin@charleswembley.com",
                PasswordHash = hasher.HashPassword(null, "Admin@123"),
                RoleId = 1, // Gán quyền Admin
                FullName = "System Admin",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}