using Microsoft.EntityFrameworkCore;
using OrigenCacao.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrigenCacao.Infrastructure;

public sealed class SaleReceiptService(AppDbContext db) : ISaleReceiptService
{
    public async Task<PurchaseReceiptResult?> GenerateAsync(Guid saleId, CancellationToken ct)
    {
        var sale = await db.Sales.AsNoTracking().SingleOrDefaultAsync(x => x.Id == saleId, ct);
        if (sale is null) return null;
        var settings = await db.BusinessSettings.AsNoTracking().SingleAsync(x => x.Id == 1, ct);
        QuestPDF.Settings.License = LicenseType.Community;
        var pdf = Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(42);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor("#263126"));
            page.Header().Column(header =>
            {
                header.Item().Text(settings.BusinessName).FontSize(22).SemiBold().FontColor("#173B2C");
                header.Item().Text("COMPROBANTE DE VENTA DE CACAO").FontSize(10).SemiBold().FontColor("#B86B2B");
                header.Item().PaddingTop(8).LineHorizontal(1).LineColor("#DDE5DD");
            });
            page.Content().PaddingVertical(20).Column(column =>
            {
                column.Spacing(14);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(x => { x.Item().Text("Comprobante").FontColor(Colors.Grey.Medium); x.Item().Text(sale.Code).FontSize(14).SemiBold(); });
                    row.RelativeItem().AlignRight().Column(x => { x.Item().AlignRight().Text("Fecha").FontColor(Colors.Grey.Medium); x.Item().AlignRight().Text(sale.SoldAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm")); });
                });
                column.Item().Background("#F2F6F2").Padding(14).Column(box =>
                {
                    box.Item().Text("CLIENTE").FontSize(8).SemiBold().FontColor("#52705E");
                    box.Item().PaddingTop(3).Text(sale.CustomerName).FontSize(13).SemiBold();
                    box.Item().Text($"RUC/Identificación: {sale.CustomerTaxId ?? "—"}");
                    if (!string.IsNullOrWhiteSpace(sale.CustomerEmail)) box.Item().Text($"Correo: {sale.CustomerEmail}");
                });
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });
                    void Row(string label, string value, bool strong = false)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#E7ECE7").PaddingVertical(7).Text(label);
                        var text = table.Cell().BorderBottom(1).BorderColor("#E7ECE7").PaddingVertical(7).AlignRight().Text(value);
                        if (strong) text.SemiBold();
                    }
                    Row("Producto", $"Cacao {sale.Variety} · {sale.State}");
                    Row("Cantidad", $"{sale.QuantityQuintals:N4} qq", true);
                    Row("Precio por quintal", $"${sale.UnitPrice:N2}");
                    Row("Método de pago", sale.PaymentMethod.ToString());
                    Row("Costo por quintal", $"${sale.CostBasisPerQuintal:N2}");
                    Row("Utilidad bruta", $"${sale.GrossProfit:N2}");
                });
                column.Item().Background("#173B2C").Padding(15).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL DE VENTA").FontColor(Colors.White).SemiBold();
                    row.RelativeItem().AlignRight().Text($"${sale.Total:N2}").FontSize(20).Bold().FontColor(Colors.White);
                });
                if (!string.IsNullOrWhiteSpace(sale.Notes)) column.Item().Text($"Observación: {sale.Notes}").FontColor(Colors.Grey.Darken1);
                if (sale.IsVoided) column.Item().AlignCenter().Text("COMPROBANTE ANULADO").Bold().FontColor(Colors.Red.Medium);
                column.Item().PaddingTop(24).Row(row =>
                {
                    row.RelativeItem().Column(x => { x.Item().LineHorizontal(1).LineColor("#8A998F"); x.Item().AlignCenter().Text("Firma del cliente").FontSize(8); });
                    row.ConstantItem(28);
                    row.RelativeItem().Column(x => { x.Item().LineHorizontal(1).LineColor("#8A998F"); x.Item().AlignCenter().Text("Firma del vendedor").FontSize(8); });
                });
            });
            page.Footer().AlignCenter().Text(text => { text.Span($"{settings.ContactAddress} · {settings.ContactPhone} · "); text.CurrentPageNumber(); text.Span("/"); text.TotalPages(); });
        })).GeneratePdf();
        return new PurchaseReceiptResult(pdf, $"comprobante-venta-{sale.Code}.pdf");
    }
}
