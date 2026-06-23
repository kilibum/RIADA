namespace Riada.Domain.Entities.Membership;

public class SubscriptionPlan
{
    public int Id { get; private set; }
    public string PlanName { get; private set; }
    public decimal BasePrice { get; private set; }
    public int CommitmentMonths { get; private set; }
    public decimal EnrollmentFee { get; private set; }
    public bool LimitedClubAccess { get; private set; }
    public bool DuoPassAllowed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public ICollection<Contract> Contracts { get; } = [];

    public SubscriptionPlan(string planName, decimal basePrice, int commitmentMonths = 12,
                            decimal enrollmentFee = 19.99m, bool limitedClubAccess = false,
                            bool duoPassAllowed = false)
    {
        if (string.IsNullOrWhiteSpace(planName)) throw new ArgumentException("Le nom du plan est obligatoire.", nameof(planName));
        if (basePrice < 0)                       throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(basePrice));

        PlanName          = planName;
        BasePrice         = basePrice;
        CommitmentMonths  = commitmentMonths;
        EnrollmentFee     = enrollmentFee;
        LimitedClubAccess = limitedClubAccess;
        DuoPassAllowed    = duoPassAllowed;
        CreatedAt         = DateTime.UtcNow;
        UpdatedAt         = DateTime.UtcNow;
    }
}
