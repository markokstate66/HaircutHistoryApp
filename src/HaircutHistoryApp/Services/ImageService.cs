namespace HaircutHistoryApp.Services;

public class ImageService : IImageService
{
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
            await Shell.Current.DisplayAlert("Error",
                $"Failed to pick image: {ex.Message}", "OK");
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
            await Shell.Current.DisplayAlert("Error",
                $"Failed to take photo: {ex.Message}", "OK");
        }

        return null;
    }

    public async Task<string?> UploadImageAsync(string localPath, string userId, string profileId)
    {
        // For local storage, we just return the local path
        // In a Firebase implementation, this would upload to Firebase Storage
        // and return the download URL
        return await Task.FromResult(localPath);
    }

    public Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (File.Exists(imageUrl))
            {
                File.Delete(imageUrl);
            }
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<string> GetLocalPathAsync(string imageUrl)
    {
        // For local storage, the URL is already the local path
        return Task.FromResult(imageUrl);
    }

    private async Task<string> SaveImageLocallyAsync(FileResult photo)
    {
        var fileName = $"haircut_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.jpg";
        var localPath = Path.Combine(FileSystem.AppDataDirectory, "images");

        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        var fullPath = Path.Combine(localPath, fileName);

        using var sourceStream = await photo.OpenReadAsync();
        using var destStream = File.OpenWrite(fullPath);
        await sourceStream.CopyToAsync(destStream);

        return fullPath;
    }
}
