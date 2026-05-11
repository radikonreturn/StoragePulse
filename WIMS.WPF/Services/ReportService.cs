using ClosedXML.Excel;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.Services;

public class ReportService : IReportService
{
    private void FormatWorksheet(IXLWorksheet ws, string title, int headerRowIndex, int columnCount)
    {
        // Add Title
        ws.Cell(1, 1).Value = title;
        var titleRange = ws.Range(1, 1, 1, columnCount);
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 14;
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Add Timestamp
        ws.Cell(2, 1).Value = $"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";
        var dateRange = ws.Range(2, 1, 2, columnCount);
        dateRange.Merge();
        dateRange.Style.Font.Italic = true;
        dateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        // Format Headers
        var headerRange = ws.Range(headerRowIndex, 1, headerRowIndex, columnCount);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    public void ExportStockValuation(string filePath, IEnumerable<ProductDto> products)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Stok Değerleme");
        
        int headerRow = 4;
        ws.Cell(headerRow, 1).Value = "Kod";
        ws.Cell(headerRow, 2).Value = "Ürün Adı";
        ws.Cell(headerRow, 3).Value = "Kategori";
        ws.Cell(headerRow, 4).Value = "Birim Fiyat";
        ws.Cell(headerRow, 5).Value = "Mevcut Stok";
        ws.Cell(headerRow, 6).Value = "Toplam Değer";

        FormatWorksheet(ws, "Stok Değerleme Raporu", headerRow, 6);

        var row = headerRow + 1;
        foreach (var p in products)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.CategoryName;
            
            ws.Cell(row, 4).Value = p.UnitPrice;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            
            ws.Cell(row, 5).Value = p.CurrentStock;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            
            ws.Cell(row, 6).Value = p.CurrentStock * p.UnitPrice;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₺";
            row++;
        }

        // Add Totals
        var totalRow = row + 1;
        ws.Cell(totalRow, 5).Value = "Genel Toplam:";
        ws.Cell(totalRow, 5).Style.Font.Bold = true;
        
        ws.Cell(totalRow, 6).FormulaA1 = $"SUM(F{headerRow + 1}:F{row - 1})";
        ws.Cell(totalRow, 6).Style.Font.Bold = true;
        ws.Cell(totalRow, 6).Style.NumberFormat.Format = "#,##0.00 ₺";

        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportMovementHistory(string filePath, IEnumerable<StockMovementDto> movements)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Hareket Geçmişi");
        
        int headerRow = 4;
        ws.Cell(headerRow, 1).Value = "Belge No";
        ws.Cell(headerRow, 2).Value = "Tarih";
        ws.Cell(headerRow, 3).Value = "Tür";
        ws.Cell(headerRow, 4).Value = "Ürün Kodu";
        ws.Cell(headerRow, 5).Value = "Ürün Adı";
        ws.Cell(headerRow, 6).Value = "Miktar";
        ws.Cell(headerRow, 7).Value = "Birim Fiyat";
        ws.Cell(headerRow, 8).Value = "Toplam";
        ws.Cell(headerRow, 9).Value = "Referans";

        FormatWorksheet(ws, "Stok Hareketleri Geçmişi", headerRow, 9);

        var row = headerRow + 1;
        foreach (var m in movements)
        {
            ws.Cell(row, 1).Value = m.DocumentNumber;
            ws.Cell(row, 2).Value = m.MovementDate;
            ws.Cell(row, 2).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            
            ws.Cell(row, 3).Value = m.Type.ToString();
            ws.Cell(row, 4).Value = m.ProductCode;
            ws.Cell(row, 5).Value = m.ProductName;
            
            ws.Cell(row, 6).Value = m.Quantity;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            
            ws.Cell(row, 7).Value = m.UnitPrice;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00 ₺";
            
            ws.Cell(row, 8).Value = m.TotalPrice;
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 ₺";
            
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
        
        int headerRow = 4;
        ws.Cell(headerRow, 1).Value = "Kod";
        ws.Cell(headerRow, 2).Value = "Ürün Adı";
        ws.Cell(headerRow, 3).Value = "Mevcut Stok";
        ws.Cell(headerRow, 4).Value = "Tekrar Sipariş Noktası";
        ws.Cell(headerRow, 5).Value = "Durum";

        FormatWorksheet(ws, "Kritik ve Düşük Stok Raporu", headerRow, 5);

        var row = headerRow + 1;
        foreach (var p in products)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.Name;
            
            ws.Cell(row, 3).Value = p.CurrentStock;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            
            ws.Cell(row, 4).Value = p.ReorderPoint;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            
            ws.Cell(row, 5).Value = p.StockStatus.ToString();
            
            // Highlight critical items
            if (p.StockStatus == WIMS.Core.Enums.StockStatus.OutOfStock || p.StockStatus == WIMS.Core.Enums.StockStatus.Critical)
            {
                ws.Range(row, 1, row, 5).Style.Font.FontColor = XLColor.DarkRed;
                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.MistyRose;
            }
            
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }
}
