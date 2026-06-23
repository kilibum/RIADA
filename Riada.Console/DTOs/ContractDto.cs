using Riada.Domain.Enums;

namespace Riada.Console.DTOs;

/// <summary>
/// Données d'un contrat exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class ContractDto
{
    public int Id { get; init; }
    public int MemberId { get; init; }
    public int PlanId { get; init; }
    public ContractStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal MonthlyPrice { get; init; }
}
