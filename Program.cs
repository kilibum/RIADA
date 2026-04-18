using Riada.Domain;

// --- Initialisation des donnees ---

var plan = new SubscriptionPlan(
    id: 1,
    planName: "Premium 12 mois",
    basePrice: 49.99m,
    commitmentMonths: 12);

var club = new Club(
    id: 1,
    name: "Riada Bruxelles Centre",
    addressCity: "Bruxelles");

var employee = new Employee(
    id: 1,
    firstName: "Iliesse",
    lastName: "Oumbarki",
    email: "Oumbarki.iliesse@gmail.com",
    club: club,
    role: "Coach");

var member = new Member(
    id: 1,
    firstName: "Marco",
    lastName: "Bonizzi",
    email: "Marco.Bonizzi@gmail.com",
    status: "Active",
    dateOfBirth: new DateOnly(1995, 3, 15));

// --- Affichage des entites ---

Console.WriteLine("=== Riada Domain ===");
Console.WriteLine();

Console.WriteLine($"Club       : {club.Name} ({club.AddressCity})");
Console.WriteLine($"Plan       : {plan.PlanName} - {plan.BasePrice} €/mois - {plan.CommitmentMonths} mois");
Console.WriteLine($"Employe    : {employee.FirstName} {employee.LastName} - {employee.Role}");
Console.WriteLine($"Membre     : {member.FirstName} {member.LastName} - Statut: {member.Status}");
Console.WriteLine($"             Ne le {member.DateOfBirth:dd/MM/yyyy}");
Console.WriteLine($"             Contrats: {member.Contracts.Count}");
Console.WriteLine();

// --- Verification heritage et polymorphisme ---

Person[] personnes = [member, employee];

Console.WriteLine("=== Heritage (Person) ===");
foreach (var personne in personnes)
{
    Console.WriteLine($"  {personne.GetType().Name}: {personne.FirstName} {personne.LastName} ({personne.Email})");
}

Console.WriteLine();
Console.WriteLine("Compilation et execution reussies.");
