using Riada.Domain.Enums;

namespace Riada.Console.DTOs;

/// <summary>
/// Données d'un employé exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class EmployeeDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public EmployeeRole Role { get; init; }
    public string ClubName { get; init; } = null!;
    public decimal? MonthlySalary { get; init; }
    public string DisplayInfo { get; init; } = null!;
}
