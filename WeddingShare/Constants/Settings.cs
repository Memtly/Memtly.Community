namespace WeddingShare.Constants
{
    public class Settings
    {
        public const string IsDemoMode = "Settings:Demo_Mode";

        public class Account
        {
            public const string BaseKey = "Settings:Account:";
            public const string ShowProfileIcon = "Settings:Account:Show_Profile_Icon";
            public const string LockoutAttempts = "Settings:Account:Lockout_Attempts";
            public const string LockoutMins = "Settings:Account:Lockout_Mins";

            public class Admin
            {
                public const string BaseKey = "Settings:Account:Admin:";
                public const string Password = "Settings:Account:Admin:Password";
            }

            public class Registration
            {
                public const string BaseKey = "Settings:Account:Registration:";
                public const string Enabled = "Settings:Account:Registration:Enabled";
                public const string RequireEmailValidation = "Settings:Account:Registration:Require_Email_Validation";
            }
        }

        public class Basic
        {
            public const string BaseKey = "Settings:";
            public const string Title = "Settings:Title";
            public const string Logo = "Settings:Logo";
            public const string BaseUrl = "Settings:Base_Url";
            public const string DefaultGallerySecretKey = "Settings:Gallery_Secret_Key";
            public const string AllowInsecureGalleries = "Settings:Allow_Insecure_Galleries";
            public const string ForceHttps = "Settings:Force_Https";
            public const string SingleGalleryMode = "Settings:Single_Gallery_Mode";
            public const string MaxGalleryCount = "Settings:Max_Gallery_Count";
            public const string HomeLink = "Settings:Home_Link";
            public const string GuestGalleryCreation = "Settings:Guest_Gallery_Creation";
            public const string HideKeyFromQRCode = "Settings:Hide_Key_From_QR_Code";
            public const string LinksOpenNewTab = "Settings:Links_Open_New_Tab";
            public const string ThumbnailSize = "Settings:Thumbnail_Size";
            public const string EmailReport = "Settings:Email_Report";
        }

        public class Gallery
        {
            public const string BaseKey = "Settings:Gallery:";
            public const string ShowTitle = "Settings:Gallery:Show_Title";
            public const string BannerImage = "Settings:Gallery:Banner_Image";
            public const string Quote = "Settings:Gallery:Quote";
            public const string Columns = "Settings:Gallery:Columns";
            public const string ItemsPerPage = "Settings:Gallery:Items_Per_Page";
            public const string FullWidth = "Settings:Gallery:Full_Width";
            public const string RetainRejectedItems = "Settings:Gallery:Retain_Rejected_Items";
            public const string Likes = "Settings:Gallery:Likes";
            public const string Upload = "Settings:Gallery:Upload";
            public const string Download = "Settings:Gallery:Download";
            public const string RequireReview = "Settings:Gallery:Require_Review";
            public const string ReviewCounter = "Settings:Gallery:Review_Counter";
            public const string PreventDuplicates = "Settings:Gallery:Prevent_Duplicates";
            public const string IdleRefreshMins = "Settings:Gallery:Idle_Refresh_Mins";
            public const string MaxSizeMB = "Settings:Gallery:Max_Size_MB";
            public const string MaxFileSizeMB = "Settings:Gallery:Max_File_Size_MB";
            public const string DefaultView = "Settings:Gallery:Default_View";
            public const string DefaultGroup = "Settings:Gallery:Default_Group";
            public const string DefaultOrder = "Settings:Gallery:Default_Order";
            public const string DefaultFilter = "Settings:Gallery:Default_Filter";
            public const string UploadPeriod = "Settings:Gallery:Upload_Period";
            public const string AllowedFileTypes = "Settings:Gallery:Allowed_File_Types";
            public const string CameraUploads = "Settings:Gallery:Camera_Uploads";
            public const string ShowFilters = "Settings:Gallery:Show_Filters";

            public class QRCode
            {
                public const string BaseKey = "Settings:Gallery:QR_Code:";
                public const string Enabled = "Settings:Gallery:QR_Code:Enabled";
                public const string DefaultView = "Settings:Gallery:QR_Code:Default_View";
                public const string DefaultSort = "Settings:Gallery:QR_Code:Default_Sort";
                public const string IncludeCulture = "Settings:Gallery:QR_Code:Include_Culture";
            }
        }

        public class GallerySelector
        {
            public const string BaseKey = "Settings:Gallery_Selector:";
            public const string Dropdown = "Settings:Gallery_Selector:Dropdown";
            public const string HideDefaultOption = "Settings:Gallery_Selector:Hide_Default_Option";
            public const string ShowUsernames = "Settings:Gallery_Selector:Show_Usernames";
        }

        public class IdentityCheck
        {
            public const string BaseKey = "Settings:Identity_Check:";
            public const string Enabled = "Settings:Identity_Check:Enabled";
            public const string ShowOnPageLoad = "Settings:Identity_Check:Show_On_Page_Load";
            public const string RequireIdentityForUpload = "Settings:Identity_Check:Require_Identity_For_Upload";
            public const string RequireName = "Settings:Identity_Check:Require_Name";
            public const string RequireEmail = "Settings:Identity_Check:Require_Email";
        }

        public class Languages
        {
            public const string BaseKey = "Settings:Languages:";
            public const string Enabled = "Settings:Languages:Enabled";
            public const string Default = "Settings:Languages:Default";
        }

        public class Slideshow
        {
            public const string BaseKey = "Settings:Slideshow:";
            public const string Interval = "Settings:Slideshow:Interval";
            public const string Fade = "Settings:Slideshow:Fade";
            public const string Limit = "Settings:Slideshow:Limit";
            public const string IncludeShareSlide = "Settings:Slideshow:Include_Share_Slide";
        }

        public class Themes
        {
            public const string BaseKey = "Settings:Themes:";
            public const string Enabled = "Settings:Themes:Enabled";
            public const string Default = "Settings:Themes:Default";
        }

        public class Policies
        {
            public const string BaseKey = "Settings:Policies:";
            public const string Enabled = "Settings:Policies:Enabled";
            public const string CookiePolicy = "Settings:Policies:CookiePolicy";
        }
    }
}