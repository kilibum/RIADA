using Xunit;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Entities.ClubManagement;
using Riada.Domain.Enums;
using Riada.Domain.Exceptions;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests des règles métier portées par les entités (Contract, Club).
/// </summary>
public class BusinessRulesTests
{
    // ── Contract ─────────────────────────────────────────────────────────

    [Fact]
    public void Contract_IsActive_ReturnsTrueParDefaut()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);

        Assert.True(contrat.IsActive());
    }

    [Fact]
    public void Contract_Cancel_PasseStatutAnnule()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);

        contrat.Cancel("Déménagement");

        Assert.Equal(ContractStatus.Cancelled, contrat.Status);
        Assert.NotNull(contrat.CancelledOn);
        Assert.Equal("Déménagement", contrat.CancellationReason);
        Assert.False(contrat.IsActive());
    }

    [Fact]
    public void Contract_Cancel_DejaAnnule_LeveBusinessRuleException()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);
        contrat.Cancel();

        Assert.Throws<BusinessRuleException>(() => contrat.Cancel());
    }

    [Fact]
    public void Contract_Freeze_DatesValides_EnregistreGel()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);
        var debut = new DateOnly(2026, 7, 1);
        var fin   = new DateOnly(2026, 7, 31);

        contrat.Freeze(debut, fin);

        Assert.Equal(debut, contrat.FreezeStartDate);
        Assert.Equal(fin,   contrat.FreezeEndDate);
    }

    [Fact]
    public void Contract_Freeze_ContratAnnule_LeveBusinessRuleException()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);
        contrat.Cancel();

        Assert.Throws<BusinessRuleException>(() =>
            contrat.Freeze(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31)));
    }

    [Fact]
    public void Contract_Freeze_FinAvantDebut_LeveArgumentException()
    {
        var contrat = new Contract(1, 1, 1, new DateOnly(2026, 1, 1), null);

        Assert.Throws<ArgumentException>(() =>
            contrat.Freeze(new DateOnly(2026, 7, 31), new DateOnly(2026, 7, 1)));
    }

    // ── Club ─────────────────────────────────────────────────────────────

    [Fact]
    public void Club_IsOperational_NouveauClub_ReturnsTrue()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        Assert.True(club.IsOperational());
    }

    [Fact]
    public void Club_Close_PasseStatutFerme()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        club.Close();

        Assert.Equal(ClubOperationalStatus.TemporarilyClosed, club.OperationalStatus);
        Assert.False(club.IsOperational());
    }

    [Fact]
    public void Club_Reopen_ApresFermeture_RepasseAOuvert()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));
        club.Close();

        club.Reopen();

        Assert.True(club.IsOperational());
    }

    [Fact]
    public void Club_GetDisplayInfo_ContientNomEtVille()
    {
        var club = new Club("Riada Bruxelles", "Rue de la Loi", "Bruxelles", "1000",
            new DateOnly(2024, 1, 1));

        var info = club.GetDisplayInfo();

        Assert.Contains("Riada Bruxelles", info);
        Assert.Contains("Bruxelles", info);
    }
}
