namespace costs.net.core.tests.Services.Approvals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using Builders.Requisitioner;
    using core.Services.Approvals;
    using FluentAssertions;
    using core.Models;
    using core.Models.Approvals;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RequisitionerServiceTests
    {
        private Mock<IRequisitionerBuilder> _builderMock;
        private RequisitionerService _service;

        [SetUp]
        public void Init()
        {
            _builderMock = new Mock<IRequisitionerBuilder>();
            _service = new RequisitionerService(new[]
            {
                new Lazy<IRequisitionerBuilder, PluginMetadata>(() => _builderMock.Object, new PluginMetadata { BuType = BuType.Pg })
            });
        }

        [Test]
        public async Task Get_Always_ShouldGetRequisitionersFromBuilder()
        {
            // Arrange
            var requisitionerId = Guid.NewGuid();
            _builderMock.Setup(b => b.GetRequisitioners()).ReturnsAsync(new List<RequisitionerModel>
            {
                new RequisitionerModel
                {
                    Id = requisitionerId
                }
            });

            // Act
            var result = await _service.Get(BuType.Pg);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(requisitionerId);
        }
    }
}