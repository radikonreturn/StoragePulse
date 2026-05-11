using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIMS.Data.Migrations;

public partial class HardenModelConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Products_Categories_CategoryId", "Products");
        migrationBuilder.DropForeignKey("FK_Products_Units_UnitId", "Products");
        migrationBuilder.DropForeignKey("FK_PurchaseOrderLines_Products_ProductId", "PurchaseOrderLines");
        migrationBuilder.DropForeignKey("FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId", "PurchaseOrderLines");
        migrationBuilder.DropForeignKey("FK_PurchaseOrders_Suppliers_SupplierId", "PurchaseOrders");
        migrationBuilder.DropForeignKey("FK_StockMovements_Products_ProductId", "StockMovements");

        migrationBuilder.AlterColumn<string>("CompanyName", "AppSettings", "TEXT", maxLength: 180, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("CompanyAddress", "AppSettings", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("CompanyPhone", "AppSettings", "TEXT", maxLength: 40, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("CompanyEmail", "AppSettings", "TEXT", maxLength: 160, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("TaxNumber", "AppSettings", "TEXT", maxLength: 40, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("LogoPath", "AppSettings", "TEXT", maxLength: 260, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);

        migrationBuilder.AlterColumn<string>("Name", "Categories", "TEXT", maxLength: 120, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Description", "Categories", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);

        migrationBuilder.AlterColumn<string>("Code", "Products", "TEXT", maxLength: 40, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Name", "Products", "TEXT", maxLength: 160, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Description", "Products", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<decimal>("ReorderPoint", "Products", "TEXT", precision: 18, scale: 3, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "Products", "TEXT", precision: 18, scale: 2, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");

        migrationBuilder.AlterColumn<string>("OrderNumber", "PurchaseOrders", "TEXT", maxLength: 60, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Notes", "PurchaseOrders", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);

        migrationBuilder.AlterColumn<decimal>("Quantity", "PurchaseOrderLines", "TEXT", precision: 18, scale: 3, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "PurchaseOrderLines", "TEXT", precision: 18, scale: 2, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");

        migrationBuilder.AlterColumn<string>("DocumentNumber", "StockMovements", "TEXT", maxLength: 60, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<decimal>("Quantity", "StockMovements", "TEXT", precision: 18, scale: 3, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "StockMovements", "TEXT", precision: 18, scale: 2, nullable: false, oldClrType: typeof(decimal), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("ReferenceNumber", "StockMovements", "TEXT", maxLength: 80, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("Notes", "StockMovements", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);

        migrationBuilder.AlterColumn<string>("Code", "Suppliers", "TEXT", maxLength: 40, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Name", "Suppliers", "TEXT", maxLength: 180, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("ContactName", "Suppliers", "TEXT", maxLength: 120, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("Phone", "Suppliers", "TEXT", maxLength: 40, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("Email", "Suppliers", "TEXT", maxLength: 160, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("Address", "Suppliers", "TEXT", maxLength: 500, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);
        migrationBuilder.AlterColumn<string>("TaxNumber", "Suppliers", "TEXT", maxLength: 40, nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldNullable: true);

        migrationBuilder.AlterColumn<string>("Name", "Units", "TEXT", maxLength: 80, nullable: false, oldClrType: typeof(string), oldType: "TEXT");
        migrationBuilder.AlterColumn<string>("Symbol", "Units", "TEXT", maxLength: 20, nullable: false, oldClrType: typeof(string), oldType: "TEXT");

        AddRestrictForeignKeys(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Products_Categories_CategoryId", "Products");
        migrationBuilder.DropForeignKey("FK_Products_Units_UnitId", "Products");
        migrationBuilder.DropForeignKey("FK_PurchaseOrderLines_Products_ProductId", "PurchaseOrderLines");
        migrationBuilder.DropForeignKey("FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId", "PurchaseOrderLines");
        migrationBuilder.DropForeignKey("FK_PurchaseOrders_Suppliers_SupplierId", "PurchaseOrders");
        migrationBuilder.DropForeignKey("FK_StockMovements_Products_ProductId", "StockMovements");

        migrationBuilder.AlterColumn<string>("CompanyName", "AppSettings", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 180);
        migrationBuilder.AlterColumn<string>("CompanyAddress", "AppSettings", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);
        migrationBuilder.AlterColumn<string>("CompanyPhone", "AppSettings", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40, oldNullable: true);
        migrationBuilder.AlterColumn<string>("CompanyEmail", "AppSettings", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 160, oldNullable: true);
        migrationBuilder.AlterColumn<string>("TaxNumber", "AppSettings", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40, oldNullable: true);
        migrationBuilder.AlterColumn<string>("LogoPath", "AppSettings", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 260, oldNullable: true);

        migrationBuilder.AlterColumn<string>("Name", "Categories", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 120);
        migrationBuilder.AlterColumn<string>("Description", "Categories", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);

        migrationBuilder.AlterColumn<string>("Code", "Products", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40);
        migrationBuilder.AlterColumn<string>("Name", "Products", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 160);
        migrationBuilder.AlterColumn<string>("Description", "Products", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);
        migrationBuilder.AlterColumn<decimal>("ReorderPoint", "Products", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 3);
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "Products", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 2);

        migrationBuilder.AlterColumn<string>("OrderNumber", "PurchaseOrders", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 60);
        migrationBuilder.AlterColumn<string>("Notes", "PurchaseOrders", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);

        migrationBuilder.AlterColumn<decimal>("Quantity", "PurchaseOrderLines", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 3);
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "PurchaseOrderLines", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 2);

        migrationBuilder.AlterColumn<string>("DocumentNumber", "StockMovements", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 60);
        migrationBuilder.AlterColumn<decimal>("Quantity", "StockMovements", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 3);
        migrationBuilder.AlterColumn<decimal>("UnitPrice", "StockMovements", "TEXT", nullable: false, oldClrType: typeof(decimal), oldType: "TEXT", oldPrecision: 18, oldScale: 2);
        migrationBuilder.AlterColumn<string>("ReferenceNumber", "StockMovements", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 80, oldNullable: true);
        migrationBuilder.AlterColumn<string>("Notes", "StockMovements", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);

        migrationBuilder.AlterColumn<string>("Code", "Suppliers", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40);
        migrationBuilder.AlterColumn<string>("Name", "Suppliers", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 180);
        migrationBuilder.AlterColumn<string>("ContactName", "Suppliers", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 120, oldNullable: true);
        migrationBuilder.AlterColumn<string>("Phone", "Suppliers", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40, oldNullable: true);
        migrationBuilder.AlterColumn<string>("Email", "Suppliers", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 160, oldNullable: true);
        migrationBuilder.AlterColumn<string>("Address", "Suppliers", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 500, oldNullable: true);
        migrationBuilder.AlterColumn<string>("TaxNumber", "Suppliers", "TEXT", nullable: true, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 40, oldNullable: true);

        migrationBuilder.AlterColumn<string>("Name", "Units", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 80);
        migrationBuilder.AlterColumn<string>("Symbol", "Units", "TEXT", nullable: false, oldClrType: typeof(string), oldType: "TEXT", oldMaxLength: 20);

        AddCascadeForeignKeys(migrationBuilder);
    }

    private static void AddRestrictForeignKeys(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddForeignKey("FK_Products_Categories_CategoryId", "Products", "CategoryId", "Categories", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Products_Units_UnitId", "Products", "UnitId", "Units", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PurchaseOrderLines_Products_ProductId", "PurchaseOrderLines", "ProductId", "Products", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId", "PurchaseOrderLines", "PurchaseOrderId", "PurchaseOrders", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PurchaseOrders_Suppliers_SupplierId", "PurchaseOrders", "SupplierId", "Suppliers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_StockMovements_Products_ProductId", "StockMovements", "ProductId", "Products", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    private static void AddCascadeForeignKeys(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddForeignKey("FK_Products_Categories_CategoryId", "Products", "CategoryId", "Categories", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_Products_Units_UnitId", "Products", "UnitId", "Units", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_PurchaseOrderLines_Products_ProductId", "PurchaseOrderLines", "ProductId", "Products", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId", "PurchaseOrderLines", "PurchaseOrderId", "PurchaseOrders", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_PurchaseOrders_Suppliers_SupplierId", "PurchaseOrders", "SupplierId", "Suppliers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey("FK_StockMovements_Products_ProductId", "StockMovements", "ProductId", "Products", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
    }
}
