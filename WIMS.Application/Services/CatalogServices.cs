using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Data;

namespace WIMS.Application.Services;

public interface IProductCatalogService
{
    Task<IReadOnlyList<ProductListItem>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductEditModel?> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductLookups> GetProductLookupsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult> SaveProductAsync(ProductEditModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}

public interface ISupplierCatalogService
{
    Task<IReadOnlyList<SupplierListItem>> GetSuppliersAsync(CancellationToken cancellationToken = default);
    Task<SupplierEditModel?> GetSupplierAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> SaveSupplierAsync(SupplierEditModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteSupplierAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class ProductCatalogService : IProductCatalogService
{
    private readonly WIMSDbContext _db;

    public ProductCatalogService(WIMSDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductListItem>> GetProductsAsync(CancellationToken cancellationToken = default)
        => await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .Select(p => new ProductListItem(
                p.Id,
                p.Code,
                p.Name,
                p.Category.Name,
                p.Unit.Symbol,
                p.ReorderPoint,
                p.UnitPrice))
            .ToListAsync(cancellationToken);

    public async Task<ProductEditModel?> GetProductAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductEditModel
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                UnitId = p.UnitId,
                ReorderPoint = p.ReorderPoint,
                UnitPrice = p.UnitPrice
            })
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ProductLookups> GetProductLookupsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CatalogLookupItem(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        var units = await _db.Units
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new CatalogLookupItem(u.Id, $"{u.Name} ({u.Symbol})"))
            .ToListAsync(cancellationToken);

        return new ProductLookups(categories, units);
    }

    public async Task<ServiceResult> SaveProductAsync(ProductEditModel model, CancellationToken cancellationToken = default)
    {
        var validation = CatalogValidator.ValidateProduct(model);
        if (!validation.Success)
        {
            return validation;
        }

        var code = model.Code.Trim();
        var duplicateCode = await _db.Products.AnyAsync(
            p => p.IsActive && p.Code == code && p.Id != model.Id,
            cancellationToken);
        if (duplicateCode)
        {
            return ServiceResult.Fail("Ürün kodu benzersiz olmalıdır.");
        }

        Product entity;
        if (model.Id == 0)
        {
            entity = new Product { CreatedAt = DateTime.UtcNow };
            _db.Products.Add(entity);
        }
        else
        {
            entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == model.Id && p.IsActive, cancellationToken);
            if (entity is null)
            {
                return ServiceResult.Fail("Ürün bulunamadı.");
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Code = code;
        entity.Name = model.Name.Trim();
        entity.Description = Normalize(model.Description);
        entity.CategoryId = model.CategoryId;
        entity.UnitId = model.UnitId;
        entity.ReorderPoint = model.ReorderPoint;
        entity.UnitPrice = model.UnitPrice;

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Ürün bulunamadı.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class SupplierCatalogService : ISupplierCatalogService
{
    private readonly WIMSDbContext _db;

    public SupplierCatalogService(WIMSDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SupplierListItem>> GetSuppliersAsync(CancellationToken cancellationToken = default)
        => await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Code)
            .Select(s => new SupplierListItem(
                s.Id,
                s.Code,
                s.Name,
                s.ContactName ?? string.Empty,
                s.Phone ?? string.Empty,
                s.Email ?? string.Empty,
                s.TaxNumber ?? string.Empty))
            .ToListAsync(cancellationToken);

    public async Task<SupplierEditModel?> GetSupplierAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new SupplierEditModel
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                TaxNumber = s.TaxNumber
            })
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ServiceResult> SaveSupplierAsync(SupplierEditModel model, CancellationToken cancellationToken = default)
    {
        var validation = CatalogValidator.ValidateSupplier(model);
        if (!validation.Success)
        {
            return validation;
        }

        var code = model.Code.Trim();
        var duplicateCode = await _db.Suppliers.AnyAsync(
            s => s.IsActive && s.Code == code && s.Id != model.Id,
            cancellationToken);
        if (duplicateCode)
        {
            return ServiceResult.Fail("Tedarikçi kodu benzersiz olmalıdır.");
        }

        Supplier entity;
        if (model.Id == 0)
        {
            entity = new Supplier { CreatedAt = DateTime.UtcNow };
            _db.Suppliers.Add(entity);
        }
        else
        {
            entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == model.Id && s.IsActive, cancellationToken);
            if (entity is null)
            {
                return ServiceResult.Fail("Tedarikçi bulunamadı.");
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Code = code;
        entity.Name = model.Name.Trim();
        entity.ContactName = Normalize(model.ContactName);
        entity.Phone = Normalize(model.Phone);
        entity.Email = Normalize(model.Email);
        entity.Address = Normalize(model.Address);
        entity.TaxNumber = Normalize(model.TaxNumber);

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteSupplierAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Tedarikçi bulunamadı.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class CatalogValidator
{
    public static ServiceResult ValidateProduct(ProductEditModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Name))
        {
            return ServiceResult.Fail("Ürün kodu ve adı zorunludur.");
        }

        if (model.CategoryId <= 0 || model.UnitId <= 0)
        {
            return ServiceResult.Fail("Kategori ve birim seçin.");
        }

        if (model.ReorderPoint < 0)
        {
            return ServiceResult.Fail("Yeniden sipariş noktası negatif olamaz.");
        }

        if (model.UnitPrice < 0)
        {
            return ServiceResult.Fail("Birim fiyat negatif olamaz.");
        }

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateSupplier(SupplierEditModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Name))
        {
            return ServiceResult.Fail("Tedarikçi kodu ve adı zorunludur.");
        }

        if (!string.IsNullOrWhiteSpace(model.Email) && !model.Email.Contains('@', StringComparison.Ordinal))
        {
            return ServiceResult.Fail("Geçerli bir e-posta adresi girin.");
        }

        return ServiceResult.Ok();
    }
}

public sealed record ProductListItem(
    int Id,
    string Code,
    string Name,
    string CategoryName,
    string UnitSymbol,
    decimal ReorderPoint,
    decimal UnitPrice);

public sealed record SupplierListItem(
    int Id,
    string Code,
    string Name,
    string ContactName,
    string Phone,
    string Email,
    string TaxNumber);

public sealed record ProductLookups(
    IReadOnlyList<CatalogLookupItem> Categories,
    IReadOnlyList<CatalogLookupItem> Units);

public sealed record CatalogLookupItem(int Id, string Name);

public sealed class ProductEditModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int UnitId { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class SupplierEditModel
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

public sealed class ServiceResult
{
    private ServiceResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }
    public string? ErrorMessage { get; }

    public static ServiceResult Ok() => new(true, null);
    public static ServiceResult Fail(string errorMessage) => new(false, errorMessage);
}
