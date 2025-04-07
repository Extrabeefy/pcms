using System;
using System.Collections.Generic;

namespace PCMSApi.Models
{
    /// <summary>
    /// Represents an attachment associated with a patient.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the attachment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the attachment.
        /// </summary>
        public Guid AttachmentId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the file name of the attachment.
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the S3 key of the attachment.
        /// </summary>
        public string S3Key { get; set; } = null!;

        /// <summary>
        /// Gets or sets the document type of the attachment.
        /// </summary>
        public string DocumentType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time when the attachment was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the ID of the patient to whom the attachment belongs.
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Gets or sets the patient to whom the attachment belongs.
        /// </summary>
        public Patient? Patient { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the attachment.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}