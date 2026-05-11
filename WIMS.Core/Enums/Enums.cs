namespace WIMS.Core.Enums;

public enum MovementType
{
    Inbound = 1,
    Outbound = 2
}

public enum PurchaseOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Received = 2,
    Cancelled = 3
}

public enum StockStatus
{
    Normal = 0,
    Low = 1,
    Critical = 2,
    OutOfStock = 3
}
