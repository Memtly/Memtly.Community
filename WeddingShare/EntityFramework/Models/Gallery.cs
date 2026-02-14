using WeddingShare.Helpers;

namespace WeddingShare.EntityFramework.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = GalleryHelper.GenerateGalleryIdentifier();
        public string Name { get; set; } = string.Empty;
        public string? SecretKey { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public bool IsSecure 
        { 
            get { return !string.IsNullOrWhiteSpace(this.SecretKey); }
        }
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<GalleryItem> Items { get; set; } = new List<GalleryItem>();
        public ICollection<GallerySetting> Settings { get; set; } = new List<GallerySetting>();
    }
}