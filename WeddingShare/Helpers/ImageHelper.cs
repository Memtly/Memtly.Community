using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WeddingShare.Enums;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace WeddingShare.Helpers
{
    public interface IImageHelper
    {
        Task<bool> GenerateThumbnail(string filePath, string savePath, int size = 720);
        Task<ImageOrientation> GetOrientation(string path);
        ImageOrientation GetOrientation(Image img);
        MediaType GetMediaType(string filePath);
        Task<bool> DownloadFFMPEG(string path);
    }

    public class ImageHelper : IImageHelper
    {
        private readonly IFileHelper _fileHelper;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Lang.Translations> _localizer;

        private static bool FfmpegInstalled = false;

        public ImageHelper(IFileHelper fileHelper, ILogger<ImageHelper> logger, IStringLocalizer<Lang.Translations> localizer)
        {
            _fileHelper = fileHelper;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<bool> GenerateThumbnail(string filePath, string savePath, int size = 720)
        {
            if (_fileHelper.FileExists(filePath))
            { 
                try
                {
                    var mediaType = GetMediaType(filePath);
                    if (mediaType == MediaType.Image || mediaType == MediaType.Video)
                    {
                        var filename = Path.GetFileName(filePath);

                        if (mediaType == MediaType.Video)
                        {
                            if (FfmpegInstalled == false)
                            {
                                _logger.LogWarning(_localizer["FFMPEG_Downloading"].Value);
                                return false;
                            }

                            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(filePath, savePath, TimeSpan.FromSeconds(0));
                            await conversion.Start();
                            filePath = savePath;
                        }

                        using (var img = await Image.LoadAsync(filePath))
                        {
                            var width = 0;
                            var height = 0;

                            var orientation = this.GetOrientation(img);
                            if (orientation == ImageOrientation.Square)
                            {
                                width = size;
                                height = size;
                            }
                            else if (orientation == ImageOrientation.Landscape)
                            {
                                var scale = (decimal)size / (decimal)img.Width;
                                width = (int)((decimal)img.Width * scale);
                                height = (int)((decimal)img.Height * scale);
                            }
                            else if (orientation == ImageOrientation.Portrait)
                            {
                                var scale = (decimal)size / (decimal)img.Height;
                                width = (int)((decimal)img.Width * scale);
                                height = (int)((decimal)img.Height * scale);
                            }

                            img.Mutate(x =>
                            {
                                x.Resize(width, height);
                                x.AutoOrient();
                            });

                            await img.SaveAsWebpAsync(savePath);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to generate thumbnail - '{filePath}'");
                }
            }

            return false;
        }

        public MediaType GetMediaType(string path)
        {
            try
            {
                var provider = new FileExtensionContentTypeProvider();
                if (provider.TryGetContentType(path, out string? contentType))
                {
                    if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        return MediaType.Image;
                    }
                    else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    {
                        return MediaType.Video;
                    }
                }
            }
            catch { }
                
            return MediaType.Unknown;
        }

        public async Task<ImageOrientation> GetOrientation(string path)
        {
            var orientation = ImageOrientation.Unknown;

            if (_fileHelper.FileExists(path))
            {
                try
                {
                    using (var img = await Image.LoadAsync(path))
                    {
                        orientation = this.GetOrientation(img);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to get image orientation- '{path}'");
                }
            }

            return orientation;
        }

        public ImageOrientation GetOrientation(Image img)
        {
            if (img != null)
            {
                if (img.Width > img.Height)
                {
                    return ImageOrientation.Landscape;
                }
                else if (img.Width < img.Height)
                {
                    return ImageOrientation.Portrait;
                }
                else if (img.Width == img.Height)
                {
                    return ImageOrientation.Square;
                }
            }

            return ImageOrientation.Unknown;
        }

        public async Task<bool> DownloadFFMPEG(string path)
        {
            try
            {
                if (!_fileHelper.DirectoryExists(path))
                {
                    _fileHelper.CreateDirectoryIfNotExists(path);
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, path);
                }

                FFmpeg.SetExecutablesPath(path);
                FfmpegInstalled = true;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to download FFmpeg - '{path}'");
            }

            return false;
        }
    }
}