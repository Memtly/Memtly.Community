using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TwoFactorAuthNet;
using WeddingShare.Attributes;
using WeddingShare.Constants;
using WeddingShare.Enums;
using WeddingShare.Extensions;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;

namespace WeddingShare.Controllers
{
    [Authorize]
    public class MultiFactorController : BaseController
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISettingsHelper _settings;
        private readonly IDatabaseHelper _database;
        private readonly IAuditHelper _audit;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Lang.Translations> _localizer;

        public MultiFactorController(IWebHostEnvironment hostingEnvironment, ISettingsHelper settings, IDatabaseHelper database, IAuditHelper audit, ILoggerFactory loggerFactory, IStringLocalizer<Lang.Translations> localizer)
            : base()
        {
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
            _database = database;
            _audit = audit;
            _logger = loggerFactory.CreateLogger<MultiFactorController>();
            _localizer = localizer;
        }

        [HttpGet]
        [RequiresRole(UserPermission = UserPermissions.Login)]
        public async Task<IActionResult> GenerateToken()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated && await _settings.GetOrDefault(Settings.IsDemoMode, false) == false)
            {
                var title = await _settings.GetOrDefault(Settings.Basic.Title, "WeddingShare");
                var tfa = new TwoFactorAuth(title);

                var secret = tfa.CreateSecret(160);
                var qrCode = tfa.GetQrCodeImageAsDataUri(title, secret);
            
                return Json(new { secret = secret, qr_code = qrCode });
            }

            return Json(new { secret = string.Empty, qr_code = string.Empty });
        }

        [HttpPost]
        [RequiresRole(UserPermission = UserPermissions.Login)]
        public async Task<IActionResult> Register(string secret, string code)
        {
            if (!string.IsNullOrWhiteSpace(secret) && !string.IsNullOrWhiteSpace(code))
            {
                if (User?.Identity != null && User.Identity.IsAuthenticated && await _settings.GetOrDefault(Settings.IsDemoMode, false) == false)
                {
                    try
                    {
                        var tfa = new TwoFactorAuth(await _settings.GetOrDefault(Settings.Basic.Title, "WeddingShare"));

                        var valid = tfa.VerifyCode(secret, code);
                        if (valid)
                        {
                            var userId = User.Identity.GetUserId();
                            if (userId > 0)
                            {
                                var set = await _database.SetMultiFactorToken(userId, secret);
                                if (set)
                                {
                                    await _audit.LogAction(User?.Identity?.GetUserId(), _localizer["Audit_MultiFactorAdded"].Value, AuditSeverity.Verbose);

                                    HttpContext.Session.SetString(SessionKey.MultiFactor.TokenSet, "true");
                                    return Json(new { success = true });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                    }
                }
            }

            return Json(new { success = false });
        }

        [HttpDelete]
        [RequiresRole(UserPermission = UserPermissions.Reset_MFA)]
        public async Task<IActionResult> Reset()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    await _audit.LogAction(User?.Identity?.GetUserId(), _localizer["Audit_MultiFactorReset"].Value, AuditSeverity.Information);

                    return await ResetForUser(User.Identity.GetUserId());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }

        [HttpDelete]
        [RequiresRole(UserPermission = UserPermissions.Reset_MFA)]
        public async Task<IActionResult> ResetForUser(int userId)
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    if (userId > 0)
                    {
                        var user = await _database.GetUser(userId);
                        if (user != null && User.Identity.CanEdit(UserPermissions.Reset_MFA, user.Id))
                        { 
                            var cleared = await _database.SetMultiFactorToken(userId, string.Empty);
                            if (cleared)
                            {
                                var currentUserId = User.Identity.GetUserId();
                                if (userId == currentUserId)
                                { 
                                    HttpContext.Session.SetString(SessionKey.MultiFactor.TokenSet, "false");
                                }

                                await _audit.LogAction(User?.Identity?.GetUserId(), $"{_localizer["Audit_MultiFactorResetUser"].Value} '{user?.Username}'", AuditSeverity.Information);

                                return Json(new { success = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }
    }
}