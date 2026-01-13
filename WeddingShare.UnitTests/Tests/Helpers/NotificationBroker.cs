using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Notifications;
using WeddingShare.UnitTests.Helpers;

namespace WeddingShare.UnitTests.Tests.Helpers
{
    public class NotificationBrokerTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly IHttpClientFactory _clientFactory = Substitute.For<IHttpClientFactory>();
        private readonly ISmtpClientWrapper _smtp = Substitute.For<ISmtpClientWrapper>();
        private readonly ILoggerFactory _logger = Substitute.For<ILoggerFactory>();
        private readonly IStringLocalizer<Lang.Translations> _localizer = Substitute.For<IStringLocalizer<Lang.Translations>>();

        public NotificationBrokerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            _clientFactory.CreateClient("NtfyClient").Returns(new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK)));
            _clientFactory.CreateClient("GotifyClient").Returns(new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK)));

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

            _settings.GetOrDefault(Constants.Notifications.Ntfy.Enabled, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Endpoint, Arg.Any<string>()).Returns("https://unit.test.com/");
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Token, Arg.Any<string>()).Returns("UnitTest");
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Topic, Arg.Any<string>()).Returns("UnitTest");
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Priority, Arg.Any<int>()).Returns(4);

            _settings.GetOrDefault(Constants.Notifications.Gotify.Enabled, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(Constants.Notifications.Gotify.Endpoint, Arg.Any<string>()).Returns("https://unit.test.com/");
            _settings.GetOrDefault(Constants.Notifications.Gotify.Token, Arg.Any<string>()).Returns("UnitTest");
            _settings.GetOrDefault(Constants.Notifications.Gotify.Priority, Arg.Any<int>()).Returns(4);
        }

        [TestCase(false, false, false, true)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(true, true, true, true)]
        public async Task NotificationBroker_Success(bool smtp, bool ntfy, bool gotify, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Enabled, Arg.Any<bool>()).Returns(smtp);
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Enabled, Arg.Any<bool>()).Returns(ntfy);
            _settings.GetOrDefault(Constants.Notifications.Gotify.Enabled, Arg.Any<bool>()).Returns(gotify);

            var actual = await new NotificationBroker(_settings, _smtp, _clientFactory, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase()]
        public async Task NotificationBroker_Issue_Smtp()
        {
            _settings.GetOrDefault(Constants.Notifications.Smtp.Host, Arg.Any<string>()).Returns(string.Empty);

            var actual = await new NotificationBroker(_settings, _smtp, _clientFactory, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(false));
        }

        [TestCase()]
        public async Task NotificationBroker_Issue_Ntfy()
        {
            _settings.GetOrDefault(Constants.Notifications.Ntfy.Endpoint, Arg.Any<string>()).Returns(string.Empty);

            var actual = await new NotificationBroker(_settings, _smtp, _clientFactory, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(false));
        }

        [TestCase()]
        public async Task NotificationBroker_Issue_Gotify()
        {
            _settings.GetOrDefault(Constants.Notifications.Gotify.Endpoint, Arg.Any<string>()).Returns(string.Empty);

            var actual = await new NotificationBroker(_settings, _smtp, _clientFactory, _logger, _localizer).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(false));
        }
    }
}