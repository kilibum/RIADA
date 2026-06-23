using Riada.Console.DTOs;
using Riada.Console.Services;
using Riada.Console.UI;
using Riada.Domain.Enums;
using Spectre.Console;

namespace Riada.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des membres (ajout, liste paginée).
/// </summary>
public class MemberMenu
{
    private readonly MemberService _memberService;

    public MemberMenu(MemberService memberService)
    {
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
    }

    // Saisie séquentielle des informations puis création via le service.
    public async Task ShowAddMemberMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("AJOUTER UN MEMBRE");
        ConsoleUI.AfficherSeparateur("saisie séquentielle");
        AnsiConsole.WriteLine();

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
            ConsoleUI.AfficherAlerte("Email invalide ou déjà utilisé.");
            return;
        }

        var dobStr = AnsiConsole.Ask<string>("[bold]Date de naissance :[/] [dim](yyyy-MM-dd)[/] ");
        if (!DateOnly.TryParseExact(dobStr, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dob))
        {
            ConsoleUI.AfficherAlerte("Format invalide — saisir la date au format yyyy-MM-dd (ex : 2003-07-14).");
            return;
        }

        var aujourd_hui = DateOnly.FromDateTime(DateTime.Today);
        if (dob >= aujourd_hui)
        {
            ConsoleUI.AfficherAlerte("La date de naissance doit être dans le passé.");
            return;
        }

        var age = aujourd_hui.Year - dob.Year
                  - (aujourd_hui < dob.AddYears(aujourd_hui.Year - dob.Year) ? 1 : 0);
        if (age < 5)
        {
            ConsoleUI.AfficherAlerte($"Âge calculé ({age} ans) trop faible — minimum 5 ans requis.");
            return;
        }
        if (age > 120)
        {
            ConsoleUI.AfficherAlerte($"Âge calculé ({age} ans) non plausible — vérifiez la date saisie.");
            return;
        }

        AnsiConsole.Markup("       [bold]Ville :[/] ");
        AnsiConsole.Markup("[dim](facultatif — laisser vide pour passer)[/] ");
        var city = System.Console.ReadLine()?.Trim();

        AnsiConsole.Markup("  [bold]Téléphone :[/] ");
        AnsiConsole.Markup("[dim](facultatif — laisser vide pour passer)[/] ");
        var phone = System.Console.ReadLine()?.Trim();

        AnsiConsole.WriteLine();
        ConsoleUI.AfficherSeparateur("résultat");

        try
        {
            var memberDto = await _memberService.CreateMemberAsync(
                firstName,
                lastName,
                email,
                dob,
                string.IsNullOrWhiteSpace(phone) ? null : phone,
                string.IsNullOrWhiteSpace(city) ? null : city
            );

            ConsoleUI.AfficherSucces(
                $"Membre créé — ID #{memberDto.Id:D4} — {memberDto.FullName}",
                $"#{memberDto.Id:D4}");
        }
        catch (InvalidOperationException ex)
        {
            ConsoleUI.AfficherAlerte(ex.Message);
        }
        catch (ArgumentException ex)
        {
            ConsoleUI.AfficherAlerte("validation échouée", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }

    }

    // Liste paginée des membres (20 par page) avec navigation.
    public async Task ShowListMembersMenu()
    {
        int pageSize = 20;
        int pageNumber = 1;
        bool viewing = true;

        while (viewing)
        {
            AnsiConsole.Clear();
            ConsoleUI.AfficherTitre("LISTE DES MEMBRES");

            try
            {
                var members = await _memberService.GetAllMembersAsync(pageNumber, pageSize);

                if (!members.Any())
                {
                    var videPanel = new Panel(
                        new Markup("[dim][[ État vide ]] — Aucun membre trouvé.[/]"))
                        .Border(BoxBorder.Rounded)
                        .BorderStyle(new Style(Color.Grey))
                        .Padding(1, 0);

                    AnsiConsole.Write(videPanel);
                    ConsoleUI.RetourMenu();
                    break;
                }

                var table = new Table()
                    .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Nom[/]"))
                    .AddColumn(new TableColumn("[bold]Email[/]"))
                    .AddColumn(new TableColumn("[bold]Âge[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Ville[/]"))
                    .AddColumn(new TableColumn("[bold]Statut[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Visites[/]").Centered())
                    .Border(TableBorder.Rounded)
                    .BorderStyle(new Style(Color.Grey));

                foreach (var member in members)
                {
                    var statutColor = member.Status == MemberStatus.Active ? "green" : "yellow";

                    table.AddRow(
                        member.Id.ToString("D3"),
                        Markup.Escape(member.FullName),
                        Markup.Escape(member.Email),
                        member.Age.ToString(),
                        Markup.Escape(member.AddressCity ?? "---"),
                        $"[{statutColor}]{member.Status.ToString().ToUpper()}[/]",
                        member.TotalVisits.ToString()
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();

                var choix = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[dim]Page {pageNumber} — {members.Count} membres affichés ({pageSize} membres/page)[/]")
                        .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                        .AddChoices(
                            "Page précédente",
                            "Page suivante",
                            "Retour au menu"
                        )
                );

                switch (choix)
                {
                    case "Page précédente":
                        if (pageNumber > 1) pageNumber--;
                        break;
                    case "Page suivante":
                        if (members.Count == pageSize) pageNumber++;
                        break;
                    case "Retour au menu":
                        viewing = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.AfficherErreur(ex.Message);
                ConsoleUI.RetourMenu();
                viewing = false;
            }
        }
    }

    // Sélection du membre dans une liste, puis mise à jour de ses coordonnées.
    public async Task ShowEditMemberMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("MODIFIER UN MEMBRE");

        try
        {
            var membres = await _memberService.GetSelectableMembersAsync();
            if (membres.Count == 0)
            {
                ConsoleUI.AfficherAlerte("Aucun membre actif à modifier.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<MemberOption>()
                    .Title(" [bold]Sélectionnez le membre à modifier :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.FullName)}{(m.City is null ? "" : $"  [dim]({Markup.Escape(m.City)})[/]")}")
                    .AddChoices(membres));

            var contact = await _memberService.GetMemberContactAsync(membre.Id);
            var villeActuelle = contact?.City ?? string.Empty;
            var telActuel     = contact?.Phone ?? string.Empty;

            AnsiConsole.WriteLine();
            ConsoleUI.AfficherSeparateur("valeurs actuelles — ENTRÉE pour conserver");
            AnsiConsole.WriteLine();

            var ville = AnsiConsole.Prompt(
                new TextPrompt<string>("     [bold]Ville :[/] ")
                    .DefaultValue(villeActuelle)
                    .AllowEmpty());

            var tel = AnsiConsole.Prompt(
                new TextPrompt<string>(" [bold]Téléphone :[/] ")
                    .DefaultValue(telActuel)
                    .AllowEmpty());

            AnsiConsole.WriteLine();
            ConsoleUI.AfficherSeparateur("résultat");

            bool success = await _memberService.UpdateMemberContactAsync(
                membre.Id,
                string.IsNullOrWhiteSpace(tel) ? null : tel.Trim(),
                string.IsNullOrWhiteSpace(ville) ? null : ville.Trim());

            if (success)
                ConsoleUI.AfficherSucces($"Coordonnées mises à jour — {membre.FullName}.", $"#{membre.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune modification effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    // Sélection du membre puis suppression de lui et de ses contrats, après confirmation.
    public async Task ShowDeleteMemberMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("SUPPRIMER UN MEMBRE");

        try
        {
            var membres = await _memberService.GetSelectableMembersAsync();
            if (membres.Count == 0)
            {
                ConsoleUI.AfficherAlerte("Aucun membre à supprimer.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<MemberOption>()
                    .Title(" [bold]Sélectionnez le membre à supprimer :[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.FullName)}{(m.City is null ? "" : $"  [dim]({Markup.Escape(m.City)})[/]")}")
                    .AddChoices(membres));

            var confirmer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold red]Supprimer {Markup.Escape(membre.FullName)} et tous ses contrats ?[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .AddChoices("[[ Oui, supprimer ]]", "[[ Annuler ]]"));

            if (confirmer == "[[ Annuler ]]")
            {
                ConsoleUI.AfficherAlerte("Suppression annulée.");
                return;
            }

            ConsoleUI.AfficherSeparateur("résultat");

            bool success = await _memberService.DeleteMemberAsync(membre.Id);
            if (success)
                ConsoleUI.AfficherSucces($"Membre et contrats associés supprimés — {membre.FullName}.", $"#{membre.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }
}
