namespace WeddingShare.EntityFramework.Models
{
    public class GallerySetting
    {
        public int Id { get; set; }
        public int? GalleryId { get; set; }
        public Gallery? Gallery { get; set; }
        public int? SettingId { get; set; }
        public Setting? Setting { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}