using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PCMSApi.Services;

/// <summary>
/// Service for handling S3 operations such as uploading, deleting, and generating presigned URLs.
/// </summary>
public class S3Service(IAmazonS3 s3Client, IConfiguration config, ILogger<S3Service> logger) : IS3Service
{
    private readonly IAmazonS3 _s3 = s3Client;
    private readonly ILogger<S3Service> _logger = logger;
    private readonly string _bucketName = config["S3:BucketName"] ?? "medical-files";
    private readonly string _publicHost = config["S3:PublicHost"] ?? "localhost";

    /// <summary>
    /// Uploads a file to S3.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="key">The S3 key where the file will be stored.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task UploadAsync(IFormFile file, string key, CancellationToken ct)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(_s3);
            await transferUtility.UploadAsync(uploadRequest, ct);

            _logger.LogInformation("Uploaded file to S3: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from S3.
    /// </summary>
    /// <param name="key">The S3 key of the file to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3.DeleteObjectAsync(request, ct);
            _logger.LogInformation("Deleted file from S3: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Generates a presigned URL for accessing a file in S3.
    /// </summary>
    /// <param name="s3Key">The S3 key of the file.</param>
    /// <param name="expiresIn">The duration for which the presigned URL is valid.</param>
    /// <returns>The presigned URL.</returns>
    public string GeneratePresignedUrl(string s3Key, TimeSpan expiresIn)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.Add(expiresIn),
                Protocol = Protocol.HTTP
            };

            var url = _s3.GetPreSignedURL(request);

            // Rewrite internal Docker host to a public one for frontend access
            var rewrittenUrl = url.Replace("localstack", _publicHost);

            _logger.LogDebug("Generated pre-signed URL: {Url}", rewrittenUrl);
            return rewrittenUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate pre-signed URL for key: {Key}", s3Key);
            return string.Empty;
        }
    }
}