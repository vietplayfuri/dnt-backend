namespace costs.net.plugins.tests.PG.Builders.Requisitioner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Builders.Requisitioner;

    [TestFixture]
    public class PgRequisitionerBuilderTests
    {
        private Mock<EFContext> _efContextMock;
        private PgRequisitionerBuilder _builder;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _builder = new PgRequisitionerBuilder(_efContextMock.Object);
        }

        [Test]
        public async Task GetRequisitioners_Always_ShouldReturnAllCostUsersWithBrandManagerRole()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var users = new List<CostUser>
            {
                new CostUser
                {
                    Id = userId1,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole()
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = Constants.BusinessRole.BrandManager
                            }
                        }
                    }
                },
                new CostUser
                {
                    Id = userId2,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole()
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.Ipm,
                                Value = Constants.BusinessRole.Ipm
                            }
                        }
                    }
                }
            };
            _efContextMock.MockAsyncQueryable(users.AsQueryable(), c => c.CostUser);

            // Act
            var requisitioners = await _builder.GetRequisitioners();

            // Assert
            requisitioners.Should().HaveCount(1);
            requisitioners.First().Id.Should().Be(userId1);
        }

        [Test]
        public async Task GetRequisitioners_Always_ShouldReturnAllBusinessRoleValueRatherThenKey()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            const string businessRoleName1 = "Not the same as key 1";
            const string businessRoleName2 = "Not the same as key 2";
            var users = new List<CostUser>
            {
                new CostUser
                {
                    Id = userId1,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = businessRoleName1
                            }
                        }
                    }
                },
                new CostUser
                {
                    Id = userId2,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.Ipm,
                                Value = businessRoleName2
                            }
                        }
                    }
                }
            };
            _efContextMock.MockAsyncQueryable(users.AsQueryable(), c => c.CostUser);

            // Act
            var requisitioners = await _builder.GetRequisitioners();

            // Assert
            requisitioners.Should().HaveCount(1);
            requisitioners.First().Id.Should().Be(userId1);
            requisitioners.First().BusinessRoles.First().Should().Be(businessRoleName1);
        }

        [Test]
        public async Task GetRequisitioners_Always_ShouldReturnUniqueRoles()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            const string businessRoleName1 = "Brand Manager";
            var users = new List<CostUser>
            {
                new CostUser
                {
                    Id = userId1,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = businessRoleName1
                            }
                        },
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = businessRoleName1
                            }
                        }
                    }
                }
            };
            _efContextMock.MockAsyncQueryable(users.AsQueryable(), c => c.CostUser);

            // Act
            var requisitioners = await _builder.GetRequisitioners();

            // Assert
            requisitioners.First().BusinessRoles.Should().HaveCount(1);
            requisitioners.First().BusinessRoles.First().Should().Be(businessRoleName1);
        }
    }
}