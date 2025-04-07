namespace PCMSApi.Models
{
    /// <summary>
    /// Data transfer object for an attachment.
    /// </summary>
    public class AttachmentDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the attachment.
        /// </summary>
        public Guid AttachmentId { get; set; }

        /// <summary>
        /// Gets or sets the file name of the attachment.
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the document type of the attachment.
        /// </summary>
        public string DocumentType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time when the attachment was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Gets or sets the URL for accessing the attachment.
        /// </summary>
        public string? Url { get; set; }
    }
}