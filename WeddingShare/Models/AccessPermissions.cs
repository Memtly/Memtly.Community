using WeddingShare.Enums;

namespace WeddingShare.Models
{
    public class Permissions
    {
        public Permissions()
        {
            Review = ReviewPermissions.None;
            Gallery = GalleryPermissions.None;
            Users = UserPermissions.None;
            CustomResources = CustomResourcePermissions.None;
            Settings = SettingsPermissions.None;
            Audit = AuditPermissions.None;
            Data = DataPermissions.None;
            Features = FeaturePermissions.None;
        }

        public ReviewPermissions Review { get; set; }
        public GalleryPermissions Gallery { get; set; }
        public UserPermissions Users { get; set; }
        public CustomResourcePermissions CustomResources { get; set; }
        public SettingsPermissions Settings { get; set; }
        public AuditPermissions Audit { get; set; }
        public DataPermissions Data { get; set; }
        public FeaturePermissions Features { get; set; }
    }

    public class FreeUserPermissions : Permissions
    {
        public FreeUserPermissions()
            : base()
        {
            Features = 
                FeaturePermissions.UpgradeToUnlock;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Update;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Update
                | UserPermissions.Change_Password
                | UserPermissions.Reset_MFA;
            Audit =
                AuditPermissions.View;
        }
    }

    public class PaidUserPermissions : Permissions
    {
        public PaidUserPermissions()
            : base()
        {
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Create
                | GalleryPermissions.Update
                | GalleryPermissions.Delete
                | GalleryPermissions.Upload
                | GalleryPermissions.Download
                | GalleryPermissions.Wipe;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Update
                | UserPermissions.Change_Password
                | UserPermissions.Reset_MFA;
            CustomResources =
                CustomResourcePermissions.View
                | CustomResourcePermissions.Create
                | CustomResourcePermissions.Update
                | CustomResourcePermissions.Delete;
            Settings =
                SettingsPermissions.Gallery_Update;
            Audit =
                AuditPermissions.View;
        }
    }

    public class ReviewerPermissions : Permissions
    {
        public ReviewerPermissions()
            : base()
        {
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject
                | ReviewPermissions.Delete;
            Gallery =
                GalleryPermissions.View;
            Users =
                UserPermissions.Login;
        }
    }

    public class ModeratorPermissions : Permissions
    {
        public ModeratorPermissions()
            : base()
        {
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject
                | ReviewPermissions.Delete;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Update
                | GalleryPermissions.Upload
                | GalleryPermissions.Download;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Reset_MFA
                | UserPermissions.Freeze;
            CustomResources =
                CustomResourcePermissions.View;
            Audit =
                AuditPermissions.View;
        }
    }

    public class AdminPermissions : Permissions
    {
        public AdminPermissions()
            : base()
        {
            Review =
                 ReviewPermissions.View
                 | ReviewPermissions.Approve
                 | ReviewPermissions.Reject
                 | ReviewPermissions.Delete;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.ViewAllGallery
                | GalleryPermissions.Create
                | GalleryPermissions.Update
                | GalleryPermissions.Delete
                | GalleryPermissions.Upload
                | GalleryPermissions.Download
                | GalleryPermissions.Wipe;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Create
                | UserPermissions.Update
                | UserPermissions.Delete
                | UserPermissions.Change_Password
                | UserPermissions.Change_Permissions_Level
                | UserPermissions.Reset_MFA
                | UserPermissions.Freeze;
            CustomResources =
                CustomResourcePermissions.View
                | CustomResourcePermissions.Create
                | CustomResourcePermissions.Update
                | CustomResourcePermissions.Delete;
            Settings =
                SettingsPermissions.View
                | SettingsPermissions.Update
                | SettingsPermissions.Gallery_Update;
            Audit =
                AuditPermissions.View;
            Data =
                DataPermissions.View
                | DataPermissions.Import
                | DataPermissions.Export
                | DataPermissions.Wipe;
        }
    }
}