-- ============================================================================
-- RIADA — Membres et contrats
-- Exécuter après 01_reference_data.sql.
-- ============================================================================

USE riada_db;
SET SQL_SAFE_UPDATES = 0;

-- ============================================================================
-- MEMBRES (10 adhérents)
-- ============================================================================
INSERT IGNORE INTO members
    (last_name, first_name, email, gender, date_of_birth, nationality,
     mobile_phone, address_street, address_city, address_postal_code,
     status, primary_goal, acquisition_source,
     medical_certificate_provided, marketing_consent, total_visits)
VALUES
('Moreau',    'Lucas',    'lucas.moreau@email.com',    'male',        '1995-04-12', 'French',
 '+32470111001', 'Rue Neuve 12',       'Brussels', '1000', 'active', 'muscle_gain',  'social_media',    1, 1, 14),
('Dubois',    'Emma',     'emma.dubois@email.com',     'female',      '1998-07-23', 'Belgian',
 '+32470111002', 'Rue du Trône 45',    'Brussels', '1050', 'active', 'weight_loss',  'web_advertising', 1, 0,  8),
('Nguyen',    'Kevin',    'kevin.nguyen@email.com',    'male',        '1990-11-05', 'Belgian',
 '+32470111003', 'Quai de Rome 8',     'Liège',    '4000', 'active', 'fitness',      'word_of_mouth',   1, 1, 31),
('Bernard',   'Sophie',   'sophie.bernard@email.com',  'female',      '2001-02-14', 'Belgian',
 '+32470111004', 'Korenmarkt 3',       'Gand',     '9000', 'active', 'fitness',      'social_media',    0, 1,  5),
('Laurent',   'Thomas',   'thomas.laurent@email.com',  'male',        '1988-09-30', 'French',
 '+32470111005', 'Meir 20',            'Anvers',   '2000', 'active', 'maintenance',  'word_of_mouth',   1, 0, 52),
('Peeters',   'Julie',    'julie.peeters@email.com',   'female',      '1993-06-17', 'Belgian',
 '+32470111006', 'Rue de Fer 10',      'Namur',    '5000', 'active', 'weight_loss',  'web_advertising', 1, 1, 19),
('Jacobs',    'Nathan',   'nathan.jacobs@email.com',   'male',        '2000-12-01', 'Belgian',
 '+32470111007', 'Avenue Louise 88',   'Brussels', '1050', 'active', 'muscle_gain',  'social_media',    1, 1,  3),
('Fontaine',  'Camille',  'camille.fontaine@email.com','female',      '1997-03-28', 'Belgian',
 '+32470111008', 'Boulevard Saucy 14', 'Liège',    '4000', 'active', 'fitness',      'word_of_mouth',   1, 0, 27),
('De Backer', 'Arne',     'arne.debacker@email.com',   'male',        '1985-08-09', 'Belgian',
 '+32470111009', 'Coupure Links 55',   'Gand',     '9000', 'active', 'maintenance',  'web_advertising', 1, 1, 41),
('Simon',     'Chloé',    'chloe.simon@email.com',     'female',      '2002-05-20', 'Belgian',
 '+32470111010', 'Frankrijklei 77',    'Anvers',   '2000', 'active', 'weight_loss',  'social_media',    0, 1,  1);


-- ============================================================================
-- CONTRATS (10 contrats — 8 actifs, 1 annulé, 1 expiré)
-- Règle métier : un seul contrat actif par membre (contrôlé par trigger)
-- ============================================================================
INSERT IGNORE INTO contracts
    (member_id, plan_id, home_club_id, start_date, end_date,
     contract_type, status, cancelled_on, cancellation_reason)
VALUES
-- Membres 1-8 : contrats actifs
(1,  2, 1, '2025-01-01', '2026-01-01', 'fixed_term', 'active',    NULL, NULL),
(2,  1, 1, '2025-03-01', '2026-03-01', 'fixed_term', 'active',    NULL, NULL),
(3,  3, 2, '2024-06-15', '2025-06-15', 'fixed_term', 'active',    NULL, NULL),
(4,  1, 3, '2025-09-01', '2026-09-01', 'fixed_term', 'active',    NULL, NULL),
(5,  3, 4, '2023-04-01', '2024-04-01', 'fixed_term', 'active',    NULL, NULL),
(6,  2, 5, '2025-02-01', '2026-02-01', 'fixed_term', 'active',    NULL, NULL),
(7,  1, 1, '2026-01-15', '2027-01-15', 'fixed_term', 'active',    NULL, NULL),
(8,  2, 2, '2025-07-01', '2026-07-01', 'fixed_term', 'active',    NULL, NULL),
-- Membre 9 : contrat annulé
(9,  1, 3, '2024-01-01', '2025-01-01', 'fixed_term', 'cancelled', '2024-06-15', 'Déménagement hors Belgique'),
-- Membre 10 : contrat expiré
(10, 2, 4, '2024-03-01', '2025-03-01', 'fixed_term', 'expired',   NULL, NULL);


-- Vérification
SELECT status, COUNT(*) AS nb FROM contracts GROUP BY status;
