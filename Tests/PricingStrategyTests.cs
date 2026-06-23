using Xunit;
using Riada.Domain.Pricing;

namespace Riada.Tests.Domain.Pricing;

/// <summary>
/// Tests des calculs de tarif mensuel et annuel pour chaque stratégie de prix.
/// </summary>
public class PricingStrategyTests
{
    private const decimal BasePrice = 49.99m;

    [Fact]
    public void StandardPricing_CalculateMonthly_ReturnsBasePrice()
    {
        var strategy = new StandardPricing();

        var result = strategy.CalculateMonthly(BasePrice);

        Assert.Equal(BasePrice, result);
    }

    [Fact]
    public void StandardPricing_CalculateAnnual_AppliesNinetyPercentDiscount()
    {
        var strategy = new StandardPricing();
        var expectedAnnual = BasePrice * 12 * 0.90m;

        var result = strategy.CalculateAnnual(BasePrice);

        Assert.Equal(expectedAnnual, result);
    }

    [Fact]
    public void StudentPricing_CalculateMonthly_AppliesFortyPercentDiscount()
    {
        var strategy = new StudentPricing();
        var expectedMonthly = BasePrice * 0.60m;  // 40% off

        var result = strategy.CalculateMonthly(BasePrice);

        Assert.Equal(expectedMonthly, result);
    }

    [Fact]
    public void StudentPricing_CalculateAnnual_AppliesFortyPercentDiscount()
    {
        var strategy = new StudentPricing();
        var expectedAnnual = BasePrice * 0.60m * 12;  // 40% off, 12 months

        var result = strategy.CalculateAnnual(BasePrice);

        Assert.Equal(expectedAnnual, result);
    }

    [Fact]
    public void PromotionalPricing_WithFifteenPercentDiscount_CalculatesCorrectly()
    {
        var strategy = new PromotionalPricing(0.15m);  // 15% off
        var expectedMonthly = BasePrice * 0.85m;
        var expectedAnnual = BasePrice * 0.85m * 12;

        var monthlyResult = strategy.CalculateMonthly(BasePrice);
        var annualResult = strategy.CalculateAnnual(BasePrice);

        Assert.Equal(expectedMonthly, monthlyResult);
        Assert.Equal(expectedAnnual, annualResult);
    }

    [Fact]
    public void PromotionalPricing_WithInvalidDiscount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PromotionalPricing(1.5m));  // > 1.0
        Assert.Throws<ArgumentException>(() => new PromotionalPricing(-0.1m));  // < 0.0
    }

    [Fact]
    public void StandardPricing_HasCorrectStrategyName()
    {
        var strategy = new StandardPricing();

        var name = strategy.StrategyName;

        Assert.Equal("Standard", name);
    }

    [Fact]
    public void StudentPricing_HasCorrectStrategyName()
    {
        var strategy = new StudentPricing();

        var name = strategy.StrategyName;

        Assert.Contains("40%", name);
    }
}
