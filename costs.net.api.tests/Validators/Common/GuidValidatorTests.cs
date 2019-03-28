namespace costs.net.api.tests.Validators.Common
{
    using System;
    using System.Threading.Tasks;
    using api.Validators.Common;
    using FluentAssertions;
    using NUnit.Framework;

    public class GuidValidatorTests
    {
        private GuidValidator _validator;
        [SetUp]
        public void Init()
        {
            _validator = new GuidValidator();
        }

        [Test]
        public async Task Validate_WhenGuidIsEmpty_ShouldFail()
        {
            // Arrange
            var guid = Guid.Empty;

            // Act
            var validationResult = await _validator.ValidateAsync(guid);

            // Assert
            validationResult.IsValid.Should().BeFalse();
        }

        [Test]
        public async Task Validate_WhenGuidIsNotEmpty_ShouldSucceed()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var validationResult = await _validator.ValidateAsync(guid);

            // Assert
            validationResult.IsValid.Should().BeTrue();
        }
    }
}
