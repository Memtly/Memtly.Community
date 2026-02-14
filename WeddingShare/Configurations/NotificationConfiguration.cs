using WeddingShare.Helpers.Notifications;

namespace WeddingShare.Configurations
{
    internal static class NotificationConfiguration
    {
        private const int CLIENT_DEFAULT_TIMEOUT = 10;

        public static void AddNotificationConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<INotificationHelper, NotificationBroker>();
            services.AddNtfyConfiguration();
            services.AddGotifyConfiguration();
        }

        public static void AddNtfyConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("NtfyClient", (serviceProvider, httpClient) =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(CLIENT_DEFAULT_TIMEOUT);
            });
        }

        public static void AddGotifyConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("GotifyClient", (serviceProvider, httpClient) =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(CLIENT_DEFAULT_TIMEOUT);
            });
        }
    }
}