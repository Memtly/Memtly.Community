using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;
using WeddingShare.UnitTests.Helpers;

namespace WeddingShare.UnitTests.Tests.Helpers
{
    public class SettingsHelperTests
    {
        private readonly IConfigHelper _config;
        private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IDatabaseHelper _database = Substitute.For<IDatabaseHelper>();
        private readonly ILogger<SettingsHelper> _logger = Substitute.For<ILogger<SettingsHelper>>();

        public SettingsHelperTests()
        {
            _scopeFactory.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);
            _serviceProvider.GetService(typeof(IDatabaseHelper)).Returns(_database);

            var environment = Substitute.For<IEnvironmentWrapper>();
            environment.GetEnvironmentVariable("VERSION").Returns("v2.0.0");
            environment.GetEnvironmentVariable("ENVKEY_1").Returns("EnvValue1");
            environment.GetEnvironmentVariable("ENVKEY_2").Returns("EnvValue2");
            environment.GetEnvironmentVariable("ENVKEY_3").Returns("EnvValue3");

            var configuration = ConfigurationHelper.MockConfiguration(new Dictionary<string, string?>()
            {
                { "Release:Version", "v1.0.0" },
                { "Release:Plugin:Version", "v3.0.0" },

                { "String1:Key1", "Value1" },
                { "String1:Key2", "Value2" },
                { "String2:Key1", "Value3" },

                { "Int1:Key1", "1" },
                { "Int1:Key2", "2" },
                { "Int2:Key1", "3" },

                { "Long1:Key1", "4" },
                { "Long1:Key2", "5" },
                { "Long2:Key1", "6" },

                { "Decimal1:Key1", "4.12" },
                { "Decimal1:Key2", "5.45" },
                { "Decimal2:Key1", "6.733" },

                { "Double1:Key1", "4.12" },
                { "Double1:Key2", "5.45" },
                { "Double2:Key1", "6.733" },

                { "Boolean1:Key1", "true" },
                { "Boolean1:Key2", "false" },
                { "Boolean2:Key1", "true" },

                { "DateTime1:Key1", "1987-11-20 08:00:00" },
                { "DateTime1:Key2", "2000-08-12 12:00:00" },
                { "DateTime2:Key1", "2018-01-01 20:30:10" },
            });
            _config = new ConfigHelper(environment, configuration, Substitute.For<ILogger<ConfigHelper>>());

            _database.GetSetting("Setting1").Returns(new SettingModel() { Value = "001" });
            _database.GetSetting("Setting2").Returns(new SettingModel() { Value = "002" });
            _database.GetSetting("Version").Returns(new SettingModel() { Value = "v4.0.0" });

            _database.AddSetting(Arg.Any<SettingModel>()).Returns(new SettingModel() { Value = "Added" });
            _database.EditSetting(Arg.Any<SettingModel>()).Returns(new SettingModel() { Value = "Updated" });
            _database.SetSetting(Arg.Any<SettingModel>()).Returns(new SettingModel() { Value = "Set" });
            _database.DeleteSetting(Arg.Any<SettingModel>()).Returns(Task.CompletedTask);
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("Setting1", "001")]
        [TestCase("Setting2", "002")]
        public async Task SettingsHelper_GetSetting(string key, string expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).Get(key);

            Assert.That(actual?.Value, Is.EqualTo(expected));
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("Setting1", "Set")]
        public async Task SettingsHelper_SetSetting(string key, string expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).SetSetting(key, "FakeValue");

            Assert.That(actual?.Value, Is.EqualTo(expected));
        }

        [TestCase("Setting1", true)]
        public async Task SettingsHelper_DeleteSetting(string key, bool expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).DeleteSetting(key);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("String1:Key1", "Default", "Value1")]
        [TestCase("String1:Key2", "Default", "Value2")]
        [TestCase("String2:Key1", "Default", "Value3")]
        [TestCase("String2:Key2", "Default", "Default")]
        [TestCase("Release:Version", "v0.0.0", "v1.0.0")]
        [TestCase("Release:Plugin:Version", "v0.0.0", "v3.0.0")]
        public async Task SettingsHelper_GetOrDefault(string key, string defaultValue, string expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Int1:Key1", 999, 1)]
        [TestCase("Int1:Key2", 999, 2)]
        [TestCase("Int2:Key1", 999, 3)]
        [TestCase("Int2:Key2", 999, 999)]
        public async Task SettingsHelper_GetOrDefault(string key, int defaultValue, int expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Long1:Key1", 999, 4)]
        [TestCase("Long1:Key2", 999, 5)]
        [TestCase("Long2:Key1", 999, 6)]
        [TestCase("Long2:Key2", 999, 999)]
        public async Task SettingsHelper_GetOrDefault(string key, long defaultValue, long expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Decimal1:Key1", 999, 4.12)]
        [TestCase("Decimal1:Key2", 999, 5.45)]
        [TestCase("Decimal2:Key1", 999, 6.733)]
        [TestCase("Decimal2:Key2", 999, 999)]
        public async Task SettingsHelper_GetOrDefault(string key, decimal defaultValue, decimal expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Double1:Key1", 999, 4.12)]
        [TestCase("Double1:Key2", 999, 5.45)]
        [TestCase("Double2:Key1", 999, 6.733)]
        [TestCase("Double2:Key2", 999, 999)]
        public async Task SettingsHelper_GetOrDefault(string key, double defaultValue, double expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Boolean1:Key1", false, true)]
        [TestCase("Boolean1:Key2", false, false)]
        [TestCase("Boolean2:Key1", false, true)]
        [TestCase("Boolean2:Key2", true, true)]
        public async Task SettingsHelper_GetOrDefault(string key, bool defaultValue, bool expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("DateTime1:Key1", null, "1987-11-20 08:00:00")]
        [TestCase("DateTime1:Key2", null, "2000-08-12 12:00:00")]
        [TestCase("DateTime2:Key1", null, "2018-01-01 20:30:10")]
        [TestCase("DateTime2:Key2", null, null)]
        [TestCase("DateTime3:Key3", "2350-05-05 00:05:12", "2350-05-05 00:05:12")]
        public async Task SettingsHelper_GetOrDefault(string key, DateTime? defaultValue, string? expected)
        {
            var actual = await new SettingsHelper(_scopeFactory, _config, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(!string.IsNullOrWhiteSpace(expected) ? DateTime.Parse(expected) : null));
        }

        [TestCase("1.0.0", 3, "1.0.0")]
        [TestCase("1.0.0", 4, "1.0.0.0")]
        [TestCase("1.0.0.0", 3, "1.0.0")]
        [TestCase("1.0.0.0", 4, "1.0.0.0")]
        [TestCase("1.2.3.4", 2, "1.2")]
        public async Task SettingsHelper_GetReleaseVersion(string version, int places, string expected)
        {
            var environment = Substitute.For<IEnvironmentWrapper>();
            var configuration = ConfigurationHelper.MockConfiguration(new Dictionary<string, string?>());
            var config = new ConfigHelper(environment, configuration, Substitute.For<ILogger<ConfigHelper>>());

            var actual = new SettingsHelper(_scopeFactory, config, _logger).GetReleaseVersion(version, places);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}