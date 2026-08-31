using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/public"), AllowAnonymous]
public sealed class PublicController(ISettingsService service) : ControllerBase
{
    [HttpGet("price")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<PublicPriceDto>> Price(CancellationToken ct) => Ok(await service.GetPublicPriceAsync(ct));

    [HttpGet("price/history")]
    public async Task<ActionResult<IReadOnlyList<PricePointDto>>> History([FromQuery] int days = 7, CancellationToken ct = default) =>
        Ok(await service.GetHistoryAsync(days, ct));
}
