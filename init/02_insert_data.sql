-- Insert fake patients (capture actual DB-generated IDs for FK use)
INSERT INTO patients (patient_uid, name, age, contact_phone, contact_email, contact_address)
VALUES
  ('779553dd-de36-4b19-a414-834af7b3e319', 'Alice Carter', 39, '555-123-4567', 'alice@example.com', '123 Elm St, Springfield'),
  ('1e21bca6-c9ec-486e-a8bc-80533080c6bc', 'Bob Jensen', 34, '555-987-6543', 'bob@example.com', '456 Oak Ave, Rivertown'),
  ('a520a0d7-f575-4d8c-aff3-e9ebe6646b40', 'Charlie Kim', 46, '555-654-3210', 'charlie@example.com', '789 Pine Rd, Lakeside');

-- Insert conditions using subqueries to resolve FK by patient_uid
INSERT INTO conditions (patient_id, condition, notes, since, frequency, history, status)
VALUES
  -- Alice Carter
  ((SELECT id FROM patients WHERE patient_uid = '779553dd-de36-4b19-a414-834af7b3e319'), 'Penicillin allergy', null, null, null, null, null),
  ((SELECT id FROM patients WHERE patient_uid = '779553dd-de36-4b19-a414-834af7b3e319'), 'Migraines', 'Chronic, stress-induced', null, null, null, null),

  -- Bob Jensen
  ((SELECT id FROM patients WHERE patient_uid = '1e21bca6-c9ec-486e-a8bc-80533080c6bc'), 'Abdominal pain', null, null, null, null, null),
  ((SELECT id FROM patients WHERE patient_uid = '1e21bca6-c9ec-486e-a8bc-80533080c6bc'), 'Gastrointestinal issues', null, null, null, 'Recurrent', null),

  -- Charlie Kim
  ((SELECT id FROM patients WHERE patient_uid = 'a520a0d7-f575-4d8c-aff3-e9ebe6646b40'), 'Asthma', null, 'childhood', null, null, null),
  ((SELECT id FROM patients WHERE patient_uid = 'a520a0d7-f575-4d8c-aff3-e9ebe6646b40'), 'High blood pressure', null, null, 'occasional', null, null);

-- Insert fake attachments (linking by patient_id via lookup)
INSERT INTO attachments (attachment_uid, patient_id, filename, s3_key, document_type, metadata)
VALUES
  (
    '5240699f-3811-443b-b2a4-655a877a7795',
    (SELECT id FROM patients WHERE patient_uid = '779553dd-de36-4b19-a414-834af7b3e319'),
    'mri_brain.jpg',
    'patients/779553dd-de36-4b19-a414-834af7b3e319/MRI/5240699f-3811-443b-b2a4-655a877a7795/mri_brain.jpg',
    'MRI',
    '{"scan_date": "2024-04-01"}'
  ),
  (
    'f9f6cdcd-dbf5-4735-a456-f7146d5b030b',
    (SELECT id FROM patients WHERE patient_uid = '1e21bca6-c9ec-486e-a8bc-80533080c6bc'),
    'cat_abdomen.jpg',
    'patients/1e21bca6-c9ec-486e-a8bc-80533080c6bc/CAT_SCAN/f9f6cdcd-dbf5-4735-a456-f7146d5b030b/cat_abdomen.jpg',
    'CAT_SCAN',
    '{"scan_date": "2024-02-20"}'
  ),
  (
    'bc62b2d3-8b49-4c9c-b3cb-8efb57a18aa4',
    (SELECT id FROM patients WHERE patient_uid = 'a520a0d7-f575-4d8c-aff3-e9ebe6646b40'),
    'doctor_report.pdf',
    'patients/a520a0d7-f575-4d8c-aff3-e9ebe6646b40/DOCTOR_REPORT/bc62b2d3-8b49-4c9c-b3cb-8efb57a18aa4/doctor_report.pdf',
    'DOCTOR_REPORT',
    '{"scan_date": "2024-01-10"}'
  );
