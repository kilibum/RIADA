namespace Riada.Domain.Entities.Common;

/// <summary>
/// Classe de base abstraite pour les personnes du système (membres et employés).
/// </summary>
public abstract class Person
{
    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    protected Person(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("Le prénom est obligatoire.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))  throw new ArgumentException("Le nom est obligatoire.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))     throw new ArgumentException("L'email est obligatoire.", nameof(email));

        FirstName = firstName;
        LastName  = lastName;
        Email     = email;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Reconstruit une instance à partir des données persistées en base.
    protected Person(int id, string firstName, string lastName, string email, DateTime createdAt, DateTime updatedAt)
    {
        Id        = id;
        FirstName = firstName;
        LastName  = lastName;
        Email     = email;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // Affecte l'ID généré par la base après insertion.
    protected void SetId(int id) => Id = id;

    // Chaque sous-classe fournit sa propre représentation affichable.
    public abstract string GetDisplayInfo();
}
