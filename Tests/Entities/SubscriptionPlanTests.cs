using Xunit;
using Riada.Domain.Entities.Membership;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests unitaires pour l'entité SubscriptionPlan.
/// Couvre : construction, validation, valeurs par défaut.
/// </summary>
public class SubscriptionPlanTests
{
    [Fact]
    public void Constructeur_AvecDonneesValides_CreePlan()
    {
        var plan = new SubscriptionPlan("Premium", 49.99m, commitmentMonths: 12);

        Assert.Equal("Premium", plan.PlanName);
        Assert.Equal(49.99m, plan.BasePrice);
        Assert.Equal(12, plan.CommitmentMonths);
    }

    [Fact]
    public void Constructeur_ValeursParDefaut_Correctes()
    {
        var plan = new SubscriptionPlan("Basic", 29.99m);

        Assert.Equal(12, plan.CommitmentMonths);
        Assert.Equal(19.99m, plan.EnrollmentFee);
        Assert.False(plan.LimitedClubAccess);
        Assert.False(plan.DuoPassAllowed);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new SubscriptionPlan("", 29.99m));
    }

    [Fact]
    public void Constructeur_NomNull_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new SubscriptionPlan(null!, 29.99m));
    }

    [Fact]
    public void Constructeur_PrixNegatif_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new SubscriptionPlan("Test", -10m));
    }

    [Fact]
    public void Constructeur_PrixZero_EstValide()
    {
        var plan = new SubscriptionPlan("Essai Gratuit", 0m);

        Assert.Equal(0m, plan.BasePrice);
    }

    [Fact]
    public void Constructeur_OptionsPersonnalisees_Respectees()
    {
        var plan = new SubscriptionPlan("VIP", 99.99m,
            commitmentMonths: 6,
            enrollmentFee: 0m,
            limitedClubAccess: true,
            duoPassAllowed: true);

        Assert.Equal(6, plan.CommitmentMonths);
        Assert.Equal(0m, plan.EnrollmentFee);
        Assert.True(plan.LimitedClubAccess);
        Assert.True(plan.DuoPassAllowed);
    }

    [Fact]
    public void NouveauPlan_CollectionContratsVide()
    {
        var plan = new SubscriptionPlan("Basic", 29.99m);

        Assert.Empty(plan.Contracts);
    }
}
