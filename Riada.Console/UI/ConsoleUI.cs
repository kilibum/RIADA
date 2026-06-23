using Spectre.Console;
using Spectre.Console.Rendering;

namespace Riada.Console.UI;

/// <summary>
/// Composants d'affichage transversaux pour la console : titres, panels de succès,
/// d'alerte et d'erreur, séparateurs et invites d'attente.
/// </summary>
public static class ConsoleUI
{
    public static void AfficherTitre(string titre)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold] {titre} [/]")
            .RuleStyle("grey")
            .LeftJustified());
        AnsiConsole.WriteLine();
    }

    // Panel vert affichant un message de succès et, en option, l'ID de l'entité créée.
    public static void AfficherSucces(string message, string? idEntite = null)
    {
        var contenu = new Rows(
            new Markup($"[bold green]✓ SUCCÈS[/]"),
            new Markup(""),
            new Markup($"[green]> {Markup.Escape(message)}[/]")
        );

        if (idEntite != null)
        {
            contenu = new Rows(
                new Markup($"[bold green]✓ SUCCÈS[/]"),
                new Markup(""),
                new Markup($"[green]> {Markup.Escape(message)}[/]"),
                new Markup(""),
                new Markup($"[dim]ID entité : {Markup.Escape(idEntite)} | Date/heure : {DateTime.Now:yyyy-MM-dd HH:mm:ss}[/]")
            );
        }

        var panel = new Panel(contenu)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green))
            .Padding(1, 0);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    // Panel jaune signalant une validation métier échouée.
    public static void AfficherAlerte(string message)
    {
        var panel = new Panel(
            new Markup($"[bold yellow]! ALERTE[/]  [dim]validation échouée[/]\n\n[yellow]> {Markup.Escape(message)}[/]"))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Yellow))
            .Padding(1, 0);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    // Variante de l'alerte affichant plusieurs points de détail.
    public static void AfficherAlerte(string titre, List<string> details)
    {
        var lignes = new List<IRenderable>
        {
            new Markup($"[bold yellow]! ALERTE MÉTIER[/]  [dim]{Markup.Escape(titre)}[/]"),
            new Markup("")
        };

        foreach (var detail in details)
            lignes.Add(new Markup($"[yellow]> {Markup.Escape(detail)}[/]"));

        var panel = new Panel(new Rows(lignes))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Yellow))
            .Padding(1, 0);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    // Panel rouge pour une erreur technique (exception système ou base de données).
    public static void AfficherErreur(string message)
    {
        var panel = new Panel(new Rows(
            new Markup($"[bold red]✗ ERREUR TECHNIQUE[/]  [dim]exception système[/]"),
            new Markup(""),
            new Markup($"[red]✗ {Markup.Escape(message)}[/]"),
            new Markup(""),
            new Markup("[dim]AIDE CORRECTIVE :[/]"),
            new Markup("[dim]1. Vérifiez les données saisies et réessayez.[/]"),
            new Markup("[dim]2. Vérifiez que MySQL est démarré.[/]"),
            new Markup("[dim]3. Contactez l'administrateur si l'erreur persiste.[/]")
        ))
        .Border(BoxBorder.Rounded)
        .BorderStyle(new Style(Color.Red))
        .Padding(1, 0);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    public static void AfficherSeparateur(string? label = null)
    {
        var rule = label != null
            ? new Rule($"[dim]{Markup.Escape(label)}[/]").RuleStyle("grey")
            : new Rule().RuleStyle("grey");

        AnsiConsole.Write(rule);
    }

    public static void AttendreEntree(string message = "ENTRÉE pour continuer")
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup($"[dim][[{Markup.Escape(message)}]][/]");
        System.Console.ReadLine();
    }

    public static void RetourMenu()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Appuyez sur ENTRÉE pour revenir au menu...[/]");
        System.Console.ReadLine();
    }
}
