using System.Text.Json.Serialization;

namespace WeddingShare.Models.Notifications
{
    public class EmailConfiguration
    {
        [JsonPropertyName("recipients")]
        public string? Recipients { get; set; }

        [JsonPropertyName("host")]
        public string? Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; } = 587;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("enable_ssl")]
        public bool EnableSSL { get; set; } = true;
    }
}