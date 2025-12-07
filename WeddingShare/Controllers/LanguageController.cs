using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WeddingShare.Constants;
using WeddingShare.Helpers;
using WeddingShare.Models;
using static WeddingShare.Constants.Settings;

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
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            var options = new SupportedLanguageList();

            try
            {
                var selectedLang = HttpContext.Session.GetString(SessionKey.SelectedLanguage);
                if (string.IsNullOrWhiteSpace(selectedLang))
                {
                    selectedLang = await _languageHelper.GetOrFallbackCulture(string.Empty, await _settings.GetOrDefault(Settings.Languages.Default, "en-GB"));
                }

                var detected = await _languageHelper.DetectSupportedCulturesAsync();
                foreach (var item in detected)
                {
                    var match = Regex.Match(item.EnglishName, @"^(.+?)(\(|$)", RegexOptions.Compiled);
                    if (match?.Groups != null && match.Groups.Count == 3)
                    {
                        var key = match.Groups[1].Value.Trim();

                        var language = options.Languages.FirstOrDefault(language => language.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                        if (language == null)
                        {
                            language = new SupportedLanguage(key);
                            options.Languages.Add(language);
                        }

                        language.Cultures.Add(new SupportedCulture()
                        {
                            Name = item.Name,
                            Selected = selectedLang.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }

                options.Languages = options.Languages.OrderBy(lang => lang.Name).ToList();
                options.Languages.ForEach(language => 
                {
                    language.Cultures = language.Cultures.OrderBy(lang => lang.Name).ToList();
                });
            }
            catch { }

            return Json(options);
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