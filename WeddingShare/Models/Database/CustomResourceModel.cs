namespace WeddingShare.Models.Database
{
    public class CustomResourceModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = "Unknown";
        public int Owner { get; set; }
    }
}