using Xunit;
using Riada.Domain.Entities.ClubManagement;
using Riada.Domain.Enums;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests unitaires de l'entité Employee.
/// </summary>
public class EmployeeTests
{
    // ── Factory Create ───────────────────────────────────────────────────

    [Fact]
    public void Create_AvecDonneesValides_CreeEmploye()
    {
        var employe = Employee.Create(
            clubId: 1,
            firstName: "Jean",
            lastName: "Coach",
            email: "jean@riada.com",
            role: EmployeeRole.Instructor,
            hiredOn: new DateOnly(2024, 1, 15),
            monthlySalary: 2500m,
            qualifications: "BPJEPS");

        Assert.Equal("Jean", employe.FirstName);
        Assert.Equal("Coach", employe.LastName);
        Assert.Equal("jean@riada.com", employe.Email);
        Assert.Equal(1, employe.ClubId);
        Assert.Equal(EmployeeRole.Instructor, employe.Role);
        Assert.Equal(2500m, employe.MonthlySalary);
        Assert.Equal("BPJEPS", employe.Qualifications);
    }

    [Fact]
    public void Create_SansSalaireNiQualifications_ValeursNull()
    {
        var employe = Employee.Create(1, "Marie", "Réception", "marie@riada.com",
            EmployeeRole.Receptionist, new DateOnly(2024, 6, 1));

        Assert.Null(employe.MonthlySalary);
        Assert.Null(employe.Qualifications);
    }

    // ── Validation héritée de Person ──────────────────────────────────────

    [Fact]
    public void Create_PrenomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(1, "", "Nom", "email@test.com", EmployeeRole.Manager, new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public void Create_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(1, "Prénom", "", "email@test.com", EmployeeRole.Manager, new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public void Create_EmailVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(1, "Prénom", "Nom", "", EmployeeRole.Manager, new DateOnly(2024, 1, 1)));
    }

    // ── FullName hérité de Person ─────────────────────────────────────────

    [Fact]
    public void FullName_RetournePrenomEtNom()
    {
        var employe = Employee.Create(1, "Jean", "Coach", "jean@riada.com",
            EmployeeRole.Instructor, new DateOnly(2024, 1, 1));

        Assert.Equal("Jean Coach", employe.FullName);
    }

    // ── GetDisplayInfo ────────────────────────────────────────────────────

    [Fact]
    public void GetDisplayInfo_ContientRoleEtEmail()
    {
        var employe = Employee.Create(1, "Jean", "Coach", "jean@riada.com",
            EmployeeRole.Instructor, new DateOnly(2024, 1, 1), monthlySalary: 2500m);

        var info = employe.GetDisplayInfo();

        Assert.Contains("Employee", info);
        Assert.Contains("Jean Coach", info);
        Assert.Contains("Instructor", info);
        Assert.Contains("jean@riada.com", info);
    }

    [Fact]
    public void GetDisplayInfo_DiffereDeMembreGetDisplayInfo()
    {
        var employe = Employee.Create(1, "Jean", "Test", "jean@test.com",
            EmployeeRole.Instructor, new DateOnly(2024, 1, 1));

        var membre = Riada.Domain.Entities.Membership.Member.Create(
            "Jean", "Test", "jean@test.com", new DateOnly(2000, 1, 1));

        var infoEmploye = employe.GetDisplayInfo();
        var infoMembre  = membre.GetDisplayInfo();

        Assert.NotEqual(infoEmploye, infoMembre);
        Assert.Contains("Employee", infoEmploye);
        Assert.Contains("Member", infoMembre);
    }
}
