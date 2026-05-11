using Microsoft.EntityFrameworkCore;
using WIMS.Core.Enums;
using WIMS.Core.Interfaces;

namespace WIMS.Data.Services;

public sealed class InventoryDashboardService : IInventoryDashboardService
{
    private readonly WIMSDbContext _context;

    public InventoryDashboardService(WIMSDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var productStocks = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new ProductStockSnapshot
            {
                ProductCode = p.Code,
                ProductName = p.Name,
                UnitSymbol = p.Unit.Symbol,
                UnitPrice = p.UnitPrice,
                ReorderPoint = p.ReorderPoint,
                CurrentStock = p.StockMovements
                    .Where(m => m.IsActive)
                    .Sum(m => m.Type == MovementType.Inbound ? m.Quantity : -m.Quantity)
            })
            .ToListAsync(cancellationToken);

        var lowStockItems = productStocks
            .Where(p => p.CurrentStock <= p.ReorderPoint)
            .OrderBy(p => p.CurrentStock)
            .ThenBy(p => p.ProductName)
            .Take(8)
            .Select(p => new DashboardLowStockItem
            {
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CurrentStock = p.CurrentStock,
                ReorderPoint = p.ReorderPoint,
                UnitSymbol = p.UnitSymbol,
                Status = GetStockStatus(p.CurrentStock, p.ReorderPoint)
            })
            .ToList();

        var recentMovements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.Id)
            .Take(8)
            .Select(m => new DashboardMovementItem
            {
                MovementDate = m.MovementDate,
                DocumentNumber = m.DocumentNumber,
                ProductCode = m.Product.Code,
                ProductName = m.Product.Name,
                Type = m.Type,
                Quantity = m.Quantity,
                UnitSymbol = m.Product.Unit.Symbol
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummary
        {
            ProductCount = productStocks.Count,
            SupplierCount = await _context.Suppliers.CountAsync(s => s.IsActive, cancellationToken),
            PendingPurchaseOrderCount = await _context.PurchaseOrders.CountAsync(
                o => o.IsActive && (o.Status == PurchaseOrderStatus.Draft || o.Status == PurchaseOrderStatus.Confirmed),
                cancellationToken),
            TotalStockValue = productStocks.Sum(p => p.CurrentStock * p.UnitPrice),
            LowStockProductCount = productStocks.Count(p => p.CurrentStock <= p.ReorderPoint),
            OutOfStockProductCount = productStocks.Count(p => p.CurrentStock <= 0),
            LowStockItems = lowStockItems,
            RecentMovements = recentMovements
        };
    }

    private static StockStatus GetStockStatus(decimal currentStock, decimal reorderPoint)
    {
        if (currentStock <= 0)
        {
            return StockStatus.OutOfStock;
        }

        if (currentStock <= reorderPoint / 2)
        {
            return StockStatus.Critical;
        }

        return currentStock <= reorderPoint ? StockStatus.Low : StockStatus.Normal;
    }

    private sealed class ProductStockSnapshot
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string UnitSymbol { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal CurrentStock { get; set; }
    }
}
