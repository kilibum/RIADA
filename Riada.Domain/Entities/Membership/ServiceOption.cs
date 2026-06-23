namespace Riada.Domain.Entities.Membership;

public class ServiceOption
{
    public int Id { get; private set; }
    public string OptionName { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ServiceOption(string optionName, decimal monthlyPrice)
    {
        if (string.IsNullOrWhiteSpace(optionName)) throw new ArgumentException("Le nom de l'option est obligatoire.", nameof(optionName));
        if (monthlyPrice < 0)                      throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(monthlyPrice));

        OptionName   = optionName;
        MonthlyPrice = monthlyPrice;
        CreatedAt    = DateTime.UtcNow;
        UpdatedAt    = DateTime.UtcNow;
    }
}
