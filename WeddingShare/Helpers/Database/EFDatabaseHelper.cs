using System.Data;
using Microsoft.EntityFrameworkCore;
using WeddingShare.Constants;
using WeddingShare.EntityFramework;
using WeddingShare.EntityFramework.Models;
using WeddingShare.Enums;
using WeddingShare.Models.Database;

namespace WeddingShare.Helpers.Database
{
    public class EFDatabaseHelper : IDatabaseHelper
    {
        private readonly CoreDbContext _db;
        private readonly ILogger _logger;

        public EFDatabaseHelper(CoreDbContext db, ILogger<EFDatabaseHelper> logger)
        {
            _db = db;
            _logger = logger;
        }

        #region Gallery
        public async Task<int> GetGalleryCount()
        {
            return await _db.Galleries.CountAsync();
        }

        public async Task<IDictionary<string, string>> GetGalleryNames(bool showUsernames = false)
        {
            return await _db.Galleries
                .ToDictionaryAsync(
                    g => g.Identifier,
                    g => showUsernames ? $"{g.Name} ({g.User!.Username})" : g.Name
                );
        }

        public async Task<List<GalleryModel>> GetGalleries(int? userId = null)
        {
            return await _db.Galleries
                .Where(g => userId == null || g.UserId == userId)
                .Select(g => new GalleryModel
                {
                    Id = g.Id,
                    Identifier = g.Identifier,
                    Name = g.Name,
                    SecretKey = g.SecretKey,
                    OwnerName = g.User!.Username,
                    TotalItems = g.Items.Count,
                    ApprovedItems = g.Items.Count(gi => gi.State == GalleryItemState.Approved),
                    PendingItems = g.Items.Count(gi => gi.State == GalleryItemState.Pending),
                    TotalGallerySize = g.Items.Sum(gi => (long?)gi.FileSize) ?? 0
                })
                .OrderByDescending(g => g.TotalGallerySize)
                .ToListAsync();
        }

