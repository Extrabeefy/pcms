using Microsoft.AspNetCore.Http;

namespace PCMSApi.Services
{
    /// <summary>
    /// Interface for S3 service operations.
    /// </summary>
    public interface IS3Service
    {
        /// <summary>
        /// Uploads a file to S3.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="key">The S3 key where the file will be stored.</param>
        /// <param name="ct">Cancellation token.</param>
        Task UploadAsync(IFormFile file, string key, CancellationToken ct);

        /// <summary>
        /// Deletes a file from S3.
        /// </summary>
        /// <param name="key">The S3 key of the file to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        Task DeleteAsync(string key, CancellationToken ct);

        /// <summary>
        /// Generates a presigned URL for accessing a file in S3.
        /// </summary>
        /// <param name="s3Key">The S3 key of the file.</param>
        /// <param name="expiry">The duration for which the presigned URL is valid.</param>
        /// <returns>The presigned URL.</returns>
        string GeneratePresignedUrl(string s3Key, TimeSpan expiry);
    }
}