using System.Reflection;
using System.Text;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;

namespace WeddingShare.Helpers
{
    public interface ISettingsHelper
    {
        Task<SettingModel?> Get(string key, int? galleryId = null);
        Task<string> GetOrDefault(string key, string defaultValue, int? galleryId = null);
        Task<int> GetOrDefault(string key, int defaultValue, int? galleryId = null);
        Task<long> GetOrDefault(string key, long defaultValue, int? galleryId = null);
        Task<decimal> GetOrDefault(string key, decimal defaultValue, int? galleryId = null);
        Task<double> GetOrDefault(string key, double defaultValue, int? galleryId = null);
        Task<bool> GetOrDefault(string key, bool defaultValue, int? galleryId = null);
        Task<DateTime?> GetOrDefault(string key, DateTime? defaultValue, int? galleryId = null);
        Task<SettingModel?> SetSetting(string key, string value, int? galleryId = null);
        Task<bool> DeleteSetting(string key, int? galleryId = null);
        string GetReleaseVersion(int places = 3);
    }

    public class SettingsHelper : ISettingsHelper
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfigHelper _configHelper;
        private readonly ILogger _logger;

        public SettingsHelper(IServiceScopeFactory scopeFactory, IConfigHelper configHelper, ILogger<SettingsHelper> logger)
        { 
            _scopeFactory = scopeFactory;
            _configHelper = configHelper;
            _logger = logger;
        }

        public async Task<SettingModel?> Get(string key, int? galleryId = null)
        {
            if (!string.IsNullOrWhiteSpace(key))
            { 
                try
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                            var dbValue = galleryId != null ? await db.GetSetting(key, galleryId.Value) : await db.GetSetting(key);
                            if (dbValue != null)
                            {
                                return dbValue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, $"Failed to find key '{key}' in database. If you are seeing this on first setup please ignore as the database might not have initialized the table yet.");
                    }

                    var configValue = _configHelper.Get(key);
                    if (configValue != null)
                    { 
                        return new SettingModel()
                        {
                            Id = key.ToUpper(),
                            Value = configValue
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, $"Failed to find key '{key}' in either database or config. If you are seeing this on first setup please ignore as the database might not have initialized the table yet.");
                }
            }

            return null;
        }

        public async Task<string> GetOrDefault(string key, string defaultValue, int? galleryId = null)
        {
            try
            {
                var value = (await this.Get(key, galleryId))?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<int> GetOrDefault(string key, int defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToInt32(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<long> GetOrDefault(string key, long defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToInt64(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<decimal> GetOrDefault(string key, decimal defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToDecimal(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<double> GetOrDefault(string key, double defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToDouble(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<bool> GetOrDefault(string key, bool defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToBoolean(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<DateTime?> GetOrDefault(string key, DateTime? defaultValue, int? galleryId = null)
        {
            try
            {
                var value = await this.GetOrDefault(key, string.Empty, galleryId);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Convert.ToDateTime(value);
                }
            }
            catch { }

            return defaultValue;
        }

        public async Task<SettingModel?> SetSetting(string key, string value, int? galleryId = null)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                    return await db.SetSetting(new SettingModel()
                    {
                        Id = key,
                        Value = value
                    }, galleryId);
                }
            }

            return null;
        }

        public async Task<bool> DeleteSetting(string key, int? galleryId = null)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                    await db.DeleteSetting(new SettingModel()
                    {
                        Id = key.ToUpper()
                    }, galleryId);
                }

                return true;
            }

            return false;
        }

        public async Task DeleteAllSettings(int? galleryId = null)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                await db.DeleteAllSettings(galleryId);
            }
        }

        public string GetReleaseVersion(int places = 3)
        {
            return GetReleaseVersion(Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0.0", places);
        }

        public string GetReleaseVersion(string version, int places = 3)
        {
            try
            {
                var versionNumberParts = version?.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (versionNumberParts != null && versionNumberParts.Length > 0)
                {
                    var builder = new StringBuilder();
                    for (var i = 0; i < places; i++)
                    {
                        if (i < versionNumberParts.Length)
                        {
                            builder.Append($".{versionNumberParts[i]}");
                        }
                        else
                        {
                            builder.Append(".0");
                        }
                    }

                    return builder.ToString().Trim('.');
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to build release version string - {ex?.Message}");
            }

            return "1.0.0";
        }
    }
}