using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;

namespace HaircutHistoryApp.Services;

public class ImageService : IImageService
{
    private readonly BlobContainerClient? _containerClient;
    private readonly string _localImagePath;
    private readonly string _pendingUploadsPath;
    private readonly HttpClient _httpClient;

    public bool IsCloudStorageEnabled => _containerClient != null;

    public ImageService()
    {
        _localImagePath = Path.Combine(FileSystem.AppDataDirectory, "images");
        _pendingUploadsPath = Path.Combine(FileSystem.AppDataDirectory, "pending_uploads.json");
        _httpClient = new HttpClient();

        // Initialize Azure Blob Storage client if configured
        if (!string.IsNullOrEmpty(AzureStorageConfig.ConnectionString) &&
            AzureStorageConfig.ConnectionString != "YOUR_AZURE_STORAGE_CONNECTION_STRING")
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(AzureStorageConfig.ConnectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(AzureStorageConfig.ContainerName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageService] Failed to initialize Azure Storage: {ex.Message}");
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
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a haircut photo"
            });

            if (result != null)
            {
                return await SaveImageLocallyAsync(result);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Please grant photo library access in settings.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ImageService] Pick image error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error",
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
                await Shell.Current.DisplayAlert("Not Supported",
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
            await Shell.Current.DisplayAlert("Permission Denied",
                "Please grant camera access in settings.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ImageService] Take photo error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error",
                "Failed to take photo. Please try again.", "OK");
        }

        return null;
    }

    public async Task<string?> UploadImageAsync(string localPath, string userId, string profileId)
    {
        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
        {
            Debug.WriteLine($"[ImageService] Upload failed: file not found at {localPath}");
            return null;
        }

        // If cloud storage is not configured, save for later sync
        if (!IsCloudStorageEnabled)
        {
            Debug.WriteLine("[ImageService] Cloud storage not configured, queuing for later sync");
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
                Debug.WriteLine($"[ImageService] File too large: {fileInfo.Length} bytes");
                await Shell.Current.DisplayAlert("Image Too Large",
                    "Please select an image smaller than 5 MB.", "OK");
                return null;
            }

            // Determine content type
            var contentType = GetContentType(localPath);
            if (!AzureStorageConfig.AllowedContentTypes.Contains(contentType))
            {
                Debug.WriteLine($"[ImageService] Invalid content type: {contentType}");
                await Shell.Current.DisplayAlert("Invalid Format",
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
            Debug.WriteLine($"[ImageService] Uploaded to: {cloudUrl}");

            return cloudUrl;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ImageService] Upload error: {ex.Message}");
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
                Debug.WriteLine($"[ImageService] Deleted local file: {imageUrl}");
            }

            // Delete from cloud if it's a cloud URL
            if (IsCloudStorageEnabled && IsCloudUrl(imageUrl))
            {
                var blobName = GetBlobNameFromUrl(imageUrl);
                if (!string.IsNullOrEmpty(blobName))
                {
                    var blobClient = _containerClient!.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                    Debug.WriteLine($"[ImageService] Deleted from cloud: {blobName}");
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
            Debug.WriteLine($"[ImageService] Delete error: {ex.Message}");
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

            using var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(cachedPath);
            await stream.CopyToAsync(fileStream);

            Debug.WriteLine($"[ImageService] Downloaded and cached: {cachedPath}");
            return cachedPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ImageService] Download error: {ex.Message}");
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
                    Debug.WriteLine($"[ImageService] Synced pending upload: {item.LocalPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageService] Failed to sync: {ex.Message}");
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

        Debug.WriteLine($"[ImageService] Saved locally: {fullPath}");
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
        var hash = url.GetHashCode().ToString("X8");
        var extension = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrEmpty(extension))
            extension = ".jpg";

        return Path.Combine(_localImagePath, "cache", $"{hash}{extension}");
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
            Debug.WriteLine($"[ImageService] Error reading pending uploads: {ex.Message}");
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
            Debug.WriteLine($"[ImageService] Error saving pending uploads: {ex.Message}");
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
