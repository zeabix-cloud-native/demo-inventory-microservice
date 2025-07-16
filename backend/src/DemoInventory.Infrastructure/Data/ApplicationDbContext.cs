using DemoInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoInventory.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Demo Inventory application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

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
    }
}