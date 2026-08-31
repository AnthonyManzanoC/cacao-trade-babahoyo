using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/producers"), Authorize]
public sealed class ProducersController(IProducerService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<ProducerDto>>> List([FromQuery] string? search, CancellationToken ct) => Ok(await service.ListAsync(search, ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<ProducerDto>> Get(Guid id, CancellationToken ct) => (await service.GetAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    [HttpPost] public async Task<ActionResult<ProducerDto>> Create(UpsertProducerRequest request, CancellationToken ct) { var result = await service.CreateAsync(request, ct); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPut("{id:guid}")] public async Task<ActionResult<ProducerDto>> Update(Guid id, UpsertProducerRequest request, CancellationToken ct) => (await service.UpdateAsync(id, request, ct)) is { } result ? Ok(result) : NotFound();
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
