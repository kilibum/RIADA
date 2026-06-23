using Xunit;
using Riada.Domain.Entities.ClubManagement;
using Riada.Domain.Enums;

namespace Riada.Tests.Entities;

public class ClubTests
{
    [Fact]
    public void Constructeur_AvecDonneesValides_CreeClub()
    {
        var club = new Club("Riada Bruxelles", "Rue de la Loi 42", "Bruxelles", "1000",
            new DateOnly(2024, 3, 1));

        Assert.Equal("Riada Bruxelles", club.Name);
        Assert.Equal("Rue de la Loi 42", club.AddressStreet);
        Assert.Equal("Bruxelles", club.AddressCity);
        Assert.Equal("1000", club.AddressPostalCode);
    }

    [Fact]
    public void Constructeur_PaysParDefaut_EstBelgique()
    {
        var club = new Club("Test Club", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        Assert.Equal("Belgium", club.Country);
    }

    [Fact]
    public void Constructeur_PaysPersonnalise_EstRespected()
    {
        var club = new Club("Test Club", "Rue", "Paris", "75001",
            new DateOnly(2024, 1, 1), country: "France");

        Assert.Equal("France", club.Country);
    }

    [Fact]
    public void Constructeur_StatutInitial_EstOuvert()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        Assert.Equal(ClubOperationalStatus.Open, club.OperationalStatus);
    }

    [Fact]
    public void Constructeur_OuvertParDefaut_247()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        Assert.True(club.IsOpen247);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Club("", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public void Constructeur_NomNull_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Club(null!, "Rue", "Ville", "1000", new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public void NouveauClub_CollectionEmployesVide()
    {
        var club = new Club("Test", "Rue", "Ville", "1000", new DateOnly(2024, 1, 1));

        Assert.Empty(club.Employees);
    }
}
