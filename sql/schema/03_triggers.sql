-- ============================================================================
-- RIADA — Triggers
-- Exécuter après 02_tables.sql.
-- Maintien automatique de la cohérence des données métier.
-- ============================================================================

USE riada_db;

DELIMITER $$

-- ============================================================================
-- DOMAINE CONTRATS : contrôle avant insertion
-- ============================================================================

-- Empêche la création d'un second contrat actif pour le même membre
CREATE TRIGGER trg_contracts_before_insert
BEFORE INSERT ON contracts
FOR EACH ROW
BEGIN
    DECLARE active_count INT;
    IF NEW.member_id IS NOT NULL THEN
        SELECT COUNT(*) INTO active_count
        FROM   contracts
        WHERE  member_id = NEW.member_id
          AND  status    = 'active';
        IF active_count > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Ce membre possède déjà un contrat actif.';
        END IF;
    END IF;
END$$

DELIMITER ;

-- Vérification
SELECT TRIGGER_NAME, EVENT_MANIPULATION, EVENT_OBJECT_TABLE, ACTION_TIMING
FROM   information_schema.TRIGGERS
WHERE  TRIGGER_SCHEMA = DATABASE()
ORDER  BY EVENT_OBJECT_TABLE, ACTION_TIMING;
