namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing user profile pictures.
/// Handles fetching from social providers and uploading custom pictures.
/// </summary>
public interface IProfilePictureService
{
    /// <summary>
    /// Gets the profile picture URL from a Google access token.
    /// </summary>
    Task<string?> GetGoogleProfilePictureAsync(string accessToken);

    /// <summary>
    /// Gets the profile picture URL from a Facebook access token.
    /// </summary>
    Task<string?> GetFacebookProfilePictureAsync(string accessToken);

    /// <summary>
    /// Upload a custom profile picture for the user.
    /// </summary>
    /// <param name="localPath">Local file path of the image to upload</param>
    /// <param name="userId">User's ID</param>
    /// <returns>The cloud URL of the uploaded image, or null if failed</returns>
    Task<string?> UploadProfilePictureAsync(string localPath, string userId);

    /// <summary>
    /// Delete a user's custom profile picture.
    /// </summary>
    Task<bool> DeleteProfilePictureAsync(string userId, string imageUrl);

    /// <summary>
    /// Pick an image from the device gallery for profile picture.
    /// </summary>
    Task<string?> PickProfilePictureAsync();

    /// <summary>
    /// Take a photo using camera for profile picture.
    /// </summary>
    Task<string?> TakeProfilePictureAsync();
}
