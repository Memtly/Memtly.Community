using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WeddingShare.Constants;
using WeddingShare.Helpers;
using WeddingShare.Models;

namespace WeddingShare.Controllers
{
    [AllowAnonymous]
    public class LanguageController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly ILanguageHelper _languageHelper;
        private readonly ILogger<LanguageController> _logger; 
        private readonly IStringLocalizer<Lang.Translations> _localizer;

        public LanguageController(ISettingsHelper settings, ILanguageHelper languageHelper, ILogger<LanguageController> logger, IStringLocalizer<Lang.Translations> localizer)
            : base()
        {
            _settings = settings;
            _languageHelper = languageHelper;
            _logger = logger; 
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var options = new List<SupportedLanguage>();

            try
            {
                var defaultLang = HttpContext.Session.GetString(SessionKey.SelectedLanguage);
                if (string.IsNullOrWhiteSpace(defaultLang))
                {
                    defaultLang = await _languageHelper.GetOrFallbackCulture(string.Empty, await _settings.GetOrDefault(Settings.Languages.Default, "en-GB"));
                }

                options = (await _languageHelper.DetectSupportedCulturesAsync())
                    .Select(x => new SupportedLanguage() { Key = x.Name, Value = $"{(x.EnglishName.Contains("(") ? x.EnglishName.Substring(0, x.EnglishName.IndexOf("(")) : x.EnglishName).Trim()} ({x.Name})", Selected = string.Equals(defaultLang, x.Name, StringComparison.OrdinalIgnoreCase) })
                    .OrderBy(x => x.Value.ToLower())
                    .ToList();
            }
            catch { }

            return Json(new { supported = options });
        }

        [HttpGet]
        public IActionResult GetTranslations()
        {
            return Json(new
            {
                current = new
                {
                    full = $"{CultureInfo.CurrentCulture.EnglishName} ({CultureInfo.CurrentCulture.Name})",
                    code = CultureInfo.CurrentCulture.Name,
                    name = CultureInfo.CurrentCulture.EnglishName
                },
                translations = _localizer.GetAllStrings().ToDictionary(x => x.Name, x => x.Value)
            });
        }


        [HttpPost]
        public async Task<IActionResult> ChangeDisplayLanguage(string culture)
        {
            try
            {
                culture = await _languageHelper.GetOrFallbackCulture(culture, await _settings.GetOrDefault(Settings.Languages.Default, "en-GB"));

                HttpContext.Session.SetString(SessionKey.SelectedLanguage, culture);
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

                return Json(new { success = true });
            }
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, $"Failed to set display language to '{culture}' - {ex?.Message}");

                culture = "en-GB";

                HttpContext.Session.SetString(SessionKey.SelectedLanguage, culture);
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            return Json(new { success = false });
        }
    }
}