namespace WeddingShare.EntityFramework.Models
{
    public class GalleryLike
    {
        public int Id { get; set; }
        public int? GalleryItemId { get; set; }
        public GalleryItem? GalleryItem { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}