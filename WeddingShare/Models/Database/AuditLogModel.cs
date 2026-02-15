using WeddingShare.Enums;

namespace WeddingShare.Models.Database
{
    public class AuditLogModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
        public DateTimeOffset Timestamp { get; set; }
    }
}