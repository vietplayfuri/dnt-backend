namespace costs.net.api.tests.Validators.Costs
{
    using api.Validators.Costs;
    using core.Models.Costs;
    using core.Models.Utils;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SupportingDocumentUploadValidatorTests
    {
        private Mock<IOptions<AppSettings>> _appSettingsOptionsMock;
        private AppSettings _appSettings;
        private SupportingDocumentUploadValidator _validator;

        [SetUp]
        public void Init()
        {
            _appSettingsOptionsMock = new Mock<IOptions<AppSettings>>();
            _appSettings = new AppSettings();
            _appSettingsOptionsMock.Setup(m => m.Value).Returns(_appSettings);
        }

        [Test]
        public void When_FileSizeIsLessThenMaximum_Should_BeValid()
        {
            // Arrange
            _appSettings.MaxFileUploadSize = 20;// MB
            _validator = new SupportingDocumentUploadValidator(_appSettingsOptionsMock.Object);
            var toValidate = new SupportingDocumentRegisterRequest
            {
                FileSize = 10 // Bytes
            };

            // Act
            var result = _validator.Validate(toValidate);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void When_FileSizeIsMoreThenMaximum_Should_BeInvalid()
        {
            // Arrange
            _appSettings.MaxFileUploadSize = 1;// MB
            _validator = new SupportingDocumentUploadValidator(_appSettingsOptionsMock.Object);
            var toValidate = new SupportingDocumentRegisterRequest
            {
                FileSize = 1024 * 1024 + 1 // Bytes
            };

            // Act
            var result = _validator.Validate(toValidate);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void When_FileSizeIsEqualToMaximum_Should_BeValid()
        {
            // Arrange
            _appSettings.MaxFileUploadSize = 1;// MB
            _validator = new SupportingDocumentUploadValidator(_appSettingsOptionsMock.Object);
            var toValidate = new SupportingDocumentRegisterRequest
            {
                FileSize = 1024 * 1024 // Bytes
            };

            // Act
            var result = _validator.Validate(toValidate);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}