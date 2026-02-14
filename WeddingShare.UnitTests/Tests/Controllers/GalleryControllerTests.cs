using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute.ReturnsExtensions;
using WeddingShare.Constants;
using WeddingShare.Controllers;
using WeddingShare.Enums;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;
using WeddingShare.Helpers.Notifications;
using WeddingShare.Models;
using WeddingShare.Models.Database;
using WeddingShare.UnitTests.Helpers;

namespace WeddingShare.UnitTests.Tests.Helpers
{
    public class GalleryControllerTests
    {
        private readonly IWebHostEnvironment _env = Substitute.For<IWebHostEnvironment>();
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly IDatabaseHelper _database = Substitute.For<IDatabaseHelper>();
        private readonly IFileHelper _file = Substitute.For<IFileHelper>();
        private readonly IDeviceDetector _deviceDetector = Substitute.For<IDeviceDetector>();
        private readonly IImageHelper _image = Substitute.For<IImageHelper>();
        private readonly INotificationHelper _notification = Substitute.For<INotificationHelper>();
        private readonly IEncryptionHelper _encryption = Substitute.For<IEncryptionHelper>();
        private readonly WeddingShare.Helpers.IUrlHelper _url = Substitute.For<WeddingShare.Helpers.IUrlHelper>();
        private readonly ILogger<GalleryController> _logger = Substitute.For<ILogger<GalleryController>>();
        private readonly IStringLocalizer<Lang.Translations> _localizer = Substitute.For<IStringLocalizer<Lang.Translations>>();
        
        public GalleryControllerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            _env.WebRootPath.Returns("/app/wwwroot");

			var mockData = GetMockData();

            _database.GetGallery(1).Returns(Task.FromResult<GalleryModel?>(mockData["default"]));
			_database.GetGallery(2).Returns(Task.FromResult<GalleryModel?>(mockData["blaa"]));
			_database.GetGallery(3).Returns(Task.FromResult<GalleryModel?>(null));

            _database.GetGalleryId("default").Returns(Task.FromResult<int?>(mockData["default"].Id));
            _database.GetGalleryId("blaa").Returns(Task.FromResult<int?>(mockData["blaa"].Id));
            _database.GetGalleryId("missing").Returns(Task.FromResult<int?>(null));

            _database.GetGalleryIdByName("default").Returns(Task.FromResult<int?>(mockData["default"].Id));
            _database.GetGalleryIdByName("blaa").Returns(Task.FromResult<int?>(mockData["blaa"].Id));
            _database.GetGalleryIdByName("missing").Returns(Task.FromResult<int?>(null));

            _database.AddGallery(Arg.Any<GalleryModel>()).Returns(Task.FromResult<GalleryModel?>(new GalleryModel()
            {
                Id = 101,
                Name = "missing",
                SecretKey = "123456",
                ApprovedItems = 0,
                PendingItems = 0,
                TotalItems = 0,
				Owner = 0
            }));
			_database.AddGalleryItem(Arg.Any<GalleryItemModel>()).Returns(Task.FromResult<GalleryItemModel?>(MockData.MockGalleryItem()));

