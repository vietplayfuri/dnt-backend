namespace costs.net.plugins.tests.Services.User
{
    using System.Collections.Generic;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    public class CanCreateCostPgUserServiceTests : UserServiceTestsBase
    {
        private static readonly string[] CreateCostBusinessRoles =
        {
            Constants.BusinessRole.AgencyOwner,
            Constants.BusinessRole.AgencyAdmin,
            Constants.BusinessRole.CentralAdaptationSupplier
        };

        [Test]
        public void CanCreateCost_When_UserWithAnyNonCostCreateBusinessRole_Should_ReturnFalse()
        {
            // Arrange
            var businessRole = "any random role....";
            var user = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = businessRole
                        }
                    }
                }
            };
            EfContext.CostUser.Add(user);
            EfContext.SaveChanges();

            // Act
            var result = _pgUserService.CanCreateCost(user);

            // Assert
            result.Should().Be(false);
        }

        [Test]
        [TestCaseSource(nameof(CreateCostBusinessRoles))]
        public void CanCreateCost_When_UserWithValidBusinessRole_Should_ReturnTrue(string businessRole)
        {
            // Arrange
            var user = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = businessRole
                        }
                    }
                }
            };
            EfContext.CostUser.Add(user);
            EfContext.SaveChanges();

            // Act
            var result = _pgUserService.CanCreateCost(user);

            // Assert
            result.Should().Be(true);
        }
    }
}