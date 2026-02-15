using System.Text.Json.Serialization;
using WeddingShare.Enums;
using WeddingShare.Models.Database;

namespace WeddingShare.Models
{
    public class PhotoGallery
    {
        public PhotoGallery()
            : this(ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending)
        {
        }

        public PhotoGallery(ViewMode viewMode, GalleryGroup groupBy, GalleryOrder orderBy)
            : this(null, string.Empty, viewMode, groupBy, orderBy, new List<PhotoGalleryImage>(), false)
        {
        }

        public PhotoGallery(GalleryModel? gallery, string secretKey, ViewMode viewMode, GalleryGroup groupBy, GalleryOrder orderBy, List<PhotoGalleryImage> images, bool uploadActivated)
        {
            this.Gallery = gallery;
            this.SecretKey = secretKey;
            this.ViewMode = viewMode;
            this.GroupBy = groupBy;
            this.OrderBy = orderBy;
            this.PendingCount = 0;
            this.Images = images;
            this.UploadActivated = uploadActivated;
        }

        public GalleryModel? Gallery { get; set; }
        public string? SecretKey { get; set; }
        public ViewMode ViewMode { get; set; }
        public GalleryGroup GroupBy { get; set; }
        public GalleryOrder OrderBy { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int ItemsPerPage { get; set; } = 50;
        public int CurrentPage { get; set; } = 1;
        public bool Pagination { get; set; } = true;
        public bool LoadScripts { get; set; } = true;
        public int TotalCount
        {
            get
            {
                return this.ApprovedCount + this.PendingCount;
            }
        }
        public List<PhotoGalleryImage>? Images { get; set; }
        public bool UploadActivated { get; set; } = false;
    }

    public class PhotoGalleryImage
    {
        public PhotoGalleryImage()
        { 
        }

        public int Id { get; set; }
        public int? GalleryId { get; set; }
        public string? GalleryName { get; set; }
        public string? Name { get; set; }
        public string UploadedBy { get; set; }
        public string? UploaderEmailAddress { get; set; }
        public DateTimeOffset? UploadDate { get; set; }
        public string? ImagePath { get; set; }
        public string? ThumbnailPath { get; set; }
        public MediaType MediaType { get; set; }
    }

    public class PhotoGalleryImageExtended
    {
        public PhotoGalleryImageExtended()
        {
            this.Likes = new PhotoGalleryImageLikes();
        }

        [JsonPropertyName("likes")]
        public PhotoGalleryImageLikes Likes { get; set; }
    }

    public class PhotoGalleryImageLikes
    {
        public PhotoGalleryImageLikes()
        {
        }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("can_like")]
        public bool CanUserLike { get; set; } = false;

        [JsonPropertyName("has_liked")]
        public bool HasUserLiked { get; set; } = false;

        [JsonPropertyName("count")]
        public long Count { get; set; } = 0;
    }
}