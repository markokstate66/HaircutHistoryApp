using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace HaircutHistoryApp.Api.Services;

/// <summary>
/// Implementation of Azure Blob Storage operations.
/// </summary>
public class BlobService : IBlobService
{
    private readonly BlobContainerClient _containerClient;
    private readonly string _containerName;

    public BlobService(string connectionString, string containerName)
    {
        _containerName = containerName;
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<(string UploadUrl, string BlobUrl, DateTime ExpiresAt)> GetUploadUrlAsync(string fileName, string contentType)
    {
        // Generate a unique blob name
        var extension = Path.GetExtension(fileName);
        var blobName = $"{Guid.NewGuid()}{extension}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Create SAS token for upload (valid for 15 minutes)
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return (sasUri.ToString(), blobClient.Uri.ToString(), expiresAt);
    }

    public async Task DeleteBlobAsync(string blobUrl)
    {
        var blobName = GetBlobNameFromUrl(blobUrl);
        if (!string.IsNullOrEmpty(blobName))
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }

    public async Task<bool> BlobExistsAsync(string blobUrl)
    {
        var blobName = GetBlobNameFromUrl(blobUrl);
        if (string.IsNullOrEmpty(blobName)) return false;

        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
    }

    private string? GetBlobNameFromUrl(string blobUrl)
    {
        try
        {
            var uri = new Uri(blobUrl);
            // URL format: https://account.blob.core.windows.net/container/blobname
            var segments = uri.Segments;
            if (segments.Length >= 3)
            {
                // Last segment is the blob name
                return Uri.UnescapeDataString(segments[^1]);
            }
        }
        catch
        {
            // Invalid URL
        }

        return null;
    }
}
