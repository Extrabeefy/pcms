namespace PCMSApi.Models
{
    /// <summary>
    /// Data transfer object for a patient.
    /// </summary>
    public class PatientDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the patient.
        /// </summary>
        public Guid PatientId { get; set; }

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

        /// <summary>
        /// Gets or sets the medical history of the patient.
        /// </summary>
        public List<MedicalCondition> MedicalHistory { get; set; } = new();

        /// <summary>
        /// Gets or sets the attachments associated with the patient.
        /// </summary>
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}