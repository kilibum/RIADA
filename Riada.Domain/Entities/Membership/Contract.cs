using Riada.Domain.Enums;
using Riada.Domain.Entities.ClubManagement;
using Riada.Domain.Exceptions;

namespace Riada.Domain.Entities.Membership;

/// <summary>
/// Représente un contrat d'abonnement entre un membre et un club.
/// </summary>
public class Contract
{
    public int Id { get; private set; }
    public int MemberId { get; private set; }
    public int PlanId { get; private set; }
    public int HomeClubId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public ContractType ContractType { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateOnly? CancelledOn { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateOnly? FreezeStartDate { get; private set; }
    public DateOnly? FreezeEndDate { get; private set; }
    public DateTime StatusChangedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Member Member { get; private set; } = null!;
    public SubscriptionPlan Plan { get; private set; } = null!;
    public Club HomeClub { get; private set; } = null!;

    public Contract(int memberId, int planId, int homeClubId,
                    DateOnly startDate, DateOnly? endDate,
                    ContractType contractType = ContractType.FixedTerm)
    {
        MemberId        = memberId;
        PlanId          = planId;
        HomeClubId      = homeClubId;
        StartDate       = startDate;
        EndDate         = endDate;
        ContractType    = contractType;
        Status          = ContractStatus.Active;
        StatusChangedAt = DateTime.UtcNow;
        CreatedAt       = DateTime.UtcNow;
        UpdatedAt       = DateTime.UtcNow;
    }

    // ── Comportements ────────────────────────────────────────────────────

    /// <summary>Indique si le contrat est actif.</summary>
    public bool IsActive() => Status == ContractStatus.Active;

    /// <summary>
    /// Annule le contrat. Lève une BusinessRuleException si déjà annulé ou expiré.
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == ContractStatus.Cancelled)
            throw new BusinessRuleException("CONTRACT_ALREADY_CANCELLED",
                "Ce contrat est déjà annulé.");
        if (Status == ContractStatus.Expired)
            throw new BusinessRuleException("CONTRACT_EXPIRED",
                "Un contrat expiré ne peut pas être annulé.");

        Status             = ContractStatus.Cancelled;
        CancelledOn        = DateOnly.FromDateTime(DateTime.UtcNow);
        CancellationReason = reason;
        StatusChangedAt    = DateTime.UtcNow;
        UpdatedAt          = DateTime.UtcNow;
    }

    /// <summary>
    /// Gèle le contrat pour une période donnée.
    /// Seul un contrat actif peut être gelé ; la date de fin doit être postérieure au début.
    /// </summary>
    public void Freeze(DateOnly start, DateOnly end)
    {
        if (!IsActive())
            throw new BusinessRuleException("CONTRACT_NOT_ACTIVE",
                "Seul un contrat actif peut être gelé.");
        if (end <= start)
            throw new ArgumentException(
                "La date de fin du gel doit être postérieure à la date de début.", nameof(end));

        FreezeStartDate = start;
        FreezeEndDate   = end;
        UpdatedAt       = DateTime.UtcNow;
    }
}
