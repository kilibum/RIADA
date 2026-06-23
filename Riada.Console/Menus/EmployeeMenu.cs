using Riada.Console.DTOs;
using Riada.Console.Services;
using Riada.Console.UI;
using Riada.Domain.Enums;
using Spectre.Console;

namespace Riada.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des employés (ajout, liste).
/// Le club et le rôle sont choisis dans des listes pour éviter la saisie d'un code.
/// </summary>
public class EmployeeMenu
{
    private readonly EmployeeService _employeeService;

    public EmployeeMenu(EmployeeService employeeService)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
    }

    public async Task ShowAddEmployeeMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("AJOUTER UN EMPLOYÉ");
        ConsoleUI.AfficherSeparateur("saisie séquentielle");
        AnsiConsole.WriteLine();

        try
        {
            var firstName = AnsiConsole.Ask<string>("     [bold]Prénom :[/] ");
            if (string.IsNullOrWhiteSpace(firstName))
            {
                ConsoleUI.AfficherAlerte("Le prénom est obligatoire.");
                return;
            }

            var lastName = AnsiConsole.Ask<string>("       [bold]Nom :[/] ");
            if (string.IsNullOrWhiteSpace(lastName))
            {
                ConsoleUI.AfficherAlerte("Le nom est obligatoire.");
                return;
            }

            var email = AnsiConsole.Ask<string>("     [bold]Email :[/] ");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                ConsoleUI.AfficherAlerte("Email invalide.");
                return;
            }

            var clubs = await _employeeService.GetSelectableClubsAsync();
            if (clubs.Count == 0)
            {
                ConsoleUI.AfficherAlerte("Aucun club disponible — créez d'abord un club.");
                return;
            }

            var club = AnsiConsole.Prompt(
                new SelectionPrompt<ClubOption>()
                    .Title(" [bold]Club d'affectation :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Name)}  [dim]({Markup.Escape(c.City)})[/]")
                    .AddChoices(clubs));

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<EmployeeRole>()
                    .Title(" [bold]Rôle :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .UseConverter(RoleLabel)
                    .AddChoices(Enum.GetValues<EmployeeRole>()));

            var hireStr = AnsiConsole.Ask<string>("[bold]Date d'embauche :[/] [dim](yyyy-MM-dd, vide = aujourd'hui)[/] ", string.Empty);
            DateOnly hiredOn;
            if (string.IsNullOrWhiteSpace(hireStr))
            {
                hiredOn = DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!DateOnly.TryParseExact(hireStr, "yyyy-MM-dd",
                         System.Globalization.CultureInfo.InvariantCulture,
                         System.Globalization.DateTimeStyles.None, out hiredOn))
            {
                ConsoleUI.AfficherAlerte("Format de date invalide (attendu : yyyy-MM-dd).");
                return;
            }

            var salaireStr = AnsiConsole.Ask<string>("[bold]Salaire mensuel :[/] [dim](optionnel, ex : 2350.00)[/] ", string.Empty);
            decimal? salaire = null;
            if (!string.IsNullOrWhiteSpace(salaireStr))
            {
                if (!decimal.TryParse(salaireStr, System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out var s) || s < 0)
                {
                    ConsoleUI.AfficherAlerte("Salaire invalide.");
                    return;
                }
                salaire = s;
            }

            AnsiConsole.Markup("[bold]Qualifications :[/] [dim](optionnel)[/] ");
            var qualifications = System.Console.ReadLine()?.Trim();

            AnsiConsole.WriteLine();
            ConsoleUI.AfficherSeparateur("résultat");

            var dto = await _employeeService.CreateEmployeeAsync(
                firstName, lastName, email, club.Id, role, hiredOn,
                salaire, string.IsNullOrWhiteSpace(qualifications) ? null : qualifications);

            ConsoleUI.AfficherSucces(
                $"Employé créé — {dto.FullName} ({RoleLabel(dto.Role)}) au club {dto.ClubName}.",
                $"#{dto.Id:D4}");
        }
        catch (InvalidOperationException ex)
        {
            ConsoleUI.AfficherAlerte("validation échouée", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    public async Task ShowListEmployeesMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("LISTE DES EMPLOYÉS");

        try
        {
            var employes = await _employeeService.GetAllEmployeesAsync();

            if (employes.Count == 0)
            {
                var videPanel = new Panel(
                    new Markup("[dim][[ État vide ]] — Aucun employé trouvé.[/]"))
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(Color.Grey))
                    .Padding(1, 0);

                AnsiConsole.Write(videPanel);
                ConsoleUI.RetourMenu();
                return;
            }

            var table = new Table()
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold]Nom[/]"))
                .AddColumn(new TableColumn("[bold]Email[/]"))
                .AddColumn(new TableColumn("[bold]Rôle[/]"))
                .AddColumn(new TableColumn("[bold]Club[/]"))
                .AddColumn(new TableColumn("[bold]Salaire[/]").Centered())
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey));

            foreach (var e in employes)
            {
                table.AddRow(
                    e.Id.ToString("D3"),
                    Markup.Escape(e.FullName),
                    Markup.Escape(e.Email),
                    Markup.Escape(RoleLabel(e.Role)),
                    Markup.Escape(e.ClubName),
                    e.MonthlySalary.HasValue ? $"{e.MonthlySalary:F2} €" : "—");
            }

            AnsiConsole.Write(table);
            ConsoleUI.RetourMenu();
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
            ConsoleUI.RetourMenu();
        }
    }

    // Sélection d'un employé dans une liste, puis mise à jour de son affectation.
    public async Task ShowEditEmployeeMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("MODIFIER UN EMPLOYÉ");

        try
        {
            var employe = await ChoisirEmployeAsync(" [bold]Sélectionnez l'employé à modifier :[/]", Color.Blue);
            if (employe is null) return;

            AnsiConsole.MarkupLine($" [dim]Actuellement : {Markup.Escape(RoleLabel(employe.Role))} au club {Markup.Escape(employe.ClubName)}[/]");
            AnsiConsole.WriteLine();

            var clubs = await _employeeService.GetSelectableClubsAsync();
            var club = AnsiConsole.Prompt(
                new SelectionPrompt<ClubOption>()
                    .Title(" [bold]Nouveau club :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Name)}  [dim]({Markup.Escape(c.City)})[/]")
                    .AddChoices(clubs));

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<EmployeeRole>()
                    .Title(" [bold]Nouveau rôle :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .UseConverter(RoleLabel)
                    .AddChoices(Enum.GetValues<EmployeeRole>()));

            var infos = await _employeeService.GetEmployeeEditInfoAsync(employe.Id);
            var salaireActuel = infos?.Salary?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var qualifActuelles = infos?.Qualifications ?? string.Empty;

            AnsiConsole.WriteLine();
            ConsoleUI.AfficherSeparateur("valeurs actuelles — ENTRÉE pour conserver");
            AnsiConsole.WriteLine();

            var salaireStr = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Salaire mensuel :[/] ").DefaultValue(salaireActuel).AllowEmpty());
            decimal? salaire = null;
            if (!string.IsNullOrWhiteSpace(salaireStr))
            {
                if (!decimal.TryParse(salaireStr, System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out var s) || s < 0)
                {
                    ConsoleUI.AfficherAlerte("Salaire invalide.");
                    return;
                }
                salaire = s;
            }

            var qualifications = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Qualifications :[/] ").DefaultValue(qualifActuelles).AllowEmpty());

            ConsoleUI.AfficherSeparateur("résultat");

            bool success = await _employeeService.UpdateEmployeeAsync(
                employe.Id, club.Id, role, salaire,
                string.IsNullOrWhiteSpace(qualifications) ? null : qualifications.Trim());

            if (success)
                ConsoleUI.AfficherSucces($"Employé mis à jour — {employe.FullName} ({RoleLabel(role)}) au club {club.Name}.", $"#{employe.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune modification effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'un employé dans une liste, puis suppression après confirmation.
    public async Task ShowDeleteEmployeeMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("SUPPRIMER UN EMPLOYÉ");

        try
        {
            var employe = await ChoisirEmployeAsync(" [bold]Sélectionnez l'employé à supprimer :[/]", Color.Red);
            if (employe is null) return;

            var confirmer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold red]Supprimer définitivement {Markup.Escape(employe.FullName)} ?[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .AddChoices("[[ Oui, supprimer ]]", "[[ Annuler ]]"));

            if (confirmer == "[[ Annuler ]]")
            {
                ConsoleUI.AfficherAlerte("Suppression annulée.");
                return;
            }

            ConsoleUI.AfficherSeparateur("résultat");
            bool success = await _employeeService.DeleteEmployeeAsync(employe.Id);
            if (success)
                ConsoleUI.AfficherSucces($"Employé supprimé — {employe.FullName}.", $"#{employe.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    // Demande le choix d'un employé dans la liste ; retourne null si aucun employé.
    private async Task<EmployeeDto?> ChoisirEmployeAsync(string titre, Color couleur)
    {
        var employes = await _employeeService.GetAllEmployeesAsync();
        if (employes.Count == 0)
        {
            ConsoleUI.AfficherAlerte("Aucun employé disponible.");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<EmployeeDto>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .PageSize(10)
                .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                .UseConverter(e => $"#{e.Id:D4} — {Markup.Escape(e.FullName)}  [dim]({Markup.Escape(RoleLabel(e.Role))}, {Markup.Escape(e.ClubName)})[/]")
                .AddChoices(employes));
    }

    // Libellé lisible (français) d'un rôle d'employé.
    private static string RoleLabel(EmployeeRole role) => role switch
    {
        EmployeeRole.Instructor   => "Instructeur",
        EmployeeRole.Manager      => "Manager",
        EmployeeRole.Receptionist => "Réceptionniste",
        EmployeeRole.Technician   => "Technicien",
        EmployeeRole.Intern       => "Stagiaire",
        EmployeeRole.Management    => "Direction",
        _                         => role.ToString()
    };
}
