namespace Riada.Console.DTOs;

// Options proposées dans les listes de sélection interactives des menus.
// Elles évitent à l'utilisateur de saisir un identifiant à l'aveugle.

public record MemberOption(int Id, string FullName, string? City);

public record PlanOption(int Id, string PlanName, decimal BasePrice, int CommitmentMonths);

public record ClubOption(int Id, string Name, string City);
