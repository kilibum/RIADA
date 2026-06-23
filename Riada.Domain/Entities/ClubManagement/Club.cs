using Riada.Domain.Enums;

namespace Riada.Domain.Entities.ClubManagement;

/// <summary>
/// Représente un club de fitness.
/// </summary>
public class Club
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string AddressStreet { get; private set; }
    public string AddressCity { get; private set; }
    public string AddressPostalCode { get; private set; }
    public string Country { get; private set; }
    public bool IsOpen247 { get; private set; }
    public DateOnly OpenedOn { get; private set; }
    public ClubOperationalStatus OperationalStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public ICollection<Employee> Employees { get; } = [];

    public Club(string name, string addressStreet, string addressCity,
                string addressPostalCode, DateOnly openedOn, string country = "Belgium")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Le nom du club est obligatoire.", nameof(name));

        Name               = name;
        AddressStreet      = addressStreet;
        AddressCity        = addressCity;
        AddressPostalCode  = addressPostalCode;
        Country            = country;
        OpenedOn           = openedOn;
        IsOpen247          = true;
        OperationalStatus  = ClubOperationalStatus.Open;
        CreatedAt          = DateTime.UtcNow;
        UpdatedAt          = DateTime.UtcNow;
    }

    // ── Comportements ────────────────────────────────────────────────────

    /// <summary>Indique si le club est opérationnel.</summary>
    public bool IsOperational() => OperationalStatus == ClubOperationalStatus.Open;

    /// <summary>Ferme temporairement le club (maintenance, travaux).</summary>
    public void Close()
    {
        OperationalStatus = ClubOperationalStatus.TemporarilyClosed;
        UpdatedAt         = DateTime.UtcNow;
    }

    /// <summary>Réouvre un club précédemment fermé.</summary>
    public void Reopen()
    {
        OperationalStatus = ClubOperationalStatus.Open;
        UpdatedAt         = DateTime.UtcNow;
    }

    /// <summary>Représentation lisible du club pour l'affichage.</summary>
    public string GetDisplayInfo()
        => $"Club: {Name} — {AddressCity}, {Country} ({OperationalStatus})";
}
