using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace HaircutHistoryApp.Services;

public class ImageService : IImageService
{
    private readonly BlobContainerClient? _containerClient;
    private readonly string _localImagePath;
    private readonly string _pendingUploadsPath;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogService _log;

    private const string Tag = "ImageService";
    private const string HttpClientName = "ImageService";

    public bool IsCloudStorageEnabled => _containerClient != null;

    public ImageService(ILogService logService, IHttpClientFactory httpClientFactory)
    {
        _log = logService;
        _httpClientFactory = httpClientFactory;
        _localImagePath = Path.Combine(FileSystem.AppDataDirectory, "images");
        _pendingUploadsPath = Path.Combine(FileSystem.AppDataDirectory, "pending_uploads.json");

        // Initialize Azure Blob Storage client if configured
        if (!string.IsNullOrEmpty(AzureStorageConfig.ConnectionString) &&
            AzureStorageConfig.ConnectionString != "YOUR_AZURE_STORAGE_CONNECTION_STRING")
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(AzureStorageConfig.ConnectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(AzureStorageConfig.ContainerName);
                _log.Info("Azure Blob Storage initialized successfully", Tag);
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to initialize Azure Storage", Tag, ex);
                _containerClient = null;
            }
        }

        // Ensure local directories exist
        if (!Directory.Exists(_localImagePath))
            Directory.CreateDirectory(_localImagePath);
    }

    public async Task<string?> PickImageAsync()
    {
        try
        {
            var results = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Select a haircut photo"
            });

            var result = results?.FirstOrDefault();
            if (result != null)
            {
                return await SaveImageLocallyAsync(result);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permission Denied",
                "Please grant photo library access in settings.", "OK");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to pick image", Tag, ex);
            await Shell.Current.DisplayAlertAsync("Error",
                "Failed to select image. Please try again.", "OK");
        }

        return null;
    }

    public async Task<string?> TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlertAsync("Not Supported",
                    "Camera capture is not supported on this device.", "OK");
                return null;
            }

            var result = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a haircut photo"
            });

            if (result != null)
            {
                return await SaveImageLocallyAsync(result);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permission Denied",
                "Please grant camera access in settings.", "OK");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to take photo", Tag, ex);
            await Shell.Current.DisplayAlertAsync("Error",
                "Failed to take photo. Please try again.", "OK");
        }

        return null;
    }

    public async Task<string?> UploadImageAsync(string localPath, string userId, string profileId)
    {
        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
        {
            _log.Warning($"Upload failed: file not found at {localPath}", Tag);
            return null;
        }

        // If cloud storage is not configured, save for later sync
        if (!IsCloudStorageEnabled)
        {
            _log.Debug("Cloud storage not configured, queuing for later sync", Tag);
            await AddToPendingUploadsAsync(localPath, userId, profileId);
            return localPath;
        }

        try
        {
            // Generate unique blob name
            var fileName = Path.GetFileName(localPath);
            var blobName = $"{userId}/{profileId}/{fileName}";

            var blobClient = _containerClient!.GetBlobClient(blobName);

            // Check file size
            var fileInfo = new FileInfo(localPath);
            if (fileInfo.Length > AzureStorageConfig.MaxImageSizeBytes)
            {
                _log.Warning($"File too large: {fileInfo.Length} bytes (max: {AzureStorageConfig.MaxImageSizeBytes})", Tag);
                await Shell.Current.DisplayAlertAsync("Image Too Large",
                    "Please select an image smaller than 5 MB.", "OK");
                return null;
            }

            // Determine content type
            var contentType = GetContentType(localPath);
            if (!AzureStorageConfig.AllowedContentTypes.Contains(contentType))
            {
                _log.Warning($"Invalid content type: {contentType}", Tag);
                await Shell.Current.DisplayAlertAsync("Invalid Format",
                    "Please select a JPEG, PNG, or WebP image.", "OK");
                return null;
            }

            // Upload to Azure
            using var fileStream = File.OpenRead(localPath);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    CacheControl = "public, max-age=31536000" // Cache for 1 year
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);

            var cloudUrl = blobClient.Uri.ToString();
            _log.Info($"Uploaded image to cloud: {blobName}", Tag);

            return cloudUrl;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to upload image to cloud", Tag, ex);
            // Queue for retry later
            await AddToPendingUploadsAsync(localPath, userId, profileId);
            return localPath; // Return local path as fallback
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Delete local file
            if (File.Exists(imageUrl))
            {
                File.Delete(imageUrl);
                _log.Debug($"Deleted local file: {imageUrl}", Tag);
            }

            // Delete from cloud if it's a cloud URL
            if (IsCloudStorageEnabled && IsCloudUrl(imageUrl))
            {
                var blobName = GetBlobNameFromUrl(imageUrl);
                if (!string.IsNullOrEmpty(blobName))
                {
                    var blobClient = _containerClient!.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                    _log.Debug($"Deleted from cloud: {blobName}", Tag);
                }
            }

            // Also delete from local cache if it exists
            var cachedPath = GetCachedPath(imageUrl);
            if (File.Exists(cachedPath))
            {
                File.Delete(cachedPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to delete image", Tag, ex);
            return false;
        }
    }

    public async Task<string> GetLocalPathAsync(string imageUrl)
    {
        // If it's already a local path, return it
        if (File.Exists(imageUrl))
        {
            return imageUrl;
        }

        // If it's not a cloud URL, return as-is
        if (!IsCloudUrl(imageUrl))
        {
            return imageUrl;
        }

        // Check if we have it cached locally
        var cachedPath = GetCachedPath(imageUrl);
        if (File.Exists(cachedPath))
        {
            return cachedPath;
        }

        // Download from cloud
        try
        {
            var cacheDir = Path.GetDirectoryName(cachedPath);
            if (!string.IsNullOrEmpty(cacheDir) && !Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(cachedPath);
            await stream.CopyToAsync(fileStream);

            _log.Debug($"Downloaded and cached image: {cachedPath}", Tag);
            return cachedPath;
        }
        catch (Exception ex)
        {
            _log.Warning("Failed to download image from cloud", Tag, ex);
            return imageUrl; // Return original URL as fallback
        }
    }

    public async Task<int> SyncPendingUploadsAsync(string userId)
    {
        if (!IsCloudStorageEnabled)
            return 0;

        var pending = await GetPendingUploadsAsync();
        var userPending = pending.Where(p => p.UserId == userId).ToList();

        if (!userPending.Any())
            return 0;

        int syncedCount = 0;

        foreach (var item in userPending)
        {
            if (!File.Exists(item.LocalPath))
            {
                pending.Remove(item);
                continue;
            }

            try
            {
                var cloudUrl = await UploadToCloudAsync(item.LocalPath, item.UserId, item.ProfileId);
                if (!string.IsNullOrEmpty(cloudUrl) && IsCloudUrl(cloudUrl))
                {
                    pending.Remove(item);
                    syncedCount++;
                    _log.Info($"Synced pending upload: {item.LocalPath}", Tag);
                }
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to sync pending upload", Tag, ex);
            }
        }

        await SavePendingUploadsAsync(pending);
        return syncedCount;
    }

    #region Private Helper Methods

    private async Task<string> SaveImageLocallyAsync(FileResult photo)
    {
        var fileName = $"haircut_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.jpg";
        var fullPath = Path.Combine(_localImagePath, fileName);

        using var sourceStream = await photo.OpenReadAsync();
        using var destStream = File.OpenWrite(fullPath);
        await sourceStream.CopyToAsync(destStream);

        _log.Debug($"Saved image locally: {fullPath}", Tag);
        return fullPath;
    }

    private async Task<string?> UploadToCloudAsync(string localPath, string userId, string profileId)
    {
        if (!IsCloudStorageEnabled || !File.Exists(localPath))
            return null;

        var fileName = Path.GetFileName(localPath);
        var blobName = $"{userId}/{profileId}/{fileName}";
        var blobClient = _containerClient!.GetBlobClient(blobName);

        using var fileStream = File.OpenRead(localPath);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(localPath),
                CacheControl = "public, max-age=31536000"
            }
        };

        await blobClient.UploadAsync(fileStream, uploadOptions);
        return blobClient.Uri.ToString();
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    private static bool IsCloudUrl(string url)
    {
        return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
    }

    private string? GetBlobNameFromUrl(string url)
    {
        if (_containerClient == null)
            return null;

        var containerUrl = _containerClient.Uri.ToString();
        if (url.StartsWith(containerUrl))
        {
            return url.Substring(containerUrl.Length).TrimStart('/');
        }
        return null;
    }

    private string GetCachedPath(string url)
    {
        // Use SHA256 for deterministic hash across app restarts and platforms
        var hash = ComputeSha256Hash(url);
        var extension = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrEmpty(extension))
            extension = ".jpg";

        return Path.Combine(_localImagePath, "cache", $"{hash}{extension}");
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        // Take first 16 characters (64 bits) for a reasonable cache key length
        return Convert.ToHexString(bytes)[..16];
    }

    private async Task AddToPendingUploadsAsync(string localPath, string userId, string profileId)
    {
        var pending = await GetPendingUploadsAsync();
        pending.Add(new PendingUpload
        {
            LocalPath = localPath,
            UserId = userId,
            ProfileId = profileId,
            CreatedAt = DateTime.UtcNow
        });
        await SavePendingUploadsAsync(pending);
    }

    private async Task<List<PendingUpload>> GetPendingUploadsAsync()
    {
        try
        {
            if (File.Exists(_pendingUploadsPath))
            {
                var json = await File.ReadAllTextAsync(_pendingUploadsPath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<PendingUpload>>(json) ?? new List<PendingUpload>();
            }
        }
        catch (Exception ex)
        {
            _log.Warning("Error reading pending uploads", Tag, ex);
        }
        return new List<PendingUpload>();
    }

    private async Task SavePendingUploadsAsync(List<PendingUpload> pending)
    {
        try
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(pending);
            await File.WriteAllTextAsync(_pendingUploadsPath, json);
        }
        catch (Exception ex)
        {
            _log.Warning("Error saving pending uploads", Tag, ex);
        }
    }

    #endregion

    private class PendingUpload
    {
        public string LocalPath { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ProfileId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
