using NCrontab;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WeddingShare.Constants;
using WeddingShare.Enums;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;

namespace WeddingShare.BackgroundWorkers
{
    public sealed class DirectoryScanner(IServiceScopeFactory scopeFactory, IWebHostEnvironment hostingEnvironment, ISettingsHelper settingsHelper, IFileHelper fileHelper, IImageHelper imageHelper, ILogger<DirectoryScanner> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = await settingsHelper.GetOrDefault(BackgroundServices.DirectoryScanner.Enabled, true);
            if (enabled)
            {
                var cron = await settingsHelper.GetOrDefault(BackgroundServices.DirectoryScanner.Schedule, "*/30 * * * *");
                var nextExecutionTime = DateTime.Now.AddMinutes(1);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentCron = await settingsHelper.GetOrDefault(BackgroundServices.DirectoryScanner.Schedule, "*/30 * * * *");

                    var now = DateTime.Now;
                    if (now >= nextExecutionTime)
                    {
                        await ScanForFiles();

                        var schedule = CrontabSchedule.Parse(cron, new CrontabSchedule.ParseOptions() { IncludingSeconds = cron.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 6 });
                        nextExecutionTime = schedule.GetNextOccurrence(now);
                    }
                    else
                    {
                        if (!currentCron.Equals(cron))
                        {
                            nextExecutionTime = DateTime.Now;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                    
                    cron = currentCron;
                }
            }
        }

        private async Task ScanForFiles()
        {
            if (Startup.Ready)
            {
                await this.ScanGalleryImages();
                await this.ScanCustomResources();
            }
            else
            {
                logger.LogInformation($"Skipping directory scan, application not ready yet");
            }
        }

        private async Task ScanGalleryImages()
        {
            try
            { 
                var thumbnailsDirectory = Path.Combine(hostingEnvironment.WebRootPath, Directories.Thumbnails);
                fileHelper.CreateDirectoryIfNotExists(thumbnailsDirectory);

                var uploadsDirectory = Path.Combine(hostingEnvironment.WebRootPath, Directories.Uploads);
                if (fileHelper.DirectoryExists(uploadsDirectory))
                {
                    var galleryDirs = fileHelper.GetDirectories(uploadsDirectory, "*", SearchOption.TopDirectoryOnly)?.Where(x => !Path.GetFileName(x).StartsWith("."));
                    if (galleryDirs != null)
                    {
                        using (var scope = scopeFactory.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                            
                            var systemUser = await db.GetUserByUsername(UserAccounts.SystemUser);

                            foreach (var galleryDir in galleryDirs)
                            {
                                try
                                {
                                    var galleryName = Path.GetFileName(galleryDir).ToLower();
                                    var identifier = galleryName;

                                    var galleryId = await db.GetGalleryId(identifier);
                                    if (galleryId == null && await db.GetGalleryCount() < await settingsHelper.GetOrDefault(Settings.Basic.MaxGalleryCount, 1000000))
                                    {
                                        identifier = GalleryHelper.IsValidGalleryIdentifier(galleryName) ? galleryName : GalleryHelper.GenerateGalleryIdentifier();
                                        galleryId = (await db.AddGallery(new GalleryModel()
                                        {
                                            Identifier = identifier,
                                            Name = galleryName,
                                            SecretKey = PasswordHelper.GenerateGallerySecretKey(),
                                            Owner = systemUser!.Id
                                        }))?.Id;
                                    }

                                    if (galleryId != null)
                                    {
                                        var galleryItem = await db.GetGallery(galleryId.Value);
                                        if (galleryItem != null)
                                        {
                                            var galleryPath = Path.Combine(uploadsDirectory, galleryItem.Identifier);
                                            if (!galleryDir.Equals(galleryPath))
                                            {
                                                fileHelper.MoveDirectoryIfExists(galleryDir, galleryPath);
                                            }

                                            var allowedFileTypes = settingsHelper.GetOrDefault(Settings.Gallery.AllowedFileTypes, ".jpg,.jpeg,.png,.mp4,.mov", galleryItem?.Id).Result.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                            var galleryItems = await db.GetGalleryItems(null, galleryItem!.Id);

                                            if (Path.Exists(galleryPath))
                                            {
                                                var approvedFiles = fileHelper.GetFiles(galleryPath, "*.*", SearchOption.TopDirectoryOnly).Where(x => allowedFileTypes.Any(y => string.Equals(Path.GetExtension(x).Trim('.'), y.Trim('.'), StringComparison.OrdinalIgnoreCase)));
                                                if (approvedFiles != null)
                                                {
                                                    foreach (var file in approvedFiles)
                                                    {
                                                        try
                                                        {
                                                            var filename = Path.GetFileName(file);
                                                            var g = galleryItems.FirstOrDefault(x => string.Equals(x.Title, filename, StringComparison.OrdinalIgnoreCase));
                                                            if (g == null)
                                                            {
                                                                g = await db.AddGalleryItem(new GalleryItemModel()
                                                                {
                                                                    GalleryId = galleryItem.Id,
                                                                    Title = filename,
                                                                    Checksum = await fileHelper.GetChecksum(file),
                                                                    MediaType = imageHelper.GetMediaType(file),
                                                                    State = GalleryItemState.Approved,
                                                                    UploadedDate = await fileHelper.GetCreationDatetime(file),
                                                                    FileSize = fileHelper.FileSize(file)
                                                                });
                                                            }

                                                            var thumbnailDir = Path.Combine(thumbnailsDirectory, galleryItem.Identifier);
                                                            var thumbnailPath = Path.Combine(thumbnailDir, $"{Path.GetFileNameWithoutExtension(file)}.webp");
                                                            if (!fileHelper.FileExists(thumbnailPath))
                                                            {
                                                                fileHelper.CreateDirectoryIfNotExists(thumbnailDir);
                                                                await imageHelper.GenerateThumbnail(file, thumbnailPath, settingsHelper.GetOrDefault(Settings.Basic.ThumbnailSize, 720).Result);
                                                                fileHelper.DeleteFileIfExists(Path.Combine(thumbnailsDirectory, $"{Path.GetFileNameWithoutExtension(file)}.webp"));
                                                            }
                                                            else
                                                            {
                                                                using (var img = await Image.LoadAsync(thumbnailPath))
                                                                {
                                                                    var width = img.Width;

                                                                    img.Mutate(x => x.AutoOrient());

                                                                    if (width != img.Width)
                                                                    {
                                                                        await img.SaveAsWebpAsync(thumbnailPath);
                                                                    }
                                                                }
                                                            }

                                                            if (g != null)
                                                            {
                                                                var updated = false;

                                                                if (g.UploadedDate == null)
                                                                {
                                                                    g.UploadedDate = new FileInfo(file).CreationTimeUtc;
                                                                    updated = true;
                                                                }

                                                                if (g.MediaType == MediaType.Unknown)
                                                                {
                                                                    g.MediaType = imageHelper.GetMediaType(file);
                                                                    updated = true;
                                                                }

                                                                if (g.Orientation == ImageOrientation.Unknown)
                                                                {
                                                                    g.Orientation = await imageHelper.GetOrientation(thumbnailPath);
                                                                    updated = true;
                                                                }

                                                                if (g.FileSize == 0)
                                                                {
                                                                    g.FileSize = fileHelper.FileSize(file);
                                                                    updated = true;
                                                                }

                                                                if (updated)
                                                                {
                                                                    await db.EditGalleryItem(g);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            logger.LogError(ex, $"An error occurred while scanning file '{file}'");
                                                        }
                                                    }
                                                }

                                                if (Path.Exists(Path.Combine(galleryPath, "Pending")))
                                                {
                                                    var pendingFiles = fileHelper.GetFiles(Path.Combine(galleryPath, "Pending"), "*.*", SearchOption.TopDirectoryOnly).Where(x => allowedFileTypes.Any(y => string.Equals(Path.GetExtension(x).Trim('.'), y.Trim('.'), StringComparison.OrdinalIgnoreCase)));
                                                    if (pendingFiles != null)
                                                    {
                                                        foreach (var file in pendingFiles)
                                                        {
                                                            try
                                                            {
                                                                var filename = Path.GetFileName(file);
                                                                if (!galleryItems.Exists(x => string.Equals(x.Title, filename, StringComparison.OrdinalIgnoreCase)))
                                                                {
                                                                    await db.AddGalleryItem(new GalleryItemModel()
                                                                    {
                                                                        GalleryId = galleryItem.Id,
                                                                        Title = filename,
                                                                        Checksum = await fileHelper.GetChecksum(file),
                                                                        MediaType = imageHelper.GetMediaType(file),
                                                                        State = GalleryItemState.Pending,
                                                                        UploadedDate = await fileHelper.GetCreationDatetime(file),
                                                                        FileSize = new FileInfo(file).Length
                                                                    });
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                logger.LogError(ex, $"An error occurred while scanning file '{file}'");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"An error occurred while scanning directory '{galleryDir}'");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"DirectoryScanner - ScanGalleryImages - Failed to scan files - {ex?.Message}");
            }
        }

        private async Task ScanCustomResources()
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                    var systemUser = await db.GetUserByUsername(UserAccounts.SystemUser);

                    var existing = await db.GetCustomResources();

                    var customResourcesDirectory = Path.Combine(hostingEnvironment.WebRootPath, Directories.CustomResources);
                    fileHelper.CreateDirectoryIfNotExists(customResourcesDirectory);

                    foreach (var resource in fileHelper.GetFiles(customResourcesDirectory))
                    {
                        try
                        {
                            var filename = Path.GetFileName(resource);
                            if (!existing.Any(x => filename.Equals(x.FileName, StringComparison.OrdinalIgnoreCase)))
                            {
                                await db.AddCustomResource(new CustomResourceModel()
                                {
                                    FileName = filename,
                                    UploadedBy = "DirectoryScanner",
                                    Owner = systemUser!.Id
                                });
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"DirectoryScanner - ScanCustomResources - Failed to scan files - {ex?.Message}");
            }
        }
    }
}