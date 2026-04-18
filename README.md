# Riada.Domain

Modele de domaine pour une application de gestion de clubs de fitness. Ce projet definit les 8 entites metier principales avec heritage, polymorphisme et associations.

## Entites

1. `Person` — classe abstraite commune a Member et Employee
2. `Member` — adherent avec contrats
3. `Employee` — employe rattache a un club
4. `Club` — centre de fitness
5. `SubscriptionPlan` — plan d'abonnement (tarification)
6. `Contract` — contrat liant un membre a un plan et un club
7. `Invoice` — facture generee depuis un contrat
8. `Payment` — paiement associe a une facture

## Fonctionnalites

1. **Gestion des adherents et contrats** — creer/mettre a jour un membre, rattacher un contrat et un plan d'abonnement
2. **Facturation d'abonnement** — generer des factures periodiques a partir des contrats
3. **Encaissement des paiements** — enregistrer les paiements et calculer le solde restant
4. **Pilotage club et staff** — affecter des employes a un club et suivre l'activite contractuelle

## Heritage et polymorphisme

- Parent : `Person` (abstraite)
- Derivees : `Member`, `Employee`

Methodes polymorphes :
- `CalculateEngagementScore()` — calcul du score d'engagement (virtual/override)
- `DescribeRole()` — description du role (virtual/override)

## Design Pattern : Strategy

La tarification et le calcul d'engagement peuvent varier selon les regles metier. Le pattern Strategy permet de remplacer les algorithmes sans modifier les entites.

Strategies prevues :
- `IInvoicePricingStrategy`
- `IEngagementScoringStrategy`
- `IPaymentValidationStrategy`

## Diagramme UML

Le diagramme de classes est disponible a la racine du repository (`Diagram-UML.pdf`).

## Compilation et execution

```bash
dotnet build
dotnet run
```

## Structure

```
Riada.Domain/
├── Riada.Domain.csproj
├── Riada.Domain.sln
├── Program.cs              # Point d'entree
├── DomainEntities.cs       # 8 entites metier
├── Diagram-UML.pdf         # Diagramme de classes UML
└── README.md
```
