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
            e.Property(p => p.Code).HasMaxLength(40).IsRequired();
            e.Property(p => p.Name).HasMaxLength(160).IsRequired();
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.ReorderPoint).HasPrecision(18, 3);
            e.Property(p => p.UnitPrice).HasPrecision(18, 2);
            e.HasIndex(p => p.Code).IsUnique();
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Unit).WithMany(u => u.Products).HasForeignKey(p => p.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(120).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Unit>(e =>
        {
            e.Property(u => u.Name).HasMaxLength(80).IsRequired();
            e.Property(u => u.Symbol).HasMaxLength(20).IsRequired();
            e.HasIndex(u => u.Symbol).IsUnique();
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.Property(s => s.Code).HasMaxLength(40).IsRequired();
            e.Property(s => s.Name).HasMaxLength(180).IsRequired();
            e.Property(s => s.ContactName).HasMaxLength(120);
            e.Property(s => s.Phone).HasMaxLength(40);
            e.Property(s => s.Email).HasMaxLength(160);
            e.Property(s => s.Address).HasMaxLength(500);
            e.Property(s => s.TaxNumber).HasMaxLength(40);
            e.HasIndex(s => s.Code).IsUnique();
            e.HasIndex(s => s.Email);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.Property(s => s.DocumentNumber).HasMaxLength(60).IsRequired();
            e.Property(s => s.Quantity).HasPrecision(18, 3);
            e.Property(s => s.UnitPrice).HasPrecision(18, 2);
            e.Property(s => s.ReferenceNumber).HasMaxLength(80);
            e.Property(s => s.Notes).HasMaxLength(500);
            e.HasIndex(s => s.DocumentNumber);
            e.HasOne(s => s.Product).WithMany(p => p.StockMovements).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.Property(o => o.OrderNumber).HasMaxLength(60).IsRequired();
            e.Property(o => o.Notes).HasMaxLength(500);
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasOne(o => o.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(o => o.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.Property(l => l.Quantity).HasPrecision(18, 3);
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.HasOne(l => l.PurchaseOrder).WithMany(o => o.Lines).HasForeignKey(l => l.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Product).WithMany(p => p.PurchaseOrderLines).HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppSettings>(e =>
        {
            e.Property(s => s.CompanyName).HasMaxLength(180).IsRequired();
            e.Property(s => s.CompanyAddress).HasMaxLength(500);
            e.Property(s => s.CompanyPhone).HasMaxLength(40);
            e.Property(s => s.CompanyEmail).HasMaxLength(160);
            e.Property(s => s.TaxNumber).HasMaxLength(40);
            e.Property(s => s.LogoPath).HasMaxLength(260);
        });

        base.OnModelCreating(modelBuilder);
    }
}
