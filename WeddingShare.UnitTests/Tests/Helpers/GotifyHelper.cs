using System.Net;
using Microsoft.Extensions.Logging;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Notifications;
using WeddingShare.UnitTests.Helpers;

namespace WeddingShare.UnitTests.Tests.Helpers
{
    public class GotifyHelperTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly IHttpClientFactory _clientFactory = Substitute.For<IHttpClientFactory>();
        private readonly ILogger<GotifyHelper> _logger = Substitute.For<ILogger<GotifyHelper>>();

        public GotifyHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            var client = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK));
            client.BaseAddress = new Uri("https://unit.test.com/");

            _clientFactory.CreateClient(Arg.Any<string>()).Returns(client);

            _settings.GetOrDefault(Constants.Notifications.Gotify.Enabled, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(Constants.Notifications.Gotify.Endpoint, Arg.Any<string>()).Returns("https://unit.test.com/");
            _settings.GetOrDefault(Constants.Notifications.Gotify.Token, Arg.Any<string>()).Returns("UnitTest");
            _settings.GetOrDefault(Constants.Notifications.Gotify.Priority, Arg.Any<int>()).Returns(4);
        }

        [TestCase("unit", "test")]
        public async Task GotifyHelper_Success(string title, string message)
        {
            var actual = await new GotifyHelper(_settings, _clientFactory, _logger).Send(title, message);
            Assert.That(actual, Is.EqualTo(true));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task GotifyHelper_Enabled(bool enabled, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Gotify.Enabled, Arg.Any<bool>()).Returns(enabled);

            var actual = await new GotifyHelper(_settings, _clientFactory, _logger).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("http://unittest.com", true)]
        [TestCase("https://unittest.com", true)]
        public async Task GotifyHelper_Endpoint(string? endpoint, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Gotify.Endpoint, Arg.Any<string>()).Returns(endpoint);

            var client = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK));
            _clientFactory.CreateClient(Arg.Any<string>()).Returns(client);

            var actual = await new GotifyHelper(_settings, _clientFactory, _logger).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("UnitTest", true)]
        public async Task GotifyHelper_Token(string? token, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Gotify.Token, Arg.Any<string>()).Returns(token);

            var actual = await new GotifyHelper(_settings, _clientFactory, _logger).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-100, false)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(100, true)]
        public async Task GotifyHelper_Priority(int priority, bool expected)
        {
            _settings.GetOrDefault(Constants.Notifications.Gotify.Priority, Arg.Any<int>()).Returns(priority);

            var actual = await new GotifyHelper(_settings, _clientFactory, _logger).Send("unit", "test");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}