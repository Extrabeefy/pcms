using System;

namespace PCMSApi.Models
{
    /// <summary>
    /// Represents a medical condition associated with a patient.
    /// </summary>
    public class MedicalCondition
    {
        /// <summary>
        /// Gets or sets the unique identifier for the medical condition.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the patient to whom the medical condition belongs.
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Gets or sets the patient to whom the medical condition belongs.
        /// </summary>
        public Patient? Patient { get; set; }

        /// <summary>
        /// Gets or sets the name of the medical condition.
        /// </summary>
        public string Condition { get; set; } = null!;

        /// <summary>
        /// Gets or sets the notes about the medical condition.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the date since the medical condition has been present.
        /// </summary>
        public string? Since { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the medical condition.
        /// </summary>
        public string? Frequency { get; set; }

        /// <summary>
        /// Gets or sets the history of the medical condition.
        /// </summary>
        public string? History { get; set; }

        /// <summary>
        /// Gets or sets the status of the medical condition.
        /// </summary>
        public string? Status { get; set; }
    }
}