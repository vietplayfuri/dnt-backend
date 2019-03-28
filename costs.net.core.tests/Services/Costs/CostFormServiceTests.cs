namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class CostFormServiceTests
    {
        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _sut = new CostFormService(_efContextMock.Object);
        }

        private Mock<EFContext> _efContextMock;
        private CostFormService _sut;

        private class TestModel
        {
            public string Name { get; set; }

            public ComplexProperty[] ComplexProperties { get; set; }

            public class ComplexProperty
            {
                public Guid Id { get; set; }

                public string PropertyName { get; set; }
            }
        }

        [Test]
        public async Task GetCostFormDetails_should_return_deserializedFormDetailsModel()
        {
            // Arrange
            var testModel = new TestModel
            {
                Name = "Test model 1",
                ComplexProperties = new[]
                {
                    new TestModel.ComplexProperty { Id = Guid.NewGuid(), PropertyName = "Property 1" },
                    new TestModel.ComplexProperty { Id = Guid.NewGuid(), PropertyName = "Property " }
                }
            };

            var testModelData = JsonConvert.SerializeObject(testModel);
            var costStageRevisionId = Guid.NewGuid();
            var costStageRevision = new CostStageRevision
            {
                Id = costStageRevisionId,
                CostFormDetails = new List<CostFormDetails>
                {
                    new CostFormDetails
                    {
                        FormDefinition = new FormDefinition
                        {
                            Name = "testModel"
                        },
                        CustomFormData = new CustomFormData
                        {
                            Data = testModelData
                        }
                    }
                }
            };

            _efContextMock.MockAsyncQueryable(new List<CostStageRevision> { costStageRevision }.AsQueryable(), c => c.CostStageRevision);

            // Act
            var result = await _sut.GetCostFormDetails<TestModel>(costStageRevisionId);

            // Assert
            result.Name.Should().Be(testModel.Name);
            result.ComplexProperties.Should().HaveCount(testModel.ComplexProperties.Length);
            result.ComplexProperties[0].Id.Should().Be(testModel.ComplexProperties[0].Id);
            result.ComplexProperties[0].PropertyName.Should().Be(testModel.ComplexProperties[0].PropertyName);
            result.ComplexProperties[1].Id.Should().Be(testModel.ComplexProperties[1].Id);
            result.ComplexProperties[1].PropertyName.Should().Be(testModel.ComplexProperties[1].PropertyName);
        }
    }
}