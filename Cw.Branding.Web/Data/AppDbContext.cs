using Cw.Branding.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // News
        modelBuilder.Entity<News>(entity =>
        {
            entity.HasIndex(x => x.SlugVi);
            entity.HasIndex(x => x.SlugEn);
        });

        // ContactFormEntry
        modelBuilder.Entity<ContactFormEntry>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
        });
    }
}
