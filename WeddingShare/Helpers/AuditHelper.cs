using WeddingShare.Constants;
using WeddingShare.Enums;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;

namespace WeddingShare.Helpers
{
    public interface IAuditHelper
    {
        Task<bool> LogAction(string? action, AuditSeverity severity = AuditSeverity.Information);
        Task<bool> LogAction(int? userId, string? action, AuditSeverity severity = AuditSeverity.Information);
    }

    public class AuditHelper : IAuditHelper
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISettingsHelper _settings;
        private readonly ILogger _logger;

        public AuditHelper(IServiceScopeFactory scopeFactory, ISettingsHelper settings, ILogger<AuditHelper> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        public async Task<bool> LogAction(string? action, AuditSeverity severity = AuditSeverity.Information)
        {
            return await LogAction(null, action, severity);
        }

        public async Task<bool> LogAction(int? userId, string? action, AuditSeverity severity = AuditSeverity.Information)
        {
            if (!string.IsNullOrWhiteSpace(action) && await _settings.GetOrDefault(Audit.Enabled, true))
            {
                try 
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
                     
                        return await db.AddAuditLog(new AuditLogModel()
                        {
                            UserId = userId,
                            Message = action,
                            Severity = severity
                        }) != null;
                    }
                }
                catch (Exception ex) 
                {
                    _logger.LogError($"Failed to log audit message '{action}' for user '{userId ?? 0}'", ex);
                }
            }

            return false;
        }
    }
}