namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Costs;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ApprovalProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<ApprovalProfile>()));
        }

        [Test]
        public void ApprovalProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void ApprovalModel_Member_To_ApprovalMember_IsValid()
        {
            //Arrange
            var expectedMemberId = Guid.NewGuid();
            var model = new ApprovalModel.Member
            {
                Id = expectedMemberId
            };

            //Act
            var result = _mapper.Map<ApprovalModel.Member, ApprovalMember>(model);

            //Assert
            result.Should().NotBeNull();
            result.MemberId.Should().Be(expectedMemberId);
        }

        [Test]
        public void ApprovalMember_To_ApprovalModel_Member_IsValid()
        {
            //Arrange
            var expectedMemberId = Guid.NewGuid();
            var expectedFullName = "Mr Smith";
            var expectedEmail = "costs.admin@adstream.com";
            var expectedApprovalLimit = 25M;
            var expectedApprovalBandId = Guid.NewGuid();
            var expectedBusinessRole = "A business role";
            var expectedBusinessRoleCount = 1;
            var expectedComments = "Rejected cost because I wanted to";
            var businessRoles = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Value = expectedBusinessRole
                    }
                }
            };
            var entity = new ApprovalMember
            {
                MemberId = expectedMemberId,
                CostUser = new CostUser
                {
                    ApprovalBandId = expectedApprovalBandId,
                    ApprovalLimit = expectedApprovalLimit,
                    FullName = expectedFullName,
                    Email = expectedEmail,
                    UserBusinessRoles = businessRoles
                },
                RejectionDetails = new RejectionDetails
                {
                    Comments = expectedComments
                }
            };

            //Act
            var result = _mapper.Map<ApprovalMember, ApprovalModel.Member>(entity);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedMemberId);
            result.FullName.Should().Be(expectedFullName);
            result.Email.Should().Be(expectedEmail);
            result.ApprovalLimit.Should().Be(expectedApprovalLimit.ToString());
            result.ApprovalBandId.Should().Be(expectedApprovalBandId.ToString());
            result.BusinessRoles.Should().HaveCount(expectedBusinessRoleCount);
            result.BusinessRoles.First().Should().Be(expectedBusinessRole);
            result.Comments.Should().Be(expectedComments);
        }        
    }
}
