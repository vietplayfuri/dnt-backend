namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Costs;
    using core.Models.User;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using Constants = Constants;

    [TestFixture]
    public class UserProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<UserProfile>()));
        }

        [Test]
        public void CostUser_To_CostUserModel_IsValid()
        {
            // Arrange
            var costUser = GetUser();

            // Act
            var model = _mapper.Map<CostUser, CostUserModel>(costUser);

            // Assert
            model.Id.Should().Be(costUser.Id);
            model.BusinessRoles.Count.Should().Be(1);
            model.BusinessRoles.First().Value.Should().Be(costUser.UserBusinessRoles.First().BusinessRole.Value);
            model.BusinessRoles.First().Id.Should().Be(costUser.UserBusinessRoles.First().BusinessRole.Id);
            model.BusinessRoles.First().Id.Should().Be(costUser.UserBusinessRoles.First().BusinessRoleId);
            model.BusinessRoles.First().ObjectId.Should().Be(costUser.UserBusinessRoles.First().ObjectId);
            model.BusinessRoles.First().ObjectId.Should().Be(costUser.UserBusinessRoles.First().ObjectId);
            model.BusinessRoles.First().ObjectType.Should().Be(costUser.UserBusinessRoles.First().ObjectType);
        }

        [Test]
        public void CostUser_To_UserIdentity_IsValid()
        {
            // Arrange
            var costUser = GetUser();

            // Act
            var model = _mapper.Map<CostUser, UserIdentity>(costUser);

            // Assert
            model.Id.Should().Be(costUser.Id);
            model.GdamUserId.Should().Be(costUser.GdamUserId);
            model.Email.Should().Be(costUser.Email);
            model.FirstName.Should().Be(costUser.FirstName);
            model.LastName.Should().Be(costUser.LastName);
            model.FullName.Should().Be(costUser.FullName);
            model.AgencyId.Should().Be(costUser.AgencyId);
        }

        [Test]
        public void NotificationSubscriber_To_CostWatcherModel_IsValid()
        {
            // Arrange
            var notificationSubscriber = GetNotificationSubscriber();

            // Act
            var model = _mapper.Map<NotificationSubscriber, CostWatcherModel>(notificationSubscriber);

            // Assert
            model.UserId.Should().Be(notificationSubscriber.CostUser.Id);
            model.FullName.Should().Be(notificationSubscriber.CostUser.FullName);
            model.Owner.Should().Be(notificationSubscriber.Owner);
            model.CostId.Should().Be(notificationSubscriber.CostId);
            model.BusinessRoles.Count.Should().Be(1);
        }

        [Test]
        public void CostUser_To_UserMode_IsValid()
        {
            // Arrange
            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                FirstName = "test first name",
                LastName = "test last name",
                FullName = "test full name",
                Email = "test email",
                Agency = new Agency
                {
                    Name = "test agency name",
                    Country = new Country
                    {
                        Name = "test country name"
                    }
                }
            };

            // Act
            var model = _mapper.Map<CostUser, UserModel>(costUser);

            // Assert
            model.Id.Should().Be(costUser.Id);
            model.FirstName.Should().Be(costUser.FirstName);
            model.LastName.Should().Be(costUser.LastName);
            model.FullName.Should().Be(costUser.FullName);
            model.Email.Should().Be(costUser.Email);
            model.AgencyName.Should().Be(costUser.Agency.Name);
            model.AgencyCountryName.Should().Be(costUser.Agency.Country.Name);
        }

        private CostUser GetUser()
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var email = "me@me.com";
            var agencyId = Guid.NewGuid();
            var businessRoleId = Guid.NewGuid();
            var objectId = Guid.NewGuid();
            return new CostUser
            {
                Id = userId,
                FirstName = "Clark",
                LastName = "Kent",
                FullName = "Clark Kent",
                GdamUserId = "gdam",
                Email = email,
                AgencyId = agencyId,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        Id = Guid.NewGuid(),
                        BusinessRoleId = businessRoleId,
                        BusinessRole = new BusinessRole
                        {
                            Id = businessRoleId,
                            Key = "Owner",
                            Value = "Owner",
                            RoleId = roleId
                        },
                        ObjectId = objectId,
                        ObjectType = "Tent"
                    }
                }
            };
        }

        private Cost GetCost(CostUser user)
        {
            return new Cost
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                CostNumber = "number",
                CostType = CostType.Production,
                CostStages = new List<CostStage>(),
                Created = DateTime.Now,
                CreatedById = user.Id,
                OwnerId = user.Id,
                UserGroups = new[] { $"{Guid.NewGuid()}" },
                Deleted = false
            };
        }
        private NotificationSubscriber GetNotificationSubscriber()
        {
            var costUser = GetUser();
            var cost = GetCost(costUser);
            return new NotificationSubscriber
            {
                CostUser = costUser,
                CostUserId = costUser.Id,
                Cost = cost,
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                Owner = false
            };
        }


        [Test]
        public void UserBusinessRole_To_AccessDetail_IsValid()
        {
            // Arrange
            var businessRole = GetUserBusinessRole(1, Constants.AccessObjectType.Region);

            // Act
            var model = _mapper.Map<AccessDetail>(businessRole);

            // Assert
            model.ObjectId.Should().Be(businessRole.ObjectId);
            model.ObjectType.Should().Be(businessRole.ObjectType);
            model.BusinessRoleId.Should().Be(businessRole.BusinessRoleId);
            model.LabelName.Should().BeNullOrEmpty();
            model.OriginalObjectId.Should().BeNull();
        }

        [Test]
        public void UserBusinessRoles_To_AccessDetails_IsValid()
        {
            // Arrange
            var businessRole1 = GetUserBusinessRole(1, Constants.AccessObjectType.Region);
            var businessRole2 = GetUserBusinessRole(2, Constants.AccessObjectType.Region);
            var businessRole3 = GetUserBusinessRole(3, Constants.AccessObjectType.Region);
            var businessRoles = new List<UserBusinessRole>{businessRole1, businessRole2, businessRole3};
            // Act
            var models = _mapper.Map<IEnumerable<AccessDetail>>(businessRoles);

            // Assert
            models.Count().Should().Be(3);
        }

        private UserBusinessRole GetUserBusinessRole(int integer, string objectType)
        {
            var labels = new List<string>();
            for (var i = 0; i < integer; i++)
            {
                labels.Add($"labe{i}");
            }
            return new UserBusinessRole
            {
                Id = Guid.NewGuid(),
                BusinessRoleId = Guid.NewGuid(),
                CostUserId = Guid.NewGuid(),
                Labels = labels.ToArray(),
                ObjectType = objectType,
                ObjectId = Guid.NewGuid()
            };
        }

        [Test]
        public void UserProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}