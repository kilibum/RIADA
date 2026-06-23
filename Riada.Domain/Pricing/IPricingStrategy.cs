namespace Riada.Domain.Pricing;

/// <summary>
/// Contrat de calcul de tarif d'un abonnement.
/// Chaque implémentation encapsule une politique de prix interchangeable.
/// </summary>
public interface IPricingStrategy
{
    decimal CalculateMonthly(decimal basePricePerMonth);

    decimal CalculateAnnual(decimal basePricePerMonth);

    string StrategyName { get; }
}
