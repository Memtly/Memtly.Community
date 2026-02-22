using WeddingShare.Enums;
using WeddingShare.Models.Database;

namespace WeddingShare.Helpers.Database
{
    public interface IDatabaseHelper
    {
        #region Gallery
        Task<int> GetGalleryCount(int? userId = null);
        Task<IDictionary<string, string>> GetGalleryNames(bool showUsernames = false);
        Task<List<GalleryModel>> GetGalleries(int? userId = null, string term = "", int page = 1, int limit = int.MaxValue);
        Task<int?> GetGalleryId(string identifier);
        Task<int?> GetGalleryIdByName(string name);
        Task<string?> GetGalleryIdentifier(int id);
        Task<string?> GetGalleryName(int id);
        Task<GalleryModel?> GetAllGallery();
        Task<GalleryModel?> GetGallery(int id);
        Task<GalleryModel?> AddGallery(GalleryModel model);
        Task<GalleryModel?> EditGallery(GalleryModel model);
        Task<GalleryModel?> RelinkGallery(GalleryModel model);
        Task WipeGallery(GalleryModel model);
        Task WipeAllGalleries();
        Task DeleteGallery(GalleryModel model);
        #endregion

        #region Gallery Items
        Task<IDictionary<string, int>> GetGalleryItemCount(int? galleryId, GalleryItemState state = GalleryItemState.All, MediaType type = MediaType.All, ImageOrientation orientation = ImageOrientation.All);
        Task<GalleryItemModel?> GetGalleryItem(int id);
        Task<GalleryItemModel?> GetGalleryItemByChecksum(int galleryId, string checksum);
        Task<List<GalleryItemModel>> GetGalleryItems(int? userId = null, int? galleryId = null, GalleryItemState state = GalleryItemState.All, MediaType type = MediaType.All, ImageOrientation orientation = ImageOrientation.All, GalleryGroup group = GalleryGroup.None, GalleryOrder order = GalleryOrder.Descending, int page = 1, int limit = int.MaxValue);
        Task<GalleryItemModel?> AddGalleryItem(GalleryItemModel model);
        Task<GalleryItemModel?> EditGalleryItem(GalleryItemModel model);
        Task DeleteGalleryItem(GalleryItemModel model);
        #endregion

        #region Gallery Item Likes
        Task<long> GetGalleryItemLikesCount(int galleryItemId);
        Task<IEnumerable<GalleryItemLikeModel>> GetGalleryItemLikes(int galleryItemId);
        Task<IEnumerable<GalleryItemLikeModel>> GetUsersGalleryItemLikes(int userId);
        Task<IEnumerable<GalleryItemLikeModel>> GetUnassignedGalleryItemLikes();
        Task<bool> CheckUserHasLikedGalleryItem(int galleryItemId, int userId);
        Task<long> LikeGalleryItem(GalleryItemLikeModel model);
        Task<long> UnLikeGalleryItem(GalleryItemLikeModel model);
        Task WipeGalleryItemLikes(int galleryItemId);
        #endregion

        #region Users
        Task<bool> ValidateCredentials(string username, string password);
        Task<int> GetUserCount();
        Task<List<UserModel>?> GetUsers(string term = "", int page = 1, int limit = int.MaxValue);
        Task<UserModel?> GetUser(int id);
        Task<UserModel?> GetUserByUsername(string name);
        Task<UserModel?> GetUserByEmail(string email);
        Task<UserModel?> AddUser(UserModel model);
        Task<UserModel?> EditUser(UserModel model);
        Task DeleteUser(UserModel model);
        Task<bool> ChangePassword(UserModel model);
        Task<string> SetUserSecret(int id, string secretCode);
        Task<bool> VerifyUserSecret(int id, string secretCode);
        Task<int> IncrementLockoutCount(int id);
        Task<bool> SetLockout(int id, DateTime? datetime);
        Task<bool> ResetLockoutCount(int id);
        Task<bool> SetMultiFactorToken(int id, string token);
        Task ResetMultiFactorToDefault();
        #endregion

        #region Settings
        Task<IEnumerable<SettingModel>?> GetAllSettings(int? galleryId = null);
        Task<SettingModel?> GetSetting(string id, int? gallery = null);
        Task<SettingModel?> GetGallerySpecificSetting(string id, int galleryId);
        Task<SettingModel?> AddSetting(SettingModel model, int? galleryId = null);
        Task<SettingModel?> EditSetting(SettingModel model, int? galleryId = null);
        Task<SettingModel?> SetSetting(SettingModel model, int? galleryId = null);
        Task DeleteSetting(SettingModel model, int? galleryId = null);
        Task DeleteAllSettings(int? galleryId = null);
        #endregion

        #region Custom Resources
        Task<int> GetCustomResourceCount(int? userId = null);
        Task<CustomResourceModel?> GetCustomResource(int id);
        Task<List<CustomResourceModel>> GetCustomResources(int? userId = null, string term = "", int page = 1, int limit = int.MaxValue);
        Task<CustomResourceModel?> AddCustomResource(CustomResourceModel model);
        Task<CustomResourceModel?> EditCustomResource(CustomResourceModel model);
        Task<CustomResourceModel?> RelinkCustomResource(CustomResourceModel model);
        Task DeleteCustomResource(CustomResourceModel model);
        #endregion

        #region Audit
        Task<AuditLogModel?> GetAuditLog(int id);
        Task<IEnumerable<AuditLogModel>?> GetAuditLogs(int? userId = null, string term = "", AuditSeverity severity = AuditSeverity.Information, int limit = 100);
        Task<AuditLogModel?> AddAuditLog(AuditLogModel model);
        Task FlushLogsOlderThan(int days = 30);
        #endregion
    }
}