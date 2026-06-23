namespace Riada.Domain.Pricing;

/// <summary>
/// Tarif promotionnel à remise configurable (0 à 100 %), appliquée au mois comme à l'année.
/// </summary>
public class PromotionalPricing : IPricingStrategy
{
    private readonly decimal _discountPercentage;

    // La remise est exprimée en décimal (0.25 = 25 %) et validée à la construction.
    public PromotionalPricing(decimal discountPercentage)
    {
        if (discountPercentage < 0m || discountPercentage > 1m)
            throw new ArgumentException("La remise doit être comprise entre 0 et 1.", nameof(discountPercentage));

        _discountPercentage = discountPercentage;
    }

    public string StrategyName => $"Promo ({_discountPercentage * 100:F0}% off)";

    public decimal CalculateMonthly(decimal basePricePerMonth)
        => basePricePerMonth * (1 - _discountPercentage);

    public decimal CalculateAnnual(decimal basePricePerMonth)
        => basePricePerMonth * (1 - _discountPercentage) * 12;
}
