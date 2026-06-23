using Xunit;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Enums;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests unitaires de l'entité Member.
/// </summary>
public class MemberTests
{
    // ── Factory Create ───────────────────────────────────────────────────

    [Fact]
    public void Create_AvecDonneesValides_RetourneMembre()
    {
        var dateNaissance = new DateOnly(2000, 5, 15);

        var membre = Member.Create("Martin", "Dupont", "martin@test.com", dateNaissance, "0612345678");

        Assert.Equal("Martin", membre.FirstName);
        Assert.Equal("Dupont", membre.LastName);
        Assert.Equal("martin@test.com", membre.Email);
        Assert.Equal(dateNaissance, membre.DateOfBirth);
        Assert.Equal("0612345678", membre.MobilePhone);
        Assert.Equal(MemberStatus.Active, membre.Status);
    }

    [Fact]
    public void Create_SansTelephone_TelephoneEstNull()
    {
        var membre = Member.Create("Sophie", "Laurent", "sophie@test.com", new DateOnly(1995, 3, 10));

        Assert.Null(membre.MobilePhone);
    }

    [Fact]
    public void Create_StatutInitial_EstActif()
    {
        var membre = Member.Create("Karim", "Ben", "karim@test.com", new DateOnly(1998, 8, 22));

        Assert.Equal(MemberStatus.Active, membre.Status);
    }

    // ── Validation Person (héritage) ─────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_PrenomInvalide_LeveArgumentException(string? prenom)
    {
        Assert.Throws<ArgumentException>(() =>
            Member.Create(prenom!, "Dupont", "test@test.com", new DateOnly(2000, 1, 1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NomInvalide_LeveArgumentException(string? nom)
    {
        Assert.Throws<ArgumentException>(() =>
            Member.Create("Martin", nom!, "test@test.com", new DateOnly(2000, 1, 1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmailInvalide_LeveArgumentException(string? email)
    {
        Assert.Throws<ArgumentException>(() =>
            Member.Create("Martin", "Dupont", email!, new DateOnly(2000, 1, 1)));
    }

    // ── FullName (propriété calculée héritée de Person) ───────────────────

    [Fact]
    public void FullName_RetournePrenomEtNom()
    {
        var membre = Member.Create("Emma", "Richard", "emma@test.com", new DateOnly(1990, 6, 1));

        Assert.Equal("Emma Richard", membre.FullName);
    }

    // ── GetAge ────────────────────────────────────────────────────────────

    [Fact]
    public void GetAge_CalculeAgeCorrectement()
    {
        var anneeNaissance = DateTime.Now.Year - 25;
        var membre = Member.Create("Test", "User", "test@test.com", new DateOnly(anneeNaissance, 1, 1));

        var age = membre.GetAge();

        Assert.Equal(25, age);
    }

    // ── GetDisplayInfo ────────────────────────────────────────────────────

    [Fact]
    public void GetDisplayInfo_ContientNomComplet()
    {
        var membre = Member.Create("Martin", "Dupont", "martin@test.com", new DateOnly(2000, 5, 15));

        var info = membre.GetDisplayInfo();

        Assert.Contains("Martin Dupont", info);
        Assert.Contains("Member", info);
        Assert.Contains("martin@test.com", info);
    }

    // ── AssignId ──────────────────────────────────────────────────────────

    [Fact]
    public void AssignId_AffecteIdCorrectement()
    {
        var membre = Member.Create("Test", "User", "test@test.com", new DateOnly(2000, 1, 1));

        membre.AssignId(42);

        Assert.Equal(42, membre.Id);
    }

    // ── Factory Reconstitute ─────────────────────────────────────────────

    [Fact]
    public void Reconstitute_ReconstruitMembreDepuisBD()
    {
        var now = DateTime.UtcNow;

        var membre = Member.Reconstitute(
            id: 99,
            firstName: "Sophie",
            lastName: "Laurent",
            email: "sophie@test.com",
            dateOfBirth: new DateOnly(1995, 3, 10),
            city: "Paris",
            status: MemberStatus.Suspended,
            totalVisits: 15,
            createdAt: now,
            updatedAt: now);

        Assert.Equal(99, membre.Id);
        Assert.Equal("Sophie Laurent", membre.FullName);
        Assert.Equal("Paris", membre.AddressCity);
        Assert.Equal(MemberStatus.Suspended, membre.Status);
        Assert.Equal(15, membre.TotalVisits);
    }

    [Fact]
    public void Reconstitute_SansVille_VilleEstNull()
    {
        var membre = Member.Reconstitute(
            id: 1, firstName: "Test", lastName: "User", email: "t@t.com",
            dateOfBirth: new DateOnly(2000, 1, 1), city: null,
            status: MemberStatus.Active, totalVisits: 0,
            createdAt: DateTime.UtcNow, updatedAt: DateTime.UtcNow);

        Assert.Null(membre.AddressCity);
    }

    // ── Collections de navigation ────────────────────────────────────────

    [Fact]
    public void NouveauMembre_CollectionsVides()
    {
        var membre = Member.Create("Test", "User", "test@test.com", new DateOnly(2000, 1, 1));

        Assert.Empty(membre.Contracts);
        Assert.Empty(membre.ReferredMembers);
    }
}
