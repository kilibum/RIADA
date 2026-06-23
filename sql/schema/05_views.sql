-- ============================================================================
-- RIADA — Vues applicatives
-- Exécuter après 03_triggers.sql.
-- Vues de lecture utilisées par les rapports et l'interface console.
-- ============================================================================

USE riada_db;

-- ============================================================================
-- VUE : membres actifs avec leur contrat en cours
-- ============================================================================
CREATE OR REPLACE VIEW v_membres_actifs AS
SELECT
    m.id                AS membre_id,
    m.last_name         AS nom,
    m.first_name        AS prenom,
    m.email,
    m.address_city      AS ville,
    m.total_visits      AS visites_totales,
    m.last_visit_date   AS derniere_visite,
    c.id                AS contrat_id,
    sp.plan_name        AS formule,
    sp.base_price       AS prix_base,
    cl.name             AS club_domicile,
    c.start_date        AS debut_contrat,
    c.end_date          AS fin_contrat
FROM members m
JOIN contracts c
    ON  c.member_id = m.id
    AND c.status    = 'active'
JOIN subscription_plans sp ON sp.id = c.plan_id
JOIN clubs cl              ON cl.id = c.home_club_id
WHERE m.status = 'active';


-- ============================================================================
-- VUE : détail complet des contrats
-- ============================================================================
CREATE OR REPLACE VIEW v_contrats_details AS
SELECT
    c.id                AS contrat_id,
    c.status            AS statut,
    c.contract_type     AS type_contrat,
    c.start_date        AS debut,
    c.end_date          AS fin,
    c.cancelled_on      AS annule_le,
    CONCAT(m.first_name, ' ', m.last_name) AS membre,
    m.email             AS email_membre,
    sp.plan_name        AS formule,
    sp.base_price       AS prix_base,
    sp.commitment_months AS engagement_mois,
    cl.name             AS club,
    cl.address_city     AS ville_club
FROM contracts c
LEFT JOIN members           m  ON m.id  = c.member_id
JOIN      subscription_plans sp ON sp.id = c.plan_id
JOIN      clubs              cl ON cl.id = c.home_club_id;


-- Vérification
SELECT TABLE_NAME AS vue, VIEW_DEFINITION IS NOT NULL AS valide
FROM   information_schema.VIEWS
WHERE  TABLE_SCHEMA = DATABASE()
ORDER  BY TABLE_NAME;
