namespace WeddingShare.Models.Database
{
    public class GalleryItemLikeModel
    {
        public GalleryItemLikeModel()
            : this(0, 0, 0, 0, new DateTime(0, DateTimeKind.Utc))
        {
        }

        public GalleryItemLikeModel(int id, int galleryItemId, int galleryId, int userId, DateTimeOffset timestamp)
        {
            Id = id;
            GalleryItemId = galleryItemId;
            GalleryId = galleryId;
            UserId = userId;
            Timestamp = timestamp;
        }

        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public int GalleryId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}