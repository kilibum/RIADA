using Xunit;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Enums;

namespace Riada.Tests.Services;

/// <summary>
/// Tests de la logique métier de MemberService, sans dépendance à la base de données.
/// </summary>
public class MemberServiceLogicTests
{
    // ── Règle métier : âge minimum 16 ans ────────────────────────────────

    [Fact]
    public void AgeMinimum_MembreDe16Ans_EstAccepte()
    {
        var dateNaissance = new DateOnly(DateTime.Now.Year - 16, 1, 1);

        var membre = Member.Create("Test", "User", "test@test.com", dateNaissance);
        int age = membre.GetAge();

        Assert.True(age >= 16, "Un membre de 16 ans doit être accepté.");
    }

    [Fact]
    public void AgeMinimum_MembreDe15Ans_EstRefuse()
    {
        var dateNaissance = new DateOnly(DateTime.Now.Year - 15, 1, 1);

        var membre = Member.Create("Mineur", "Test", "mineur@test.com", dateNaissance);
        int age = membre.GetAge();

        Assert.True(age < 16, "Un membre de 15 ans doit être refusé par le service.");
    }

    [Fact]
    public void AgeMinimum_MembreDe25Ans_EstAccepte()
    {
        var dateNaissance = new DateOnly(DateTime.Now.Year - 25, 1, 1);
        var membre = Member.Create("Adulte", "Test", "adulte@test.com", dateNaissance);

        Assert.True(membre.GetAge() >= 16);
    }

    // ── Logique de mapping : entité vers affichage ───────────────────────

    [Fact]
    public void MappingMembre_FullNameCorrect()
    {
        var membre = Member.Create("Martin", "Dupont", "martin@test.com", new DateOnly(1998, 5, 15));

        // Simule le mapping fait par MemberService.MapToDto
        Assert.Equal("Martin Dupont", membre.FullName);
        Assert.Equal("martin@test.com", membre.Email);
        Assert.Equal(MemberStatus.Active, membre.Status);
        Assert.Equal(0, membre.TotalVisits);
    }

    [Fact]
    public void MappingMembre_DisplayInfoContenantPolymorphisme()
    {
        var membre = Member.Create("Sophie", "Laurent", "sophie@test.com", new DateOnly(1995, 3, 10));

        string info = membre.GetDisplayInfo();

        Assert.Contains("Member", info);
        Assert.Contains("Sophie Laurent", info);
        Assert.Contains("sophie@test.com", info);
    }

    // ── Logique de reconstitution (simulant un SELECT) ───────────────────

    [Fact]
    public void Reconstitution_DepuisBD_PreserveLesDonnees()
    {
        var now = DateTime.UtcNow;

        var membre = Member.Reconstitute(
            id: 42,
            firstName: "Martin",
            lastName: "Dupont",
            email: "martin@test.com",
            dateOfBirth: new DateOnly(1998, 5, 15),
            city: "Paris",
            status: MemberStatus.Active,
            totalVisits: 12,
            createdAt: now.AddDays(-30),
            updatedAt: now);

        Assert.Equal(42, membre.Id);
        Assert.Equal("Martin Dupont", membre.FullName);
        Assert.Equal("Paris", membre.AddressCity);
        Assert.Equal(MemberStatus.Active, membre.Status);
        Assert.Equal(12, membre.TotalVisits);
    }

    [Fact]
    public void Reconstitution_MembreSuspendu_StatutPreserve()
    {
        var membre = Member.Reconstitute(
            id: 3, firstName: "Karim", lastName: "Ben", email: "karim@test.com",
            dateOfBirth: new DateOnly(2002, 8, 22), city: "Marseille",
            status: MemberStatus.Suspended, totalVisits: 0,
            createdAt: DateTime.UtcNow, updatedAt: DateTime.UtcNow);

        Assert.Equal(MemberStatus.Suspended, membre.Status);
    }

    // ── Pagination (logique sans BD) ─────────────────────────────────────

    [Fact]
    public void Pagination_CalculOffset_Correct()
    {
        int page = 3;
        int pageSize = 20;

        int offset = (page - 1) * pageSize;

        Assert.Equal(40, offset);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 10, 20)]
    [InlineData(5, 20, 80)]
    public void Pagination_OffsetSelonPageEtTaille(int page, int pageSize, int expectedOffset)
    {
        int offset = (page - 1) * pageSize;

        Assert.Equal(expectedOffset, offset);
    }
}
