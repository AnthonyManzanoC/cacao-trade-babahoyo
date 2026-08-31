namespace OrigenCacao.Domain;

public static class PricingCalculator
{
    public const decimal PoundsPerQuintal = 100m;
    public const decimal QuintalsPerMetricTon = 22.046m;

    public static decimal CalculateDryPrice(decimal marketPricePerMetricTon, decimal marginPerQuintal)
    {
        if (marketPricePerMetricTon < 0) throw new ArgumentOutOfRangeException(nameof(marketPricePerMetricTon));
        if (marginPerQuintal < 0) throw new ArgumentOutOfRangeException(nameof(marginPerQuintal));
        return Math.Max(0, decimal.Round(marketPricePerMetricTon / QuintalsPerMetricTon - marginPerQuintal, 2));
    }

    public static PurchaseCalculation CalculatePurchase(
        decimal grossWeightLbs,
        decimal tareLbs,
        decimal humidityPercent,
        decimal shrinkagePercent,
        decimal unitPrice)
    {
        if (grossWeightLbs <= 0) throw new ArgumentOutOfRangeException(nameof(grossWeightLbs));
        if (tareLbs < 0 || tareLbs >= grossWeightLbs) throw new ArgumentOutOfRangeException(nameof(tareLbs));
        if (humidityPercent is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(humidityPercent));
        if (shrinkagePercent is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(shrinkagePercent));
        if (humidityPercent + shrinkagePercent >= 100) throw new ArgumentException("La humedad y la merma no pueden descontar el 100% del lote.");
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        var physicalNetLbs = grossWeightLbs - tareLbs;
        var payableLbs = physicalNetLbs * (1 - (humidityPercent + shrinkagePercent) / 100m);
        var quintals = payableLbs / PoundsPerQuintal;
        return new PurchaseCalculation(
            decimal.Round(physicalNetLbs, 2),
            decimal.Round(payableLbs, 2),
            decimal.Round(quintals, 4),
            decimal.Round(quintals * unitPrice, 2));
    }
}

public sealed record PurchaseCalculation(
    decimal PhysicalNetWeightLbs,
    decimal PayableWeightLbs,
    decimal PayableQuintals,
    decimal Total);
