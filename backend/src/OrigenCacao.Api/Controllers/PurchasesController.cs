using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/purchases"), Authorize]
public sealed class PurchasesController(IPurchaseService service, IPurchaseReceiptService receipts,
    IEmailService emailService) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<PurchaseDto>>> List([FromQuery] int take = 100, CancellationToken ct = default) => Ok(await service.ListAsync(take, ct));
    [HttpPost] public async Task<ActionResult<PurchaseDto>> Create(CreatePurchaseRequest request, CancellationToken ct) => Ok(await service.CreateAsync(request, ct));
    [HttpPost("preview")] public ActionResult<PurchaseCalculation> Preview(PurchasePreviewRequest request) => Ok(PricingCalculator.CalculatePurchase(request.GrossWeightLbs, request.TareLbs, request.HumidityPercent, request.ShrinkagePercent, request.UnitPrice));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Void(Guid id, CancellationToken ct) => await service.VoidAsync(id, ct) ? NoContent() : NotFound();
    [HttpGet("{id:guid}/receipt")]
    public async Task<IActionResult> Receipt(Guid id, CancellationToken ct)
    {
        var receipt = await receipts.GenerateAsync(id, ct);
        return receipt is null ? NotFound() : File(receipt.Content, "application/pdf", receipt.FileName);
    }

    [HttpPost("{id:guid}/email-receipt")]
    public async Task<ActionResult<EmailSendResult>> EmailReceipt(Guid id, ReceiptEmailRequest request, CancellationToken ct)
    {
        var purchase = await service.GetAsync(id, ct);
        var receipt = await receipts.GenerateAsync(id, ct);
        if (purchase is null || receipt is null) return NotFound();
        var recipient = string.IsNullOrWhiteSpace(request.Email) ? purchase.ProducerEmail : request.Email.Trim();
        if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentException("Ingresa el correo al que deseas enviar el comprobante.");
        await emailService.SendReceiptAsync(recipient, $"Comprobante de compra {purchase.Code}",
            $"Adjuntamos el comprobante de la compra de cacao {purchase.Code} por ${purchase.TotalPaid:N2}.",
            receipt.Content, receipt.FileName, ct);
        return Ok(new EmailSendResult(true, recipient, "Comprobante enviado correctamente."));
    }
}
