using Riada.Domain.Enums;
using Riada.Domain.Entities.Common;

namespace Riada.Domain.Entities.ClubManagement;

/// <summary>
/// Représente un employé (membre du personnel) d'un club.
/// </summary>
public class Employee : Person
{
    public int ClubId { get; private set; }
    public EmployeeRole Role { get; private set; }
    public decimal? MonthlySalary { get; private set; }
    public string? Qualifications { get; private set; }
    public DateOnly HiredOn { get; private set; }

    // Navigation
    public Club Club { get; private set; } = null!;

    private Employee(int clubId, string firstName, string lastName, string email,
                     EmployeeRole role, DateOnly hiredOn, decimal? monthlySalary = null,
                     string? qualifications = null)
        : base(firstName, lastName, email)
    {
        ClubId         = clubId;
        Role           = role;
        HiredOn        = hiredOn;
        MonthlySalary  = monthlySalary;
        Qualifications = qualifications;
    }

    public static Employee Create(int clubId, string firstName, string lastName, string email,
                                  EmployeeRole role, DateOnly hiredOn,
                                  decimal? monthlySalary = null, string? qualifications = null)
        => new(clubId, firstName, lastName, email, role, hiredOn, monthlySalary, qualifications);

    public void AssignId(int id) => SetId(id);

    public override string GetDisplayInfo()
        => $"Employee: {FullName} ({Role} at Club {ClubId}, €{MonthlySalary:F2}/month, Email: {Email})";
}
