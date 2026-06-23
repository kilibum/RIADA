# Riada — Système de gestion de clubs de fitness

Application console C# (.NET 8) développée dans le cadre de l'examen de Programmation Orientée Objet.

## Présentation

Riada est un système de gestion de clubs de fitness. Il permet de gérer les membres, les abonnements, les contrats et l'accès aux clubs via une interface console enrichie (Spectre.Console).

## Fonctionnalités

L'application offre un CRUD complet sur trois domaines, via une console interactive (Spectre.Console).

| Domaine | Opérations |
|---------|------------|
| **Membres** | Ajouter · Lister (paginé) · Modifier (coordonnées) · Supprimer |
| **Contrats** | Créer (wizard 4 étapes) · Lister (avec statut) · Modifier (abonnement) · Annuler · Supprimer |
| **Employés** | Ajouter · Lister · Modifier (affectation) · Supprimer |

> Toute opération nécessitant un identifiant propose une **liste nominative** (membre, abonnement, club, contrat, employé) : aucun ID à saisir à l'aveugle. La validation (âge ≥ 16, email unique, etc.) est appliquée côté service.

## Architecture

```
Riada/
├── Riada.Domain/     Entités métier, enums, exceptions, stratégies de tarification
├── Riada.Console/    Services (Member/Contract/Employee), DTOs, menus Spectre.Console
└── Tests/            Tests unitaires xUnit (entités, services, tarification)
```

## Concepts POO illustrés

### Héritage et polymorphisme

```
Person (abstraite)
├── Member    → GetDisplayInfo() affiche âge, statut, email
└── Employee  → GetDisplayInfo() affiche rôle, club, salaire
```

La méthode abstraite `GetDisplayInfo()` est surchargée différemment dans chaque sous-classe.
Elle est appelée dans `MemberService.MapToDto()` via une référence de type `Member` — démonstration du polymorphisme d'inclusion.

### Design Pattern — Strategy (tarification)

Le pattern Strategy est appliqué à la tarification des contrats :

```
IPricingStrategy
├── StandardPricing    mensuel = prix de base, annuel = −10 %
├── StudentPricing     mensuel = −40 %, annuel = −40 %
└── PromotionalPricing remise configurable de 0 à 100 %
```

L'utilisateur choisit la stratégie au moment de créer un contrat. Le code client (`ContractService`) ne connaît que l'interface — les algorithmes sont interchangeables sans modifier le service.

### Encapsulation

Les propriétés des entités sont exposées uniquement via des accesseurs adaptés. Les règles de validation (âge, email unique) sont centralisées dans la couche service.

## Classes métier (≥ 8 exigées)

| Classe | Couche | Description |
|--------|--------|-------------|
| `Person` | Domain | Classe abstraite parente — hiérarchie d'héritage |
| `Member` | Domain | Adhérent — hérite de Person |
| `Employee` | Domain | Employé — hérite de Person |
| `Club` | Domain | Centre de fitness |
| `SubscriptionPlan` | Domain | Plan d'abonnement avec tarif de base |
| `ServiceOption` | Domain | Option de service additionnelle |
| `Contract` | Domain | Contrat liant un membre à un plan et un club |
| `StandardPricing` | Domain | Implémentation standard de IPricingStrategy |
| `StudentPricing` | Domain | Implémentation étudiant de IPricingStrategy |
| `PromotionalPricing` | Domain | Implémentation promotionnelle de IPricingStrategy |

À ces classes s'ajoutent la hiérarchie d'exceptions (`DomainException` et ses dérivées) et la couche console (`MemberService`, `ContractService`, `EmployeeService` + les menus associés).

## Prérequis

- .NET 8 SDK
- MySQL 8.0 — requis pour les opérations de lecture/écriture. Si la base est
  indisponible, l'application reste fonctionnelle (menus accessibles) et affiche
  une erreur claire au lieu de planter.

## Configuration

Créer un fichier `.env` à la racine du dépôt :

```
MYSQL_CONNECTION_STRING=Server=localhost;Port=3306;Database=riada_db;Uid=root;Pwd=;
```

Sans ce fichier, la chaîne de connexion par défaut est utilisée.

## Lancer l'application

```bash
dotnet run --project Riada.Console
```

Ou depuis la racine :

```bash
dotnet build Riada.sln
dotnet run --project Riada.Console
```

## Lancer les tests

```bash
dotnet test Tests/Tests.csproj
```

## Diagramme UML

Le diagramme de classes complet se trouve dans `docs/` :

- `docs/Diagram-UML.svg` — **format recommandé** (vectoriel, zoomable sans perte)
- `docs/Diagram-UML.png` — image bitmap

Multiplicités présentes sur toutes les associations / agrégations / compositions ;
l'héritage, la réalisation et les dépendances n'en portent pas (règle UML).

