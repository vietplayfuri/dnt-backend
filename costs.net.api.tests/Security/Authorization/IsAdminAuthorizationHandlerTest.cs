namespace costs.net.api.tests.Security.Authorization
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using api.Security.Authorization;
    using core.Models.Utils;
    using FluentAssertions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class IsAdminAuthorizationHandlerTest
    {
        private AppSettings _appSettings;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private IsAdminAuthorizationHandler _handler;

        [SetUp]
        public void Init()
        {
            _appSettings = new AppSettings();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(s => s.Value).Returns(_appSettings);
            _handler = new IsAdminAuthorizationHandler(_appSettingsMock.Object);
        }

        [Test]
        public async Task HandleAsync_WhenUserIsAdmin_ShouldSucceed()
        {
            // Arrange
            var adminGdamUserId = "12341231";
            _appSettings.AdminUser = adminGdamUserId;

            var claims = new List<Claim>
            {
                new Claim(CostClaimTypes.GdamId, adminGdamUserId)
            };
            var claimPrincipal = new ClaimsPrincipal(new CostsClaimsIdentity(claims, "anyname"));

            var authorizationContext = new AuthorizationHandlerContext(new[] { new IsAdminRequirement() }, claimPrincipal, null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            authorizationContext.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task HandleAsync_WhenUserIsNotAdmin_ShouldNotSucceed()
        {
            // Arrange
            var adminGdamUserId = "12341231";
            _appSettings.AdminUser = adminGdamUserId;

            var claims = new List<Claim>
            {
                new Claim(CostClaimTypes.GdamId, "different id")
            };
            var claimPrincipal = new ClaimsPrincipal(new CostsClaimsIdentity(claims, "anyname"));

            var authorizationContext = new AuthorizationHandlerContext(new[] { new IsAdminRequirement() }, claimPrincipal, null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            authorizationContext.HasSucceeded.Should().BeFalse();
        }
    }
}