namespace costs.net.plugins.tests.Services.User
{
    using System.Collections.Generic;
    using System.Linq;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    public class CanManageUsersUserServiceTests : UserServiceTestsBase
    {
        private static readonly string[] CanNotChangeOwnerRoles = 
            Constants.BusinessRole.BusinessRoles.Where(br => br != Constants.BusinessRole.AgencyAdmin).ToArray();

        private static readonly string[] ManageUsersBusinessRoles =
        {
            Constants.BusinessRole.GovernanceManager,
            Constants.BusinessRole.AgencyAdmin,
            Constants.BusinessRole.AdstreamAdmin
        };

        [Test]
        public void CanManageUsers_When_UserWithAnyNonManageUsersBusinessRole_Should_ReturnFalse()
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
        [TestCaseSource(nameof(ManageUsersBusinessRoles))]
        public void CanManageUsers_When_UserWithAnyManageUsersBusinessRole_Should_ReturnTrue(string businessRole)
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
            var result = _pgUserService.CanManageUsers(user);

            // Assert
            result.Should().Be(true);
        }

        [Test]
        public void CanChangeOwner_When_UserIsAgencyAdmin_ShouldReturnTrue()
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
                            Key = Constants.BusinessRole.AgencyAdmin
                        }
                    }
                }
            };
            EfContext.CostUser.Add(user);
            EfContext.SaveChanges();

            // Act
            var result = _pgUserService.IsUserAgencyAdmin(user);

            // Assert
            result.Should().Be(true);
        }

        [Test]
        [TestCaseSource(nameof(CanNotChangeOwnerRoles))]
        public void CanChangeOwner_When_UserIsAgencyAdmin_ShouldReturnTrue(string businessRole)
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
            var result = _pgUserService.IsUserAgencyAdmin(user);

            // Assert
            result.Should().Be(false);
        }

        [Test]
        public void IsUserAgencyOwner_When_UserIsAgencyOwner_ShouldReturnTrue()
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
                            Key = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            EfContext.CostUser.Add(user);
            EfContext.SaveChanges();

            // Act
            var result = _pgUserService.IsUserAgencyOwner(user);

            // Assert
            result.Should().Be(true);
        }

    }
}