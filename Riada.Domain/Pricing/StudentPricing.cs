namespace Riada.Domain.Pricing;

/// <summary>
/// Tarif étudiant : remise de 40 % appliquée au mois comme à l'année.
/// </summary>
public class StudentPricing : IPricingStrategy
{
    private const decimal StudentDiscount = 0.40m;

    public string StrategyName => "Student (40% off)";

    public decimal CalculateMonthly(decimal basePricePerMonth)
        => basePricePerMonth * (1 - StudentDiscount);

    public decimal CalculateAnnual(decimal basePricePerMonth)
        => basePricePerMonth * (1 - StudentDiscount) * 12;
}
