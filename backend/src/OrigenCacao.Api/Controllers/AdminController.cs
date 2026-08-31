using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api"), Authorize]
public sealed class AdminController(IInventoryService inventory, ISettingsService settings,
    IDashboardService dashboard, ICocoaPriceUpdater updater) : ControllerBase
{
    [HttpGet("inventory")] public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> Inventory(CancellationToken ct) => Ok(await inventory.GetAsync(ct));
    [HttpGet("inventory/lots")] public async Task<ActionResult<IReadOnlyList<InventoryLotDto>>> Lots(
        [FromQuery] OrigenCacao.Domain.CocoaState? state, CancellationToken ct) => Ok(await inventory.GetLotsAsync(state, ct));
    [HttpGet("dashboard")] public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken ct) => Ok(await dashboard.GetAsync(ct));
    [HttpGet("settings")] public async Task<ActionResult<SettingsDto>> Settings(CancellationToken ct) => Ok(await settings.GetAsync(ct));
    [HttpPut("settings")] public async Task<ActionResult<SettingsDto>> UpdateSettings(UpdateSettingsRequest request, CancellationToken ct) => Ok(await settings.UpdateAsync(request, ct));
    [HttpPost("settings/refresh-price")] public async Task<ActionResult<PriceUpdateResult>> Refresh(CancellationToken ct) => Ok(await updater.RefreshAsync(ct));
}
