namespace costs.net.core.tests.Mapping
{
    using System;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Admin;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class PolicyExceptionProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<AdminProfile>();
                m.AddProfile<PolicyExceptionProfile>();
            }));
        }

        [Test]
        public void PolicyException_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void PolicyExceptionStatus_To_KeyValue_IsValid()
        {
            //Arrange
            var expected = new KeyValue
            {
                Key = "PendingApproval",
                Value = "Pending Approval"
            };
            var policyExceptionStatus = PolicyExceptionStatus.PendingApproval;

            //Act
            var result = _mapper.Map<PolicyExceptionStatus, KeyValue>(policyExceptionStatus);

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void Entity_To_Model_IsValid()
        {
            var exceptionKey = "TestException1";
            var exceptionValue = "Test Exception One";
            var entity = new PolicyException
            {
                CostImplication = "A cost implication",
                Id = Guid.NewGuid(),
                CostStageRevisionId = Guid.NewGuid(),
                ExceptionType = new dataAccess.Entity.DictionaryEntry
                {
                    Key = exceptionKey,
                    Value = exceptionValue
                },
                Reason = "A valid reason",
                Status = PolicyExceptionStatus.Approved
            };
            entity.CreatedInRevisionId = entity.CostStageRevisionId;
            var result = _mapper.Map<PolicyException, core.Models.PolicyExceptions.PolicyException>(entity);

            result.Should().NotBeNull();
            result.CostImplication.Should().NotBeNull();
            result.CostImplication.Should().Be(entity.CostImplication);
            result.ExceptionType.Should().NotBeNull();
            result.ExceptionType.Key.Should().Be(exceptionKey);
            result.ExceptionType.Value.Should().Be(exceptionValue);
            result.Id.Should().HaveValue();
            result.Id.Should().Be(entity.Id);
            result.CostStageRevisionId.Should().Be(entity.CostStageRevisionId);
            result.CreatedInRevisionId.Should().Be(entity.CreatedInRevisionId);
            result.Reason.Should().NotBeNull();
            result.Reason.Should().Be(entity.Reason);
            result.Status.Should().NotBeNull();
            result.Status.Key.Should().NotBeNull();
            result.Status.Key.Should().Be(PolicyExceptionStatus.Approved.ToString());
        }

        [Test]
        public void Model_To_Entity_IsValid()
        {
            var exceptionKey = "TestException1";
            var exceptionValue = "Test Exception One";
            var exceptionId = Guid.NewGuid();
            var model = new core.Models.PolicyExceptions.PolicyException
            {
                CostImplication = "A cost implication",
                Id = Guid.NewGuid(),
                CreatedInRevisionId = Guid.NewGuid(),
                ExceptionType = new core.Models.Admin.DictionaryEntry
                {
                    Id = exceptionId,
                    Key = exceptionKey,
                    Value = exceptionValue
                },
                Reason = "A valid reason",
                Status = new KeyValue
                {
                    Key = PolicyExceptionStatus.Approved.ToString()
                }
            };
            var result = _mapper.Map<core.Models.PolicyExceptions.PolicyException, PolicyException>(model);

            result.Should().NotBeNull();
            result.CostImplication.Should().NotBeNull();
            result.CostImplication.Should().Be(model.CostImplication);
            result.ExceptionTypeId.Should().Be(exceptionId);
            result.CreatedInRevisionId.Should().Be(model.CreatedInRevisionId.Value);
            result.Id.Should().Be(model.Id.Value);
            result.Reason.Should().NotBeNull();
            result.Reason.Should().Be(model.Reason);
            result.Status.Should().NotBeNull();
        }
    }
}
