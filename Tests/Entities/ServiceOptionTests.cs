using Xunit;
using Riada.Domain.Entities.Membership;

namespace Riada.Tests.Entities;

/// <summary>
/// Tests unitaires pour l'entité ServiceOption.
/// Couvre : construction, validation.
/// </summary>
public class ServiceOptionTests
{
    [Fact]
    public void Constructeur_AvecDonneesValides_CreeOption()
    {
        var option = new ServiceOption("Casier personnel", 9.99m);

        Assert.Equal("Casier personnel", option.OptionName);
        Assert.Equal(9.99m, option.MonthlyPrice);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ServiceOption("", 9.99m));
    }

    [Fact]
    public void Constructeur_NomNull_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ServiceOption(null!, 9.99m));
    }

    [Fact]
    public void Constructeur_PrixNegatif_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ServiceOption("Test", -5m));
    }

    [Fact]
    public void Constructeur_PrixZero_EstValide()
    {
        var option = new ServiceOption("Option Gratuite", 0m);

        Assert.Equal(0m, option.MonthlyPrice);
    }
}
