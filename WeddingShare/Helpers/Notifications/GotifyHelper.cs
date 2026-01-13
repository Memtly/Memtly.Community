namespace WeddingShare.Helpers.Notifications
{
    public class GotifyHelper : INotificationHelper
    {
        private readonly ISettingsHelper _settings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        public GotifyHelper(ISettingsHelper settings, IHttpClientFactory clientFactory, ILogger<GotifyHelper> logger)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<bool> Send(string title, string message, string? actionLink = null)
        {
            if (await _settings.GetOrDefault(Constants.Notifications.Gotify.Enabled, false))
            {
                try
                {
                    var endpoint = await _settings.GetOrDefault(Constants.Notifications.Gotify.Endpoint, string.Empty);
                    var token = await _settings.GetOrDefault(Constants.Notifications.Gotify.Token, string.Empty);
                    var priority = await _settings.GetOrDefault(Constants.Notifications.Gotify.Priority, 4);

                    return await Send(endpoint, token, priority, title, message, actionLink);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send Gotify message with title '{title}' - {ex?.Message}");
                }
            }

            return false;
        }

        public async Task<bool> Send(string endpoint, string token, int priority, string title, string message, string? actionLink = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogWarning($"Invalid Gotify endpoint specified");
                return false;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning($"Invalid Gotify token specified");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(token))
            { 
                if (priority > 0)
                {
                    message = !string.IsNullOrWhiteSpace(actionLink) ? $"{message} - Visit - {actionLink}" : message;

                    var client = _clientFactory.CreateClient("GotifyClient");

                    client.BaseAddress = new Uri(endpoint);

                    using (var response = await client.PostAsJsonAsync($"/message?token={token}", new { title, message, priority }))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            _logger.LogError($"Failed to send Gotify message with title '{title}' - {error}");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid Gotify priority specified");
                }
            }
            else
            {
                _logger.LogWarning($"Invalid Gotify token specified");
            }

            return false;
        }
    }
}