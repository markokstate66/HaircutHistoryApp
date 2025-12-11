namespace HaircutHistoryApp.Services;

public interface IQRService
{
    byte[] GenerateQRCode(string content, int width = 300, int height = 300);
    string? ParseQRContent(string qrContent);
}
