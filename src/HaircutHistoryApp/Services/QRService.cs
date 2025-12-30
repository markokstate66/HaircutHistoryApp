using ZXing;
using ZXing.Common;
#if WINDOWS
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

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

        var image = writer.Write(content);

#if ANDROID
        using var disposableImage = image;
        using var stream = new MemoryStream();
        image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
        return stream.ToArray();
#elif IOS
        using var disposableImage = image;
        using var pngData = image.AsPNG();
        if (pngData != null)
        {
            var bytes = new byte[pngData.Length];
            System.Runtime.InteropServices.Marshal.Copy(pngData.Bytes, bytes, 0, (int)pngData.Length);
            return bytes;
        }
        return [];
#elif WINDOWS
        // WriteableBitmap doesn't implement IDisposable on Windows
        return ConvertWriteableBitmapToPng(image).GetAwaiter().GetResult();
#else
        return [];
#endif
    }

#if WINDOWS
    private static async Task<byte[]> ConvertWriteableBitmapToPng(Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap writeableBitmap)
    {
        using var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

        var pixelStream = writeableBitmap.PixelBuffer.AsStream();
        var pixels = new byte[pixelStream.Length];
        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)writeableBitmap.PixelWidth,
            (uint)writeableBitmap.PixelHeight,
            96, 96,
            pixels);

        await encoder.FlushAsync();

        var result = new byte[stream.Size];
        stream.Seek(0);
        await stream.ReadAsync(result.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

        return result;
    }
#endif

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
