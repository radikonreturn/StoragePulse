using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;

namespace WIMS.Data;

public static class DatabaseSeeder
{
    private static readonly string[] DemoSupplierCodes = ["SUP-001", "SUP-002", "SUP-003"];
    private static readonly string[] DemoProductCodes = ["RM-STEEL-01", "RM-ALU-01", "FG-CAB-100", "FG-RACK-200", "CONS-GLOVE", "CONS-TAPE"];
    private static readonly string[] DemoPurchaseOrderNumbers = ["PO-DEMO-001", "PO-DEMO-002"];

    public static async Task SeedAsync(WIMSDbContext context, CancellationToken cancellationToken = default)
    {
        await RemoveDemoDataAsync(context, cancellationToken);

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
                CompanyName = "StoragePulse"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task RemoveDemoDataAsync(WIMSDbContext context, CancellationToken cancellationToken)
    {
        var demoProductIds = await context.Products
            .Where(p => DemoProductCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var demoPurchaseOrderIds = await context.PurchaseOrders
            .Where(o => DemoPurchaseOrderNumbers.Contains(o.OrderNumber))
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        await context.PurchaseOrderLines
            .Where(l => demoPurchaseOrderIds.Contains(l.PurchaseOrderId) || demoProductIds.Contains(l.ProductId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.PurchaseOrders
            .Where(o => DemoPurchaseOrderNumbers.Contains(o.OrderNumber))
            .ExecuteDeleteAsync(cancellationToken);

        await context.StockMovements
            .Where(m => demoProductIds.Contains(m.ProductId)
                || m.DocumentNumber.StartsWith("DEMO-")
                || m.ReferenceNumber == "DEMO-OPENING"
                || m.ReferenceNumber == "DEMO-USAGE")
            .ExecuteDeleteAsync(cancellationToken);

        await context.Products
            .Where(p => DemoProductCodes.Contains(p.Code))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Suppliers
            .Where(s => DemoSupplierCodes.Contains(s.Code))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