			_database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.All)));
            _database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.Pending)));
            _database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.Approved)));
			_database.GetGalleryItemByChecksum(Arg.Any<int>(), Arg.Any<string>()).ReturnsNull();

			_settings.GetOrDefault(Settings.Gallery.Upload, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
			_settings.GetOrDefault(Settings.Gallery.Download, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
			_settings.GetOrDefault(Settings.Gallery.UploadPeriod, Arg.Any<string>(), Arg.Any<int>()).Returns("1970-01-01 00:00:00");
			_settings.GetOrDefault(Settings.Gallery.PreventDuplicates, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(Settings.Gallery.DefaultView, Arg.Any<int>(), Arg.Any<int>()).Returns((int)ViewMode.Default);
            _settings.GetOrDefault(Settings.Gallery.AllowedFileTypes, Arg.Any<string>(), Arg.Any<int>()).Returns(".jpg,.jpeg,.png,.mp4,.mov");
			_settings.GetOrDefault(Settings.Gallery.RequireReview, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(Settings.Gallery.MaxFileSizeMB, Arg.Any<int>(), Arg.Any<int>()).Returns(10);

			_file.GetChecksum(Arg.Any<string>()).Returns(Guid.NewGuid().ToString());

			_notification.Send(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

			_localizer[Arg.Any<string>()].Returns(new LocalizedString("UnitTest", "UnitTest"));
		}

        [TestCase(DeviceType.Desktop, 1, "default", "default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending, true)]
        [TestCase(DeviceType.Mobile, 2, "blaa", "blaa", "456789", ViewMode.Presentation, GalleryGroup.Date, GalleryOrder.Ascending, true)]
        [TestCase(DeviceType.Tablet, 101, "missing", "missing", "123456", ViewMode.Slideshow, GalleryGroup.Uploader, GalleryOrder.Ascending, false)]
        public async Task GalleryController_Index(DeviceType deviceType, int id, string? identifier, string? name, string? key, ViewMode? mode, GalleryGroup group, GalleryOrder order, bool existing)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(deviceType);
            _settings.GetOrDefault(Settings.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(Settings.Basic.GuestGalleryCreation, Arg.Any<bool>()).Returns(false);

            var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

			if (existing)
			{
				ViewResult actual = (ViewResult)await controller.Index(identifier, name, key, mode, group, order);
				Assert.That(actual, Is.TypeOf<ViewResult>());
                Assert.That(actual?.Model, Is.Not.Null);

				PhotoGallery model = (PhotoGallery)actual.Model;
				Assert.That(model?.Gallery?.Id, Is.EqualTo(id));
                Assert.That(model?.Gallery?.Identifier, Is.EqualTo(identifier));
                Assert.That(model?.Gallery?.Name, Is.EqualTo(name));
				Assert.That(model?.SecretKey, Is.EqualTo(key));
				Assert.That(model.ViewMode, Is.EqualTo(mode));
			}
			else
			{
                RedirectToActionResult actual = (RedirectToActionResult)await controller.Index(identifier, name, key, mode, group, order);
				Assert.That(actual, Is.TypeOf<RedirectToActionResult>());
			}
        }

        [TestCase(null, "default", "default")]
        [TestCase("default", null, "default")]
        [TestCase("default", "blaa", "blaa")]
        public async Task GalleryController_Index_GetByIdentifier(string? id, string? identifier, string expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(Settings.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(Settings.Basic.GuestGalleryCreation, Arg.Any<bool>()).Returns(false);

            var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(id, identifier, "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Random);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(expected));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task GalleryController_UploadDisabled(bool enabled, bool expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(Settings.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(Settings.Gallery.Upload, Arg.Any<bool>(), Arg.Any<int>()).Returns(enabled);

            var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index("default", "default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
			Assert.That(model?.UploadActivated, Is.EqualTo(expected));
        }

        [TestCase("1970-01-01 00:00", true)]
        [TestCase("3000-01-01 00:00", false)]
        [TestCase("1970-01-01 00:00 / 1980-01-01 00:00", false)]
        [TestCase("2999-01-01 00:00 / 3000-01-01 00:00", false)]
        [TestCase("1970-01-01 00:00 / 3000-01-01 00:00", true)]
        public async Task GalleryController_UploadDisabled(string uploadPeriod, bool expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(Settings.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(Settings.Gallery.UploadPeriod, Arg.Any<string>(), Arg.Any<int>()).Returns(uploadPeriod);

            var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index("default", "default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.UploadActivated, Is.EqualTo(expected));
        }

        [TestCase(DeviceType.Desktop, ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending)]
		[TestCase(DeviceType.Mobile, ViewMode.Presentation, GalleryGroup.Date, GalleryOrder.Ascending)]
		[TestCase(DeviceType.Tablet, ViewMode.Slideshow, GalleryGroup.Uploader, GalleryOrder.Ascending)]
		public async Task GalleryController_Index_SingleGalleryMode(DeviceType deviceType, ViewMode? mode, GalleryGroup group, GalleryOrder order)
		{
			_deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(deviceType);
            _settings.GetOrDefault(Settings.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);

			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext();

			ViewResult actual = (ViewResult)await controller.Index("default", "default", "password", mode, group, order);
			Assert.That(actual, Is.TypeOf<ViewResult>());
			Assert.That(actual?.Model, Is.Not.Null);

			PhotoGallery model = (PhotoGallery)actual.Model;
			Assert.That(model?.Gallery?.Id, Is.EqualTo(1));
			Assert.That(model?.Gallery?.Identifier, Is.EqualTo("default"));
			Assert.That(model?.Gallery?.Name, Is.EqualTo("default"));
            Assert.That(model?.SecretKey, Is.EqualTo("password"));
			Assert.That(model.ViewMode, Is.EqualTo(mode));
		}

		[TestCase(true, 1, null)]
		[TestCase(true, 3, "Bob")]
		[TestCase(false, 1, "")]
		[TestCase(false, 3, "Unit Testing")]
		public async Task GalleryController_UploadImage(bool requiresReview, int fileCount, string? uploadedBy)
		{
            _settings.GetOrDefault(Settings.Gallery.RequireReview, Arg.Any<bool>()).Returns(requiresReview);

			var files = new FormFileCollection();
			for (var i = 0; i < fileCount; i++)
			{
				files.Add(new FormFile(null, 0, 0, "TestFile_001", $"{Guid.NewGuid()}.jpg"));
			}

			var session = new MockSession();
			session.Set(SessionKey.Viewer.Identity, uploadedBy ?? string.Empty);

			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(
				session: session,
				form: new Dictionary<string, StringValues>
				{
					{ "Id", "1" },
					{ "SecretKey", "password" }
                },
				files: files);

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.True);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(files.Count));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploadedBy", string.Empty), Is.EqualTo(!string.IsNullOrWhiteSpace(uploadedBy) ? uploadedBy : string.Empty));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.EqualTo(0));
		}

        [TestCase]
        public async Task GalleryController_UploadImage_Duplicate()
        {
            _database.GetGalleryItemByChecksum(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(MockData.MockGalleryItems(1, 1, GalleryItemState.Approved).FirstOrDefault()));

            var files = new FormFileCollection();
            files.Add(new FormFile(null, 0, 0, "TestFile_001", $"{Guid.NewGuid()}.jpg"));

            var session = new MockSession();
            session.Set(SessionKey.Viewer.Identity, string.Empty);

            var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(
                session: session,
                form: new Dictionary<string, StringValues>
                {
                    { "Id", "1" },
                    { "SecretKey", "password" }
                },
                files: files);

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase(null)]
		[TestCase("")]
		public async Task GalleryController_UploadImage_InvalidGallery(string? id)
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
			{
				{ "Id", id }
			});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		[TestCase(null)]
		[TestCase("")]
		public async Task GalleryController_UploadImage_InvalidSecretKey(string? key)
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
			{
				{ "Id", "1" },
				{ "SecretKey", key }
			});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		[TestCase()]
		public async Task GalleryController_UploadImage_MissingGallery()
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
			{
				{ "Id", Guid.NewGuid().ToString() }
			});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		[TestCase()]
		public async Task GalleryController_UploadImage_NoFiles()
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
			{
				{ "Id", "1" },
				{ "SecretKey", "password" }
			});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		[TestCase()]
		public async Task GalleryController_UploadImage_FileTooBig()
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(
				form: new Dictionary<string, StringValues>
				{
					{ "Id", "1" },
					{ "SecretKey", "password" }
				},
				files: new FormFileCollection() {
					new FormFile(null, 0, int.MaxValue, "TestFile_001", $"{Guid.NewGuid()}.jpg")
				});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		[TestCase()]
		public async Task GalleryController_UploadImage_InvalidFileType()
		{
			var controller = new GalleryController(_env, _settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _logger, _localizer);
			controller.ControllerContext.HttpContext = MockData.MockHttpContext(
				form: new Dictionary<string, StringValues>
				{
					{ "Id", "1" },
					{ "SecretKey", "password" }
				},
				files: new FormFileCollection() {
					new FormFile(null, 0, int.MaxValue, "TestFile_001", $"{Guid.NewGuid()}.blaa")
				});

			JsonResult actual = (JsonResult)await controller.UploadImage();
			Assert.That(actual, Is.TypeOf<JsonResult>());
			Assert.That(actual?.Value, Is.Not.Null);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
			Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
		}

		private IDictionary<string, GalleryModel> GetMockData()
		{
            return new Dictionary<string, GalleryModel>()
            {
                {
                    "default", new GalleryModel()
                    {
                        Id = 1,
                        Identifier = "default",
                        Name = "default",
                        SecretKey = "password",
                        ApprovedItems = 32,
                        PendingItems = 50,
                        TotalItems = 72
                    }
                },
                {
                    "blaa", new GalleryModel()
                    {
                        Id = 2,
                        Identifier = "blaa",
						Name = "blaa",
                        SecretKey = "456789",
                        ApprovedItems = 2,
                        PendingItems = 1,
                        TotalItems = 3
                    }
                },
                {
                    "missing", new GalleryModel()
                    {
                        Id = 101,
                        Identifier = "missing", 
						Name = "missing",
                        SecretKey = "123456",
                        ApprovedItems = 0,
                        PendingItems = 0,
                        TotalItems = 0,
                        Owner = 0
                    }
                }
            };
        }
	}
}