        public async Task<int?> GetGalleryIdByName(string name)
        {
            if (name.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return (await _db.Galleries
                .FirstOrDefaultAsync(g => g.Name.ToLower().Equals(name.ToLower()))
            )?.Id;
        }

        public async Task<int?> GetGalleryId(string identifier)
        {
            if (identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return (await _db.Galleries
               .FirstOrDefaultAsync(g => g.Identifier.Equals(identifier))
            )?.Id;
        }

        public async Task<string?> GetGalleryIdentifier(int id)
        {
            if (id == 0)
            {
                return SystemGalleries.AllGallery;
            }

            return (await _db.Galleries
               .FirstOrDefaultAsync(g => g.Id == id)
            )?.Identifier;
        }

        public async Task<string?> GetGalleryName(int id)
        {
            if (id == 0)
            {
                return SystemGalleries.AllGallery;
            }

            return (await _db.Galleries
               .FirstOrDefaultAsync(g => g.Id == id)
            )?.Name;
        }

        public async Task<GalleryModel?> GetAllGallery()
        {
            return new GalleryModel
            {
                Id = 0,
                Identifier = SystemGalleries.AllGallery.ToLower(),
                Name = SystemGalleries.AllGallery,
                SecretKey = null,
                TotalItems = await _db.GalleryItems.CountAsync(),
                ApprovedItems = await _db.GalleryItems.CountAsync(gi => gi.State == GalleryItemState.Approved),
                PendingItems = await _db.GalleryItems.CountAsync(gi => gi.State == GalleryItemState.Pending),
                TotalGallerySize = await _db.GalleryItems.SumAsync(gi => (long?)gi.FileSize) ?? 0,
                Owner = 0,
                OwnerName = "System"
            };
        }

        public async Task<GalleryModel?> GetGallery(int id)
        {
            if (id == 0)
            {
                return await GetAllGallery();
            }

            return await _db.Galleries
                .Select(g => new GalleryModel
                {
                    Id = g.Id,
                    Identifier = g.Identifier,
                    Name = g.Name,
                    SecretKey = g.SecretKey,
                    OwnerName = g.User!.Username,
                    TotalItems = g.Items.Count,
                    ApprovedItems = g.Items.Count(gi => gi.State == GalleryItemState.Approved),
                    PendingItems = g.Items.Count(gi => gi.State == GalleryItemState.Pending),
                    TotalGallerySize = g.Items.Sum(gi => (long?)gi.FileSize) ?? 0
                })
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<GalleryModel?> AddGallery(GalleryModel model)
        {
            if (ProtectedValues.GalleryNames.Any(x => x.ToLower().Equals(model.Name?.Trim().ToLower())))
            {
                return null; // Prevent users from creating galleries with the same name as a protected gallery
            }

            var galleryEntry = await _db.Galleries.AddAsync(new EntityFramework.Models.Gallery()
            {
                Identifier = model.Identifier,
                Name = model.Name,
                SecretKey = model.SecretKey,
                UserId = model.Owner,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync();

            return await GetGallery(galleryEntry.Entity.Id);
        }

        public async Task<GalleryModel?> EditGallery(GalleryModel model)
        {
            var gallery = await _db.Galleries.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (gallery != null)
            {
                if (ProtectedValues.GalleryNames.Any(x => x.ToLower().Equals(model.Name?.Trim().ToLower())))
                {
                    return await GetGallery(gallery.Id); // Prevent users from creating galleries with the same name as a protected gallery
                }

                gallery.Name = model.Name;
                gallery.SecretKey = model.SecretKey;

                await _db.SaveChangesAsync();

                return await GetGallery(gallery.Id);
            }

            return null;
        }

        public async Task WipeGallery(GalleryModel model)
        {
            await _db.GalleryItems
                .Where(gi => gi.GalleryId == model.Id)
                .ExecuteDeleteAsync();

            await _db.GallerySettings
                .Where(gs => gs.GalleryId == model.Id)
                .ExecuteDeleteAsync();
        }

        public async Task WipeAllGalleries()
        {
            await _db.GalleryItems
                 .ExecuteDeleteAsync();

            await _db.GallerySettings
                .ExecuteDeleteAsync();
        }

        public async Task DeleteGallery(GalleryModel model)
        {
            await _db.Galleries
                .Where(g => g.Id == model.Id)
                .ExecuteDeleteAsync();
        }
        #endregion

        #region Gallery Items
        public async Task<IDictionary<string, long>> GetGalleryItemCount(int? galleryId = null, GalleryItemState state = GalleryItemState.All, MediaType type = MediaType.All, ImageOrientation orientation = ImageOrientation.All)
        {
            var counts = await _db.GalleryItems
                .Where(gi =>
                    (galleryId == null || galleryId == 0 || gi.GalleryId == galleryId)
                    && (state == GalleryItemState.All || gi.State == state)
                    && (type == MediaType.All || gi.Type == type)
                    && (orientation == ImageOrientation.All || gi.Orientation == orientation)
                )
                 .GroupBy(gi => gi.State)
                .Select(g => new { State = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.State!.ToString(), x => (long)x.Count);

            foreach (var s in Enum.GetNames(typeof(GalleryItemState)))
            {
                var key = s.ToString();
                if (!counts.ContainsKey(key))
                {
                    counts.Add(key, s.ToLower().Equals(SystemGalleries.AllGallery.ToLower()) ? counts.Sum(x => x.Value) : 0);
                }
            }

            return counts;
        }

        public async Task<List<GalleryItemModel>> GetGalleryItems(int? userId = null, int? galleryId = null, GalleryItemState state = GalleryItemState.All, MediaType type = MediaType.All, ImageOrientation orientation = ImageOrientation.All, GalleryGroup group = GalleryGroup.None, GalleryOrder order = GalleryOrder.Descending, int limit = int.MaxValue, int page = 1)
        {
            var query = _db.GalleryItems
                .Where(gi =>
                    (userId == null || gi.Gallery!.UserId == userId)
                    && (galleryId == null || galleryId == 0 || gi.GalleryId == galleryId)
                    && (state == GalleryItemState.All || gi.State == state)
                    && (type == MediaType.All || gi.Type == type)
                    && (orientation == ImageOrientation.All || gi.Orientation == orientation)
                );

            switch (group)
            {
                case GalleryGroup.Uploader:
                    query = order == GalleryOrder.Ascending ? query.OrderBy(gi => gi.UploadedBy) : query.OrderByDescending(gi => gi.UploadedBy);
                    break;
                case GalleryGroup.MediaType:
                    query = order == GalleryOrder.Ascending ? query.OrderBy(gi => gi.Type) : query.OrderByDescending(gi => gi.Type);
                    break;
                case GalleryGroup.None:
                    switch (order)
                    {
                        case GalleryOrder.Random:
                            query = query.OrderBy(gi => EF.Functions.Random());
                            break;
                        default:
                            query = order == GalleryOrder.Ascending ? query.OrderBy(gi => gi.CreatedAt) : query.OrderByDescending(gi => gi.CreatedAt);
                            break;
                    }
                    break;
                default:
                    query = order == GalleryOrder.Ascending ? query.OrderBy(gi => gi.CreatedAt) : query.OrderByDescending(gi => gi.CreatedAt);
                    break;
            }

            return await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(gi => new GalleryItemModel()
                {
                    Id = gi.Id,
                    GalleryId = gi.GalleryId ?? 0,
                    Title = gi.Title,
                    State = gi.State,
                    UploadedBy = gi.UploadedBy,
                    //UploaderEmailAddress = gi?.Gallery?.User?.EmailAddress ?? string.Empty,
                    UploadedDate = gi.CreatedAt,
                    Checksum = gi.Checksum,
                    MediaType = gi.Type,
                    Orientation = gi.Orientation,
                    FileSize = gi.FileSize
                })
                .ToListAsync();
        }

        public async Task<GalleryItemModel?> GetGalleryItem(int id)
        {
            return await _db.GalleryItems
                .Select(gi => new GalleryItemModel()
                {
                    Id = gi.Id,
                    GalleryId = gi.GalleryId ?? 0,
                    Title = gi.Title,
                    State = gi.State,
                    UploadedBy = gi.UploadedBy,
                    //UploaderEmailAddress = gi.User
                    UploadedDate = gi.CreatedAt,
                    Checksum = gi.Checksum,
                    MediaType = gi.Type,
                    Orientation = gi.Orientation,
                    FileSize = gi.FileSize
                })
                .FirstOrDefaultAsync(gi => gi.Id == id);
        }

        public async Task<GalleryItemModel?> GetGalleryItemByChecksum(int galleryId, string checksum) 
        {
            return await _db.GalleryItems
                .Select(gi => new GalleryItemModel()
                {
                    Id = gi.Id,
                    GalleryId = gi.GalleryId ?? 0,
                    Title = gi.Title,
                    State = gi.State,
                    UploadedBy = gi.UploadedBy,
                    //UploaderEmailAddress = gi.User
                    UploadedDate = gi.CreatedAt,
                    Checksum = gi.Checksum,
                    MediaType = gi.Type,
                    Orientation = gi.Orientation,
                    FileSize = gi.FileSize
                })
                .FirstOrDefaultAsync(gi => gi.GalleryId == galleryId && gi.Checksum!.Equals(checksum));
        }

        public async Task<GalleryItemModel?> AddGalleryItem(GalleryItemModel model)
        {
            var galleryItemEntry = await _db.GalleryItems.AddAsync(new GalleryItem()
            {
                GalleryId = model.GalleryId,
                Title = model.Title,
                State = model.State,
                UploadedBy = model.UploadedBy ?? string.Empty,
                Checksum = model.Checksum ?? string.Empty,
                Type = model.MediaType,
                Orientation = model.Orientation,
                FileSize = model.FileSize,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync();

            return await GetGalleryItem(galleryItemEntry.Entity.Id);
        }

        public async Task<GalleryItemModel?> EditGalleryItem(GalleryItemModel model)
        {
            var galleryItem = await _db.GalleryItems.FirstOrDefaultAsync(gi => gi.Id == model.Id);

            if (galleryItem != null)
            {
                galleryItem.Title = model.Title;
                galleryItem.State = model.State;
                galleryItem.UploadedBy = model.UploadedBy ?? string.Empty;
                galleryItem.Checksum = model.Checksum ?? string.Empty;
                galleryItem.Type = model.MediaType;
                galleryItem.Orientation = model.Orientation;
                galleryItem.FileSize = model.FileSize;

                await _db.SaveChangesAsync();
            }

            return galleryItem != null ? await GetGalleryItem(galleryItem.Id) : null;
        }

        public async Task DeleteGalleryItem(GalleryItemModel model)
        {
            await _db.GalleryItems
                .Where(gi => gi.Id == model.Id)
                .ExecuteDeleteAsync();
        }
        #endregion

        #region Gallery Item Likes
        public async Task<long> GetGalleryItemLikesCount(int galleryItemId)
        {
            return await _db.GalleryLikes
                .CountAsync(gl => gl.GalleryItemId == galleryItemId);
        }

        public async Task<IEnumerable<GalleryItemLikeModel>> GetGalleryItemLikes(int galleryItemId)
        {
            return await _db.GalleryLikes
                .Where(gl => gl.GalleryItemId == galleryItemId)
                .Select(gl => new GalleryItemLikeModel()
                {
                    Id = gl.Id,
                    GalleryId = gl!.GalleryItem!.GalleryId ?? 0,
                    GalleryItemId = gl!.GalleryItemId ?? 0,
                    UserId = gl!.UserId ?? 0,
                    Timestamp = gl.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GalleryItemLikeModel>> GetUsersGalleryItemLikes(int userId)
        {
            return await _db.GalleryLikes
                .Where(gl => gl.UserId == userId)
                .Select(gl => new GalleryItemLikeModel()
                {
                    Id = gl.Id,
                    GalleryId = gl!.GalleryItem!.GalleryId ?? 0,
                    GalleryItemId = gl!.GalleryItemId ?? 0,
                    UserId = gl!.UserId ?? 0,
                    Timestamp = gl.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GalleryItemLikeModel>> GetUnassignedGalleryItemLikes()
        {
            return await _db.GalleryLikes
                .Where(gl => (gl!.GalleryItem!.GalleryId ?? 0) == 0)
                .Select(gl => new GalleryItemLikeModel()
                {
                    Id = gl.Id,
                    GalleryId = gl!.GalleryItem!.GalleryId ?? 0,
                    GalleryItemId = gl!.GalleryItemId ?? 0,
                    UserId = gl!.UserId ?? 0,
                    Timestamp = gl.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> CheckUserHasLikedGalleryItem(int galleryItemId, int userId)
        {
            return (await _db.GalleryLikes
                .CountAsync(gl => gl.GalleryItemId == galleryItemId && gl.UserId == userId)) > 0;
        }

        public async Task<long> LikeGalleryItem(GalleryItemLikeModel model)
        {
            var liked = await CheckUserHasLikedGalleryItem(model.GalleryItemId, model.UserId);
            if (!liked)
            {
                await _db.GalleryLikes.AddAsync(new GalleryLike()
                {
                    GalleryItemId = model.GalleryItemId,
                    UserId = model.UserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return await GetGalleryItemLikesCount(model.GalleryItemId);
        }

        public async Task<long> UnLikeGalleryItem(GalleryItemLikeModel model)
        {
            await _db.GalleryLikes
                .Where(gl => gl.GalleryItemId == model.GalleryItemId && gl.UserId == model.UserId)
                .ExecuteDeleteAsync();

            return await GetGalleryItemLikesCount(model.GalleryItemId);
        }

        public async Task WipeGalleryItemLikes(int galleryItemId)
        {
            await _db.GalleryLikes
                .Where(gl => gl.GalleryItemId == galleryItemId)
                .ExecuteDeleteAsync();
        }
        #endregion

        #region Users
        public async Task<bool> ValidateCredentials(string username, string password)
        {
            return (await _db.Users
                .CountAsync(u => u.Level != UserLevel.System && u.Username.ToLower().Equals(username.ToLower()) && u.Password.Equals(password))) > 0;
        }

        public async Task<List<UserModel>?> GetAllUsers()
        {
            return await _db.Users
                .Where(u => !u.Username.ToLower().Equals(UserAccounts.SystemUser.ToLower()))
                .Select(u => new UserModel()
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.EmailAddress,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    Level = u.Level ?? UserLevel.Free,
                    State = u.State ?? AccountState.PendingActivation,
                    FailedLogins = u.FailedLoginCount,
                    LockoutUntil = u.LockoutUntil.HasValue ? u.LockoutUntil.Value.DateTime : null,
                    MultiFactorToken = u.MultiFactorAuthToken
                })
                .OrderBy(u => u.Username.ToLower())
                .ToListAsync();
        }

        public async Task<UserModel?> GetUser(int id)
        {
            return await _db.Users
                .Select(u => new UserModel()
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.EmailAddress,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    Level = u.Level ?? UserLevel.Free,
                    State = u.State ?? AccountState.PendingActivation,
                    FailedLogins = u.FailedLoginCount,
                    LockoutUntil = u.LockoutUntil.HasValue ? u.LockoutUntil.Value.DateTime : null,
                    MultiFactorToken = u.MultiFactorAuthToken
                })
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel?> GetUserByUsername(string username)
        {
            return await _db.Users
                .Select(u => new UserModel()
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.EmailAddress,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    Level = u.Level ?? UserLevel.Free,
                    State = u.State ?? AccountState.PendingActivation,
                    FailedLogins = u.FailedLoginCount,
                    LockoutUntil = u.LockoutUntil.HasValue ? u.LockoutUntil.Value.DateTime : null,
                    MultiFactorToken = u.MultiFactorAuthToken
                })
                .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            return await _db.Users
                .Select(u => new UserModel()
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.EmailAddress,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    Level = u.Level ?? UserLevel.Free,
                    State = u.State ?? AccountState.PendingActivation,
                    FailedLogins = u.FailedLoginCount,
                    LockoutUntil = u.LockoutUntil.HasValue ? u.LockoutUntil.Value.DateTime : null,
                    MultiFactorToken = u.MultiFactorAuthToken
                })
                .FirstOrDefaultAsync(u => u.Email!.ToLower().Equals(email.ToLower()));
        }

        public async Task<UserModel?> AddUser(UserModel model)
        {
            var userEntry = await _db.Users.AddAsync(new User()
            {
                Username = model.Username,
                EmailAddress = model.Email ?? string.Empty,
                Firstname = model.Firstname ?? string.Empty,
                Lastname = model.Lastname ?? string.Empty,
                Password = model.Password ?? PasswordHelper.GenerateTempPassword(),
                Level = model.Level,
                State = model.State,
                FailedLoginCount = model.FailedLogins,
                LockoutUntil = model.LockoutUntil,
                //MultiFactorAuthToken = model.
                //ActionAuthCode = model.,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync();

            return await GetUser(userEntry.Entity.Id);
        }

        public async Task<UserModel?> EditUser(UserModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user != null)
            {
                user.EmailAddress = model.Email ?? string.Empty;
                user.Firstname = model.Firstname ?? string.Empty;
                user.Lastname = model.Lastname ?? string.Empty;
                user.Level = model.Level;
                user.State = model.State;
                user.FailedLoginCount = model.FailedLogins;
                user.LockoutUntil = model.LockoutUntil;
                //user.MultiFactorAuthToken = model.
                //ActionAuthCode = model.

                await _db.SaveChangesAsync();
            }

            return user != null ? await GetUser(user.Id) : null;
        }

        public async Task DeleteUser(UserModel model)
        {
            await _db.Users
                .Where(u => u.Level != UserLevel.System && u.Id == model.Id)
                .ExecuteDeleteAsync();
        }

        public async Task<bool> ChangePassword(UserModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Level != UserLevel.System && u.Id == model.Id);
            if (user != null)
            {
                user.Password = model.Password ?? PasswordHelper.GenerateTempPassword();

                await _db.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> SetMultiFactorToken(int id, string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Level != UserLevel.System && u.Id == id);
            if (user != null)
            {
                user.MultiFactorAuthToken = token;

                await _db.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<string> SetUserSecret(int id, string secretCode)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Level != UserLevel.System && u.Id == id);
            if (user != null)
            {
                user.ActionAuthCode = secretCode;

                await _db.SaveChangesAsync();

                return secretCode;
            }

            return string.Empty;
        }

        public async Task<bool> VerifyUserSecret(int id, string secretCode)
        {
            return (await _db.Users
                .CountAsync(u => u.Level != UserLevel.System && u.Id == id && u.ActionAuthCode.Equals(secretCode))) > 0;
        }

        public async Task<int> IncrementLockoutCount(int id)
        {
            var user = await this.GetUser(id);

            if (user != null)
            {
                user.FailedLogins++;
                
                await _db.SaveChangesAsync();
            }

            return user?.FailedLogins ?? int.MaxValue;
        }

        public async Task<bool> SetLockout(int id, DateTime? datetime) 
        {
            DateTimeOffset? normalizedDatetime = null;

            if (datetime.HasValue)
            {
                var dt = datetime.Value;
                normalizedDatetime = new DateTimeOffset(
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, 0,
                    TimeSpan.Zero
                );
            }

            try
            {
                await _db.Users
                    .Where(u => u.Level != UserLevel.System && u.Id == id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(u => u.LockoutUntil, normalizedDatetime)
                    );

                var updatedValue = await _db.Users
                    .Where(u => u.Id == id)
                    .Select(u => u.LockoutUntil)
                    .FirstOrDefaultAsync();

                return updatedValue == normalizedDatetime;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set user lockout - {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ResetLockoutCount(int id)
        {
            var user = await this.GetUser(id);

            if (user != null)
            {
                user.FailedLogins = 0;
                user.LockoutUntil = null;

                await _db.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task ResetMultiFactorToDefault()
        {
            await _db.Users
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(s => s.MultiFactorAuthToken, string.Empty)
                );
        }
        #endregion

        #region CustomResources
        public async Task<CustomResourceModel?> GetCustomResource(int id)
        {
            return await _db.CustomResources
                .Select(cr => new CustomResourceModel()
                {
                    FileName = cr.Filename,
                    Owner = cr.UserId ?? 0
                })
                .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        public async Task<List<CustomResourceModel>> GetCustomResources(int? userId = null)
        {
            return await _db.CustomResources
                .Where(cr => userId == null || cr.UserId == userId)
                .Select(cr => new CustomResourceModel()
                {
                    FileName = cr.Filename,
                    Owner = cr.UserId ?? 0
                })
                .OrderBy(cr => cr.FileName!.ToLower())
                .ToListAsync();
        }

        public async Task<CustomResourceModel?> AddCustomResource(CustomResourceModel model)
        {
            var customResourceEntry = await _db.CustomResources.AddAsync(new CustomResource()
            {
                Filename = model.FileName,
                UserId = model.Owner,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync();

            return await GetCustomResource(customResourceEntry.Entity.Id);
        }

        public async Task<CustomResourceModel?> EditCustomResource(CustomResourceModel model)
        {
            var customResource = await _db.CustomResources.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (customResource != null)
            {
                customResource.Filename = model.FileName;
                customResource.UserId = model.Owner;

                await _db.SaveChangesAsync();
            }

            return customResource != null ? await GetCustomResource(customResource.Id) : null;
        }

        public async Task DeleteCustomResource(CustomResourceModel model)
        {
            await _db.Settings
                .Where(s => s.Value.ToLower().Equals($"/custom_resources/{model.FileName}".ToLower()))
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(s => s.Value, string.Empty)
                );

            await _db.GallerySettings
                .Where(gs => gs.Setting!.Key.ToLower().Equals(Settings.Gallery.BannerImage.ToLower()) && gs.Value.ToLower().Equals($"/custom_resources/{model.FileName}".ToLower()))
                .ExecuteDeleteAsync();

            await _db.CustomResources
                .Where(cr => cr.Id == model.Id)
                .ExecuteDeleteAsync();
        }
        #endregion

        #region Settings
        public async Task<IEnumerable<SettingModel>?> GetAllSettings(int? galleryId = null)
        {
            var globalSettings = await _db.Settings
                .Select(s => new SettingModel()
                { 
                    Id = s.Key, 
                    Value = s.Value 
                })
                .ToListAsync();

            if (galleryId != null)
            { 
                return globalSettings;
            }

            var galleryOverrides = await _db.GallerySettings
                .Where(gs => gs.GalleryId == galleryId && !string.IsNullOrWhiteSpace(gs.Value))
                .Select(gs => new SettingModel()
                {
                    Id = gs.Setting!.Key,
                    Value = gs.Value 
                })
                .ToListAsync();

            if (!galleryOverrides.Any())
            { 
                return globalSettings;
            }

            var overrideIds = new HashSet<string>(galleryOverrides.Select(o => o.Id), StringComparer.OrdinalIgnoreCase);

            return globalSettings
                .Where(s => !overrideIds.Contains(s.Id))
                .Concat(galleryOverrides)
                .ToList();
        }

        public async Task<SettingModel?> GetSetting(string id, int? galleryId = null)
        {
            if (galleryId != null)
            {
                var gallerySetting = await _db.GallerySettings
                    .Where(gs => gs.Setting!.Key.ToLower().Equals(id.ToLower()) && gs.GalleryId == galleryId)
                    .Select(gs => new SettingModel
                    {
                        Id = gs.Setting!.Key,
                        Value = gs.Value
                    })
                    .FirstOrDefaultAsync();

                if (gallerySetting != null)
                { 
                    return gallerySetting;
                }
            }

            return await _db.Settings
                .Where(s => s.Key.ToLower().Equals(id.ToLower()))
                .Select(s => new SettingModel
                {
                    Id = s.Key,
                    Value = s.Value
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SettingModel?> GetGallerySpecificSetting(string id, int galleryId)
        {
            return await _db.GallerySettings
                .Where(gs => gs.Setting!.Key.ToLower().Equals(id.ToLower()) && gs.GalleryId == galleryId)
                .Select(gs => new SettingModel
                {
                    Id = gs.Setting!.Key,
                    Value = gs.Value
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SettingModel?> AddSetting(SettingModel model, int? galleryId = null)
        {
            var settingId = galleryId != null ? (await _db.Settings.FirstOrDefaultAsync(s => s.Key.ToLower().Equals(model.Id.ToLower())))?.Id : null;

            if (settingId == null)
            {
                var settingEntry = await _db.Settings.AddAsync(new Setting()
                {
                    Key = model.Id,
                    Value = model.Value ?? string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                await _db.SaveChangesAsync();

                settingId = settingEntry.Entity.Id;
            }

            if (galleryId != null)
            {
                await _db.GallerySettings.AddAsync(new GallerySetting()
                {
                    SettingId = settingId,
                    Value = model.Value ?? string.Empty,
                    GalleryId = galleryId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return galleryId != null ? await this.GetSetting(model.Id, galleryId.Value) : await this.GetSetting(model.Id);
        }

        public async Task<SettingModel?> EditSetting(SettingModel model, int? galleryId = null)
        {
            if (galleryId != null)
            {
                var setting = await _db.GallerySettings
                    .FirstOrDefaultAsync(gs => gs.GalleryId == galleryId && gs.Setting!.Key.ToLower().Equals(model.Id.ToLower()));

                if (setting != null)
                { 
                    setting.Value = model.Value ?? string.Empty;

                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                var setting = await _db.Settings
                    .FirstOrDefaultAsync(s => s.Key.ToLower().Equals(model.Id.ToLower()));

                if (setting != null)
                {
                    setting.Value = model.Value ?? string.Empty;

                    await _db.SaveChangesAsync();
                }
            }

            return await GetSetting(model.Id, galleryId);
        }

        public async Task<SettingModel?> SetSetting(SettingModel model, int? galleryId = null)
        {
            if (!string.IsNullOrWhiteSpace(model.Id))
            {
                try
                {
                    if (galleryId != null)
                    {
                        // Gallery Override
                        var result = await GetGallerySpecificSetting(model.Id, galleryId.Value);
                        if (result == null && !string.IsNullOrEmpty(model.Value))
                        {
                            return await AddSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            }, galleryId);
                        }
                        else if (result != null && !string.IsNullOrEmpty(model.Value))
                        {
                            return await EditSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            }, galleryId);
                        }
                        else if (result != null && string.IsNullOrEmpty(model.Value))
                        {
                            await DeleteSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            }, galleryId);
                        }
                    }
                    else
                    {
                        // Default Setting
                        var result = await GetSetting(model.Id);
                        if (result == null && !string.IsNullOrEmpty(model.Value))
                        {
                            return await AddSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            });
                        }
                        else if (result != null && !string.IsNullOrEmpty(model.Value))
                        {
                            return await EditSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            });
                        }
                        else if (result != null && string.IsNullOrEmpty(model.Value))
                        {
                            await DeleteSetting(new SettingModel()
                            {
                                Id = model.Id.ToUpper(),
                                Value = model.Value
                            });
                        }
                    }
                }
                catch { }
            }

            return new SettingModel()
            {
                Id = model.Id.ToUpper(),
                Value = null
            };
        }

        public async Task DeleteSetting(SettingModel model, int? galleryId = null)
        {
            if (galleryId != null)
            {
                await _db.GallerySettings
                    .Where(gs => gs.GalleryId == galleryId && gs.Setting!.Key.ToLower().Equals(model.Id.ToLower()))
                    .ExecuteDeleteAsync();
            }
            else
            {
                await _db.GallerySettings
                    .Where(gs => gs.Setting!.Key.ToLower().Equals(model.Id.ToLower()))
                    .ExecuteDeleteAsync();

                await _db.Settings
                    .Where(s => s.Key.ToLower().Equals(model.Id.ToLower()))
                    .ExecuteDeleteAsync();
            }
        }

        public async Task DeleteAllSettings(int? galleryId = null)
        {
            if (galleryId != null)
            {
                await _db.GallerySettings
                    .Where(gs => gs.GalleryId == galleryId)
                    .ExecuteDeleteAsync();
            }
            else
            {
                await _db.Settings
                    .ExecuteDeleteAsync();
            }
        }
        #endregion

        #region Audit Logs
        public async Task<AuditLogModel?> GetAuditLog(int id)
        {
            return await _db.AuditLogs
                .Select(al => new AuditLogModel()
                {
                    Id = al.Id,
                    UserId = al.UserId ?? 0,
                    Username = al.User!.Username ?? "System",
                    Message = al.Message,
                    Severity = al.Severity,
                    Timestamp = al.CreatedAt
                })
                .FirstOrDefaultAsync(al => al.Id == id);
        }

        public async Task<IEnumerable<AuditLogModel>?> GetAuditLogs(int? userId = null, string term = "", AuditSeverity severity = AuditSeverity.Information, int limit = 100)
        {
            return await _db.AuditLogs
                .Where(al => (userId == null || al.UserId == userId)
                    && (string.IsNullOrWhiteSpace(term)
                        || al.Message.ToLower().Contains(term.ToLower())
                        || (al.User != null && al.User.Username.ToLower().Contains(term.ToLower())))
                    && al.Severity >= severity)
                .OrderByDescending(al => al.CreatedAt)
                .Take(limit)
                .Select(al => new AuditLogModel()
                {
                    Id = al.Id,
                    UserId = al.UserId ?? 0,
                    Username = al.User!.Username ?? "System",
                    Message = al.Message,
                    Severity = al.Severity,
                    Timestamp = al.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AuditLogModel?> AddAuditLog(AuditLogModel model)
        {
            var auditLogEntity = await _db.AuditLogs.AddAsync(new AuditLog()
            {
                UserId = model.UserId,
                Message = model.Message,
                Severity = model.Severity,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync();

            return await GetAuditLog(auditLogEntity.Entity.Id);
        }

        public async Task FlushLogsOlderThan(int days = 30)
        {
            await _db.AuditLogs
                .Where(al => al.CreatedAt < DateTimeOffset.UtcNow.AddDays(Math.Abs(days) * -1))
                .ExecuteDeleteAsync();
        }
        #endregion

        #region Backups
        public async Task<bool> Import(string path)
        {
            bool result = false;

            //try
            //{
            //    using (var backup = await GetConnection(path))
            //    using (var conn = await GetConnection())
            //    {
            //        await backup.OpenAsync();
            //        await conn.OpenAsync();

            //        backup.BackupDatabase(conn);

            //        await conn.CloseAsync();
            //        await backup.CloseAsync();

            //        ClearPool(backup);
            //    }

            //    result = true;
            //}
            //catch { }

            return result;
        }

        public async Task<bool> Export(string path)
        {
            bool result = false;

            //try
            //{
            //    using (var conn = await GetConnection())
            //    using (var backup = await GetConnection(path))
            //    {
            //        await conn.OpenAsync();
            //        await backup.OpenAsync();

            //        conn.BackupDatabase(backup);

            //        await backup.CloseAsync();
            //        await conn.CloseAsync();

            //        ClearPool(backup);
            //    }

            //    result = true;
            //}
            //catch { }

            return result;
        }
        #endregion
    }
}