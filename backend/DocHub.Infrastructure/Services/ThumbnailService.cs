using DocHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DocHub.Infrastructure.Services;

public class ThumbnailService : IThumbnailService
{
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _thumbnailPath;
    private readonly int _maxWidth;
    private readonly int _maxHeight;
    private readonly int _quality;

    public ThumbnailService(ILogger<ThumbnailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _thumbnailPath = _configuration["FileStorage:ThumbnailPath"] ?? "Storage/Thumbnails";
        _maxWidth = _configuration.GetValue<int>("FileUpload:ThumbnailSettings:MaxWidth", 200);
        _maxHeight = _configuration.GetValue<int>("FileUpload:ThumbnailSettings:MaxHeight", 200);
        _quality = _configuration.GetValue<int>("FileUpload:ThumbnailSettings:Quality", 80);
        
        Directory.CreateDirectory(_thumbnailPath);
    }

    public async Task<string> GenerateThumbnailAsync(string filePath, string fileId, ThumbnailOptions? options = null)
    {
        try
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var thumbnailPath = Path.Combine(_thumbnailPath, $"{fileId}_thumb.jpg");
            
            // Use provided options or defaults
            var maxWidth = options?.MaxWidth ?? _maxWidth;
            var maxHeight = options?.MaxHeight ?? _maxHeight;
            var quality = options?.Quality ?? _quality;

            // Generate thumbnail based on file type
            switch (fileExtension)
            {
                case ".pdf":
                    await GeneratePdfThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                case ".doc":
                case ".docx":
                    await GenerateDocumentThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                case ".xls":
                case ".xlsx":
                    await GenerateSpreadsheetThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".tiff":
                    await GenerateImageThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                case ".txt":
                case ".csv":
                case ".rtf":
                    await GenerateTextFileThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                case ".zip":
                case ".rar":
                case ".7z":
                case ".tar":
                case ".gz":
                    await GenerateArchiveThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
                default:
                    await GenerateGenericFileThumbnailAsync(filePath, thumbnailPath, maxWidth, maxHeight, quality);
                    break;
            }
            
            _logger.LogInformation("Thumbnail generated successfully for file {FileId} at {ThumbnailPath}", fileId, thumbnailPath);
            return thumbnailPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for file {FileId}", fileId);
            return string.Empty;
        }
    }

    public async Task<string> GenerateThumbnailFromBytesAsync(byte[] fileData, string fileId, string fileExtension, ThumbnailOptions? options = null)
    {
        try
        {
            var thumbnailPath = Path.Combine(_thumbnailPath, $"{fileId}_thumb.jpg");
            var maxWidth = options?.MaxWidth ?? _maxWidth;
            var maxHeight = options?.MaxHeight ?? _maxHeight;
            var quality = options?.Quality ?? _quality;

            // Create temporary file
            var tempPath = Path.Combine(_thumbnailPath, $"temp_{fileId}{fileExtension}");
            await File.WriteAllBytesAsync(tempPath, fileData);

            try
            {
                var result = await GenerateThumbnailAsync(tempPath, fileId, options);
                return result;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail from bytes for file {FileId}", fileId);
            return string.Empty;
        }
    }

    public async Task<bool> DeleteThumbnailAsync(string fileId)
    {
        try
        {
            var thumbnailPath = Path.Combine(_thumbnailPath, $"{fileId}_thumb.jpg");
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                _logger.LogInformation("Thumbnail deleted for file {FileId}", fileId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting thumbnail for file {FileId}", fileId);
            return false;
        }
    }

    private async Task GeneratePdfThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            // Create a PDF document icon thumbnail
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Set high quality
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            // Fill background
            graphics.Clear(Color.White);
            
            // Draw PDF icon
            using var brush = new SolidBrush(Color.Red);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            // Draw a simple PDF icon representation
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            // Draw text
            using var textBrush = new SolidBrush(Color.White);
            var text = "PDF";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            // Save with specified quality
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "PDF");
        }
    }

    private async Task GenerateDocumentThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            // Create a Word document icon thumbnail
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.Clear(Color.White);
            
            // Draw Word document icon
            using var brush = new SolidBrush(Color.Blue);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            using var textBrush = new SolidBrush(Color.White);
            var text = "DOC";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating document thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "DOC");
        }
    }

    private async Task GenerateSpreadsheetThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            // Create an Excel spreadsheet icon thumbnail
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.Clear(Color.White);
            
            // Draw Excel icon
            using var brush = new SolidBrush(Color.Green);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            using var textBrush = new SolidBrush(Color.White);
            var text = "XLS";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating spreadsheet thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "XLS");
        }
    }

    private async Task GenerateImageThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            using var originalImage = Image.FromFile(filePath);
            
            // Calculate new dimensions maintaining aspect ratio
            var ratioX = (double)maxWidth / originalImage.Width;
            var ratioY = (double)maxHeight / originalImage.Height;
            var ratio = Math.Min(ratioX, ratioY);
            
            var newWidth = (int)(originalImage.Width * ratio);
            var newHeight = (int)(originalImage.Height * ratio);
            
            using var thumbnail = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(thumbnail);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            thumbnail.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "IMG");
        }
    }

    private async Task GenerateTextFileThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.Clear(Color.White);
            
            // Draw text file icon
            using var brush = new SolidBrush(Color.Gray);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            using var textBrush = new SolidBrush(Color.White);
            var text = "TXT";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text file thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "TXT");
        }
    }

    private async Task GenerateArchiveThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.Clear(Color.White);
            
            // Draw archive icon
            using var brush = new SolidBrush(Color.Orange);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            using var textBrush = new SolidBrush(Color.White);
            var text = "ZIP";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating archive thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "ZIP");
        }
    }

    private async Task GenerateGenericFileThumbnailAsync(string filePath, string thumbnailPath, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.Clear(Color.White);
            
            // Draw generic file icon
            using var brush = new SolidBrush(Color.DarkGray);
            using var font = new Font("Arial", 12, FontStyle.Bold);
            
            var rect = new Rectangle(10, 10, maxWidth - 20, maxHeight - 20);
            graphics.FillRectangle(brush, rect);
            
            using var textBrush = new SolidBrush(Color.White);
            var text = "FILE";
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating generic file thumbnail");
            await GenerateFallbackThumbnailAsync(thumbnailPath, maxWidth, maxHeight, quality, "FILE");
        }
    }

    private async Task GenerateFallbackThumbnailAsync(string thumbnailPath, int maxWidth, int maxHeight, int quality, string fileType)
    {
        try
        {
            using var bitmap = new Bitmap(maxWidth, maxHeight);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.Clear(Color.LightGray);
            
            using var brush = new SolidBrush(Color.DarkGray);
            using var font = new Font("Arial", 10, FontStyle.Regular);
            
            using var textBrush = new SolidBrush(Color.Black);
            var text = fileType;
            var textSize = graphics.MeasureString(text, font);
            var textX = (maxWidth - textSize.Width) / 2;
            var textY = (maxHeight - textSize.Height) / 2;
            graphics.DrawString(text, font, textBrush, textX, textY);
            
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(thumbnailPath, jpegCodec, encoderParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating fallback thumbnail");
        }
    }

    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid) ?? codecs[0];
    }
}
