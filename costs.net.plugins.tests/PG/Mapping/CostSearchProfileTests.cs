namespace costs.net.plugins.tests.PG.Mapping
{
    using System;
    using AutoMapper;
    using core.Builders.Response;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Mapping;

    [TestFixture]
    public class CostSearchProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<CostSearchProfile>()));
        }

        [Test]
        public void CostSearchProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
        [Test]
        public void Cost_To_CostSearchItem_IsValid()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var createdBy = Guid.NewGuid();
            var modified = DateTime.UtcNow;
            var created = DateTime.UtcNow;
            var costNumber = "1.1.1";
            var projectId = Guid.NewGuid();
            var costType = CostType.Production;
            var cost = new Cost
            {
                CostNumber = costNumber,
                CostType = costType,
                Created = created,
                Modified = modified,
                CreatedById = createdBy,
                OwnerId = createdBy,
                Id = costId,
                ProjectId = projectId,
                UserModified = modified
            };

            // Act
            var model = _mapper.Map<Cost, CostSearchItem>(cost);

            // Assert
            model.Id.Should().Be(costId.ToString());
        }
    }
}