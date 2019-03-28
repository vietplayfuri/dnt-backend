namespace costs.net.core.tests.Services.AbstractTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Models;
    using core.Services.AbstractTypes;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class AbstractTypesServiceTests
    {
        private Mock<IMapper> _mapperMock;
        private Mock<EFContext> _efContextMock;
        private AbstractTypesService _sut;
        
        [SetUp]
        public void Init()
        {
            _mapperMock = new Mock<IMapper>();
            _efContextMock = new Mock<EFContext>();
            _sut = new AbstractTypesService(_mapperMock.Object, _efContextMock.Object);
        }

        [Test]
        public async Task GetByObjectIdAndClient_whenExists_shouldReturnAbstractType()
        {
            // Arrange
            var objectId = Guid.NewGuid();
            const BuType buType = BuType.Pg;
            const string moduleName = "name of client module";

            var module = new Module
            {
                Id = Guid.NewGuid(),
                ClientType = (ClientType)buType,
                AbstractType = new AbstractType
                {
                    Id = Guid.NewGuid(),
                    ObjectId = objectId,
                    Type = AbstractObjectType.Module.ToString()
                },
                Name = moduleName
            };
            var modules = new List<Module> { module };
            _efContextMock.MockAsyncQueryable(modules.AsQueryable(), c => c.Module);

            var expected = new core.Models.AbstractTypes.Module { Id = module.AbstractType.Id };
            _mapperMock.Setup(m => m.Map<core.Models.AbstractTypes.Module>(It.Is<Module>(em => em.Id == module.Id))).Returns(expected);

            // Act
            var result = await _sut.GetClientModule(buType);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(expected);
        }
    }
}
