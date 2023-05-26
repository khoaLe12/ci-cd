using Microsoft.EntityFrameworkCore;

namespace WebService.Model;

public class NorthwindContext : DbContext
{
    public NorthwindContext() 
    {}

    public NorthwindContext(DbContextOptions<NorthwindContext> options) 
        : base(options)
    {}

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("Order Details");

            entity.HasKey(e => new { e.OrderId, e.ProductId });

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}
