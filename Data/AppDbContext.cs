using Microsoft.EntityFrameworkCore;
using AurHER.Models;

namespace AurHER.Data
{
    public class AppDbContext : DbContext
    {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public DbSet<Collection> Collections { get; set; }
    public DbSet<CollectionProduct> CollectionProducts { get; set; }
    public DbSet<DeliveryLocation> DeliveryLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasColumnType("text");
        
        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasColumnType("text");
    

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entity.GetProperties()
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in properties)
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
}