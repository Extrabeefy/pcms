using PCMSApi.Models;

namespace PCMSApi.Services
{
    /// <summary>
    /// Interface for attachment service operations.
    /// </summary>
    public interface IAttachmentService
    {
        /// <summary>
        /// Uploads a file to S3 and returns the created Attachment object.
        /// </summary>
        /// <param name="patientId">The ID of the patient to whom the file belongs.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="documentType">The type of document being uploaded.</param>
        /// <returns>The created Attachment object.</returns>
        Task<Attachment> UploadAsync(int patientId, IFormFile file, string documentType);
    }
}