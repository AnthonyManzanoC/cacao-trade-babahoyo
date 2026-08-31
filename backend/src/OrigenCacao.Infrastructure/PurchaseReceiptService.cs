using Microsoft.EntityFrameworkCore;
using OrigenCacao.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrigenCacao.Infrastructure;

public sealed class PurchaseReceiptService(AppDbContext db) : IPurchaseReceiptService
{
    public async Task<PurchaseReceiptResult?> GenerateAsync(Guid purchaseId, CancellationToken ct)
    {
        var purchase = await db.Purchases.AsNoTracking().Include(x => x.Producer)
            .SingleOrDefaultAsync(x => x.Id == purchaseId, ct);
        if (purchase is null) return null;
        var settings = await db.BusinessSettings.AsNoTracking().SingleAsync(x => x.Id == 1, ct);

        QuestPDF.Settings.License = LicenseType.Community;
        var pdf = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor("#263126"));
                page.Header().Column(header =>
                {
                    header.Item().Text(settings.BusinessName).FontSize(20).SemiBold().FontColor("#173B2C");
                    header.Item().Text("COMPROBANTE DE COMPRA DE CACAO").FontSize(9).SemiBold().FontColor("#B86B2B");
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor("#DDE5DD");
                });
                page.Content().PaddingVertical(18).Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Comprobante").FontColor(Colors.Grey.Medium);
                            left.Item().Text(purchase.Code).FontSize(13).SemiBold();
                        });
                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Item().AlignRight().Text("Fecha").FontColor(Colors.Grey.Medium);
                            right.Item().AlignRight().Text(purchase.PurchasedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        });
                    });
                    column.Item().Background("#F2F6F2").Padding(12).Column(box =>
                    {
                        box.Item().Text("PRODUCTOR").FontSize(8).SemiBold().FontColor("#52705E");
                        box.Item().PaddingTop(3).Text(purchase.Producer.FullName).FontSize(12).SemiBold();
                        box.Item().Text($"Cédula/RUC: {purchase.Producer.DocumentNumber}");
                        box.Item().Text($"Finca: {purchase.Producer.FarmLocation}");
                    });
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });
                        void Row(string label, string value, bool strong = false)
                        {
                            table.Cell().BorderBottom(1).BorderColor("#E7ECE7").PaddingVertical(6).Text(label);
                            var text = table.Cell().BorderBottom(1).BorderColor("#E7ECE7").PaddingVertical(6).AlignRight().Text(value);
                            if (strong) text.SemiBold();
                        }
                        Row("Tipo / estado", $"{purchase.Variety} · {purchase.State}");
                        Row("Peso bruto", $"{purchase.GrossWeightLbs:N2} lb");
                        Row("Tara", $"{purchase.TareLbs:N2} lb");
                        Row("Peso neto", $"{purchase.NetWeightLbs:N2} lb");
                        Row("Humedad", $"{purchase.HumidityPercent:N2}%");
                        Row("Merma", $"{purchase.ShrinkagePercent:N2}%");
                        Row("Peso pagable", $"{purchase.PayableQuintals:N4} qq", true);
                        Row("Precio por quintal", $"${purchase.UnitPrice:N2}");
                        Row("Método de pago", purchase.PaymentMethod.ToString());
                    });
                    column.Item().Background("#173B2C").Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL PAGADO").FontColor(Colors.White).SemiBold();
                        row.RelativeItem().AlignRight().Text($"${purchase.TotalPaid:N2}").FontSize(18).Bold().FontColor(Colors.White);
                    });
                    if (!string.IsNullOrWhiteSpace(purchase.Notes))
                        column.Item().Text($"Observación: {purchase.Notes}").FontColor(Colors.Grey.Darken1);
                    if (purchase.IsVoided)
                        column.Item().AlignCenter().Text("COMPROBANTE ANULADO").Bold().FontColor(Colors.Red.Medium);
                    column.Item().PaddingTop(12).Row(row =>
                    {
                        row.RelativeItem().Column(signature =>
                        {
                            signature.Item().PaddingTop(24).LineHorizontal(1).LineColor("#8A998F");
                            signature.Item().AlignCenter().Text("Firma del productor").FontSize(8);
                        });
                        row.ConstantItem(24);
                        row.RelativeItem().Column(signature =>
                        {
                            signature.Item().PaddingTop(24).LineHorizontal(1).LineColor("#8A998F");
                            signature.Item().AlignCenter().Text("Firma del comprador").FontSize(8);
                        });
                    });
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"{settings.Location} · {settings.ContactWhatsApp} · ");
                    text.CurrentPageNumber(); text.Span("/"); text.TotalPages();
                });
            });
        }).GeneratePdf();
        return new PurchaseReceiptResult(pdf, $"comprobante-{purchase.Code}.pdf");
    }
}
