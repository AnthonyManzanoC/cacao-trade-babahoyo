using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Api.Controllers;

[ApiController]
public sealed class PublicContentController(IPublicContentService service) : ControllerBase
{
    [AllowAnonymous, HttpGet("api/public-content")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<IReadOnlyList<PublicContentDto>>> PublicList(
        [FromQuery] PublicContentSection? section, CancellationToken ct) => Ok(await service.ListPublicAsync(section, ct));

    [Authorize, HttpGet("api/admin/public-content")]
    public async Task<ActionResult<IReadOnlyList<PublicContentDto>>> AdminList(CancellationToken ct) =>
        Ok(await service.ListAdminAsync(ct));

    [Authorize, HttpGet("api/admin/public-content/{id:guid}")]
    public async Task<ActionResult<PublicContentDto>> Get(Guid id, CancellationToken ct) =>
        await service.GetAsync(id, ct) is { } item ? Ok(item) : NotFound();

    [Authorize, HttpPost("api/admin/public-content")]
    public async Task<ActionResult<PublicContentDto>> Create(UpsertPublicContentRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return Created($"/api/admin/public-content/{created.Id}", created);
    }

    [Authorize, HttpPut("api/admin/public-content/{id:guid}")]
    public async Task<ActionResult<PublicContentDto>> Update(Guid id, UpsertPublicContentRequest request, CancellationToken ct) =>
        await service.UpdateAsync(id, request, ct) is { } item ? Ok(item) : NotFound();

    [Authorize, HttpDelete("api/admin/public-content/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
