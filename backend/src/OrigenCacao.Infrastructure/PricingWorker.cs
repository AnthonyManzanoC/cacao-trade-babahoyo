using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

public sealed class ApiNinjasOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public int RefreshIntervalMinutes { get; set; } = 60;
}

public sealed class CocoaPriceUpdater(AppDbContext db, IHttpClientFactory httpClientFactory,
    IOptions<ApiNinjasOptions> options, ILogger<CocoaPriceUpdater> logger) : ICocoaPriceUpdater
{
    public async Task<PriceUpdateResult> RefreshAsync(CancellationToken ct)
    {
        var settings = await db.BusinessSettings.SingleAsync(x => x.Id == 1, ct);
        if (settings.UseManualPrice) return new(false, null, settings.CurrentDryPricePerQuintal, "El precio manual está activo.");
        var errors = new List<string>();
        try
        {
            var quote = await GetApiNinjasQuoteAsync(ct);
            return await ApplyQuoteAsync(settings, quote, "API Ninjas · ICE Cocoa", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "API Ninjas no disponible; se intentará Yahoo Finance");
            errors.Add($"API Ninjas: {ex.Message}");
        }

        try
        {
            var quote = await GetYahooQuoteAsync(ct);
            return await ApplyQuoteAsync(settings, quote, "Yahoo Finance · ICE Cocoa (CC=F)", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Yahoo Finance no disponible; se usará el precio manual de respaldo");
            errors.Add($"Yahoo Finance: {ex.Message}");
        }

        settings.ApiLastError = $"Fuentes automáticas no disponibles. {string.Join(" | ", errors)}";
        if (settings.ManualDryPricePerQuintal is > 0)
        {
            settings.CurrentDryPricePerQuintal = decimal.Round(settings.ManualDryPricePerQuintal.Value, 2);
            settings.CurrentWetPricePerQuintal = decimal.Round(settings.CurrentDryPricePerQuintal * settings.WetPriceFactor, 2);
            settings.CurrentPriceUpdatedAtUtc = DateTime.UtcNow;
            settings.PriceSource = "Precio manual de respaldo";
            settings.UpdatedAtUtc = DateTime.UtcNow;
            db.PriceHistory.Add(new PriceHistory { MarketPricePerMetricTon = settings.CurrentMarketPricePerMetricTon,
                DryPricePerQuintal = settings.CurrentDryPricePerQuintal, WetPricePerQuintal = settings.CurrentWetPricePerQuintal,
                MarginPerQuintal = settings.MarginPerQuintal, Source = settings.PriceSource, QuotedAtUtc = settings.CurrentPriceUpdatedAtUtc });
            await db.SaveChangesAsync(ct);
            return new(false, null, settings.CurrentDryPricePerQuintal,
                "API Ninjas y Yahoo no respondieron; se aplicó el precio manual de respaldo.");
        }

        await db.SaveChangesAsync(ct);
        return new(false, null, settings.CurrentDryPricePerQuintal,
            "No hubo fuente automática ni precio manual; se conserva el último precio válido.");
    }

    private async Task<MarketQuote> GetApiNinjasQuoteAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
            throw new InvalidOperationException("clave no configurada");
        var client = httpClientFactory.CreateClient("ApiNinjas");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/commodityprice?name=cocoa&currency=USD&unit=metric_ton");
        request.Headers.Add("X-Api-Key", options.Value.ApiKey);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            string? apiMessage = null;
            try { apiMessage = System.Text.Json.JsonSerializer.Deserialize<ApiNinjasError>(errorBody)?.Error; }
            catch (System.Text.Json.JsonException) { }
            throw new InvalidOperationException(apiMessage ?? $"respondió {(int)response.StatusCode}");
        }
        var quote = await response.Content.ReadFromJsonAsync<ApiNinjasQuote>(cancellationToken: ct)
            ?? throw new InvalidOperationException("respuesta vacía");
        if (quote.Price <= 0) throw new InvalidOperationException("precio inválido");
        return new MarketQuote(quote.Price,
            quote.Updated > 0 ? DateTimeOffset.FromUnixTimeSeconds(quote.Updated).UtcDateTime : DateTime.UtcNow);
    }

    private async Task<MarketQuote> GetYahooQuoteAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("YahooFinance");
        var response = await client.GetFromJsonAsync<YahooResponse>("/v8/finance/chart/CC=F?interval=1d&range=5d", ct)
            ?? throw new InvalidOperationException("respuesta vacía");
        var meta = response.Chart.Result?.FirstOrDefault()?.Meta
            ?? throw new InvalidOperationException(response.Chart.Error?.Description ?? "cotización no encontrada");
        if (meta.RegularMarketPrice is null or <= 0) throw new InvalidOperationException("precio inválido");
        var quotedAt = meta.RegularMarketTime is > 0
            ? DateTimeOffset.FromUnixTimeSeconds(meta.RegularMarketTime.Value).UtcDateTime : DateTime.UtcNow;
        return new MarketQuote(meta.RegularMarketPrice.Value, quotedAt);
    }

    private async Task<PriceUpdateResult> ApplyQuoteAsync(BusinessSettings settings, MarketQuote quote,
        string source, CancellationToken ct)
    {
        var dry = PricingCalculator.CalculateDryPrice(quote.Price, settings.MarginPerQuintal);
        var wet = decimal.Round(dry * settings.WetPriceFactor, 2);
        settings.CurrentMarketPricePerMetricTon = quote.Price; settings.CurrentDryPricePerQuintal = dry;
        settings.CurrentWetPricePerQuintal = wet; settings.CurrentPriceUpdatedAtUtc = quote.QuotedAtUtc;
        settings.ApiLastSuccessAtUtc = DateTime.UtcNow; settings.ApiLastError = null; settings.PriceSource = source;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        db.PriceHistory.Add(new PriceHistory { MarketPricePerMetricTon = quote.Price, DryPricePerQuintal = dry,
            WetPricePerQuintal = wet, MarginPerQuintal = settings.MarginPerQuintal, Source = source, QuotedAtUtc = quote.QuotedAtUtc });
        await db.SaveChangesAsync(ct);
        return new(true, quote.Price, dry, $"Precio actualizado desde {source}.");
    }

    private sealed record ApiNinjasQuote([property: JsonPropertyName("price")] decimal Price,
        [property: JsonPropertyName("updated")] long Updated);
    private sealed record ApiNinjasError([property: JsonPropertyName("error")] string Error);
    private sealed record MarketQuote(decimal Price, DateTime QuotedAtUtc);
    private sealed record YahooResponse([property: JsonPropertyName("chart")] YahooChart Chart);
    private sealed record YahooChart([property: JsonPropertyName("result")] YahooResult[]? Result,
        [property: JsonPropertyName("error")] YahooError? Error);
    private sealed record YahooResult([property: JsonPropertyName("meta")] YahooMeta Meta);
    private sealed record YahooMeta([property: JsonPropertyName("regularMarketPrice")] decimal? RegularMarketPrice,
        [property: JsonPropertyName("regularMarketTime")] long? RegularMarketTime);
    private sealed record YahooError([property: JsonPropertyName("description")] string Description);
}

public sealed class CocoaPriceWorker(IServiceScopeFactory scopeFactory, IOptions<ApiNinjasOptions> options,
    ILogger<CocoaPriceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Refresh(stoppingToken);
        var minutes = Math.Clamp(options.Value.RefreshIntervalMinutes, 5, 1440);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        while (await timer.WaitForNextTickAsync(stoppingToken)) await Refresh(stoppingToken);
    }

    private async Task Refresh(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<ICocoaPriceUpdater>().RefreshAsync(ct);
            logger.LogInformation("Motor de precios: {Message}", result.Message);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) { logger.LogError(ex, "Error inesperado en el motor de precios"); }
    }
}
