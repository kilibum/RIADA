-- ============================================================================
-- RIADA — Index supplémentaires de performance
-- Exécuter après 02_tables.sql.
-- Les index PRIMARY KEY et FK sont déjà créés dans 02_tables.sql.
-- Ce fichier ajoute les index couvrants pour les requêtes applicatives courantes.
-- ============================================================================

USE riada_db;

-- Procédure pour créer les index en ignorant les doublons (erreur 1061)
DROP PROCEDURE IF EXISTS riada_create_indexes;

DELIMITER //
CREATE PROCEDURE riada_create_indexes()
BEGIN
    DECLARE CONTINUE HANDLER FOR 1061 BEGIN END;

    -- MEMBRES : recherche par email, date de naissance, objectif
    CREATE INDEX idx_members_email         ON members (email);
    CREATE INDEX idx_members_date_of_birth ON members (date_of_birth);
    CREATE INDEX idx_members_primary_goal  ON members (primary_goal);
    CREATE INDEX idx_members_last_visit    ON members (last_visit_date);

    -- CONTRATS : filtrage par statut et dates
    CREATE INDEX idx_contracts_status     ON contracts (status);
    CREATE INDEX idx_contracts_start_date ON contracts (start_date);
    CREATE INDEX idx_contracts_end_date   ON contracts (end_date);

    -- EMPLOYÉS : tri par rôle
    CREATE INDEX idx_employees_role ON employees (role);
END //
DELIMITER ;

CALL riada_create_indexes();
DROP PROCEDURE IF EXISTS riada_create_indexes;

-- Vérification
SELECT
    TABLE_NAME   AS `Table`,
    INDEX_NAME   AS `Index`,
    COLUMN_NAME  AS `Colonne`,
    NON_UNIQUE   AS `Non_unique`
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
  AND INDEX_NAME  <> 'PRIMARY'
ORDER BY TABLE_NAME, INDEX_NAME;
