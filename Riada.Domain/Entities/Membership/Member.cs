using Riada.Domain.Enums;
using Riada.Domain.Entities.Common;

namespace Riada.Domain.Entities.Membership;

/// <summary>
/// Représente un membre (client) du club de fitness.
/// </summary>
public class Member : Person
{
    public Gender Gender { get; private set; } = Gender.Unspecified;
    public DateOnly DateOfBirth { get; private set; }
    public string Nationality { get; private set; } = "Belgian";
    public string? MobilePhone { get; private set; }
    public string? AddressStreet { get; private set; }
    public string? AddressCity { get; private set; }
    public string? AddressPostalCode { get; private set; }
    public MemberStatus Status { get; private set; }
    public int? ReferralMemberId { get; private set; }
    public PrimaryGoal? PrimaryGoal { get; private set; }
    public AcquisitionSource? AcquisitionSource { get; private set; }
    public bool MedicalCertificateProvided { get; private set; }
    public DateTime GdprConsentAt { get; private set; }
    public bool MarketingConsent { get; private set; }
    public DateOnly? LastVisitDate { get; private set; }
    public int TotalVisits { get; private set; }
    public DateTime StatusChangedAt { get; private set; }

    // Navigation (lecture seule depuis l'extérieur)
    public Member? ReferralMember { get; private set; }
    public ICollection<Member> ReferredMembers { get; } = [];
    public ICollection<Contract> Contracts { get; } = [];

    private Member(string firstName, string lastName, string email, DateOnly dateOfBirth, string? phone, string? city)
        : base(firstName, lastName, email)
    {
        DateOfBirth     = dateOfBirth;
        MobilePhone     = phone;
        AddressCity     = city;
        Status          = MemberStatus.Active;
        GdprConsentAt   = DateTime.UtcNow;
        StatusChangedAt = DateTime.UtcNow;
    }

    // Reconstruit un membre à partir des données persistées en base.
    private Member(int id, string firstName, string lastName, string email,
                   DateOnly dateOfBirth, string? city, MemberStatus status, int totalVisits,
                   DateTime createdAt, DateTime updatedAt)
        : base(id, firstName, lastName, email, createdAt, updatedAt)
    {
        DateOfBirth     = dateOfBirth;
        AddressCity     = city;
        Status          = status;
        TotalVisits     = totalVisits;
        GdprConsentAt   = DateTime.UtcNow;
        StatusChangedAt = DateTime.UtcNow;
    }

    public static Member Create(string firstName, string lastName, string email,
                                DateOnly dateOfBirth, string? phone = null, string? city = null)
        => new(firstName, lastName, email, dateOfBirth, phone, city);

    public static Member Reconstitute(int id, string firstName, string lastName, string email,
                                      DateOnly dateOfBirth, string? city,
                                      MemberStatus status, int totalVisits,
                                      DateTime createdAt, DateTime updatedAt)
        => new(id, firstName, lastName, email, dateOfBirth, city, status, totalVisits, createdAt, updatedAt);

    public void AssignId(int id) => SetId(id);

    public int GetAge()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth > today.AddYears(-age)) age--;
        return age;
    }

    public override string GetDisplayInfo()
        => $"Member: {FullName} (Age: {GetAge()}, Status: {Status}, Email: {Email})";
}
