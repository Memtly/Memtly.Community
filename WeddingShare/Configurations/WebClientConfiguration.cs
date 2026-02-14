using WeddingShare.Constants;
using WeddingShare.Helpers;

namespace WeddingShare.Configurations
{
    internal static class WebClientConfiguration
    {
        public static void AddWebClientConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("SponsorsClient", (client) =>
            {
                var config = services.BuildServiceProvider().GetRequiredService<IConfigHelper>();
                client.BaseAddress = new Uri(config.GetOrDefault(Sponsors.Url, "http://localhost:5000/"));
                client.Timeout = TimeSpan.FromSeconds(5);
            });
        }
    }
}