using System.Text.Json.Serialization;

namespace WeddingShare.Models.Notifications
{
    public class NtfyConfiguration
    {
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 4;
    }
}