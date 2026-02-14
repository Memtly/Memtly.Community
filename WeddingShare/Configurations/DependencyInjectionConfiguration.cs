using WeddingShare.Helpers;

namespace WeddingShare.Configurations
{
    internal static class DependencyInjectionConfiguration
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IConfigHelper, ConfigHelper>();
            services.AddSingleton<ISettingsHelper, SettingsHelper>();
            services.AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>();
            services.AddSingleton<IImageHelper, ImageHelper>();
            services.AddSingleton<IFileHelper, FileHelper>();
            services.AddSingleton<IDeviceDetector, DeviceDetector>();
            services.AddSingleton<ISmtpClientWrapper, SmtpClientWrapper>();
            services.AddSingleton<IEncryptionHelper, EncryptionHelper>();
            services.AddSingleton<IUrlHelper, UrlHelper>();
            services.AddSingleton<IAuditHelper, AuditHelper>();
            services.AddSingleton<ILanguageHelper, LanguageHelper>();
        }
    }
}