namespace HaircutHistoryApp.Services;

public interface IImageService
{
    Task<string?> PickImageAsync();
    Task<string?> TakePhotoAsync();
    Task<string?> UploadImageAsync(string localPath, string userId, string profileId);
    Task<bool> DeleteImageAsync(string imageUrl);
    Task<string> GetLocalPathAsync(string imageUrl);
}
