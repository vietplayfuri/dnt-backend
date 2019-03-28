using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using costs.net.api.Validators.AdId;
using costs.net.core.Models.AdId;

namespace costs.net.api.tests.Validators
{
    [TestFixture]
    public class CreateAdIdRequestValidatorTest
    {
        private CreateAdIdRequestValidator _validator;
        [SetUp]
        public void Init()
        {
            _validator = new CreateAdIdRequestValidator();
        }

        [Test]
        public async Task Validate_WhenCampaignHasLessThan32Characters_ShouldSucceed()
        {
            // Arrange
            var createAdIdRequest = new CreateAdIdRequest()
            {
                Campaign = "This text has exactly 32 chars."
            };

            // Act
            var validationResult = await _validator.ValidateAsync(createAdIdRequest);

            // Assert
            validationResult.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Validate_WhenCampaignHasMoreThan32Characters_ShouldFail()
        {
            // Arrange
            var createAdIdRequest = new CreateAdIdRequest()
            {
                Campaign = "This text has more than 32 characters."
            };

            // Act
            var validationResult = await _validator.ValidateAsync(createAdIdRequest);

            // Assert
            validationResult.IsValid.Should().BeFalse();
        }
    }
}
