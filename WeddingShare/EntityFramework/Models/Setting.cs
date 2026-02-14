namespace WeddingShare.EntityFramework.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}