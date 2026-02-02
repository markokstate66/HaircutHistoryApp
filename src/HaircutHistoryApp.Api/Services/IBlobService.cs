namespace HaircutHistoryApp.Api.Services;

/// <summary>
/// Service for interacting with Azure Blob Storage.
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// Generates a SAS URL for uploading a blob.
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME content type</param>
    /// <returns>Upload URL, blob URL, and expiration time</returns>
    Task<(string UploadUrl, string BlobUrl, DateTime ExpiresAt)> GetUploadUrlAsync(string fileName, string contentType);

    /// <summary>
    /// Deletes a blob by its URL.
    /// </summary>
    /// <param name="blobUrl">The blob URL</param>
    Task DeleteBlobAsync(string blobUrl);

    /// <summary>
    /// Checks if a blob exists.
    /// </summary>
    /// <param name="blobUrl">The blob URL</param>
    Task<bool> BlobExistsAsync(string blobUrl);
}
