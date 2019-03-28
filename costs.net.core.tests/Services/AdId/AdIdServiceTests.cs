
namespace costs.net.core.tests.Services.AdId
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using core.Models;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.AdId;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class AdIdServiceTests
    {
        private readonly Mock<IOptions<AdIdSettings>> _appSettingsMock = new Mock<IOptions<AdIdSettings>>();
        private readonly Mock<IActivityLogService> _activityLogMock = new Mock<IActivityLogService>();
        private readonly AdIdSettings _adIdSettings = new AdIdSettings();
        private readonly Guid _costId = Guid.NewGuid();

        private Mock<IHttpService> _httpServiceMock;
        private Mock<HttpContent> _httpContentMock;
        private HttpResponseMessage _httpResponseMessage;
        private EFContext _efContext;
        private AdIdService _target;
        private UserIdentity _user;
        private Brand _brand;

        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _httpServiceMock = new Mock<IHttpService>();
            _adIdSettings.Url = "https://demo.ad-id.org/adid_services/{0}";
            _adIdSettings.UserName = "websvctest@ad-id.org";
            _adIdSettings.Password = "W5aGe@";
            _adIdSettings.BankId = "100000";
            _adIdSettings.Advertiser = "PROCTER & GAMBLE";
            _adIdSettings.FallbackBrandPrefix = "PGZZ";

            _appSettingsMock.Setup(s => s.Value).Returns(_adIdSettings);

            _user = new UserIdentity
            {
                Email = "e@mail.com",
                AgencyId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                BuType = BuType.Pg
            };

            var videoId = Guid.NewGuid();
            var videoContentType = new DictionaryEntry
            {
                Id = videoId,
                Key = "Video"
            };
            var contentTypeMediums = new List<ContentTypeAdidMedium>
            {
                new ContentTypeAdidMedium
                {
                    Id = Guid.NewGuid(),
                    DictionaryEntry = videoContentType,
                    DictionaryEntryId = videoId,
                    MediaType = "Video",
                    Medium = "TV -ALL",
                    MediumValue = "1"
                },
                new ContentTypeAdidMedium
                {
                    Id = Guid.NewGuid(),
                    MediaType = "Audio",
                    Medium = "Radio -ALL",
                    MediumValue = "43"
                },
                new ContentTypeAdidMedium
                {
                    Id = Guid.NewGuid(),
                    MediaType = "Internet Display",
                    Medium = "Other - Other",
                    MediumValue = "161"
                },
                new ContentTypeAdidMedium
                {
                    Id = Guid.NewGuid(),
                    MediaType = "Print",
                    Medium = "ALL - Print",
                    MediumValue = "56"
                }
            };

            var agency = new Agency
            {
                Labels = new[] { "GID_100000" }
            };
            var parent = new AbstractType
            {
                Agency = agency
            };
            _brand = new Brand
            {
                AdIdPrefix = "1ADC"
            };
            var project = new Project
            {
                Brand = _brand
            };
            var cost = new Cost
                { Id = _costId, Parent = parent, Project = project };

            _efContext.AddRange(new object[] { agency, parent, cost, _brand, project, videoContentType });
            _efContext.ContentTypeAdidMedium.AddRange(contentTypeMediums);
            _efContext.SaveChanges();

            //Unable to mock HttpContent.ReadAsStringAsync() and so Ad-Id service cannot be fully tested.
            _httpContentMock = new Mock<HttpContent>();
            _httpResponseMessage = new HttpResponseMessage
            {
                Content = _httpContentMock.Object,
                StatusCode = HttpStatusCode.OK
            };
            _httpServiceMock.Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<FormUrlEncodedContent>()))
                .ReturnsAsync(_httpResponseMessage);

            _target = new AdIdService(_appSettingsMock.Object,
                _efContext,
                _activityLogMock.Object,
                _httpServiceMock.Object);
        }

        [Test]
        public void ValidateGenerateAdIdResponse_ValidResponse()
        {
            var expected = "1ADC2500100H";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var content = "{\"10033165\":{\"cid\":\"10033165\",\"adid\":\"1ADC2500100H\",\"guid\":\"9a3e0c5f\"}}";
            string adIdToken;
            var result = _target.ValidateGenerateAdIdResponse(response, content, out adIdToken);

            result.Should().Be(AdIdService.AdIdResponseValidationResult.ValidResponse);
            adIdToken.Should().NotBeNull();
            adIdToken.Should().Be(expected);
        }

        [Test]
        public void ValidateGenerateAdIdResponse_UseFallback_1()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var content = "{\"4001\":[\"Could not load PID for prefix: 1ADD.\"]}";
            string adIdToken;
            var result = _target.ValidateGenerateAdIdResponse(response, content, out adIdToken);

            result.Should().Be(AdIdService.AdIdResponseValidationResult.UseFallback);
            adIdToken.Should().NotBeNull();
            adIdToken.Should().BeEmpty();
        }

        [Test]
        public void ValidateGenerateAdIdResponse_UseFallback_2()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };
            var content = "{\"1001\":[\"Prefix(pid) 101039 is either: not part of group 100000, not funded by an account, or the funding account is not in the group. \n\",\"Request data failed validation. Please address the issues and try again.\"]}";
            string adIdToken;
            var result = _target.ValidateGenerateAdIdResponse(response, content, out adIdToken);

            result.Should().Be(AdIdService.AdIdResponseValidationResult.UseFallback);
            adIdToken.Should().NotBeNull();
            adIdToken.Should().BeEmpty();
        }

        [Test]
        public void ValidateGenerateAdIdResponse_UseFallback_3()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };
            var content = "{\"1001\":[\"Missing prefix id.\"]}";
            string adIdToken;
            var result = _target.ValidateGenerateAdIdResponse(response, content, out adIdToken);

            result.Should().Be(AdIdService.AdIdResponseValidationResult.UseFallback);
            adIdToken.Should().NotBeNull();
            adIdToken.Should().BeEmpty();
        }

        [Test]
        public void ValidateGenerateAdIdResponse_AdIdServiceUnavailable()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            var content = string.Empty;
            string adIdToken;
            var result = _target.ValidateGenerateAdIdResponse(response, content, out adIdToken);

            result.Should().Be(AdIdService.AdIdResponseValidationResult.AdIdServiceUnavailable);
            adIdToken.Should().NotBeNull();
            adIdToken.Should().BeEmpty();
        }
    }
}
