using Riada.Domain.Enums;

namespace Riada.Console.DTOs;

/// <summary>
/// Données d'un membre exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class MemberDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = null!;
    public int Age { get; init; }
    public string Email { get; init; } = null!;
    public string? AddressCity { get; init; }
    public MemberStatus Status { get; init; }
    public int TotalVisits { get; init; }
    public string DisplayInfo { get; init; } = null!;
}
