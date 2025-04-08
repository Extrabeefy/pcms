import React, { useState, useEffect, useRef } from "react";
import "./App.css";

const API_BASE = "http://localhost:7000";
const PAGE_SIZE = 20;

const defaultCondition = {
  condition: "",
  notes: "",
  since: "",
  frequency: "",
  history: "",
  status: "",
};

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return token ? { Authorization: `Bearer ${token}` } : {};
};

const App = () => {
  const [patients, setPatients] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPatients, setTotalPatients] = useState(0);
  const [editingPatientId, setEditingPatientId] = useState(null);
  const fileInputRef = useRef(null);

  const [form, setForm] = useState({
    name: "",
    age: "",
    contactPhone: "",
    contactEmail: "",
    contactAddress: "",
    medicalHistory: [{ ...defaultCondition }],
    attachments: [],
    files: [],
    documentTypes: [],
  });

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) fetchPatients();
  }, [currentPage]);

  const fetchPatients = async () => {
    try {
      const res = await fetch(`${API_BASE}/patients?page=${currentPage}&pageSize=${PAGE_SIZE}`, {
        headers: getAuthHeaders()
      });

      if (res.status === 401) {
        setPatients([]);
        return;
      }

      const data = await res.json();

      if (Array.isArray(data.data)) {
        setPatients(data.data);
        setTotalPatients(data.total || data.data.length);
      } else {
        setPatients(data);
        setTotalPatients(data.length);
      }
    } catch (err) {
      console.error("Error fetching patients:", err);
      setPatients([]);
    }
  };

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleConditionChange = (index, field, value) => {
    const updated = [...form.medicalHistory];
    updated[index][field] = value;
    setForm({ ...form, medicalHistory: updated });
  };

  const addCondition = () => {
    setForm({ ...form, medicalHistory: [...form.medicalHistory, { ...defaultCondition }] });
  };

  const removeCondition = (index) => {
    const updated = [...form.medicalHistory];
    if (updated.length === 1) {
      updated[0] = { ...defaultCondition };
    } else {
      updated.splice(index, 1);
    }
    setForm({ ...form, medicalHistory: updated });
  };

  const handleFileChange = (e) => {
    const newFiles = Array.from(e.target.files);
    const newTypes = new Array(newFiles.length).fill("MRI");
    setForm((prev) => ({
      ...prev,
      files: [...prev.files, ...newFiles],
      documentTypes: [...prev.documentTypes, ...newTypes],
    }));
  };

  const handleRemoveFile = (index) => {
    const newFiles = [...form.files];
    const newTypes = [...form.documentTypes];
    newFiles.splice(index, 1);
    newTypes.splice(index, 1);
    setForm({ ...form, files: newFiles, documentTypes: newTypes });
  };

  const handleDocumentTypeChange = (index, value) => {
    const updated = [...form.documentTypes];
    updated[index] = value;
    setForm({ ...form, documentTypes: updated });
  };

  const handleSubmit = async () => {
    const isValidForm =
      form.name.trim() !== "" &&
      form.age &&
      form.contactPhone.trim() !== "" &&
      form.contactEmail.trim() !== "" &&
      form.contactAddress.trim() !== "" &&
      Array.isArray(form.medicalHistory);

    if (!isValidForm) {
      alert("Please fill out all required fields including medical conditions.");
      return;
    }

    const formData = new FormData();
    const patientPayload = {
      name: form.name,
      age: parseInt(form.age, 10),
      contactPhone: form.contactPhone,
      contactEmail: form.contactEmail,
      contactAddress: form.contactAddress,
    };

    if (form.medicalHistory && form.medicalHistory.length > 0) {
      patientPayload.medicalHistory = form.medicalHistory;
    }

    formData.append("patient", JSON.stringify(patientPayload));
    form.files.forEach((file) => formData.append("files", file));
    form.documentTypes.forEach((type) => formData.append("documentTypes", type));

    const method = editingPatientId ? "PUT" : "POST";
    const url = editingPatientId
      ? `${API_BASE}/patients/${editingPatientId}`
      : `${API_BASE}/patients`;

    const res = await fetch(url, {
      method,
      body: formData,
      headers: getAuthHeaders()
    });

    if (res.ok) {
      alert("Patient saved!");
      fetchPatients();
      clearForm();
    } else {
      alert("Failed to save patient.");
    }
  };

  const handleEdit = (patient) => {
    setEditingPatientId(patient.patientId || patient.patientUid);
    setForm({
      name: patient.name,
      age: patient.age,
      contactPhone: patient.contactPhone,
      contactEmail: patient.contactEmail,
      contactAddress: patient.contactAddress,
      medicalHistory: (patient.medicalHistory || []).filter(Boolean).map((c) => ({
        condition: c.condition || "",
        notes: c.notes || "",
        since: c.since || "",
        frequency: c.frequency || "",
        history: c.history || "",
        status: c.status || "",
      })),
      attachments: patient.attachments || [],
      files: [],
      documentTypes: [],
    });
  };

  const handleDelete = async (id) => {
    await fetch(`${API_BASE}/patients/${id}`, {
      method: "DELETE",
      headers: getAuthHeaders()
    });
    fetchPatients();
  };

  const clearForm = () => {
    setForm({
      name: "",
      age: "",
      contactPhone: "",
      contactEmail: "",
      contactAddress: "",
      medicalHistory: [{ ...defaultCondition }],
      attachments: [],
      files: [],
      documentTypes: [],
    });
    setEditingPatientId(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const totalPages = Math.ceil(totalPatients / PAGE_SIZE);

  const filteredPatients = patients.filter((p) => {
    const search = searchTerm.toLowerCase();
    return (
      p.name?.toLowerCase().includes(search) ||
      p.medicalHistory?.some((c) => c.condition?.toLowerCase().includes(search)) ||
      p.attachments?.some((a) => a.documentType?.toLowerCase().includes(search))
    );
  });

  return (
    <div className="container">
      <div className="auth-buttons">
        <button
          className="login-btn"
          onClick={async () => {
            try {
              const res = await fetch(`${API_BASE}/auth/dev-token`);
              const data = await res.json();
              localStorage.setItem("token", data.token);
              alert("Logged in!");
              fetchPatients();
            } catch {
              alert("Login failed");
            }
          }}
        >
          Login
        </button>
        <button
          className="logout-btn"
          onClick={() => {
            localStorage.removeItem("token");
            alert("Logged out.");
            setPatients([]);
          }}
        >
          Logout
        </button>
      </div>

      <h1>Patient Management</h1>

      <input
        className="search-input"
        placeholder="Search by name, condition, or document type"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
      />

      <table className="patient-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Age</th>
            <th>Email</th>
            <th>Conditions</th>
            <th>Documents</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {filteredPatients.map((p) => (
            <tr key={p.patientId || p.patientUid}>
              <td>{p.name}</td>
              <td>{p.age}</td>
              <td>{p.contactEmail}</td>
              <td>{(p.medicalHistory || []).filter(Boolean).map((c) => c.condition).join(", ")}</td>
              <td>{(p.attachments || []).map((a) => a.documentType).join(", ")}</td>
              <td>
                <button onClick={() => handleEdit(p)}>Edit</button>
                <button onClick={() => handleDelete(p.patientId || p.patientUid)}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button disabled={currentPage === 1} onClick={() => setCurrentPage((p) => p - 1)}>Previous</button>
        <span>Page {currentPage} of {totalPages}</span>
        <button disabled={currentPage === totalPages} onClick={() => setCurrentPage((p) => p + 1)}>Next</button>
      </div>

      {/* Patient Form */}
      <div className="form-section">
        <h2>{editingPatientId ? "Update Patient" : "Create Patient"}</h2>
        <div className="form-row">
          <input name="name" placeholder="Name" value={form.name} onChange={handleChange} />
          <input name="age" placeholder="Age" value={form.age} onChange={handleChange} />
          <input name="contactPhone" placeholder="Phone Number" value={form.contactPhone} onChange={handleChange} />
          <input name="contactEmail" placeholder="Email" value={form.contactEmail} onChange={handleChange} />
        </div>
        <input name="contactAddress" placeholder="Address" value={form.contactAddress} onChange={handleChange} />

        <h3>Medical History</h3>
        <div className="condition-grid">
          {form.medicalHistory.map((cond, i) => (
            <div className="condition-card" key={i}>
              <div className="remove-condition" onClick={() => removeCondition(i)}>❌</div>
              <div className="form-pair">
                <div><label>Condition</label><input value={cond.condition} onChange={(e) => handleConditionChange(i, "condition", e.target.value)} /></div>
                <div><label>Notes</label><input value={cond.notes} onChange={(e) => handleConditionChange(i, "notes", e.target.value)} /></div>
              </div>
              <div className="form-pair">
                <div><label>Since</label><input value={cond.since} onChange={(e) => handleConditionChange(i, "since", e.target.value)} /></div>
                <div><label>Frequency</label><input value={cond.frequency} onChange={(e) => handleConditionChange(i, "frequency", e.target.value)} /></div>
              </div>
              <div className="form-pair">
                <div><label>History</label><input value={cond.history} onChange={(e) => handleConditionChange(i, "history", e.target.value)} /></div>
                <div><label>Status</label><input value={cond.status} onChange={(e) => handleConditionChange(i, "status", e.target.value)} /></div>
              </div>
            </div>
          ))}
        </div>
        <button onClick={addCondition}>+ Add Condition</button>

        <h3>Attachments</h3>
        <input type="file" multiple onChange={handleFileChange} ref={fileInputRef} />
        {form.files.length > 0 && (
          <div>
            <h4>New Attachments</h4>
            {form.files.map((file, i) => (
              <div className="file-entry" key={i}>
                <label>{file.name}</label>
                <select value={form.documentTypes[i]} onChange={(e) => handleDocumentTypeChange(i, e.target.value)}>
                  <option value="MRI">MRI</option>
                  <option value="CAT_SCAN">CAT_SCAN</option>
                  <option value="DOCTOR_REPORT">DOCTOR_REPORT</option>
                  <option value="UNKNOWN">UNKNOWN</option>
                </select>
                <button className="remove-file-btn" onClick={() => handleRemoveFile(i)}>X</button>
              </div>
            ))}
          </div>
        )}
        {form.attachments.length > 0 && (
          <div className="attachment-section">
            <h4>Existing Attachments</h4>
            <ul>
              {form.attachments.map((att, i) => (
                <li key={i}>
                  {att.fileName} ({att.documentType}) —{" "}
                  {att.url ? (
                    <a href={att.url} target="_blank" rel="noreferrer">View</a>
                  ) : (
                    <em>No preview</em>
                  )}
                  <button
                    className="remove-file-btn"
                    style={{ marginLeft: "1rem" }}
                    onClick={async () => {
                      const patientId = editingPatientId || form.patientId;
                      if (!patientId) return alert("Missing patient ID");

                      const res = await fetch(`${API_BASE}/patients/${patientId}/attachments/${att.attachmentId}`, {
                        method: "DELETE",
                        headers: getAuthHeaders()
                      });

                      if (res.ok) {
                        setForm((prev) => ({
                          ...prev,
                          attachments: prev.attachments.filter((a) => a.attachmentId !== att.attachmentId)
                        }));
                      } else {
                        alert("Failed to delete attachment.");
                      }
                    }}
                  >
                    Delete
                  </button>
                </li>
              ))}
            </ul>
          </div>
        )}

        <div style={{ display: "flex", gap: "1rem", alignItems: "center" }}>
          <button onClick={handleSubmit}>
            {editingPatientId ? "Update Patient" : "Create Patient"}
          </button>
          <button className="close-btn" onClick={clearForm}>
            {editingPatientId ? "Close Patient" : "Clear Form"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default App;
