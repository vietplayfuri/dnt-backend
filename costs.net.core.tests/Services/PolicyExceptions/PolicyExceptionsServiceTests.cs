
namespace costs.net.core.tests.Services.PolicyExceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Mapping;
    using core.Models.User;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.PolicyExceptions;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using PolicyException = core.Models.PolicyExceptions.PolicyException;

    [TestFixture]
    public class PolicyExceptionsServiceTests
    {
        private PolicyExceptionsService _target;
        private Mock<EFContext> _efContextMock;
        private Mock<IPermissionService> _permissionServiceMock;
        private IMapper _mapper;
        private UserIdentity _user;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();

            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(PolicyExceptionProfile)
                )
            );
            _mapper = new Mapper(configuration);
            _permissionServiceMock = new Mock<IPermissionService>();
            var permissionMapper = new PermissionMapper();
            var activityLogServiceMock = new Mock<IActivityLogService>();
            _target = new PolicyExceptionsService(_efContextMock.Object, 
                _mapper, 
                _permissionServiceMock.Object, 
                permissionMapper, 
                activityLogServiceMock.Object);

            _user = new UserIdentity
            {
                Id = Guid.NewGuid(),
                IpAddress = "127.0.0.1"
            };
        }

        [Test]
        public async Task Get_PolicyExceptions_Cost_DoesNotExist()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            _efContextMock.MockAsyncQueryable(new List<Cost>().AsQueryable(), d => d.Cost);
            _efContextMock.MockAsyncQueryable(new List<dataAccess.Entity.PolicyException>().AsQueryable(), d => d.PolicyException);

            //Act
            var result = await _target.Get(costId, costStageRevisionId, _user);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Result.Should().BeNull();
        }

        [Test]
        public async Task Get_PolicyExceptions_CostStageRevision_DoesNotExist()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costs = new List<Cost>
            {
                new Cost
                {
                    Id = costId
                }
            };
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), d => d.Cost);
            _efContextMock.MockAsyncQueryable(new List<CostStageRevision>().AsQueryable(), d => d.CostStageRevision);
            _efContextMock.MockAsyncQueryable(new List<dataAccess.Entity.PolicyException>().AsQueryable(), d => d.PolicyException);

            //Act
            var result = await _target.Get(costId, costStageRevisionId, _user);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue(); //Should be true because if the user used the wrong revision id, we will returns empty data
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(0);
        }

        [Test]
        public async Task Get_PolicyExceptions_No_Exceptions_For_CostStageRevision()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var expected = 0;
            var costs = new List<Cost>
            {
                new Cost
                {
                    Id = costId
                }
            };
            var costStageRevisions = new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    Id = costStageRevisionId
                }
            };
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), d => d.Cost);
            _efContextMock.MockAsyncQueryable(costStageRevisions.AsQueryable(), d => d.CostStageRevision);
            _efContextMock.MockAsyncQueryable(new List<dataAccess.Entity.PolicyException>().AsQueryable(), d => d.PolicyException);

            //Act
            var result = await _target.Get(costId, costStageRevisionId, _user);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(expected);
        }

        [Test]
        public async Task Get_PolicyExceptions_Has_One_Exception_For_CostStageRevision()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costs = new List<Cost>
            {
                new Cost
                {
                    Id = costId
                }
            };
            var costStageRevisions = new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    Id = costStageRevisionId
                }
            };

            var policyExceptions = new List<dataAccess.Entity.PolicyException>();
            var policyException = new dataAccess.Entity.PolicyException
            {
                CostImplication = "A cost implication",
                Reason = "A policy exception reason",
                CostStageRevisionId = costStageRevisionId,
                Status = PolicyExceptionStatus.PendingApproval
            };
            policyExceptions.Add(policyException);
            var expected = _mapper.Map<PolicyException>(policyException);

            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), d => d.Cost);
            _efContextMock.MockAsyncQueryable(costStageRevisions.AsQueryable(), d => d.CostStageRevision);
            _efContextMock.MockAsyncQueryable(policyExceptions.AsQueryable(), d => d.PolicyException);

            //Act
            var result = await _target.Get(costId, costStageRevisionId, _user);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(1);
            result.Result.First().Should().Be(expected);
        }

        [Test]
        public async Task Get_PolicyExceptions_Has_Many_Exceptions_For_CostStageRevision()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var expected = 20;

            var costs = new List<Cost>
            {
                new Cost
                {
                    Id = costId
                }
            };
            var costStageRevisions = new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    Id = costStageRevisionId
                }
            };

            var policyExceptions = new List<dataAccess.Entity.PolicyException>();
            for (int i = 0; i < expected; i++)
            {
                var policyException = new dataAccess.Entity.PolicyException
                {
                    CostImplication = "A cost implication" + i,
                    Reason = "A policy exception reason",
                    CostStageRevisionId = costStageRevisionId,
                    Status = PolicyExceptionStatus.PendingApproval
                };
                policyExceptions.Add(policyException);
            }
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), d => d.Cost);
            _efContextMock.MockAsyncQueryable(costStageRevisions.AsQueryable(), d => d.CostStageRevision);
            _efContextMock.MockAsyncQueryable(policyExceptions.AsQueryable(), d => d.PolicyException);

            //Act
            var result = await _target.Get(costId, costStageRevisionId, _user);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().HaveCount(expected);
        }
    }
}
