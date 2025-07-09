using DemoInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoInventory.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.SKU)
                .HasMaxLength(50)
                .IsRequired();
            
            entity.HasIndex(e => e.SKU)
                .IsUnique();
            
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            
            entity.Property(e => e.QuantityInStock)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
                
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure Product-Category relationship
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}