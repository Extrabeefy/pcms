# 🩺 Patient Clinical Management System (PCMS)

PCMS is a full-stack system for managing patient records and their clinical attachments (e.g., MRI scans, CAT scans, doctor reports).  
It consists of a modern React UI, a .NET 9 Minimal API backend, and LocalStack for local AWS S3 simulation.

---

## 🖼️ Architecture

The following diagram outlines the high-level architecture and flow of data between components:


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
- Create, update, delete patients
- Upload and delete clinical attachments (MRI, CAT Scan, Doctor Reports)
- Stores attachments in S3 (simulated via LocalStack)
- JWT-based static token authentication for local dev

### ✅ React UI (`pcms-ui`)
- Create/Edit/Delete patients
- Upload multiple files with document type tags
- Search/filter by name, condition, or document type
- Login/logout with a static token
- Clean, responsive layout with full CRUD support

### ✅ LocalStack
- Simulates S3 locally at `localhost:4566`
- No need for real AWS credentials

---

## 🛠️ How to Run

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

This will:
- Create the `medical-files` bucket
- Upload sample files into appropriate folders under patient UIDs

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

- The backend uses an in-memory or PostgreSQL database depending on your `appsettings.json`
- S3 integration works fully offline via LocalStack
- The UI checks for auth token before loading data

---

## 🤝 License

MIT – Use freely, modify boldly.
