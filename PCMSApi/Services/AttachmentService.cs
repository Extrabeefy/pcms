using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PCMSApi.Models;

namespace PCMSApi.Services
{
    /// <summary>
    /// Service for handling attachment operations such as uploading files to S3.
    /// </summary>
    public class AttachmentService(IAmazonS3 s3, IConfiguration config, ILogger<AttachmentService> logger) : IAttachmentService
    {
        private readonly IAmazonS3 _s3 = s3;
        private readonly IConfiguration _config = config;
        private readonly ILogger<AttachmentService> _logger = logger;

        /// <summary>
        /// Uploads a file to S3 and returns the created Attachment object.
        /// </summary>
        /// <param name="patientId">The ID of the patient to whom the file belongs.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="documentType">The type of document being uploaded.</param>
        /// <returns>The created Attachment object.</returns>
        public async Task<Attachment> UploadAsync(int patientId, IFormFile file, string documentType)
        {
            var bucketName = _config["S3:BucketName"] ?? "medical-files";

            var attachmentId = Guid.NewGuid();
            var s3Key = $"patients/{patientId}/{documentType}/{attachmentId}/{file.FileName}";

            try
            {
                using var stream = file.OpenReadStream();
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = s3Key,
                    BucketName = bucketName,
                    ContentType = file.ContentType
                };

                var transferUtility = new TransferUtility(_s3);
                await transferUtility.UploadAsync(uploadRequest);

                _logger.LogInformation("Successfully uploaded file to S3: {Key}", s3Key);

                return new Attachment
                {
                    AttachmentId = attachmentId,
                    PatientId = patientId,
                    FileName = file.FileName,
                    S3Key = s3Key,
                    DocumentType = documentType,
                    UploadedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>
                    {
                        ["original_filename"] = file.FileName,
                        ["document_type"] = documentType,
                        ["uploaded_by"] = "system"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to S3: {Key}", s3Key);
                throw;
            }
        }
    }
}