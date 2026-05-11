using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;

namespace WIMS.Data;

public class WIMSDbContext : DbContext
{
    public WIMSDbContext(DbContextOptions<WIMSDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Code).IsUnique();
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
            e.HasOne(p => p.Unit).WithMany(u => u.Products).HasForeignKey(p => p.UnitId);
        });

        modelBuilder.Entity<Category>(e => e.HasIndex(c => c.Name).IsUnique());
        modelBuilder.Entity<Unit>(e => e.HasIndex(u => u.Symbol).IsUnique());

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasIndex(s => s.Code).IsUnique();
            e.HasIndex(s => s.Email);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasIndex(s => s.DocumentNumber);
            e.HasOne(s => s.Product).WithMany(p => p.StockMovements).HasForeignKey(s => s.ProductId);
        });

        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasOne(o => o.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(o => o.SupplierId);
        });

        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.HasOne(l => l.PurchaseOrder).WithMany(o => o.Lines).HasForeignKey(l => l.PurchaseOrderId);
            e.HasOne(l => l.Product).WithMany(p => p.PurchaseOrderLines).HasForeignKey(l => l.ProductId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
