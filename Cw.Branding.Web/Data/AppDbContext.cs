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
    public DbSet<VisualContent> VisualContents { get; set; }
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
        modelBuilder.Entity<VisualContent>()
        .HasIndex(x => x.PageCode);
    }
}