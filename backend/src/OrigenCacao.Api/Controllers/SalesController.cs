using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/sales"), Authorize]
public sealed class SalesController(ISaleService service, ISaleReceiptService receipts,
    IEmailService emailService) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<SaleDto>>> List([FromQuery] int take = 100, CancellationToken ct = default) => Ok(await service.ListAsync(take, ct));
    [HttpPost] public async Task<ActionResult<SaleDto>> Create(CreateSaleRequest request, CancellationToken ct) => Ok(await service.CreateAsync(request, ct));
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
        var sale = await service.GetAsync(id, ct);
        var receipt = await receipts.GenerateAsync(id, ct);
        if (sale is null || receipt is null) return NotFound();
        var recipient = string.IsNullOrWhiteSpace(request.Email) ? sale.CustomerEmail : request.Email.Trim();
        if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentException("Ingresa el correo al que deseas enviar el comprobante.");
        await emailService.SendReceiptAsync(recipient, $"Comprobante de venta {sale.Code}",
            $"Adjuntamos el comprobante de la venta de cacao {sale.Code} por ${sale.Total:N2}.",
            receipt.Content, receipt.FileName, ct);
        return Ok(new EmailSendResult(true, recipient, "Comprobante enviado correctamente."));
    }
}
