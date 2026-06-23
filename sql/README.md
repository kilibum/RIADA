# Riada — Base de données MySQL

Schéma de la base `riada_db` — 6 tables couvrant les entités métier de l'application console.

## Prérequis

| Composant | Version minimale |
|-----------|-----------------|
| MySQL     | 8.0             |
| Utilisateur | `root` ou compte avec `CREATE`, `GRANT` |

## Structure du dossier

```
sql/
├── schema/
│   ├── 01_database.sql         Création de la base riada_db
│   ├── 02_tables.sql           6 tables (CREATE TABLE IF NOT EXISTS)
│   ├── 03_triggers.sql         1 trigger — un seul contrat actif par membre
│   ├── 04_indexes.sql          Index supplémentaires de performance
│   └── 05_views.sql            2 vues applicatives (membres actifs, détail contrats)
└── seeds/
    ├── 01_reference_data.sql   Clubs, employés, plans, options de service
    └── 02_members_contracts.sql Membres et contrats
```

## Tables (6)

| Table | Entité C# | Domaine |
|-------|-----------|---------|
| `clubs` | `Club` | Clubs & personnel |
| `employees` | `Employee` | Clubs & personnel |
| `members` | `Member` | Membres |
| `subscription_plans` | `SubscriptionPlan` | Abonnements & contrats |
| `service_options` | `ServiceOption` | Abonnements & contrats |
| `contracts` | `Contract` | Abonnements & contrats |

## Ordre d'exécution — installation complète

```bash
mysql -u root -p < schema/01_database.sql
mysql -u root -p riada_db < schema/02_tables.sql
mysql -u root -p riada_db < schema/03_triggers.sql
mysql -u root -p riada_db < schema/04_indexes.sql
mysql -u root -p riada_db < schema/05_views.sql
mysql -u root -p riada_db < seeds/01_reference_data.sql
mysql -u root -p riada_db < seeds/02_members_contracts.sql
```

## Triggers

| Trigger | Table | Événement | Effet |
|---------|-------|-----------|-------|
| `trg_contracts_before_insert` | contracts | INSERT | Bloque si le membre a déjà un contrat actif |

## Vues

| Vue | Description |
|-----|-------------|
| `v_membres_actifs` | Membres avec leur contrat actif et formule |
| `v_contrats_details` | Contrats avec membre, plan et club |

## Connexion applicative

```
MYSQL_CONNECTION_STRING=Server=localhost;Port=3306;Database=riada_db;Uid=root;Pwd=;
```

Défini dans un fichier `.env` à la racine du projet — jamais dans le code source.
