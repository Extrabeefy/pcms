using System;
using System.Collections.Generic;

namespace PCMSApi.Models
{
    /// <summary>
    /// Represents a patient in the system.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Gets or sets the unique identifier for the patient.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the patient.
        /// </summary>
        public Guid PatientUid { get; set; } = Guid.NewGuid();

        // Patient profile fields

        /// <summary>
        /// Gets or sets the name of the patient.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the age of the patient.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the contact phone number of the patient.
        /// </summary>
        public string ContactPhone { get; set; } = null!;

        /// <summary>
        /// Gets or sets the contact email of the patient.
        /// </summary>
        public string ContactEmail { get; set; } = null!;

        /// <summary>
        /// Gets or sets the contact address of the patient.
        /// </summary>
        public string ContactAddress { get; set; } = null!;

        // Relationships

        /// <summary>
        /// Gets or sets the medical history of the patient.
        /// </summary>
        public List<MedicalCondition> MedicalHistory { get; set; } = new();

        /// <summary>
        /// Gets or sets the attachments associated with the patient.
        /// </summary>
        public List<Attachment> Attachments { get; set; } = new();
    }
}