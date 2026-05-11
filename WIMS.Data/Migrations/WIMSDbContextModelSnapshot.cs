using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WIMS.Data;

#nullable disable

namespace WIMS.Data.Migrations;

[DbContext(typeof(WIMSDbContext))]
partial class WIMSDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

        modelBuilder.Entity("WIMS.Core.Entities.AppSettings", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("CompanyAddress").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<string>("CompanyEmail").HasMaxLength(160).HasColumnType("TEXT");
            b.Property<string>("CompanyName").IsRequired().HasMaxLength(180).HasColumnType("TEXT");
            b.Property<string>("CompanyPhone").HasMaxLength(40).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("LogoPath").HasMaxLength(260).HasColumnType("TEXT");
            b.Property<string>("TaxNumber").HasMaxLength(40).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.ToTable("AppSettings");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Category", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Name").IsUnique();
            b.ToTable("Categories");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Product", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<int>("CategoryId").HasColumnType("INTEGER");
            b.Property<string>("Code").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            b.Property<decimal>("ReorderPoint").HasPrecision(18, 3).HasColumnType("TEXT");
            b.Property<int>("UnitId").HasColumnType("INTEGER");
            b.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CategoryId");
            b.HasIndex("Code").IsUnique();
            b.HasIndex("UnitId");
            b.ToTable("Products");
        });

        modelBuilder.Entity("WIMS.Core.Entities.PurchaseOrder", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<DateTime?>("ExpectedDeliveryDate").HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Notes").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<DateTime>("OrderDate").HasColumnType("TEXT");
            b.Property<string>("OrderNumber").IsRequired().HasMaxLength(60).HasColumnType("TEXT");
            b.Property<DateTime?>("ReceivedDate").HasColumnType("TEXT");
            b.Property<int>("Status").HasColumnType("INTEGER");
            b.Property<int>("SupplierId").HasColumnType("INTEGER");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("OrderNumber").IsUnique();
            b.HasIndex("SupplierId");
            b.ToTable("PurchaseOrders");
        });

        modelBuilder.Entity("WIMS.Core.Entities.PurchaseOrderLine", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<int>("ProductId").HasColumnType("INTEGER");
            b.Property<int>("PurchaseOrderId").HasColumnType("INTEGER");
            b.Property<decimal>("Quantity").HasPrecision(18, 3).HasColumnType("TEXT");
            b.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("ProductId");
            b.HasIndex("PurchaseOrderId");
            b.ToTable("PurchaseOrderLines");
        });

        modelBuilder.Entity("WIMS.Core.Entities.StockMovement", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("DocumentNumber").IsRequired().HasMaxLength(60).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<DateTime>("MovementDate").HasColumnType("TEXT");
            b.Property<string>("Notes").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<int>("ProductId").HasColumnType("INTEGER");
            b.Property<decimal>("Quantity").HasPrecision(18, 3).HasColumnType("TEXT");
            b.Property<string>("ReferenceNumber").HasMaxLength(80).HasColumnType("TEXT");
            b.Property<int>("Type").HasColumnType("INTEGER");
            b.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("DocumentNumber");
            b.HasIndex("ProductId");
            b.ToTable("StockMovements");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Supplier", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("Address").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<string>("Code").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<string>("ContactName").HasMaxLength(120).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Email").HasMaxLength(160).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(180).HasColumnType("TEXT");
            b.Property<string>("Phone").HasMaxLength(40).HasColumnType("TEXT");
            b.Property<string>("TaxNumber").HasMaxLength(40).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Code").IsUnique();
            b.HasIndex("Email");
            b.ToTable("Suppliers");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Unit", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            b.Property<string>("Symbol").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Symbol").IsUnique();
            b.ToTable("Units");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Product", b =>
        {
            b.HasOne("WIMS.Core.Entities.Category", "Category").WithMany("Products").HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.HasOne("WIMS.Core.Entities.Unit", "Unit").WithMany("Products").HasForeignKey("UnitId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Category");
            b.Navigation("Unit");
        });

        modelBuilder.Entity("WIMS.Core.Entities.PurchaseOrder", b =>
        {
            b.HasOne("WIMS.Core.Entities.Supplier", "Supplier").WithMany("PurchaseOrders").HasForeignKey("SupplierId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Supplier");
        });

        modelBuilder.Entity("WIMS.Core.Entities.PurchaseOrderLine", b =>
        {
            b.HasOne("WIMS.Core.Entities.Product", "Product").WithMany("PurchaseOrderLines").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.HasOne("WIMS.Core.Entities.PurchaseOrder", "PurchaseOrder").WithMany("Lines").HasForeignKey("PurchaseOrderId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Product");
            b.Navigation("PurchaseOrder");
        });

        modelBuilder.Entity("WIMS.Core.Entities.StockMovement", b =>
        {
            b.HasOne("WIMS.Core.Entities.Product", "Product").WithMany("StockMovements").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Product");
        });

        modelBuilder.Entity("WIMS.Core.Entities.Category", b => b.Navigation("Products"));
        modelBuilder.Entity("WIMS.Core.Entities.Product", b =>
        {
            b.Navigation("PurchaseOrderLines");
            b.Navigation("StockMovements");
        });
        modelBuilder.Entity("WIMS.Core.Entities.PurchaseOrder", b => b.Navigation("Lines"));
        modelBuilder.Entity("WIMS.Core.Entities.Supplier", b => b.Navigation("PurchaseOrders"));
        modelBuilder.Entity("WIMS.Core.Entities.Unit", b => b.Navigation("Products"));
    }
}
