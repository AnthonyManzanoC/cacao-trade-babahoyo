using Microsoft.EntityFrameworkCore;
using OrigenCacao.Application;
using OrigenCacao.Domain;
using OrigenCacao.Infrastructure;

namespace OrigenCacao.Tests;

public sealed class LiveWorkflowTests
{
    [Fact]
    public async Task Supabase_workflow_keeps_lot_cash_processing_sale_and_receipt_consistent()
    {
        var connection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options;
        await using var db = new AppDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var producerService = new ProducerService(db);
        var cashService = new CashRegisterService(db);
        var purchaseService = new PurchaseService(db);
        var processingService = new ProcessingService(db);
        var saleService = new SaleService(db);
        var inventoryService = new InventoryService(db);

        var producer = await producerService.CreateAsync(new UpsertProducerRequest("Productor de prueba transaccional",
            $"TEST-{Guid.NewGuid():N}"[..20], "Finca de prueba", "0990000000", "productor.prueba@example.com", PaymentMethod.Efectivo,
            "Este registro vive únicamente dentro de una transacción revertida."), default);
        var cash = await cashService.OpenAsync(new OpenCashRegisterRequest(new DateOnly(2099, 12, 30), 10_000m, "Prueba reversible"), default);
        PurchaseDto purchase;
        try
        {
            purchase = await purchaseService.CreateAsync(new CreatePurchaseRequest(producer.Id, CocoaVariety.Otro,
                CocoaState.Baba, 500m, 0m, 0m, 0m, 100m, PaymentMethod.Efectivo, DateTime.UtcNow,
                "Compra reversible para validar el sistema"), default);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var details = string.Join(", ", ex.Entries.Select(x => $"{x.Metadata.ClrType.Name}:{x.State}:{x.Property("Id").CurrentValue}"));
            throw new InvalidOperationException($"Entidades con conflicto: {details}", ex);
        }

        var lotsAfterPurchase = await inventoryService.GetLotsAsync(CocoaState.Baba, default);
        Assert.Contains(lotsAfterPurchase, x => x.PurchaseId == purchase.Id && x.AvailableQuantityQuintals == 5m);

        var processing = await processingService.CreateAsync(new CreateProcessingBatchRequest(CocoaVariety.Otro,
            1m, 40m, DateTime.UtcNow, "Secado reversible"), default);
        var completed = await processingService.CompleteAsync(processing.Id,
            new CompleteProcessingBatchRequest(0.4m, DateTime.UtcNow, "Salida seca reversible"), default);
        Assert.Equal(40m, completed.ActualDryYieldPercent);
        Assert.Contains(await inventoryService.GetLotsAsync(CocoaState.Seco, default),
            x => x.ProcessingBatchId == processing.Id && x.AvailableQuantityQuintals == 0.4m && x.UnitCost == 250m);

        var canceledProcessing = await processingService.CreateAsync(new CreateProcessingBatchRequest(CocoaVariety.Otro,
            0.5m, 40m, DateTime.UtcNow, "Secado a cancelar"), default);
        await processingService.CancelAsync(canceledProcessing.Id, default);

        var sale = await saleService.CreateAsync(new CreateSaleRequest("Exportadora de prueba", "TEST-RUC", "cliente.prueba@example.com",
            CocoaVariety.Otro, CocoaState.Seco, 0.2m, 350m, PaymentMethod.Transferencia, DateTime.UtcNow,
            "Venta reversible"), default);
        Assert.Equal(20m, sale.GrossProfit);
        var saleReceipt = await new SaleReceiptService(db).GenerateAsync(sale.Id, default);
        Assert.NotNull(saleReceipt);
        Assert.True(saleReceipt!.Content.Length > 2_000);
        await saleService.VoidAsync(sale.Id, default);

        var receipt = await new PurchaseReceiptService(db).GenerateAsync(purchase.Id, default);
        Assert.NotNull(receipt);
        Assert.True(receipt!.Content.Length > 2_000);
        var output = Environment.GetEnvironmentVariable("RECEIPT_OUTPUT_PATH");
        if (!string.IsNullOrWhiteSpace(output))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            await File.WriteAllBytesAsync(output, receipt.Content);
        }

        var current = await cashService.GetCurrentAsync(default);
        Assert.NotNull(current);
        Assert.Equal(9_500m, current!.ExpectedBalance);

        await transaction.RollbackAsync();
        db.ChangeTracker.Clear();
        Assert.False(await db.Producers.AnyAsync(x => x.Id == producer.Id));
        Assert.False(await db.CashRegisters.AnyAsync(x => x.Id == cash.Id));
    }
}
