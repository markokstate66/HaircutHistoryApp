using ZXing;
using ZXing.Common;

namespace HaircutHistoryApp.Services;

public class QRService : IQRService
{
    public byte[] GenerateQRCode(string content, int width = 300, int height = 300)
    {
        var writer = new ZXing.Net.Maui.BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 2,
                PureBarcode = true
            }
        };

        using var image = writer.Write(content);
        using var stream = new MemoryStream();

#if ANDROID
        image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
#elif IOS
        using var pngData = image.AsPNG();
        if (pngData != null)
        {
            var bytes = new byte[pngData.Length];
            System.Runtime.InteropServices.Marshal.Copy(pngData.Bytes, bytes, 0, (int)pngData.Length);
            return bytes;
        }
#endif

        return stream.ToArray();
    }

    public string? ParseQRContent(string qrContent)
    {
        if (string.IsNullOrEmpty(qrContent))
            return null;

        // Expected format: haircut://SESSIONID
        if (qrContent.StartsWith("haircut://", StringComparison.OrdinalIgnoreCase))
        {
            return qrContent.Substring("haircut://".Length);
        }

        // Also accept just the session ID directly (8 character alphanumeric)
        if (qrContent.Length == 8 && qrContent.All(c => char.IsLetterOrDigit(c)))
        {
            return qrContent;
        }

        return null;
    }
}
