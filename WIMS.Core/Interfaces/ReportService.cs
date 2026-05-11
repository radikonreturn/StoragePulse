using WIMS.Core.Enums;

namespace WIMS.Core.Interfaces;

public interface IReportService
{
    void ExportStockValuation(string filePath, IEnumerable<ProductDto> products);
    void ExportMovementHistory(string filePath, IEnumerable<StockMovementDto> movements);
    void ExportLowStock(string filePath, IEnumerable<ProductDto> products);
}

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int UnitId { get; set; }
    public string? UnitSymbol { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CurrentStock { get; set; }
    public StockStatus StockStatus { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public DateTime MovementDate { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}