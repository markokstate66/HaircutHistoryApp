using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json.Linq;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing user profile pictures.
/// Fetches from social providers (Google, Facebook) and handles custom uploads.
/// </summary>
public class ProfilePictureService : IProfilePictureService
{
    private readonly HttpClient _httpClient;
    private readonly BlobContainerClient? _containerClient;
    private readonly string _localImagePath;
    private readonly ILogService _log;

    private const string ProfilePicturesFolder = "profile-pictures";
    private const string Tag = "ProfilePictureService";

    public ProfilePictureService(ILogService logService)
    {
        _log = logService;
        _httpClient = new HttpClient();
        _localImagePath = Path.Combine(FileSystem.AppDataDirectory, "profile-pictures");

        // Initialize Azure Blob Storage client if configured
        if (!string.IsNullOrEmpty(AzureStorageConfig.ConnectionString) &&
            AzureStorageConfig.ConnectionString != "YOUR_AZURE_STORAGE_CONNECTION_STRING")
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(AzureStorageConfig.ConnectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(AzureStorageConfig.ContainerName);
                _log.Info("Azure Blob Storage initialized for profile pictures", Tag);
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to initialize Azure Storage", Tag, ex);
                _containerClient = null;
            }
        }

        // Ensure local directory exists
        if (!Directory.Exists(_localImagePath))
            Directory.CreateDirectory(_localImagePath);
    }

    public async Task<string?> GetGoogleProfilePictureAsync(string accessToken)
    {
        try
        {
            // Fetch user info from Google API
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            var userInfo = JObject.Parse(response);

            var pictureUrl = userInfo["picture"]?.ToString();

            if (!string.IsNullOrEmpty(pictureUrl))
            {
                // Google returns a small image by default, request a larger one
                // Remove the size parameter and add a larger one
                if (pictureUrl.Contains("=s"))
                {
                    pictureUrl = pictureUrl.Substring(0, pictureUrl.LastIndexOf("=s")) + "=s400-c";
                }
                else
                {
                    pictureUrl += "?sz=400";
                }

                _log.Debug($"Retrieved Google profile picture", Tag);
                return pictureUrl;
            }
        }
        catch (Exception ex)
        {
            _log.Warning("Error getting Google profile picture", Tag, ex);
        }

        return null;
    }

    public async Task<string?> GetFacebookProfilePictureAsync(string accessToken)
    {
        try
        {
            // Fetch user info from Facebook Graph API with picture
            var url = $"https://graph.facebook.com/me?fields=picture.width(400).height(400)&access_token={accessToken}";
            var response = await _httpClient.GetStringAsync(url);
            var userInfo = JObject.Parse(response);

            var pictureData = userInfo["picture"]?["data"];
            var pictureUrl = pictureData?["url"]?.ToString();

            if (!string.IsNullOrEmpty(pictureUrl))
            {
                _log.Debug("Retrieved Facebook profile picture", Tag);
                return pictureUrl;
            }
        }
        catch (Exception ex)
        {
            _log.Warning("Error getting Facebook profile picture", Tag, ex);
        }

        return null;
    }

    public async Task<string?> UploadProfilePictureAsync(string localPath, string userId)
    {
        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
        {
            _log.Warning($"Upload failed: file not found at {localPath}", Tag);
            return null;
        }

        // Check file size
        var fileInfo = new FileInfo(localPath);
        if (fileInfo.Length > AzureStorageConfig.MaxImageSizeBytes)
        {
            _log.Warning($"File too large: {fileInfo.Length} bytes (max: {AzureStorageConfig.MaxImageSizeBytes})", Tag);
            await Shell.Current.DisplayAlert("Image Too Large",
                "Please select an image smaller than 5 MB.", "OK");
            return null;
        }

        // Validate content type
        var contentType = GetContentType(localPath);
        if (!AzureStorageConfig.AllowedContentTypes.Contains(contentType))
        {
            await Shell.Current.DisplayAlert("Invalid Format",
                "Please select a JPEG, PNG, or WebP image.", "OK");
            return null;
        }

        // If cloud storage is not configured, save locally
        if (_containerClient == null)
        {
            _log.Debug("Cloud storage not configured, saving locally", Tag);
            return await SaveLocallyAsync(localPath, userId);
        }

        try
        {
            // Generate unique blob name for profile picture
            var extension = Path.GetExtension(localPath);
            var blobName = $"{ProfilePicturesFolder}/{userId}/avatar{extension}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Delete existing profile picture if any
            await blobClient.DeleteIfExistsAsync();

            // Upload new picture
            using var fileStream = File.OpenRead(localPath);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    CacheControl = "public, max-age=86400" // Cache for 1 day (profile pics may change)
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);

            var cloudUrl = blobClient.Uri.ToString();
            _log.Info($"Uploaded profile picture to cloud: {blobName}", Tag);

            // Clean up local file
            try { File.Delete(localPath); } catch { }

            return cloudUrl;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to upload profile picture to cloud", Tag, ex);
            // Fall back to local storage
            return await SaveLocallyAsync(localPath, userId);
        }
    }

    public async Task<bool> DeleteProfilePictureAsync(string userId, string imageUrl)
    {
        try
        {
            // Delete local file if it's a local path
            if (File.Exists(imageUrl))
            {
                File.Delete(imageUrl);
                _log.Debug($"Deleted local profile picture: {imageUrl}", Tag);
                return true;
            }

            // Delete from cloud if configured
            if (_containerClient != null && IsCloudUrl(imageUrl))
            {
                var extension = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
                var blobName = $"{ProfilePicturesFolder}/{userId}/avatar{extension}";
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
                _log.Debug($"Deleted profile picture from cloud: {blobName}", Tag);
            }

            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to delete profile picture", Tag, ex);
            return false;
        }
    }

    public async Task<string?> PickProfilePictureAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Profile Picture"
            });

            if (result != null)
            {
                return await SaveTempImageAsync(result);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Please grant photo library access in settings.", "OK");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to pick profile picture", Tag, ex);
            await Shell.Current.DisplayAlert("Error",
                "Failed to select image. Please try again.", "OK");
        }

        return null;
    }

    public async Task<string?> TakeProfilePictureAsync()
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
                Title = "Take Profile Picture"
            });

            if (result != null)
            {
                return await SaveTempImageAsync(result);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Please grant camera access in settings.", "OK");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to take profile picture", Tag, ex);
            await Shell.Current.DisplayAlert("Error",
                "Failed to take photo. Please try again.", "OK");
        }

        return null;
    }

    #region Private Helpers

    private async Task<string> SaveTempImageAsync(FileResult photo)
    {
        var fileName = $"profile_temp_{Guid.NewGuid():N}.jpg";
        var fullPath = Path.Combine(_localImagePath, fileName);

        using var sourceStream = await photo.OpenReadAsync();
        using var destStream = File.OpenWrite(fullPath);
        await sourceStream.CopyToAsync(destStream);

        _log.Debug($"Saved temp profile picture: {fullPath}", Tag);
        return fullPath;
    }

    private Task<string> SaveLocallyAsync(string sourcePath, string userId)
    {
        var extension = Path.GetExtension(sourcePath);
        var fileName = $"avatar_{userId}{extension}";
        var destPath = Path.Combine(_localImagePath, fileName);

        // Copy file to permanent location
        File.Copy(sourcePath, destPath, overwrite: true);

        // Clean up temp file
        try { File.Delete(sourcePath); } catch { }

        _log.Debug($"Saved profile picture locally: {destPath}", Tag);
        return Task.FromResult(destPath);
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

    #endregion
}
