using System.Net.Mail;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Notifications;

namespace WeddingShare.UnitTests.Tests.Helpers
{
    public class EmailHelperTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly ISmtpClientWrapper _smtp = Substitute.For<ISmtpClientWrapper>();
        private readonly ILogger<EmailHelper> _logger = Substitute.For<ILogger<EmailHelper>>();
        private readonly IStringLocalizer<Lang.Translations> _localizer = Substitute.For<IStringLocalizer<Lang.Translations>>();

        public EmailHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            _smtp.SendMailAsync(Arg.Any<SmtpClient>(), Arg.Any<MailMessage>()).Returns(Task.FromResult(true));

            _settings.GetOrDefault(Constants.Notifications.Smtp.Enabled, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(Constants.Notifications.Smtp.Recipient, Arg.Any<string>()).Returns("unit@test.com");
            _settings.GetOrDefault(Constants.Notifications.Smtp.Host, Arg.Any<string>()).Returns("https://unit.test.com/");
            _settings.GetOrDefault(Constants.Notifications.Smtp.Port, Arg.Any<int>()).Returns(999);
            _settings.GetOrDefault(Constants.Notifications.Smtp.Username, Arg.Any<string>()).Returns("Unit");
            _settings.GetOrDefault(Constants.Notifications.Smtp.Password, Arg.Any<string>()).Returns("Test");
            _settings.GetOrDefault(Constants.Notifications.Smtp.From, Arg.Any<string>()).Returns("unittest@test.com");
            _settings.GetOrDefault(Constants.Notifications.Smtp.DisplayName, Arg.Any<string>()).Returns("UnitTest");
            _settings.GetOrDefault(Constants.Notifications.Smtp.UseSSL, Arg.Any<bool>()).Returns(true);
        }

        [TestCase("unit", "test")]
        public async Task EmailHelper_Success(string title, string message)
        {
            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send(title, message);
            Assert.That(actual, Is.EqualTo(true));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task EmailHelper_Enabled(bool enabled, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Enabled, Arg.Any<bool>()).Returns(enabled);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("blaa@blaa.com", true)]
        public async Task EmailHelper_Recipient(string recipient, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Recipient, Arg.Any<string>()).Returns(recipient);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("https://unit.test.com/", true)]
        public async Task EmailHelper_Host(string host, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Host, Arg.Any<string>()).Returns(host);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-100, false)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public async Task EmailHelper_Port(int port, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Port, Arg.Any<int>()).Returns(port);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("blaa@blaa.com", true)]
        public async Task EmailHelper_From(string from, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.From, Arg.Any<string>()).Returns(from);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("UnitTest", true)]
        public async Task EmailHelper_DisplayName(string displayName, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.DisplayName, Arg.Any<string>()).Returns(displayName);

            var actual = await new EmailHelper(_settings, _smtp, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}