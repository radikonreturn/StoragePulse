using WIMS.Core.Entities;
using WIMS.Core.Enums;

namespace WIMS.Application.Mapping;

public class MapperService : IMapperService
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return Map<TDestination>(source);
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source switch
        {
            Product entity when typeof(TDestination) == typeof(ProductDto) => Cast<TDestination>(MapProduct(entity)),
            ProductDto dto when typeof(TDestination) == typeof(Product) => Cast<TDestination>(MapProduct(dto)),
            Category entity when typeof(TDestination) == typeof(CategoryDto) => Cast<TDestination>(new CategoryDto { Id = entity.Id, Name = entity.Name, Description = entity.Description }),
            CategoryDto dto when typeof(TDestination) == typeof(Category) => Cast<TDestination>(new Category { Id = dto.Id, Name = dto.Name, Description = dto.Description }),
            Unit entity when typeof(TDestination) == typeof(UnitDto) => Cast<TDestination>(new UnitDto { Id = entity.Id, Name = entity.Name, Symbol = entity.Symbol }),
            UnitDto dto when typeof(TDestination) == typeof(Unit) => Cast<TDestination>(new Unit { Id = dto.Id, Name = dto.Name, Symbol = dto.Symbol }),
            Supplier entity when typeof(TDestination) == typeof(SupplierDto) => Cast<TDestination>(MapSupplier(entity)),
            SupplierDto dto when typeof(TDestination) == typeof(Supplier) => Cast<TDestination>(MapSupplier(dto)),
            StockMovement entity when typeof(TDestination) == typeof(StockMovementDto) => Cast<TDestination>(MapStockMovement(entity)),
            StockMovementDto dto when typeof(TDestination) == typeof(StockMovement) => Cast<TDestination>(MapStockMovement(dto)),
            PurchaseOrder entity when typeof(TDestination) == typeof(PurchaseOrderDto) => Cast<TDestination>(MapPurchaseOrder(entity)),
            PurchaseOrderDto dto when typeof(TDestination) == typeof(PurchaseOrder) => Cast<TDestination>(MapPurchaseOrder(dto)),
            PurchaseOrderLine entity when typeof(TDestination) == typeof(PurchaseOrderLineDto) => Cast<TDestination>(MapPurchaseOrderLine(entity)),
            PurchaseOrderLineDto dto when typeof(TDestination) == typeof(PurchaseOrderLine) => Cast<TDestination>(MapPurchaseOrderLine(dto)),
            AppSettings entity when typeof(TDestination) == typeof(AppSettingsDto) => Cast<TDestination>(MapAppSettings(entity)),
            AppSettingsDto dto when typeof(TDestination) == typeof(AppSettings) => Cast<TDestination>(MapAppSettings(dto)),
            _ => throw new NotSupportedException($"Mapping from {source.GetType().Name} to {typeof(TDestination).Name} is not configured.")
        };
    }

    private static ProductDto MapProduct(Product entity)
        => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            UnitId = entity.UnitId,
            UnitSymbol = entity.Unit?.Symbol,
            ReorderPoint = entity.ReorderPoint,
            UnitPrice = entity.UnitPrice
        };

    private static Product MapProduct(ProductDto dto)
        => new()
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            UnitId = dto.UnitId,
            ReorderPoint = dto.ReorderPoint,
            UnitPrice = dto.UnitPrice
        };

    private static SupplierDto MapSupplier(Supplier entity)
        => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ContactName = entity.ContactName,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            TaxNumber = entity.TaxNumber
        };

    private static Supplier MapSupplier(SupplierDto dto)
        => new()
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            ContactName = dto.ContactName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            TaxNumber = dto.TaxNumber
        };

    private static StockMovementDto MapStockMovement(StockMovement entity)
        => new()
        {
            Id = entity.Id,
            DocumentNumber = entity.DocumentNumber,
            Type = entity.Type,
            MovementDate = entity.MovementDate,
            ProductId = entity.ProductId,
            ProductCode = entity.Product?.Code,
            ProductName = entity.Product?.Name,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            TotalPrice = entity.Quantity * entity.UnitPrice,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes
        };

    private static StockMovement MapStockMovement(StockMovementDto dto)
        => new()
        {
            Id = dto.Id,
            DocumentNumber = dto.DocumentNumber,
            Type = dto.Type,
            MovementDate = dto.MovementDate,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes
        };

    private static PurchaseOrderDto MapPurchaseOrder(PurchaseOrder entity)
        => new()
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            SupplierId = entity.SupplierId,
            SupplierName = entity.Supplier?.Name,
            OrderDate = entity.OrderDate,
            Status = entity.Status,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            ReceivedDate = entity.ReceivedDate,
            Notes = entity.Notes,
            TotalAmount = entity.Lines.Where(l => l.IsActive).Sum(l => l.Quantity * l.UnitPrice),
            Lines = entity.Lines.Where(l => l.IsActive).Select(MapPurchaseOrderLine).ToList()
        };

    private static PurchaseOrder MapPurchaseOrder(PurchaseOrderDto dto)
    {
        var entity = new PurchaseOrder
        {
            Id = dto.Id,
            OrderNumber = dto.OrderNumber,
            SupplierId = dto.SupplierId,
            OrderDate = dto.OrderDate,
            Status = dto.Status,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            ReceivedDate = dto.ReceivedDate,
            Notes = dto.Notes
        };

        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(MapPurchaseOrderLine(line));
        }

        return entity;
    }

    private static PurchaseOrderLineDto MapPurchaseOrderLine(PurchaseOrderLine entity)
        => new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            LineTotal = entity.Quantity * entity.UnitPrice
        };

    private static PurchaseOrderLine MapPurchaseOrderLine(PurchaseOrderLineDto dto)
        => new()
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

    private static AppSettingsDto MapAppSettings(AppSettings entity)
        => new()
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            CompanyAddress = entity.CompanyAddress,
            CompanyPhone = entity.CompanyPhone,
            CompanyEmail = entity.CompanyEmail,
            TaxNumber = entity.TaxNumber,
            LogoPath = entity.LogoPath
        };

    private static AppSettings MapAppSettings(AppSettingsDto dto)
        => new()
        {
            Id = dto.Id,
            CompanyName = dto.CompanyName,
            CompanyAddress = dto.CompanyAddress,
            CompanyPhone = dto.CompanyPhone,
            CompanyEmail = dto.CompanyEmail,
            TaxNumber = dto.TaxNumber,
            LogoPath = dto.LogoPath
        };

    private static T Cast<T>(object value) => (T)value;
}

public interface IMapperService
{
    TDestination Map<TSource, TDestination>(TSource source);
    TDestination Map<TDestination>(object source);
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

public class CategoryDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string? Description { get; set; } }
public class UnitDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string Symbol { get; set; } = string.Empty; }

public class SupplierDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
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

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}

public class PurchaseOrderLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class AppSettingsDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? TaxNumber { get; set; }
    public string? LogoPath { get; set; }
}
