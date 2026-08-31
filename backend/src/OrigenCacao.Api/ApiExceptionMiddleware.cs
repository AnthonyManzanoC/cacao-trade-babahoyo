using System.Net;
using Microsoft.EntityFrameworkCore;

namespace OrigenCacao.Api;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Solicitud cancelada por el cliente: {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando {Method} {Path}", context.Request.Method, context.Request.Path);
            var (status, message) = ex switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                DbUpdateException => (HttpStatusCode.Conflict, "No se pudo guardar el registro. Revisa que los datos no estén duplicados."),
                _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado. Intenta nuevamente.")
            };
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new { title = message, status = (int)status, traceId = context.TraceIdentifier });
        }
    }
}
