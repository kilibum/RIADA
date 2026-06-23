-- ============================================================================
-- RIADA — Données de référence (stables)
-- Exécuter après schema/ complet.
-- Contient : clubs, employés, plans d'abonnement, options de service.
-- Idempotent : INSERT IGNORE — re-exécution sans erreur.
-- ============================================================================

USE riada_db;
SET SQL_SAFE_UPDATES = 0;

-- ============================================================================
-- CLUBS (5 centres)
-- ============================================================================
INSERT IGNORE INTO clubs (name, address_street, address_city, address_postal_code, country, is_open_24_7, opened_on, operational_status)
VALUES
('Riada Brussels', 'Rue de la Loi 145',    'Brussels', '1000', 'Belgium', 1, '2021-01-15', 'open'),
('Riada Liège',    'Boulevard d''Avroy 28','Liège',    '4000', 'Belgium', 1, '2021-03-18', 'open'),
('Riada Gand',     'Korenmarkt 11',        'Gand',     '9000', 'Belgium', 1, '2021-05-10', 'open'),
('Riada Anvers',   'Meir 77',              'Anvers',   '2000', 'Belgium', 1, '2021-07-02', 'open'),
('Riada Namur',    'Rue de Fer 54',        'Namur',    '5000', 'Belgium', 0, '2021-09-20', 'open');

-- ============================================================================
-- EMPLOYÉS (20 personnes — instructeurs, managers, techniciens, réceptionnistes)
-- ============================================================================
INSERT IGNORE INTO employees (last_name, first_name, email, club_id, role, monthly_salary, qualifications, hired_on)
VALUES
-- Brussels (club 1)
('Lambert',    'Aline',   'aline.lambert@riada.be',    1, 'manager',      3650.00, 'Gestion de club, MBA Sports',     '2021-01-10'),
('Freeman',    'Edward',  'edward.freeman@riada.be',   1, 'instructor',   2350.00, 'CPT Level 3, CrossFit L2',        '2021-01-20'),
('Johansen',   'Mads',    'mads.johansen@riada.be',    1, 'instructor',   2325.00, 'Group Fitness AFPA',              '2021-02-11'),
('Nowak',      'Lucas',   'lucas.nowak@riada.be',      1, 'technician',   2450.00, 'Maintenance équipements sportifs', '2021-04-12'),
('Vandenberg', 'Nora',    'nora.vandenberg@riada.be',  1, 'receptionist', 1990.00, 'Bilingue FR/NL, relation client', '2021-02-18'),
-- Liège (club 2)
('Dupuis',     'Noah',    'noah.dupuis@riada.be',      2, 'manager',      3620.00, 'Management opérationnel',         '2021-03-01'),
('Petersen',   'Simon',   'simon.petersen@riada.be',   2, 'instructor',   2280.00, 'Strength & Conditioning Coach',   '2021-03-09'),
('Ryan',       'Randall', 'randall.ryan@riada.be',     2, 'instructor',   2260.00, 'Combat Instructor, MMA',          '2021-03-26'),
('Leroy',      'Emma',    'emma.leroy@riada.be',       2, 'technician',   2475.00, 'Systèmes électriques, CVC',       '2021-05-15'),
('Aydin',      'Elif',    'elif.aydin@riada.be',       2, 'intern',        980.00, 'Master Management du Sport',      '2025-09-01'),
-- Gand (club 3)
('Vanacker',   'Lena',    'lena.vanacker@riada.be',    3, 'manager',      3640.00, 'Management opérationnel',         '2021-05-01'),
('Kumar',      'Henry',   'henry.kumar@riada.be',      3, 'instructor',   2290.00, 'Cardio Specialist, Spinning L2',  '2021-05-14'),
('King',       'Molly',   'molly.king@riada.be',       3, 'instructor',   2275.00, 'Dance Fitness, Zumba Pro',        '2021-06-01'),
('Benoit',     'Jules',   'jules.benoit@riada.be',     3, 'technician',   2430.00, 'Maintenance équipements sportifs', '2021-06-20'),
-- Anvers (club 4)
('De Smet',    'Arthur',  'arthur.desmet@riada.be',    4, 'manager',      3660.00, 'Gestion de club, EPSO',           '2021-07-01'),
('Olson',      'Krin',    'krin.olson@riada.be',       4, 'instructor',   2310.00, 'Mobility & Yoga RYT-200',         '2021-07-19'),
('Germain',    'Léa',     'lea.germain@riada.be',      4, 'receptionist', 1980.00, 'Relation client, CRM Salesforce', '2021-07-10'),
-- Namur (club 5)
('Martin',     'Clara',   'clara.martin@riada.be',     5, 'manager',      3600.00, 'Management opérationnel',         '2021-09-01'),
('Mills',      'Terri',   'terri.mills@riada.be',      5, 'instructor',   2250.00, 'HIIT Coach, Nutrition Level 1',   '2021-09-27'),
('Rousseau',   'Milan',   'milan.rousseau@riada.be',   5, 'receptionist', 1960.00, 'Front desk, trilingue FR/EN/NL',  '2021-10-05');

-- ============================================================================
-- PLANS D'ABONNEMENT (3 formules)
-- ============================================================================
INSERT IGNORE INTO subscription_plans (plan_name, base_price, commitment_months, enrollment_fee, limited_club_access, duo_pass_allowed)
VALUES
('Basic',   19.99, 12, 19.99, 1, 0),
('Comfort', 24.99, 12, 19.99, 0, 0),
('Premium', 29.99, 12, 29.99, 0, 1);

-- ============================================================================
-- OPTIONS DE SERVICE (3 options)
-- ============================================================================
INSERT IGNORE INTO service_options (option_name, monthly_price)
VALUES
('Boisson sportive', 5.99),
('Séances de massage', 9.99),
('Coaching personnel', 49.99);

-- Vérification
SELECT 'clubs'              AS `Table`, COUNT(*) AS lignes FROM clubs
UNION ALL SELECT 'employees',            COUNT(*) FROM employees
UNION ALL SELECT 'subscription_plans',   COUNT(*) FROM subscription_plans
UNION ALL SELECT 'service_options',      COUNT(*) FROM service_options;
