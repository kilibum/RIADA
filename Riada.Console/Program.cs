using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Riada.Console.Services;
using Riada.Console.Menus;
using Riada.Console.UI;
using Spectre.Console;
using DotNetEnv;

namespace Riada.Console;

/// <summary>
/// Point d'entrée de l'application console Riada.
/// </summary>
internal class Program
{
    private const string RetourPrincipal = "Retour au menu principal";
    private const string NavHint = "[dim]Flèches pour naviguer, ENTRÉE pour valider[/]";

    static async Task Main(string[] args)
    {
        // Force l'UTF-8 pour un affichage correct des accents et des bordures.
        try { System.Console.OutputEncoding = Encoding.UTF8; } catch { /* sortie redirigée */ }

        // Recherche le fichier .env en remontant depuis le dossier d'exécution.
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, ".env")))
                dir = dir.Parent;

            if (dir != null)
                Env.Load(Path.Combine(dir.FullName, ".env"));
        }
        catch { /* .env absent — chaîne de connexion par défaut */ }

        var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
            ?? "Server=localhost;Port=3306;Database=riada_db;Uid=root;Pwd=;";

        AfficherSplashScreen();

        var services = new ServiceCollection();
        ServiceProvider? serviceProvider = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Chargement...", async ctx =>
            {
                ctx.Status("Initialisation services...");
                ConfigureServices(services, connectionString);
                serviceProvider = services.BuildServiceProvider();
                await Task.Delay(800);
            });

        AnsiConsole.WriteLine();
        ConsoleUI.AfficherSeparateur();
        ConsoleUI.AttendreEntree();

        await RunApplication(serviceProvider!);
    }

    private static void AfficherSplashScreen()
    {
        AnsiConsole.Clear();

        var titre = new Panel(
            new FigletText("R I A D A")
                .Centered()
                .Color(Color.Blue))
            .Border(BoxBorder.Double)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(titre);
        AnsiConsole.WriteLine();

        var sousTitre = new Panel(
            new Markup("[bold]Système de gestion de clubs de fitness[/]"))
            .Border(BoxBorder.Rounded)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(sousTitre);

        var mention = new Panel(
            new Markup("[dim]Application Console — Examen POO[/]"))
            .Border(BoxBorder.Rounded)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(mention);
        AnsiConsole.WriteLine();
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddScoped(_ => new MemberService(connectionString));
        services.AddScoped(_ => new ContractService(connectionString));
        services.AddScoped(_ => new EmployeeService(connectionString));

        services.AddScoped<MemberMenu>();
        services.AddScoped<ContractMenu>();
        services.AddScoped<EmployeeMenu>();
    }

    private static async Task RunApplication(ServiceProvider serviceProvider)
    {
        var memberMenu   = serviceProvider.GetRequiredService<MemberMenu>();
        var contractMenu = serviceProvider.GetRequiredService<ContractMenu>();
        var employeeMenu = serviceProvider.GetRequiredService<EmployeeMenu>();

        bool running = true;
        while (running)
        {
            AnsiConsole.Clear();

            var menuTitre = new Panel(
                new Markup("[bold]MENU PRINCIPAL[/]"))
                .Border(BoxBorder.Double)
                .Padding(1, 0)
                .Expand();

            AnsiConsole.Write(menuTitre);
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .AddChoices(
                        "1. Gestion des membres",
                        "2. Gestion des contrats",
                        "3. Gestion des employés",
                        "4. Quitter"
                    )
            );

            try
            {
                switch (choice)
                {
                    case "1. Gestion des membres":
                        await AfficherSousMenuMembres(memberMenu);
                        break;

                    case "2. Gestion des contrats":
                        await AfficherSousMenuContrats(contractMenu);
                        break;

                    case "3. Gestion des employés":
                        await AfficherSousMenuEmployes(employeeMenu);
                        break;

                    case "4. Quitter":
                        running = false;
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold blue]Au revoir ![/]");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.AfficherErreur(ex.Message);
                ConsoleUI.RetourMenu();
            }
        }
    }

    private static async Task AfficherSousMenuMembres(MemberMenu memberMenu)
    {
        bool actif = true;
        while (actif)
        {
            AnsiConsole.Clear();
            ConsoleUI.AfficherTitre("GESTION DES MEMBRES");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Ajouter un membre",
                        "2. Lister les membres",
                        "3. Modifier un membre",
                        "4. Supprimer un membre",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Ajouter un membre":
                    await memberMenu.ShowAddMemberMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "2. Lister les membres":
                    await memberMenu.ShowListMembersMenu();
                    break;

                case "3. Modifier un membre":
                    await memberMenu.ShowEditMemberMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "4. Supprimer un membre":
                    await memberMenu.ShowDeleteMemberMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    private static async Task AfficherSousMenuContrats(ContractMenu contractMenu)
    {
        bool actif = true;
        while (actif)
        {
            AnsiConsole.Clear();
            ConsoleUI.AfficherTitre("GESTION DES CONTRATS");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Créer un contrat",
                        "2. Lister les contrats",
                        "3. Modifier un contrat",
                        "4. Annuler un contrat",
                        "5. Supprimer un contrat",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Créer un contrat":
                    await contractMenu.ShowCreateContractMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "2. Lister les contrats":
                    await contractMenu.ShowListContractsMenu();
                    break;

                case "3. Modifier un contrat":
                    await contractMenu.ShowEditContractMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "4. Annuler un contrat":
                    await contractMenu.ShowCancelContractMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "5. Supprimer un contrat":
                    await contractMenu.ShowDeleteContractMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    private static async Task AfficherSousMenuEmployes(EmployeeMenu employeeMenu)
    {
        bool actif = true;
        while (actif)
        {
            AnsiConsole.Clear();
            ConsoleUI.AfficherTitre("GESTION DES EMPLOYÉS");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Ajouter un employé",
                        "2. Lister les employés",
                        "3. Modifier un employé",
                        "4. Supprimer un employé",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Ajouter un employé":
                    await employeeMenu.ShowAddEmployeeMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "2. Lister les employés":
                    await employeeMenu.ShowListEmployeesMenu();
                    break;

                case "3. Modifier un employé":
                    await employeeMenu.ShowEditEmployeeMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case "4. Supprimer un employé":
                    await employeeMenu.ShowDeleteEmployeeMenu();
                    ConsoleUI.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }
}
