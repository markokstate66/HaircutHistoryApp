using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Request to get a SAS URL for uploading a photo.
/// </summary>
public class PhotoUploadRequest
{
    /// <summary>
    /// Original file name.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type (e.g., "image/jpeg").
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// Response containing SAS URL for photo upload.
/// </summary>
public class PhotoUploadResponse
{
    /// <summary>
    /// The SAS URL for uploading (expires in 15 minutes).
    /// </summary>
    [JsonPropertyName("uploadUrl")]
    public string UploadUrl { get; set; } = string.Empty;

    /// <summary>
    /// The final blob URL to use in haircut records.
    /// </summary>
    [JsonPropertyName("blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;

    /// <summary>
    /// When the upload URL expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
