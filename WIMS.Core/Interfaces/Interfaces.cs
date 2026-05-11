using WIMS.Core.Entities;
using WIMS.Core.Enums;

namespace WIMS.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Unit> Units { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<StockMovement> StockMovements { get; }
    IRepository<PurchaseOrder> PurchaseOrders { get; }
    IRepository<AppSettings> AppSettings { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public interface INavigationService
{
    BaseViewModel? CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    void NavigateToRoot<TViewModel>() where TViewModel : class;
    void GoBack();
    bool CanGoBack { get; }
}

public interface IMessengerService
{
    void Send<TMessage>(TMessage message) where TMessage : class;
    void Register<TMessage>(object recipient, Action<TMessage> action) where TMessage : class;
    void Unregister<TMessage>(object recipient) where TMessage : class;
}

public interface IInventoryDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public sealed class DashboardSummary
{
    public int ProductCount { get; set; }
    public int SupplierCount { get; set; }
    public int PendingPurchaseOrderCount { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockProductCount { get; set; }
    public int OutOfStockProductCount { get; set; }
    public IReadOnlyList<DashboardLowStockItem> LowStockItems { get; set; } = [];
    public IReadOnlyList<DashboardMovementItem> RecentMovements { get; set; } = [];
}

public sealed class DashboardLowStockItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public string UnitSymbol { get; set; } = string.Empty;
    public StockStatus Status { get; set; }
}

public sealed class DashboardMovementItem
{
    public DateTime MovementDate { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public string UnitSymbol { get; set; } = string.Empty;
}
