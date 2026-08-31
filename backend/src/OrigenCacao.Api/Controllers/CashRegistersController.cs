using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/cash-registers"), Authorize]
public sealed class CashRegistersController(ICashRegisterService service) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<CashRegisterDto>> Current(CancellationToken ct) =>
        await service.GetCurrentAsync(ct) is { } item ? Ok(item) : NoContent();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CashRegisterDto>>> List([FromQuery] int take = 30, CancellationToken ct = default) =>
        Ok(await service.ListAsync(take, ct));

    [HttpPost("open")]
    public async Task<ActionResult<CashRegisterDto>> Open(OpenCashRegisterRequest request, CancellationToken ct) =>
        Ok(await service.OpenAsync(request, ct));

    [HttpPost("{id:guid}/movements")]
    public async Task<ActionResult<CashRegisterDto>> AddMovement(Guid id, AddCashMovementRequest request, CancellationToken ct) =>
        Ok(await service.AddMovementAsync(id, request, ct));

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<CashRegisterDto>> Close(Guid id, CloseCashRegisterRequest request, CancellationToken ct) =>
        Ok(await service.CloseAsync(id, request, ct));
}
