namespace HaircutHistoryApp.Services;

public interface IImageService
{
    /// <summary>
    /// Pick an image from the device gallery
    /// </summary>
    Task<string?> PickImageAsync();

    /// <summary>
    /// Take a photo using the device camera
    /// </summary>
    Task<string?> TakePhotoAsync();

    /// <summary>
    /// Upload a local image to cloud storage
    /// </summary>
    /// <returns>The cloud URL of the uploaded image</returns>
    Task<string?> UploadImageAsync(string localPath, string userId, string profileId);

    /// <summary>
    /// Delete an image from both local and cloud storage
    /// </summary>
    Task<bool> DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Download a cloud image to local cache if needed
    /// </summary>
    /// <returns>Local file path</returns>
    Task<string> GetLocalPathAsync(string imageUrl);

    /// <summary>
    /// Check if cloud storage is configured and available
    /// </summary>
    bool IsCloudStorageEnabled { get; }

    /// <summary>
    /// Sync pending local images to cloud (for offline-first uploads)
    /// </summary>
    Task<int> SyncPendingUploadsAsync(string userId);
}
