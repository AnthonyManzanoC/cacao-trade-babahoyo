using OrigenCacao.Domain;

namespace OrigenCacao.Tests;

public sealed class PricingCalculatorTests
{
    [Fact]
    public void Converts_metric_ton_price_to_quintal_and_subtracts_margin()
    {
        var result = PricingCalculator.CalculateDryPrice(8_150m, 18m);
        Assert.Equal(351.68m, result);
    }

    [Fact]
    public void Calculates_payable_weight_after_tare_humidity_and_shrinkage()
    {
        var result = PricingCalculator.CalculatePurchase(250m, 10m, 7m, 3m, 300m);
        Assert.Equal(240m, result.PhysicalNetWeightLbs);
        Assert.Equal(216m, result.PayableWeightLbs);
        Assert.Equal(2.16m, result.PayableQuintals);
        Assert.Equal(648m, result.Total);
    }

    [Fact]
    public void Rejects_tare_greater_than_the_lot()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PricingCalculator.CalculatePurchase(100m, 100m, 0m, 0m, 1m));
    }
}
