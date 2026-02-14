namespace WeddingShare.EntityFramework.Models
{
    public class CustomResource
    {
        public int Id { get; set; }
        public string Filename { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}