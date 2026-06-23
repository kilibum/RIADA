namespace Riada.Domain.Pricing;

/// <summary>
/// Tarif standard : plein tarif au mois, remise de 10 % sur l'engagement annuel.
/// </summary>
public class StandardPricing : IPricingStrategy
{
    public string StrategyName => "Standard";

    public decimal CalculateMonthly(decimal basePricePerMonth) => basePricePerMonth;

    // Remise de fidélité de 10 % pour l'engagement annuel.
    public decimal CalculateAnnual(decimal basePricePerMonth)
        => basePricePerMonth * 12 * 0.90m;
}
