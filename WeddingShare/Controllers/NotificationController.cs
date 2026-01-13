using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WeddingShare.Attributes;
using WeddingShare.Enums;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Notifications;
using WeddingShare.Models.Notifications;

namespace WeddingShare.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        private readonly IAuditHelper _audit;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Lang.Translations> _localizer;

        public NotificationController(ISettingsHelper settings, ISmtpClientWrapper smtpClientWrapper, IAuditHelper audit, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IStringLocalizer<Lang.Translations> localizer)
            : base()
        {
            _settings = settings;
            _smtpClientWrapper = smtpClientWrapper;
            _audit = audit;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NotificationController>();
            _localizer = localizer;
        }

        [HttpPost]
        [RequiresRole(SettingsPermission = SettingsPermissions.View)]
        public async Task<IActionResult> SendTestEmailNotification(EmailConfiguration config)
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    NetworkCredential? creds = null;
                    if (!string.IsNullOrWhiteSpace(config?.Username) && !string.IsNullOrWhiteSpace(config?.Password))
                    {
                        creds = new NetworkCredential(config.Username, config.Password);
                    }

                    await _audit.LogAction(User?.Identity?.Name, $"{_localizer["Audit_Sent_Test_Notification"].Value} - {_localizer["Email"].Value}");
                    return Json(new
                    {
                        success = await new EmailHelper(_settings, _smtpClientWrapper, _loggerFactory.CreateLogger<EmailHelper>(), _localizer).SendTo(config?.Host ?? string.Empty, config?.Port ?? 587, config?.From ?? string.Empty, config?.DisplayName ?? string.Empty, config?.EnableSSL ?? true, creds, config?.Recipients ?? string.Empty, _localizer["Test"].Value, _localizer["Test_Message"].Value)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["Failed_Send_Test_Notification"].Value} - {ex?.Message}");
                    return Json(new { success = false, message = ex?.Message });
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [RequiresRole(SettingsPermission = SettingsPermissions.View)]
        public async Task<IActionResult> SendTestNtfyNotification(NtfyConfiguration config)
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    await _audit.LogAction(User?.Identity?.Name, $"{_localizer["Audit_Sent_Test_Notification"].Value} - {_localizer["Ntfy"].Value}");
                    return Json(new {
                        success = await new NtfyHelper(_settings, _httpClientFactory, _loggerFactory.CreateLogger<NtfyHelper>()).Send(config?.Endpoint ?? string.Empty, config?.Topic ?? string.Empty, config?.Token ?? string.Empty, config?.Priority ?? 4, _localizer["Test"].Value, _localizer["Test_Message"].Value)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["Failed_Send_Test_Notification"].Value} - {ex?.Message}");
                    return Json(new { success = false, message = ex?.Message });
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [RequiresRole(SettingsPermission = SettingsPermissions.View)]
        public async Task<IActionResult> SendTestGotifyNotification(GotifyConfiguration config)
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    await _audit.LogAction(User?.Identity?.Name, $"{_localizer["Audit_Sent_Test_Notification"].Value} - {_localizer["Gotify"].Value}");
                    return Json(new
                    {
                        success = await new GotifyHelper(_settings, _httpClientFactory, _loggerFactory.CreateLogger<GotifyHelper>()).Send(config?.Endpoint ?? string.Empty, config?.Token ?? string.Empty, config?.Priority ?? 4, _localizer["Test"].Value, _localizer["Test_Message"].Value)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["Failed_Send_Test_Notification"].Value} - {ex?.Message}");
                    return Json(new { success = false, message = ex?.Message });
                }
            }

            return Json(new { success = false });
        }
    }
}