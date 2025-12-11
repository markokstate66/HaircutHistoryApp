namespace HaircutHistoryApp.Services;

/// <summary>
/// Azure Blob Storage configuration for image uploads.
/// </summary>
public static class AzureStorageConfig
{
    // Azure Storage Account connection string
    // Get from: Azure Portal > Storage Account > Access keys
    // IMPORTANT: For production, use Azure Key Vault or secure storage
    public const string ConnectionString = "YOUR_AZURE_STORAGE_CONNECTION_STRING";

    // Container name for haircut images
    public const string ContainerName = "haircut-images";

    // Maximum image size in bytes (5 MB)
    public const long MaxImageSizeBytes = 5 * 1024 * 1024;

    // Allowed image content types
    public static readonly string[] AllowedContentTypes = new[]
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };
}
