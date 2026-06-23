using Xunit;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Enums;
using Riada.Domain.Pricing;

namespace Riada.Tests.Services;

/// <summary>
/// Tests de la logique de tarification et de contrat de ContractService,
/// sans dépendance à la base de données.
/// </summary>
public class ContractServiceLogicTests
{
    private const decimal PrixBase = 49.99m;

    // ── Intégration Strategy + calcul de prix ────────────────────────────

    [Fact]
    public void CreationContrat_StrategyStandard_PrixComplet()
    {
        IPricingStrategy strategy = new StandardPricing();

        decimal mensuel = strategy.CalculateMonthly(PrixBase);
        decimal annuel = strategy.CalculateAnnual(PrixBase);

        Assert.Equal(PrixBase, mensuel);
        Assert.Equal(PrixBase * 12 * 0.90m, annuel);
    }

    [Fact]
    public void CreationContrat_StrategyEtudiant_Remise40Pourcent()
    {
        IPricingStrategy strategy = new StudentPricing();

        decimal mensuel = strategy.CalculateMonthly(PrixBase);

        Assert.Equal(PrixBase * 0.60m, mensuel);
    }

    [Fact]
    public void CreationContrat_StrategyPromo30_Remise30Pourcent()
    {
        IPricingStrategy strategy = new PromotionalPricing(0.30m);

        decimal mensuel = strategy.CalculateMonthly(PrixBase);

        Assert.Equal(PrixBase * 0.70m, mensuel);
    }

    // ── Calcul de tarif selon la stratégie ───────────────────────────────

    [Fact]
    public void ToutesStrategies_ImplemententInterface()
    {
        IPricingStrategy standard = new StandardPricing();
        IPricingStrategy etudiant = new StudentPricing();
        IPricingStrategy promo = new PromotionalPricing(0.20m);

        var strategies = new List<IPricingStrategy> { standard, etudiant, promo };

        var prixMensuels = strategies.Select(s => s.CalculateMonthly(PrixBase)).ToList();
        Assert.Equal(3, prixMensuels.Distinct().Count());
    }

    [Fact]
    public void ToutesStrategies_OntUnNomDistinct()
    {
        IPricingStrategy standard = new StandardPricing();
        IPricingStrategy etudiant = new StudentPricing();
        IPricingStrategy promo = new PromotionalPricing(0.25m);

        Assert.NotEqual(standard.StrategyName, etudiant.StrategyName);
        Assert.NotEqual(etudiant.StrategyName, promo.StrategyName);
        Assert.NotEqual(standard.StrategyName, promo.StrategyName);
    }

    // ── Logique de calcul des dates ──────────────────────────────────────

    [Fact]
    public void DateFinContrat_12Mois_Correcte()
    {
        var dateDebut = DateOnly.FromDateTime(DateTime.UtcNow);
        int commitmentMonths = 12;

        var dateFin = dateDebut.AddMonths(commitmentMonths);

        Assert.Equal(dateDebut.Year + 1, dateFin.Year);
        Assert.Equal(dateDebut.Month, dateFin.Month);
    }

    [Fact]
    public void DateFinContrat_6Mois_Correcte()
    {
        var dateDebut = new DateOnly(2026, 1, 15);
        int commitmentMonths = 6;

        var dateFin = dateDebut.AddMonths(commitmentMonths);

        Assert.Equal(new DateOnly(2026, 7, 15), dateFin);
    }

    // ── Création de contrat (domaine) ────────────────────────────────────

    [Fact]
    public void NouveauContrat_StatutActif()
    {
        var contrat = new Contract(
            memberId: 1,
            planId: 2,
            homeClubId: 3,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(12));

        Assert.Equal(ContractStatus.Active, contrat.Status);
        Assert.Equal(ContractType.FixedTerm, contrat.ContractType);
    }

    [Fact]
    public void NouveauContrat_DatesCoherentes()
    {
        var dateDebut = new DateOnly(2026, 6, 1);
        var dateFin = dateDebut.AddMonths(12);

        var contrat = new Contract(1, 1, 1, dateDebut, dateFin);

        Assert.True(contrat.EndDate > contrat.StartDate,
            "La date de fin doit être postérieure à la date de début.");
    }

    // ── Économie annuelle ────────────────────────────────────────────────

    [Theory]
    [InlineData(49.99)]
    [InlineData(29.99)]
    [InlineData(99.99)]
    public void EconomieAnnuelle_StandardVsMensuel_Positive(decimal basePrix)
    {
        var strategy = new StandardPricing();

        decimal coutMensuelx12 = basePrix * 12;
        decimal coutAnnuel = strategy.CalculateAnnual(basePrix);
        decimal economie = coutMensuelx12 - coutAnnuel;

        Assert.True(economie > 0, "L'abonnement annuel doit offrir une économie.");
    }

    [Fact]
    public void EconomieAnnuelle_Etudiant_PasDeReductionSupplementaire()
    {
        var strategy = new StudentPricing();

        decimal mensuel = strategy.CalculateMonthly(PrixBase);
        decimal annuel = strategy.CalculateAnnual(PrixBase);

        Assert.Equal(mensuel * 12, annuel);
    }

    // ── Validation de la remise promotionnelle ───────────────────────────

    [Fact]
    public void PromotionalPricing_RemiseZero_PrixComplet()
    {
        var strategy = new PromotionalPricing(0m);

        Assert.Equal(PrixBase, strategy.CalculateMonthly(PrixBase));
    }

    [Fact]
    public void PromotionalPricing_Remise100_PrixGratuit()
    {
        var strategy = new PromotionalPricing(1.0m);

        Assert.Equal(0m, strategy.CalculateMonthly(PrixBase));
    }

    [Fact]
    public void PromotionalPricing_RemiseHorsLimites_LeveException()
    {
        Assert.Throws<ArgumentException>(() => new PromotionalPricing(1.5m));
        Assert.Throws<ArgumentException>(() => new PromotionalPricing(-0.1m));
    }
}
