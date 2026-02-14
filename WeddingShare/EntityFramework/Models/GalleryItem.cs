using WeddingShare.Enums;

namespace WeddingShare.EntityFramework.Models
{
    public class GalleryItem
    {
        public int Id { get; set; }
        public int? GalleryId { get; set; }
        public Gallery? Gallery { get; set; }
        public string Title { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public GalleryItemState State { get; set; } = GalleryItemState.Pending;
        public MediaType Type { get; set; } = MediaType.Unknown;
        public ImageOrientation Orientation { get; set; } = ImageOrientation.Unknown;
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<GalleryLike> Likes { get; set; } = new List<GalleryLike>();
    }
}