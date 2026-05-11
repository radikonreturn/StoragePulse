using ClosedXML.Excel;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.Services;

public class ReportService : IReportService
{
    public void ExportStockValuation(string filePath, IEnumerable<ProductDto> products)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Stok Değerleme");
        ws.Cell(1, 1).Value = "Kod";
        ws.Cell(1, 2).Value = "Ürün Adı";
        ws.Cell(1, 3).Value = "Kategori";
        ws.Cell(1, 4).Value = "Birim Fiyat";
        ws.Cell(1, 5).Value = "Mevcut Stok";
        ws.Cell(1, 6).Value = "Toplam Değer";

        var row = 2;
        foreach (var p in products)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.CategoryName;
            ws.Cell(row, 4).Value = p.UnitPrice;
            ws.Cell(row, 5).Value = p.CurrentStock;
            ws.Cell(row, 6).Value = p.CurrentStock * p.UnitPrice;
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportMovementHistory(string filePath, IEnumerable<StockMovementDto> movements)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Hareket Geçmişi");
        ws.Cell(1, 1).Value = "Belge No";
        ws.Cell(1, 2).Value = "Tarih";
        ws.Cell(1, 3).Value = "Tür";
        ws.Cell(1, 4).Value = "Ürün Kodu";
        ws.Cell(1, 5).Value = "Ürün Adı";
        ws.Cell(1, 6).Value = "Miktar";
        ws.Cell(1, 7).Value = "Birim Fiyat";
        ws.Cell(1, 8).Value = "Toplam";
        ws.Cell(1, 9).Value = "Referans";

        var row = 2;
        foreach (var m in movements)
        {
            ws.Cell(row, 1).Value = m.DocumentNumber;
            ws.Cell(row, 2).Value = m.MovementDate;
            ws.Cell(row, 3).Value = m.Type.ToString();
            ws.Cell(row, 4).Value = m.ProductCode;
            ws.Cell(row, 5).Value = m.ProductName;
            ws.Cell(row, 6).Value = m.Quantity;
            ws.Cell(row, 7).Value = m.UnitPrice;
            ws.Cell(row, 8).Value = m.TotalPrice;
            ws.Cell(row, 9).Value = m.ReferenceNumber;
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportLowStock(string filePath, IEnumerable<ProductDto> products)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Düşük Stok");
        ws.Cell(1, 1).Value = "Kod";
        ws.Cell(1, 2).Value = "Ürün Adı";
        ws.Cell(1, 3).Value = "Mevcut Stok";
        ws.Cell(1, 4).Value = "Tekrar Sipariş Noktası";
        ws.Cell(1, 5).Value = "Durum";

        var row = 2;
        foreach (var p in products)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.CurrentStock;
            ws.Cell(row, 4).Value = p.ReorderPoint;
            ws.Cell(row, 5).Value = p.StockStatus.ToString();
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }
}
