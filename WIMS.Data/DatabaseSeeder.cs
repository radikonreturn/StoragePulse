using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Enums;

namespace WIMS.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WIMSDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            context.Categories.AddRange(
                new Category { Name = "Hammadde", Description = "Üretimde kullanılan temel malzemeler" },
                new Category { Name = "Yarı Mamul", Description = "Üretim süreci devam eden ürünler" },
                new Category { Name = "Mamul", Description = "Satışa veya sevke hazır ürünler" },
                new Category { Name = "Sarf Malzeme", Description = "Operasyonel tüketim malzemeleri" });
        }

        if (!await context.Units.AnyAsync(cancellationToken))
        {
            context.Units.AddRange(
                new Unit { Name = "Adet", Symbol = "adet" },
                new Unit { Name = "Kilogram", Symbol = "kg" },
                new Unit { Name = "Litre", Symbol = "lt" },
                new Unit { Name = "Metre", Symbol = "m" },
                new Unit { Name = "Kutu", Symbol = "kutu" });
        }

        if (!await context.AppSettings.AnyAsync(cancellationToken))
        {
            context.AppSettings.Add(new AppSettings
            {
                CompanyName = "StoragePulse",
                CompanyPhone = "+90 212 000 00 00",
                CompanyEmail = "info@storagepulse.local"
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        await SeedDemoDataAsync(context, cancellationToken);
    }

    private static async Task SeedDemoDataAsync(WIMSDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Suppliers.AnyAsync(cancellationToken))
        {
            context.Suppliers.AddRange(
                new Supplier
                {
                    Code = "SUP-001",
                    Name = "Anadolu Metal A.Ş.",
                    ContactName = "Murat Demir",
                    Phone = "+90 216 555 10 10",
                    Email = "satis@anadolumetal.local",
                    TaxNumber = "3470012345"
                },
                new Supplier
                {
                    Code = "SUP-002",
                    Name = "Marmara Ambalaj",
                    ContactName = "Ece Yılmaz",
                    Phone = "+90 212 555 20 20",
                    Email = "operasyon@marmaraambalaj.local",
                    TaxNumber = "4210098765"
                },
                new Supplier
                {
                    Code = "SUP-003",
                    Name = "Ege Kimya",
                    ContactName = "Selin Kaya",
                    Phone = "+90 232 555 30 30",
                    Email = "tedarik@egekimya.local",
                    TaxNumber = "1280045678"
                });
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.Products.AnyAsync(cancellationToken))
        {
            var rawMaterial = await context.Categories.FirstAsync(c => c.Name == "Hammadde", cancellationToken);
            var finishedGoods = await context.Categories.FirstAsync(c => c.Name == "Mamul", cancellationToken);
            var consumable = await context.Categories.FirstAsync(c => c.Name == "Sarf Malzeme", cancellationToken);

            var piece = await context.Units.FirstAsync(u => u.Symbol == "adet", cancellationToken);
            var kilogram = await context.Units.FirstAsync(u => u.Symbol == "kg", cancellationToken);
            var box = await context.Units.FirstAsync(u => u.Symbol == "kutu", cancellationToken);
            var meter = await context.Units.FirstAsync(u => u.Symbol == "m", cancellationToken);

            context.Products.AddRange(
                new Product { Code = "RM-STEEL-01", Name = "Çelik Levha 2mm", CategoryId = rawMaterial.Id, UnitId = kilogram.Id, ReorderPoint = 250, UnitPrice = 42.50m },
                new Product { Code = "RM-ALU-01", Name = "Alüminyum Profil", CategoryId = rawMaterial.Id, UnitId = meter.Id, ReorderPoint = 120, UnitPrice = 78.90m },
                new Product { Code = "FG-CAB-100", Name = "Endüstriyel Kabin", CategoryId = finishedGoods.Id, UnitId = piece.Id, ReorderPoint = 12, UnitPrice = 3450m },
                new Product { Code = "FG-RACK-200", Name = "Depo Raf Modülü", CategoryId = finishedGoods.Id, UnitId = piece.Id, ReorderPoint = 20, UnitPrice = 1890m },
                new Product { Code = "CONS-GLOVE", Name = "Koruyucu Eldiven", CategoryId = consumable.Id, UnitId = box.Id, ReorderPoint = 15, UnitPrice = 220m },
                new Product { Code = "CONS-TAPE", Name = "Endüstriyel Bant", CategoryId = consumable.Id, UnitId = piece.Id, ReorderPoint = 30, UnitPrice = 65m });
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.StockMovements.AnyAsync(cancellationToken))
        {
            var products = await context.Products.Where(p => p.IsActive).OrderBy(p => p.Code).ToListAsync(cancellationToken);
            var baseDate = DateTime.Today;

            var movements = new List<StockMovement>();
            foreach (var product in products)
            {
                var inboundQuantity = product.Code.StartsWith("FG", StringComparison.Ordinal) ? 35 : 500;
                var outboundQuantity = product.Code.StartsWith("CONS", StringComparison.Ordinal) ? 490 : product.Code.StartsWith("FG", StringComparison.Ordinal) ? 18 : 160;

                movements.Add(new StockMovement
                {
                    DocumentNumber = $"DEMO-IN-{product.Code}",
                    Type = MovementType.Inbound,
                    MovementDate = baseDate.AddDays(-10),
                    ProductId = product.Id,
                    Quantity = inboundQuantity,
                    UnitPrice = product.UnitPrice,
                    ReferenceNumber = "DEMO-OPENING",
                    Notes = "Demo açılış stoğu"
                });

                movements.Add(new StockMovement
                {
                    DocumentNumber = $"DEMO-OUT-{product.Code}",
                    Type = MovementType.Outbound,
                    MovementDate = baseDate.AddDays(-2),
                    ProductId = product.Id,
                    Quantity = outboundQuantity,
                    UnitPrice = product.UnitPrice,
                    ReferenceNumber = "DEMO-USAGE",
                    Notes = "Demo tüketim hareketi"
                });
            }

            context.StockMovements.AddRange(movements);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.PurchaseOrders.AnyAsync(cancellationToken))
        {
            var suppliers = await context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Code).ToListAsync(cancellationToken);
            var products = await context.Products.Where(p => p.IsActive).OrderBy(p => p.Code).ToListAsync(cancellationToken);

            if (suppliers.Count > 0 && products.Count >= 3)
            {
                var order1 = new PurchaseOrder
                {
                    OrderNumber = "PO-DEMO-001",
                    SupplierId = suppliers[0].Id,
                    OrderDate = DateTime.Today.AddDays(-4),
                    ExpectedDeliveryDate = DateTime.Today.AddDays(3),
                    Status = PurchaseOrderStatus.Confirmed,
                    Notes = "Demo onaylı sipariş"
                };
                order1.Lines.Add(new PurchaseOrderLine { ProductId = products[0].Id, Quantity = 200, UnitPrice = products[0].UnitPrice });
                order1.Lines.Add(new PurchaseOrderLine { ProductId = products[1].Id, Quantity = 80, UnitPrice = products[1].UnitPrice });

                var order2 = new PurchaseOrder
                {
                    OrderNumber = "PO-DEMO-002",
                    SupplierId = suppliers[Math.Min(1, suppliers.Count - 1)].Id,
                    OrderDate = DateTime.Today,
                    ExpectedDeliveryDate = DateTime.Today.AddDays(7),
                    Status = PurchaseOrderStatus.Draft,
                    Notes = "Demo taslak sipariş"
                };
                order2.Lines.Add(new PurchaseOrderLine { ProductId = products[4].Id, Quantity = 20, UnitPrice = products[4].UnitPrice });

                context.PurchaseOrders.AddRange(order1, order2);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
