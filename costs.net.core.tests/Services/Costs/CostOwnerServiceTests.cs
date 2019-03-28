namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using core.Events.Cost;
    using core.Models;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.User;
    using costs.net.core.Mapping;
    using costs.net.core.Services.Notifications;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Exception;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class CostOwnerServiceTests
    {
        private UserIdentity _userIdentity;
        private CostUser _identityUser;
        private EFContext _efContext;
        private Mock<ICostService> _costServiceMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IPgUserService> _pgUserService;
        private IEnumerable<Lazy<IPgUserService, PluginMetadata>> _pluginUserServices;
        private Mock<IActivityLogService> _activityLogServiceMock;
        private Mock<IEmailNotificationService> _emailNotificationServiceMock;
        private CostOwnerService _costOwnerService;
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _userIdentity = new UserIdentity { Id = Guid.NewGuid(), BuType = BuType.Pg };
            _identityUser = new CostUser { Id = _userIdentity.Id };
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _efContext.CostUser.Add(_identityUser);
            _efContext.SaveChanges();

            _costServiceMock = new Mock<ICostService>();
            _eventServiceMock = new Mock<IEventService>();
            _pgUserService = new Mock<IPgUserService>();
            _pluginUserServices = new[]
            {
                new Lazy<IPgUserService, PluginMetadata>(() => _pgUserService.Object, new PluginMetadata { BuType = BuType.Pg })
            };
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _emailNotificationServiceMock = new Mock<IEmailNotificationService>();
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<UserProfile>();
            }));
            _costOwnerService = new CostOwnerService(_efContext, _costServiceMock.Object, _eventServiceMock.Object, _pluginUserServices,
                _activityLogServiceMock.Object, _emailNotificationServiceMock.Object, _mapper);
        }

        [Test]
        public async Task ChangeOwner_WhenValidUserIdentity_ShouldLoadCostAndOwnerFrom()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);
            _pgUserService.Setup(us => us.IsUserAgencyOwner(owner)).Returns(true);

            // Act
            await _costOwnerService.ChangeOwner(_userIdentity, costId, ownerId);

            // Assert
            _costServiceMock.Verify(cs => cs.ChangeOwner(_userIdentity, cost, owner));
        }

        [Test]
        public void ChangeOwner_WhenUserIdentityIsNull_ShouldThrowException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);

            // Act
            // Assert
            _costOwnerService.Awaiting(s => s.ChangeOwner(null, costId, ownerId)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ChangeOwner_WhenCostIsNotInDB_ShouldThrowException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);

            // Act
            // Assert
            _costOwnerService.Awaiting(s => s.ChangeOwner(_userIdentity, costId, ownerId)).ShouldThrow<EntityNotFoundException<Cost>>();
        }

        [Test]
        public void ChangeOwner_WhenNewOwnertIsNotInDB_ShouldThrowException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();

            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);

            // Act
            // Assert
            _costOwnerService.Awaiting(s => s.ChangeOwner(_userIdentity, costId, ownerId)).ShouldThrow<EntityNotFoundException<CostUser>>();
        }

        [Test]
        public async Task ChangeOwner_WhenSuccessful_ShouldEmitCostOwnerChangedEvent()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);
            _pgUserService.Setup(us => us.IsUserAgencyOwner(owner)).Returns(true);

            // Act
            await _costOwnerService.ChangeOwner(_userIdentity, costId, ownerId);

            // Assert
            _eventServiceMock.Verify(a => a.SendAsync(It.IsAny<CostOwnerChanged>()));
        }

        [Test]
        public void ChangeOwner_WhenUserCanNotChangeOwner_ShouldThrowException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(false);

            // Act
            // Assert
            _costOwnerService.Awaiting(s => s.ChangeOwner(_userIdentity, costId, ownerId)).ShouldThrow<Exception>();
        }

        [Test]
        public async Task ChangeOwner_WhenSuccessful_ShouldLogCostOwnerChangedActivity()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser { Id = ownerId };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);
            _pgUserService.Setup(us => us.IsUserAgencyOwner(owner)).Returns(true);

            // Act
            await _costOwnerService.ChangeOwner(_userIdentity, costId, ownerId);

            // Assert
            _activityLogServiceMock.Verify(a => a.Log(It.IsAny<core.Models.ActivityLog.CostOwnerChanged>()));
        }

        [Test]
        public async Task ChangeOwner_WhenNewOwnerHasAgencyOwnerRole_ShouldChangeCostOwner()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser
            {
                Id = ownerId
            };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);
            _pgUserService.Setup(us => us.IsUserAgencyOwner(owner)).Returns(true);

            // Act
            await _costOwnerService.ChangeOwner(_userIdentity, costId, ownerId);

            // Assert
            _costServiceMock.Verify(c => c.ChangeOwner(_userIdentity, cost, owner), Times.Once);
        }

        [Test]
        public async Task ChangeOwner_WhenNewOwnerHasAgencyAdminRole_ShouldChangeCostOwner()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser
            {
                Id = ownerId
            };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);
            _pgUserService.Setup(us => us.IsUserAgencyAdmin(owner)).Returns(true);

            // Act
            await _costOwnerService.ChangeOwner(_userIdentity, costId, ownerId);

            // Assert
            _costServiceMock.Verify(c => c.ChangeOwner(_userIdentity, cost, owner), Times.Once);
        }

        [Test]
        public void ChangeOwner_NewOwnerIsNotAgencyOwner_ShouldThorwException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var ownerId = Guid.NewGuid();
            var owner = new CostUser
            {
                Id = ownerId
            };

            _pgUserService.Setup(us => us.IsUserAgencyOwner(owner)).Returns(false);

            _efContext.Cost.Add(cost);
            _efContext.CostUser.Add(owner);
            _efContext.SaveChanges();
            _pgUserService.Setup(s => s.IsUserAgencyAdmin(_identityUser)).Returns(true);

            // Act
            // Assert
            _costOwnerService.Awaiting(os => os.ChangeOwner(_userIdentity, costId, ownerId)).ShouldThrow<Exception>();
        }

        [Test]
        public async Task GetHistory_ShouldReturnEntriesForSpecifiedCost()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                Owner = new CostUser()
            };
            var historicalUsers = new List<CostUser>()
            {
                new CostUser()
                {
                    Id = Guid.NewGuid(),
                },
                new CostUser()
                {
                    Id = Guid.NewGuid(),
                },
                new CostUser()
                {
                    Id = Guid.NewGuid(),
                },
            };
            var historicalOwners = new List<CostOwner>()
            {
                new CostOwner()
                {
                    CostId = costId,
                    EndDate = DateTime.MinValue,
                    Id = Guid.NewGuid(),
                    UserId = historicalUsers[0].Id,
                },
                new CostOwner()
                {
                    CostId = costId,
                    EndDate = DateTime.MinValue,
                    Id = Guid.NewGuid(),
                    UserId = historicalUsers[1].Id,
                },
                new CostOwner()
                {
                    CostId = Guid.NewGuid(),
                    EndDate = DateTime.MinValue,
                    Id = Guid.NewGuid(),
                    UserId = historicalUsers[2].Id,
                },
            };

            _efContext.Cost.Add(cost);
            _efContext.CostUser.AddRange(historicalUsers);
            _efContext.CostOwner.AddRange(historicalOwners);
            _efContext.SaveChanges();

            // Act
            var result = (await _costOwnerService.GetHistory(_userIdentity, costId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            Assert.That(result.All(x => x.CostId == cost.Id));
            Assert.That(result.Any(x => x.UserId == historicalUsers[0].Id));
            Assert.That(result.Any(x => x.UserId == historicalUsers[1].Id));
            Assert.That(!result.Any(x => x.UserId == historicalUsers[2].Id));
        }

        /// <summary>
        /// https://jira.adstream.com/browse/SPB-2813
        /// </summary>
        [Test]
        public void ChangeOwner_ChangeToTheCurrentOwner_ShouldThorwException()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var owner = new CostUser() { Id = ownerId };
            var cost = new Cost
            {
                Id = costId,
                OwnerId = ownerId,
                Owner = owner
            };

            _efContext.Cost.Add(cost);
            _efContext.SaveChanges();

            // Act
            // Assert
            _costOwnerService.Awaiting(os => os.ChangeOwner(_userIdentity, costId, ownerId)).ShouldThrow<Exception>();
        }
    }
}
