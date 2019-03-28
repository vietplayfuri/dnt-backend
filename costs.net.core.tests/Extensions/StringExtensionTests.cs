
namespace costs.net.core.tests.Extensions
{
    using core.Extensions;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void AddSpacesToSentence_EmptyString_Returns_EmptyString()
        {
            //Arrange
            var target = string.Empty;

            //Act
            var result = target.AddSpacesToSentence();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void AddSpacesToSentence_NoCapitals_Returns_SameString()
        {
            //Arrange
            var target = "nocapitalsinthisstring";
            var expected = target;

            //Act
            var result = target.AddSpacesToSentence();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void AddSpacesToSentence_WithCapitals_Returns_LongerStringWithSpacesBeforeCapitals()
        {
            //Arrange
            var target = "SomeCapitalsInThisString";
            var expected = "Some Capitals In This String";

            //Act
            var result = target.AddSpacesToSentence();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expected);
        }
    }
}
