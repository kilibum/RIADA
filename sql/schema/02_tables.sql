-- ============================================================================
-- RIADA — Schéma des tables (version examen — 6 tables)
-- Exécuter après 01_database.sql.
-- Tables retenues : uniquement celles couvertes par les entités Domain C#.
-- ============================================================================

USE riada_db;


-- ============================================================================
-- DOMAINE : CLUBS & PERSONNEL
-- ============================================================================

CREATE TABLE IF NOT EXISTS clubs (
    id                   INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    name                 VARCHAR(150)    NOT NULL,
    address_street       VARCHAR(255)    NOT NULL,
    address_city         VARCHAR(100)    NOT NULL,
    address_postal_code  VARCHAR(10)     NOT NULL,
    country              VARCHAR(50)     NOT NULL DEFAULT 'Belgium',
    is_open_24_7         TINYINT(1)      NOT NULL DEFAULT 1,
    opened_on            DATE            NOT NULL,
    operational_status   ENUM('open','temporarily_closed','permanently_closed')
                                         NOT NULL DEFAULT 'open',
    created_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                                   ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE IF NOT EXISTS employees (
    id              INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    last_name       VARCHAR(100)    NOT NULL,
    first_name      VARCHAR(100)    NOT NULL,
    email           VARCHAR(100)    NOT NULL,
    club_id         INT UNSIGNED    NOT NULL,
    role            ENUM('instructor','manager','receptionist','technician','intern','management')
                                    NOT NULL,
    monthly_salary  DECIMAL(10,2)   NULL,
    qualifications  TEXT            NULL,
    hired_on        DATE            NOT NULL,
    created_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                             ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_employees_email (email),
    CONSTRAINT fk_employees_club
        FOREIGN KEY (club_id) REFERENCES clubs (id)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    INDEX idx_employees_club (club_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- ============================================================================
-- DOMAINE : MEMBRES
-- ============================================================================

CREATE TABLE IF NOT EXISTS members (
    id                           INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    last_name                    VARCHAR(100)    NOT NULL,
    first_name                   VARCHAR(100)    NOT NULL,
    email                        VARCHAR(100)    NOT NULL,
    gender                       ENUM('male','female','unspecified')
                                                 NOT NULL DEFAULT 'unspecified',
    date_of_birth                DATE            NOT NULL,
    nationality                  VARCHAR(50)     NOT NULL DEFAULT 'Belgian',
    mobile_phone                 VARCHAR(20)     NULL,
    address_street               VARCHAR(255)    NULL,
    address_city                 VARCHAR(100)    NULL,
    address_postal_code          VARCHAR(10)     NULL,
    status                       ENUM('active','suspended','anonymized')
                                                 NOT NULL DEFAULT 'active',
    referral_member_id           INT UNSIGNED    NULL,
    primary_goal                 ENUM('weight_loss','muscle_gain','fitness','maintenance','other')
                                                 NULL,
    acquisition_source           ENUM('web_advertising','social_media','word_of_mouth','other')
                                                 NULL,
    medical_certificate_provided TINYINT(1)      NOT NULL DEFAULT 0,
    gdpr_consent_at              DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    marketing_consent            TINYINT(1)      NOT NULL DEFAULT 0,
    last_visit_date              DATE            NULL,
    total_visits                 INT UNSIGNED    NOT NULL DEFAULT 0,
    created_at                   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at                   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                                          ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_members_email (email),
    CONSTRAINT fk_members_referral
        FOREIGN KEY (referral_member_id) REFERENCES members (id)
        ON DELETE SET NULL ON UPDATE RESTRICT,
    -- Validation âge >= 16 appliquée dans la couche service C#.
    INDEX idx_members_referral (referral_member_id),
    INDEX idx_members_status   (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- ============================================================================
-- DOMAINE : ABONNEMENTS & CONTRATS
-- ============================================================================

CREATE TABLE IF NOT EXISTS subscription_plans (
    id                   INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    plan_name            VARCHAR(100)    NOT NULL,
    base_price           DECIMAL(10,2)   NOT NULL,
    commitment_months    INT UNSIGNED    NOT NULL DEFAULT 12,
    enrollment_fee       DECIMAL(10,2)   NOT NULL DEFAULT 19.99,
    limited_club_access  TINYINT(1)      NOT NULL DEFAULT 0,
    duo_pass_allowed     TINYINT(1)      NOT NULL DEFAULT 0,
    created_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                                   ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_subscription_plans_name (plan_name),
    CONSTRAINT chk_subscription_plans_base_price     CHECK (base_price > 0),
    CONSTRAINT chk_subscription_plans_enrollment_fee CHECK (enrollment_fee >= 0),
    CONSTRAINT chk_subscription_plans_commitment     CHECK (commitment_months > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE IF NOT EXISTS service_options (
    id             INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    option_name    VARCHAR(100)    NOT NULL,
    monthly_price  DECIMAL(10,2)   NOT NULL,
    created_at     DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at     DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                             ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_service_options_name (option_name),
    CONSTRAINT chk_service_options_monthly_price CHECK (monthly_price > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE IF NOT EXISTS contracts (
    id                   INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    member_id            INT UNSIGNED    NULL,
    plan_id              INT UNSIGNED    NOT NULL,
    home_club_id         INT UNSIGNED    NOT NULL,
    start_date           DATE            NOT NULL,
    end_date             DATE            NULL,
    contract_type        ENUM('fixed_term','open_ended')
                                         NOT NULL DEFAULT 'fixed_term',
    monthly_price        DECIMAL(10,2)   NULL,
    status               ENUM('active','suspended','expired','cancelled')
                                         NOT NULL DEFAULT 'active',
    cancelled_on         DATE            NULL,
    cancellation_reason  VARCHAR(255)    NULL,
    freeze_start_date    DATE            NULL,
    freeze_end_date      DATE            NULL,
    created_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    updated_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                                   ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    CONSTRAINT fk_contracts_member
        FOREIGN KEY (member_id)    REFERENCES members (id)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    CONSTRAINT fk_contracts_plan
        FOREIGN KEY (plan_id)      REFERENCES subscription_plans (id)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_contracts_club
        FOREIGN KEY (home_club_id) REFERENCES clubs (id)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT chk_contracts_dates
        CHECK (end_date IS NULL OR end_date > start_date),
    CONSTRAINT chk_contracts_cancelled_on
        CHECK (cancelled_on IS NULL OR cancelled_on >= start_date),
    CONSTRAINT chk_contracts_freeze_dates
        CHECK (freeze_end_date IS NULL OR freeze_end_date >= freeze_start_date),
    INDEX idx_contracts_member_status_end (member_id, status, end_date),
    INDEX idx_contracts_plan              (plan_id),
    INDEX idx_contracts_home_club         (home_club_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- ============================================================================
-- Vérification : inventaire des tables créées
-- ============================================================================
SELECT
    TABLE_NAME      AS `Table`,
    ENGINE          AS `Moteur`,
    TABLE_ROWS      AS `Lignes_estimées`,
    TABLE_COLLATION AS `Collation`
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
ORDER BY TABLE_NAME;
