namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;

    [TestFixture]
    public class CostNumberGeneratorServiceTests
    {
        [SetUp]
        public void Init()
        {
            _target = new CostNumberGeneratorService(_efContext);
        }

        private readonly EFContext _efContext = EFContextFactory.CreateInMemoryEFContext();
        private CostNumberGeneratorService _target;

        [Test]
        public async Task EmptyCostType_ReturnsEmptyString()
        {
            //Arrange
            var project = new Project();
            const string contentType = "";
            const string costType = "";
            const string expected = "";

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task NullCostType_ReturnsEmptyString()
        {
            //Arrange
            var project = new Project();
            const string contentType = null;
            const string costType = null;
            const string expected = "";

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task NullProject_ReturnsEmptyString()
        {
            //Arrange
            var project = new Project();
            const string contentType = "Video";
            const string costType = "Production";
            const string expected = "";

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task ProjectAndNoCosts_ReturnsFirstCostNumber()
        {
            //Arrange
            var project = new Project
                {
                    Id = Guid.NewGuid(),
                    AdCostNumber = "PG001"
                };
            const string contentType = "Video";
            const string costType = "Production";
            const string expected = "PG001V0000001";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task Buyout_ProjectAndNoCosts_ReturnsFirstCostNumber()
        {
            //Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                AdCostNumber = "PG001"
            };
            const string contentType = null;
            const string costType = "Buyout";
            const string expected = "PG001U0000001";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task Trafficking_ProjectAndNoCosts_ReturnsFirstCostNumber()
        {
            //Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                AdCostNumber = "PG001"
            };
            const string contentType = null;
            const string costType = "Trafficking";
            const string expected = "PG001T0000001";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task ProjectAndOneCost_ReturnsSecondCostNumber()
        {
            //Arrange
            var project = new Project
            {
                AdCostNumber = "PG001",
                Id = Guid.NewGuid()
            };
            const string contentType = "Video";
            const string costType = "Production";
            const int costCount = 1;
            const string expected = "PG001V0000002";

            var costs = new List<Cost>();
            for (var i = 0; i < costCount; i++)
            {
                costs.Add(new Cost { ProjectId = project.Id });
            }
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(costs);
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task ProjectAndTenCosts_ReturnsEleventhCostNumber()
        {
            //Arrange
            var project = new Project
            {
                AdCostNumber = "PG001",
                Id = Guid.NewGuid()
            };
            const string contentType = "Video";
            const string costType = "Production";
            const int costCount = 10;
            const string expected = "PG001V0000011";

            var costs = new List<Cost>();
            for (var i = 0; i < costCount; i++)
            {
                costs.Add(new Cost { ProjectId = project.Id });
            }
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(costs);
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().Be(expected);
        }

        [Test]
        public async Task ProjectLengthIs10Characters_CostsNumberShouldBe15Characters()
        {
            //Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                AdCostNumber = "PGG0000123"
            };
            const string contentType = "Video";
            const string costType = "Production";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Length.Should().Be(15);
        }

        [Test]
        public async Task ProjectLengthIs10Characters_ShouldContaintEntireProjectNumber_And_CostsModuleContribution_Should_Be5Character()
        {
            //Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                AdCostNumber = "PGG0000123"
            };
            const string contentType = null;
            const string costType = "Any";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().StartWith(project.AdCostNumber);
            result.Replace(project.AdCostNumber, "").Should().HaveLength(5);
        }

        [Test]
        public async Task ProjectLengthIs7Characters_ShouldContaintEntireProjectNumber_And_CostsModuleContribution_Should_Be8Character()
        {
            //Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                AdCostNumber = "PGG0123"
            };
            const string contentType = null;
            const string costType = "Any";
            _efContext.Project.Add(project);
            _efContext.Cost.AddRange(new List<Cost>());
            _efContext.SaveChanges();

            //Act
            var result = await _target.Generate(project.Id, costType, contentType);

            result.Should().StartWith(project.AdCostNumber);
            result.Replace(project.AdCostNumber, "").Should().HaveLength(8);
        }
    }
}