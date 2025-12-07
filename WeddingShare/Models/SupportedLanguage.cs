using System.Text.Json.Serialization;

namespace WeddingShare.Models
{
    public class SupportedLanguageList
    {
        [JsonPropertyName("languages")]
        public List<SupportedLanguage> Languages { get; set; } = new List<SupportedLanguage>();

        [JsonPropertyName("selected")]
        public SelectedLanguageOption Selected 
        {
            get
            {
                try
                {
                    var language = this.Languages?.FirstOrDefault(language => language.Selected);
                    if (!string.IsNullOrWhiteSpace(language?.Name))
                    {
                        var culture = language.Cultures?.FirstOrDefault(culture => culture.Selected);
                        if (!string.IsNullOrWhiteSpace(culture?.Name))
                        {
                            return new SelectedLanguageOption(language.Name, culture.Name);
                        }
                    }
                }
                catch { }

                return new SelectedLanguageOption();
            }
        }
    }

    public class SupportedLanguage
    {
        public SupportedLanguage(string name = "Unknown")
        {
            this.Name = name;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cultures")]
        public List<SupportedCulture> Cultures { get; set; } = new List<SupportedCulture>();

        [JsonPropertyName("selected")]
        public bool Selected
        {
            get
            {
                return this.Cultures != null && this.Cultures.Any(culture => culture.Selected);
            }
        }
    }

    public class SupportedCulture
    {
        public SupportedCulture(string name = "Unknown")
        {
            this.Name = name;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("selected")]
        public bool Selected { get; set; } = false;
    }

    public class SelectedLanguageOption
    {
        public SelectedLanguageOption()
            : this("English", "en-GB")
        {
        }

        public SelectedLanguageOption(string language, string culture)
        {
            this.Language = language;
            this.Culture = culture;
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get 
            {
                return $"{this.Language} ({this.Culture})";
            } 
        }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("culture")]
        public string Culture { get; set; }
    }
}