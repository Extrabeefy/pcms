# 🩺 Patient Clinical Management System (PCMS)

PCMS is a full-stack system for managing patient records and their clinical attachments (e.g., MRI scans, CAT scans, doctor reports).  
It consists of a modern React UI, a .NET 9 Minimal API backend, and LocalStack for local AWS S3 simulation.

---

## 🖼️ Architecture

The following diagram outlines the high-level architecture and flow of data between components:

![Design Diagram](pcms.drawio.png)

---

## 📁 Project Structure

```
PCMSApi/        # .NET 9 Minimal API for managing patients
pcms-ui/        # React frontend for interacting with the API
docker-compose.yml
Makefile        # Command shortcuts for running LocalStack & uploading files
```

---

## 🧪 Features

### ✅ WebAPI (.NET 9 Minimal API)
- Built using the latest .NET 9 Minimal API style for performance, simplicity, and clarity
- Supports full CRUD operations on patient records
- Uploads and manages clinical attachments (MRI, CAT Scan, Doctor Reports)
- Stores metadata in Postgres and files in S3-compatible storage (LocalStack)
- JWT-based static token authentication for development use

### ✅ React UI (`pcms-ui`)
- Responsive, accessible interface with full Create, Read, Update, Delete support
- Upload multiple files with associated document types via dropdown
- Search/filter patients by name, condition, or document type
- User authentication with login/logout buttons

### ✅ LocalStack (Simulated AWS)
- Emulates AWS S3 fully locally, no cloud dependency
- **Why LocalStack?** Using LocalStack is better than managing files in mounted volumes because:
  - Mimics production-like S3 behavior (folder structure, presigned URLs, etc.)
  - Enables testing lifecycle events and security boundaries without touching real cloud resources
  - Great for team workflows, CI/CD, and integration tests

### ✅ PostgreSQL (Database)
- Stores structured patient data including medical history and attachment metadata
- Efficiently handles relational data models with foreign keys (e.g., one patient → many attachments or conditions)
- **Why PostgreSQL?**
  - Chosen over SQLite for its full-featured relational capabilities, concurrent access support, and robust transaction handling
  - Scales easily with future needs like audit logging, advanced querying, and indexing
  - Supported natively in cloud and containerized environments, making it a production-ready choice even during local development
---

## 🛠️ How to Run 

### 🍎 MacOS Bash

### 🔄 Run Backend + Frontend + LocalStack

```bash
make run
```

- Spins up LocalStack and required containers using `docker-compose`
- Starts the database, API, and UI

---

### 📂 Upload Fake Scan Files to S3

```bash
make init-s3
```

- Creates the `medical-files` bucket
- Upload sample files into appropriate folders under patient UIDs

### 🪟 Windows (PowerShell)

```powershell
.\run.ps1 -Command run
.\run.ps1 -Command init-s3

- Starts everything (api, UI, localstack, db)
- Initializes the fake medical scan uploads into S3

---

## 🔐 Authentication

- When you press **Login** in the UI, it fetches a static dev token from `/auth/dev-token`
- This token is stored in `localStorage` and attached to all future requests
- Logout clears the token and disables access

---

## 📦 Dependencies

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Node.js & npm](https://nodejs.org/) (for `pcms-ui`)
- [Docker & Docker Compose](https://www.docker.com/)
- [Make](https://formulae.brew.sh/formula/make) (included by default on macOS)

---

## 📥 API Endpoints

| Method | Endpoint                                 | Description                                  |
|--------|------------------------------------------|----------------------------------------------|
| GET    | `/patients`                              | List patients (paginated)                    |
| POST   | `/patients`                              | Create patient (with attachments)            |
| PUT    | `/patients/{id}`                         | Update patient (with attachments)            |
| DELETE | `/patients/{id}`                         | Delete patient and all attachments           |
| DELETE | `/patients/{id}/attachments/{attachmentId}` | Delete a specific attachment              |
| GET    | `/auth/dev-token`                        | Get a static development token               |

---

## 🧠 Notes

- S3 integration works fully offline via LocalStack
- The UI checks for auth token before loading data