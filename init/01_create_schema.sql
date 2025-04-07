-- Enable full-text search on JSONB
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Patients table
CREATE TABLE patients (
    id SERIAL PRIMARY KEY,
    patient_uid UUID NOT NULL UNIQUE, -- app-facing GUID
    name TEXT NOT NULL,
    age INTEGER,
    contact_phone TEXT,
    contact_email TEXT,
    contact_address TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_patients_name ON patients (name);
CREATE INDEX idx_patients_patient_uid ON patients (patient_uid);

-- Attachments table
CREATE TABLE attachments (
    id SERIAL PRIMARY KEY,
    attachment_uid UUID NOT NULL UNIQUE,
    patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
    filename TEXT NOT NULL,
    s3_key TEXT NOT NULL,
    document_type TEXT CHECK (document_type IN ('MRI', 'CAT_SCAN', 'DOCTOR_REPORT')),
    uploaded_at TIMESTAMPTZ DEFAULT NOW(),
    metadata JSONB
);

CREATE INDEX idx_attachments_document_type ON attachments (document_type);
CREATE INDEX idx_attachments_patient_id ON attachments (patient_id);

-- Medical Conditions table
CREATE TABLE conditions (
    id SERIAL PRIMARY KEY,
    patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
    condition TEXT NOT NULL,
    notes TEXT,
    since TEXT,
    frequency TEXT,
    history TEXT,
    status TEXT
);

CREATE INDEX idx_conditions_patient_id ON conditions (patient_id);
CREATE INDEX idx_conditions_condition ON conditions (condition);