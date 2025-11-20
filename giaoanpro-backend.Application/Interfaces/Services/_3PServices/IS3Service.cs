namespace giaoanpro_backend.Application.Interfaces.Services._3PServices
{
    public interface IS3Service
    {
        /// <summary>
        /// Upload a file to S3 bucket
        /// </summary>
        /// <param name="fileName">File name with extension</param>
        /// <param name="fileContent">File content as byte array</param>
        /// <param name="contentType">MIME content type</param>
        /// <returns>Public URL of uploaded file</returns>
        Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType);

        /// <summary>
        /// Delete a file from S3 bucket
        /// </summary>
        /// <param name="fileUrl">Full URL or file key</param>
        Task DeleteFileAsync(string fileUrl);
    }
}
