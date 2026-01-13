using System.Text.Json.Serialization;

namespace WeddingShare.Models.Notifications
{
    public class GotifyConfiguration
    {
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 4;
    }
}