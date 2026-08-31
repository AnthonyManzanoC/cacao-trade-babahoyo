using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;

namespace OrigenCacao.Api.Controllers;

[ApiController, Route("api/processing"), Authorize]
public sealed class ProcessingController(IProcessingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProcessingBatchDto>>> List([FromQuery] int take = 100, CancellationToken ct = default) =>
        Ok(await service.ListAsync(take, ct));

    [HttpPost]
    public async Task<ActionResult<ProcessingBatchDto>> Create(CreateProcessingBatchRequest request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ProcessingBatchDto>> Complete(Guid id, CompleteProcessingBatchRequest request, CancellationToken ct) =>
        Ok(await service.CompleteAsync(id, request, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProcessingBatchDto>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await service.CancelAsync(id, ct));
}
