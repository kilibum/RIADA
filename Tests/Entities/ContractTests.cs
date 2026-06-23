using Xunit;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Enums;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests unitaires pour l'entité Contract.
/// Couvre : construction, statut initial, encapsulation, navigation.
/// </summary>
public class ContractTests
{
    [Fact]
    public void Constructeur_AvecDonneesValides_CreeContrat()
    {
        var dateDebut = new DateOnly(2026, 1, 1);
        var dateFin = new DateOnly(2026, 12, 31);

        var contrat = new Contract(
            memberId: 1,
            planId: 2,
            homeClubId: 3,
            startDate: dateDebut,
            endDate: dateFin);

        Assert.Equal(1, contrat.MemberId);
        Assert.Equal(2, contrat.PlanId);
        Assert.Equal(3, contrat.HomeClubId);
        Assert.Equal(dateDebut, contrat.StartDate);
        Assert.Equal(dateFin, contrat.EndDate);
    }

    [Fact]
    public void Constructeur_StatutInitial_EstActif()
    {
        var contrat = new Contract(1, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow), null);

        Assert.Equal(ContractStatus.Active, contrat.Status);
    }

    [Fact]
    public void Constructeur_TypeParDefaut_EstFixedTerm()
    {
        var contrat = new Contract(1, 1, 1,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Equal(ContractType.FixedTerm, contrat.ContractType);
    }

    [Fact]
    public void Constructeur_TypePersonnalise_EstRespected()
    {
        var contrat = new Contract(1, 1, 1,
            new DateOnly(2026, 1, 1), null,
            contractType: ContractType.OpenEnded);

        Assert.Equal(ContractType.OpenEnded, contrat.ContractType);
    }

    [Fact]
    public void Constructeur_SansDateFin_DateFinEstNull()
    {
        var contrat = new Contract(1, 1, 1,
            new DateOnly(2026, 6, 1), endDate: null);

        Assert.Null(contrat.EndDate);
    }

    [Fact]
    public void Constructeur_TimestampsInitialises()
    {
        var avant = DateTime.UtcNow;
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);
        var apres = DateTime.UtcNow;

        Assert.InRange(contrat.CreatedAt, avant, apres);
        Assert.InRange(contrat.UpdatedAt, avant, apres);
    }

    [Fact]
    public void NouveauContrat_PasAnnule()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);

        Assert.Null(contrat.CancelledOn);
        Assert.Null(contrat.CancellationReason);
    }

}
