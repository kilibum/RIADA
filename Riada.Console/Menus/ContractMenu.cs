using Riada.Console.DTOs;
using Riada.Console.Services;
using Riada.Console.UI;
using Riada.Domain.Enums;
using Riada.Domain.Pricing;
using Spectre.Console;

namespace Riada.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des contrats (création, liste, modification,
/// annulation, suppression). Chaque opération propose une liste nominative.
/// </summary>
public class ContractMenu
{
    private readonly ContractService _contractService;

    public ContractMenu(ContractService contractService)
    {
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
    }

    // Wizard en 4 étapes : Membre → Plan → Club → Stratégie, puis confirmation.
    public async Task ShowCreateContractMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("CRÉER UN CONTRAT");

        try
        {
            AfficherBarreEtapes(1);
            var membres = await _contractService.GetSelectableMembersAsync();
            if (membres.Count == 0)
            {
                ConsoleUI.AfficherAlerte("Aucun membre actif disponible — créez d'abord un membre.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<MemberOption>()
                    .Title(" [bold]Sélectionnez le membre :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.FullName)}{(m.City is null ? "" : $"  [dim]({Markup.Escape(m.City)})[/]")}")
                    .AddChoices(membres));

            AfficherBarreEtapes(2);
            var plan = await ChoisirPlanAsync(" [bold]Sélectionnez l'abonnement :[/]");
            if (plan is null) return;

            AfficherBarreEtapes(3);
            var clubs = await _contractService.GetSelectableClubsAsync();
            if (clubs.Count == 0)
            {
                ConsoleUI.AfficherAlerte("Aucun club disponible.");
                return;
            }

            var club = AnsiConsole.Prompt(
                new SelectionPrompt<ClubOption>()
                    .Title(" [bold]Sélectionnez le club :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Name)}  [dim]({Markup.Escape(c.City)})[/]")
                    .AddChoices(clubs));

            AfficherBarreEtapes(4);
            var strategy = ChoisirStrategie();

            AfficherApercuPrix(plan, strategy, membre.FullName, club.Name);

            if (!Confirmer("[bold]Confirmer la création ?[/]"))
            {
                ConsoleUI.AfficherAlerte("Création annulée par l'utilisateur.");
                return;
            }

            var contractDto = await _contractService.CreateContractAsync(membre.Id, plan.Id, club.Id, strategy);
            ConsoleUI.AfficherSucces(
                $"Contrat créé pour {membre.FullName} — {plan.PlanName}.",
                $"C-{contractDto.Id:D4}");
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

    // Tableau de tous les contrats avec leur statut (actif, annulé, expiré...).
    public async Task ShowListContractsMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("LISTE DES CONTRATS");

        try
        {
            var contrats = await _contractService.GetAllContractsAsync();
            if (contrats.Count == 0)
            {
                AnsiConsole.Write(new Panel(new Markup("[dim][[ État vide ]] — Aucun contrat enregistré.[/]"))
                    .Border(BoxBorder.Rounded).BorderStyle(new Style(Color.Grey)).Padding(1, 0));
                ConsoleUI.RetourMenu();
                return;
            }

            var noms = await ChargerNomsAsync(contrats);

            var table = new Table()
                .AddColumn(new TableColumn("[bold]Contrat[/]"))
                .AddColumn(new TableColumn("[bold]Membre[/]"))
                .AddColumn(new TableColumn("[bold]Abonnement[/]"))
                .AddColumn(new TableColumn("[bold]Mensuel[/]").Centered())
                .AddColumn(new TableColumn("[bold]Début[/]"))
                .AddColumn(new TableColumn("[bold]Fin[/]"))
                .AddColumn(new TableColumn("[bold]Statut[/]").Centered())
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey));

            foreach (var c in contrats)
            {
                table.AddRow(
                    $"C-{c.Id:D4}",
                    Markup.Escape(noms[c.Id].Membre),
                    Markup.Escape(noms[c.Id].Plan),
                    c.MonthlyPrice > 0 ? $"{c.MonthlyPrice:F2} €" : "—",
                    c.StartDate.ToString("yyyy-MM-dd"),
                    c.EndDate?.ToString("yyyy-MM-dd") ?? "—",
                    $"[{CouleurStatut(c.Status)}]{StatutLabel(c.Status)}[/]");
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

    // Sélection d'un contrat actif puis réaffectation de l'abonnement et de la stratégie.
    public async Task ShowEditContractMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("MODIFIER UN CONTRAT");

        try
        {
            var contrat = await ChoisirContratAsync(
                await _contractService.GetActiveContractsAsync(maxCount: 20),
                " [bold]Sélectionnez le contrat à modifier :[/]",
                "Aucun contrat actif à modifier.");
            if (contrat is null) return;

            var plan = await ChoisirPlanAsync(" [bold]Nouvel abonnement :[/]");
            if (plan is null) return;

            var strategy = ChoisirStrategie();
            AfficherApercuPrix(plan, strategy, null, null);

            if (!Confirmer("[bold]Confirmer la modification ?[/]"))
            {
                ConsoleUI.AfficherAlerte("Modification annulée.");
                return;
            }

            ConsoleUI.AfficherSeparateur("résultat");
            decimal nouveauPrix = strategy.CalculateMonthly(plan.BasePrice);
            bool success = await _contractService.ChangeContractPlanAsync(contrat.Id, plan.Id, nouveauPrix);
            if (success)
                ConsoleUI.AfficherSucces($"Abonnement changé pour {plan.PlanName}.", $"C-{contrat.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune modification effectuée (contrat inactif ?).");
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

    // Sélection d'un contrat actif puis passage de son statut à « annulé ».
    public async Task ShowCancelContractMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("ANNULER UN CONTRAT");

        try
        {
            var contrat = await ChoisirContratAsync(
                await _contractService.GetActiveContractsAsync(maxCount: 20),
                " [bold]Sélectionnez le contrat à annuler :[/]",
                "Aucun contrat actif trouvé.");
            if (contrat is null) return;

            if (!Confirmer("[bold]Confirmer l'annulation ?[/]", "[[ Confirmer l'annulation ]]"))
            {
                ConsoleUI.AfficherAlerte("Annulation abandonnée.");
                return;
            }

            ConsoleUI.AfficherSeparateur("résultat");
            bool success = await _contractService.CancelContractAsync(contrat.Id);
            if (success)
                ConsoleUI.AfficherSucces("Annulation du contrat effectuée.", $"C-{contrat.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Contrat déjà annulé — aucune action effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'un contrat (tous statuts) puis suppression définitive.
    public async Task ShowDeleteContractMenu()
    {
        AnsiConsole.Clear();
        ConsoleUI.AfficherTitre("SUPPRIMER UN CONTRAT");

        try
        {
            var contrat = await ChoisirContratAsync(
                await _contractService.GetAllContractsAsync(),
                " [bold]Sélectionnez le contrat à supprimer :[/]",
                "Aucun contrat à supprimer.",
                rouge: true);
            if (contrat is null) return;

            if (!Confirmer("[bold red]Supprimer définitivement ce contrat ?[/]", "[[ Oui, supprimer ]]", rouge: true))
            {
                ConsoleUI.AfficherAlerte("Suppression annulée.");
                return;
            }

            ConsoleUI.AfficherSeparateur("résultat");
            bool success = await _contractService.DeleteContractAsync(contrat.Id);
            if (success)
                ConsoleUI.AfficherSucces("Contrat supprimé.", $"C-{contrat.Id:D4}");
            else
                ConsoleUI.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            ConsoleUI.AfficherErreur(ex.Message);
        }
    }

    // ── Helpers privés ───────────────────────────────────────────────────

    // Demande le choix d'un plan d'abonnement ; retourne null si aucun n'existe.
    private async Task<PlanOption?> ChoisirPlanAsync(string titre)
    {
        var plans = await _contractService.GetSelectablePlansAsync();
        if (plans.Count == 0)
        {
            ConsoleUI.AfficherAlerte("Aucun plan d'abonnement disponible.");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<PlanOption>()
                .Title(titre)
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .PageSize(10)
                .UseConverter(p => $"{Markup.Escape(p.PlanName)}  —  [green]{p.BasePrice:F2} €[/]/mois  —  engagement {p.CommitmentMonths} mois")
                .AddChoices(plans));
    }

    // Demande le choix d'un contrat dans une liste nominative ; null si liste vide.
    private async Task<ContractDto?> ChoisirContratAsync(
        List<ContractDto> contrats, string titre, string messageVide, bool rouge = false)
    {
        if (contrats.Count == 0)
        {
            ConsoleUI.AfficherAlerte(messageVide);
            return null;
        }

        var noms = await ChargerNomsAsync(contrats);
        var couleur = rouge ? Color.Red : Color.Blue;

        return AnsiConsole.Prompt(
            new SelectionPrompt<ContractDto>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .PageSize(10)
                .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                .UseConverter(c => Markup.Escape(
                    $"C-{c.Id:D4} — {noms[c.Id].Membre} — {noms[c.Id].Plan} ({StatutLabel(c.Status)})"))
                .AddChoices(contrats));
    }

    private static IPricingStrategy ChoisirStrategie()
    {
        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(" [bold]Stratégie de tarification :[/]")
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .AddChoices("Standard", "Étudiant (-40%)", "Promotionnel (-30%)"));

        return choix switch
        {
            "Standard"            => new StandardPricing(),
            "Étudiant (-40%)"     => new StudentPricing(),
            "Promotionnel (-30%)" => new PromotionalPricing(0.30m),
            _                     => new StandardPricing()
        };
    }

    private static void AfficherApercuPrix(PlanOption plan, IPricingStrategy strategy, string? membre, string? club)
    {
        decimal mensuel = strategy.CalculateMonthly(plan.BasePrice);
        decimal annuel  = strategy.CalculateAnnual(plan.BasePrice);
        decimal economie = (plan.BasePrice * 12) - annuel;

        ConsoleUI.AfficherSeparateur("récapitulatif");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded).BorderStyle(new Style(Color.Grey));
        if (membre is not null) table.AddColumn("[bold]Membre[/]");
        table.AddColumn("[bold]Abonnement[/]");
        if (club is not null) table.AddColumn("[bold]Club[/]");
        table.AddColumn("[bold]Stratégie[/]");
        table.AddColumn("[bold]Mensuel[/]");
        table.AddColumn("[bold]Annuel[/]");
        table.AddColumn("[bold]Économie[/]");

        var cells = new List<string>();
        if (membre is not null) cells.Add(Markup.Escape(membre));
        cells.Add(Markup.Escape(plan.PlanName));
        if (club is not null) cells.Add(Markup.Escape(club));
        cells.Add(Markup.Escape(strategy.StrategyName));
        cells.Add($"{mensuel:F2} €");
        cells.Add($"{annuel:F2} €");
        cells.Add($"{economie:F2} €");
        table.AddRow(cells.ToArray());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static bool Confirmer(string titre, string ouiLabel = "[[ Confirmer ]]", bool rouge = false)
    {
        var couleur = rouge ? Color.Red : Color.Blue;
        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .AddChoices(ouiLabel, "[[ Annuler ]]"));
        return choix == ouiLabel;
    }

    // Charge, pour chaque contrat, le nom du membre et de l'abonnement.
    private async Task<Dictionary<int, (string Membre, string Plan)>> ChargerNomsAsync(List<ContractDto> contrats)
    {
        var noms = new Dictionary<int, (string, string)>();
        foreach (var c in contrats)
        {
            var membre = c.MemberId == 0
                ? "(membre supprimé)"
                : await _contractService.GetMemberFullNameAsync(c.MemberId) ?? "(membre inconnu)";
            var plan   = (await _contractService.GetPlanInfoAsync(c.PlanId))?.PlanName ?? "(formule inconnue)";
            noms[c.Id] = (membre, plan);
        }
        return noms;
    }

    private static string StatutLabel(ContractStatus s) => s switch
    {
        ContractStatus.Active    => "ACTIF",
        ContractStatus.Suspended => "SUSPENDU",
        ContractStatus.Expired   => "EXPIRÉ",
        ContractStatus.Cancelled => "ANNULÉ",
        _                        => s.ToString().ToUpper()
    };

    private static string CouleurStatut(ContractStatus s) => s switch
    {
        ContractStatus.Active    => "green",
        ContractStatus.Suspended => "grey",
        ContractStatus.Expired   => "yellow",
        ContractStatus.Cancelled => "red",
        _                        => "white"
    };

    // Fil d'Ariane du wizard ; l'étape courante est encadrée et en surbrillance.
    private static void AfficherBarreEtapes(int etapeActive)
    {
        var etapes = new[] { "Membre", "Plan", "Club", "Stratégie" };
        var parts = etapes.Select((e, i) =>
        {
            var n = i + 1;
            return n == etapeActive
                ? $"[bold blue][[ {n}. {e} ]][/]"
                : $"[dim]{n}. {e}[/]";
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  " + string.Join("   [dim]>[/]   ", parts));
        AnsiConsole.WriteLine();
    }
}
