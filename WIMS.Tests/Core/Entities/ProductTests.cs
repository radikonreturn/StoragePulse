namespace WIMS.Tests.Core.Entities;

public class ProductTests
{
    [Fact]
    public void Product_Creation_SetsDefaultProperties()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        Assert.True(product.IsActive);
        Assert.NotEqual(default, product.CreatedAt);
        Assert.Null(product.UpdatedAt);
        Assert.NotNull(product.StockMovements);
        Assert.Empty(product.StockMovements);
        Assert.NotNull(product.PurchaseOrderLines);
        Assert.Empty(product.PurchaseOrderLines);
        Assert.Equal(string.Empty, product.Code);
        Assert.Equal(string.Empty, product.Name);
    }
}
