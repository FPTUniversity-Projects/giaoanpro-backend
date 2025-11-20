using Amazon.S3;
using Amazon.S3.Model;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using Microsoft.Extensions.Logging;

namespace giaoanpro_backend.Infrastructure._3PServices
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly ILogger<S3Service> _logger;

        public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            
            // Get bucket name from environment variable
            _bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME") 
                ?? throw new InvalidOperationException("AWS_S3_BUCKET_NAME environment variable is not configured");
            
            // Get region from environment variable, default to us-east-1
            _region = Environment.GetEnvironmentVariable("AWS_REGION") 
                ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") 
                ?? "us-east-1";

            _logger.LogInformation("S3Service initialized with bucket: {BucketName}, region: {Region}", _bucketName, _region);
        }

        public async Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType)
        {
            var key = $"lesson-plans/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{fileName}";

            _logger.LogInformation("Starting S3 upload - Bucket: {Bucket}, Key: {Key}, Size: {Size} bytes", 
                _bucketName, key, fileContent.Length);

            try
            {
                using var memoryStream = new MemoryStream(fileContent);
                
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = memoryStream,
                    ContentType = contentType,
                    //CannedACL = S3CannedACL.PublicRead // Make file publicly accessible
                };

                _logger.LogDebug("Sending PutObject request to S3...");

                // Add timeout using CancellationTokenSource
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var response = await _s3Client.PutObjectAsync(request, cts.Token);

                _logger.LogInformation("S3 upload successful - Status: {StatusCode}, ETag: {ETag}", 
                    response.HttpStatusCode, response.ETag);

                // Return the public URL
                var fileUrl = $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
                _logger.LogInformation("File URL generated: {FileUrl}", fileUrl);
                
                return fileUrl;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "S3 upload timed out after 60 seconds for key: {Key}", key);
                throw new InvalidOperationException($"S3 upload timed out. Please check your network connection and AWS configuration.", ex);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 error during upload - Status: {StatusCode}, Error: {ErrorCode}, Message: {Message}", 
                    ex.StatusCode, ex.ErrorCode, ex.Message);
                throw new InvalidOperationException($"AWS S3 error: {ex.Message} (ErrorCode: {ex.ErrorCode})", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during S3 upload for key: {Key}", key);
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                _logger.LogWarning("DeleteFileAsync called with empty URL, skipping");
                return;
            }

            try
            {
                // Extract key from URL
                var uri = new Uri(fileUrl);
                var key = uri.AbsolutePath.TrimStart('/');

                _logger.LogInformation("Deleting S3 file - Bucket: {Bucket}, Key: {Key}", _bucketName, key);

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                // Add timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await _s3Client.DeleteObjectAsync(request, cts.Token);

                _logger.LogInformation("S3 file deleted successfully: {Key}", key);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "S3 delete timed out for URL: {Url}", fileUrl);
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("S3 file not found (already deleted?): {Url}", fileUrl);
                }
                else
                {
                    _logger.LogError(ex, "AWS S3 error during delete - Status: {StatusCode}, Error: {ErrorCode}", 
                        ex.StatusCode, ex.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during S3 delete for URL: {Url}", fileUrl);
            }
        }
    }
}
