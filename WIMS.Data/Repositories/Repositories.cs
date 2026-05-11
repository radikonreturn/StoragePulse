using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Interfaces;

namespace WIMS.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WIMSDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(WIMSDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.Where(e => e.IsActive).ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
        => await _dbSet.AnyAsync(e => e.Id == id && e.IsActive);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly WIMSDbContext _context;
    private IRepository<Product>? _products;
    private IRepository<Category>? _categories;
    private IRepository<Unit>? _units;
    private IRepository<Supplier>? _suppliers;
    private IRepository<StockMovement>? _stockMovements;
    private IRepository<PurchaseOrder>? _purchaseOrders;
    private IRepository<AppSettings>? _appSettings;

    public UnitOfWork(WIMSDbContext context)
    {
        _context = context;
    }

    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Unit> Units => _units ??= new Repository<Unit>(_context);
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
    public IRepository<StockMovement> StockMovements => _stockMovements ??= new Repository<StockMovement>(_context);
    public IRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new Repository<PurchaseOrder>(_context);
    public IRepository<AppSettings> AppSettings => _appSettings ??= new Repository<AppSettings>(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();
    public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

    public void Dispose() => _context.Dispose();
}
