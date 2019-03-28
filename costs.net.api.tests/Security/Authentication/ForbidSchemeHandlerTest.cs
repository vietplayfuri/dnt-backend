namespace costs.net.api.tests.Security.Authentication
{
    using System.Net;
    using System.Threading.Tasks;
    using api.Security.Authentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ForbidSchemeHandlerTest
    {
        [SetUp]
        public void Init()
        {
            _handler = new ForbidSchemeHandler();
        }

        private ForbidSchemeHandler _handler;

        [Test]
        public async Task InitializeAsync_Always_ShouldSetForbiddenStatusCode()
        {
            // Arrange
            var schema = new AuthenticationScheme("TestSchema", "TestSchema", typeof(ForbidSchemeHandler));
            var httpContextMock = new Mock<HttpContext>();
            var httpResponsetMock = new Mock<HttpResponse>();
            httpContextMock.Setup(c => c.Response).Returns(httpResponsetMock.Object);

            // Act
            await _handler.InitializeAsync(schema, httpContextMock.Object);

            // Assert
            httpResponsetMock.VerifySet(c => c.StatusCode = (int) HttpStatusCode.Forbidden);
        }
    }
}