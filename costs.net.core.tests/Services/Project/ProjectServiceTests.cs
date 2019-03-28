namespace costs.net.core.tests.Services.Project
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture.NUnit3;
    using AutoMapper;
    using Builders.Response;
    using core.Services;
    using core.Services.Dictionary;
    using core.Services.Project;
    using dataAccess;
    using dataAccess.Entity;
    using ExternalResource.Gdam;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Serilog;
    using Microsoft.Extensions.Options;
    using core.Models.AMQ;
    using core.Models.Utils;
    using core.Services.Module;
    using FluentAssertions;
    using core.Services.Search;
    using Moq;
    using net.tests.common.Extensions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;
    using Project = dataAccess.Entity.Project;
    using Newtonsoft.Json.Linq;
    using costs.net.core.Extensions;

    [TestFixture]
    public class ProjectServiceTests
    {
        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _efContextMemory = EFContextFactory.CreateInMemoryEFContext();
            _dictionaryServiceMock = new Mock<IDictionaryService>();
            _moduleServiceMock = new Mock<IModuleService>();
            _gdamClientMock = new Mock<IGdamClient>();
            _loggerPsMock = new Mock<ILogger>();
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _mapperMock = new Mock<IMapper>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings { AdminUser = "4ef31ce1766ec96769b399c0", CostsAdminUserId = "dcc8c610-5eb5-473f-a5f7-b7d5d3ee9b55", BrandPrefix = new[] { "brand" } });
            _permissionServiceMock = new Mock<IPermissionService>();
            var databaseFacadeMock = new Mock<DatabaseFacade>(_efContextMock.Object);
            var dbContextTransactionMock = new Mock<IDbContextTransaction>();
            databaseFacadeMock.Setup(d => d.CurrentTransaction).Returns(dbContextTransactionMock.Object);
            _efContextMock.Setup(s => s.Database).Returns(databaseFacadeMock.Object);

            _projectService = new ProjectService(
                _loggerPsMock.Object,
                _efContextMock.Object,
                _mapperMock.Object,
                _dictionaryServiceMock.Object,
                _moduleServiceMock.Object,
                _appSettingsMock.Object,
                _gdamClientMock.Object,
                _elasticSearchServiceMock.Object
            );

            _projectServiceEfMemory = new ProjectService(
                _loggerPsMock.Object,
                _efContextMemory,
                _mapperMock.Object,
                _dictionaryServiceMock.Object,
                _moduleServiceMock.Object,
                _appSettingsMock.Object,
                _gdamClientMock.Object,
                _elasticSearchServiceMock.Object
            );

            _jsonReader = new JsonTestReader();
        }

        private JsonTestReader _jsonReader;
        private Mock<ILogger> _loggerPsMock;
        private Mock<IGdamClient> _gdamClientMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<EFContext> _efContextMock;
        private EFContext _efContextMemory;
        private Mock<IPermissionService> _permissionServiceMock;
        private ProjectService _projectService;
        private ProjectService _projectServiceEfMemory;
        private Mock<IDictionaryService> _dictionaryServiceMock;
        private Mock<IModuleService> _moduleServiceMock;

        [Test]
        public async Task HandleA5EventObject_ProjectCreated()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var projectFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var agencyFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(projectFilePath, false);
            _gdamClientMock.Setup(a => a.FindProjectById(a5Project._id)).ReturnsAsync(a5Project);
            _permissionServiceMock.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() });
            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                GdamUserId = a5Project.CreatedBy._id,
                ParentId = Guid.NewGuid()
            };
            var agency = new Agency { Id = Guid.NewGuid(), GdamAgencyId = a5Project.Agency._id, Labels = new[] { Constants.BusinessUnit.CostModulePrimaryLabelPrefix } };
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Brand", AdIdPrefix = "prefix", Sector = new Sector { AgencyId = agency.Id } };

            var a5Agency = await _jsonReader.GetObject<A5Agency>(agencyFilePath, true);
            _gdamClientMock.Setup(a => a.FindAgencyById(agency.GdamAgencyId)).ReturnsAsync(a5Agency);

            var agencies = new List<Agency> { agency }.AsQueryable();
            var projectsList = new List<Project>().AsQueryable();
            var costUsers = new List<CostUser> { costUser }.AsQueryable();
            var brands = new List<Brand> { brand }.AsQueryable();
            var dictionary =
                new Dictionary
                {
                    Name = "Campaign",
                    DictionaryEntries = new List<DictionaryEntry> { new DictionaryEntry { Id = Guid.NewGuid(), Key = "Key", Value = "Value", Visible = true } },
                    Id = Guid.NewGuid()
                };

            _moduleServiceMock.Setup(a => a.GetClientModulePerUserAsync(It.IsAny<CostUser>())).ReturnsAsync(new core.Models.AbstractTypes.Module { Id = Guid.NewGuid() });
            _dictionaryServiceMock.Setup(a => a.GetDictionaryWithEntriesByName(It.IsAny<Guid>(), It.IsAny<string>())).Returns(dictionary);
            _elasticSearchServiceMock.Setup(a => a.UpdateSearchItem(It.IsAny<ProjectSearchItem>(), Constants.ElasticSearchIndices.ProjectsIndexName)).Returns(Task.CompletedTask);
            _efContextMock.MockAsyncQueryable(projectsList, c => c.Project);
            _efContextMock.MockAsyncQueryable(costUsers, c => c.CostUser);
            _efContextMock.MockAsyncQueryable(brands, c => c.Brand);
            _efContextMock.MockAsyncQueryable(agencies, c => c.Agency);

            //Act
            await _projectService.AddProjectToDb(a5Project);

            //Assert
            _efContextMock.Verify(a => a.Add(It.IsAny<Project>()), Times.Once);
            _elasticSearchServiceMock.Verify(a => a.UpdateSearchItem(It.IsAny<ProjectSearchItem>(), Constants.ElasticSearchIndices.ProjectsIndexName), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectDeleted()
        {
            //Setup

            const string projectGdamId = "1jh2j3g3j1hg23j1g3";
            const string gdamUserId = "userId";
            var agencyId = Guid.NewGuid();
            var projectGuid = Guid.NewGuid();
            var project = new Project()
            {
                Id = projectGuid,
                GdamProjectId = projectGdamId,
                AgencyId = agencyId,
                Deleted = false,
            };
            _efContextMemory.Project.Add(project);
            await _efContextMemory.SaveChangesAsync();
            _mapperMock.Setup(m => m.Map<ProjectSearchItem[]>(It.IsAny<Project>())).Returns(new bool[1]
                .Select(i => new ProjectSearchItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Version = 1
                }).ToArray());
            //Act
            await _projectServiceEfMemory.DeleteProject(projectGdamId, gdamUserId);

            //Assert
            _elasticSearchServiceMock.Verify(es =>
                es.UpdateSearchItem(It.IsAny<ProjectSearchItem>(), Constants.ElasticSearchIndices.ProjectsIndexName), Times.Once);

            var expected = _efContextMemory.Project.ToList().First();
            expected.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task HandleA5EventObject_ProjectCreated_WithA4Brand_field()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var projectFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var agencyFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency_a4brandfield.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(projectFilePath, false);
            _gdamClientMock.Setup(a => a.FindProjectById(a5Project._id)).ReturnsAsync(a5Project);
            _permissionServiceMock.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() });
            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                GdamUserId = a5Project.CreatedBy._id,
                ParentId = Guid.NewGuid()
            };
            var agency = new Agency { Id = Guid.NewGuid(), GdamAgencyId = a5Project.Agency._id, Labels = new[] { Constants.BusinessUnit.CostModulePrimaryLabelPrefix } };
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Brand", AdIdPrefix = "prefix", Sector = new Sector { AgencyId = agency.Id } };

            var a5Agency = await _jsonReader.GetObject<A5Agency>(agencyFilePath, true);
            _gdamClientMock.Setup(a => a.FindAgencyById(agency.GdamAgencyId)).ReturnsAsync(a5Agency);

            var agencies = new List<Agency> { agency }.AsQueryable();
            var projectsList = new List<Project>().AsQueryable();
            var costUsers = new List<CostUser> { costUser }.AsQueryable();
            var brands = new List<Brand> { brand }.AsQueryable();
            var dictionary =
                new Dictionary
                {
                    Name = "Campaign",
                    DictionaryEntries = new List<DictionaryEntry> { new DictionaryEntry { Id = Guid.NewGuid(), Key = "Key", Value = "Value", Visible = true } },
                    Id = Guid.NewGuid()
                };

            _moduleServiceMock.Setup(a => a.GetClientModulePerUserAsync(It.IsAny<CostUser>())).ReturnsAsync(new core.Models.AbstractTypes.Module { Id = Guid.NewGuid() });
            _dictionaryServiceMock.Setup(a => a.GetDictionaryWithEntriesByName(It.IsAny<Guid>(), It.IsAny<string>())).Returns(dictionary);

            _efContextMock.MockAsyncQueryable(projectsList, c => c.Project);
            _efContextMock.MockAsyncQueryable(costUsers, c => c.CostUser);
            _efContextMock.MockAsyncQueryable(brands, c => c.Brand);
            _efContextMock.MockAsyncQueryable(agencies, c => c.Agency);

            //Act
            await _projectService.AddProjectToDb(a5Project);

            //Assert
            _efContextMock.Verify(a => a.Add(It.IsAny<Project>()), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectUpdated_BrandHasMultipleSectors_Query()
        {
            //Setup
            string brandName = "Rejoice";
            Guid agencyId = Guid.Parse("1c9eb953-f297-43ac-90d4-ecc5f5af5602");

            var brand1 = new Brand
            {
                Name = brandName,
                Id = Guid.NewGuid(),
                Sector = new Sector { Name = "Baby", AgencyId = agencyId }
            };
            var brand2 = new Brand
            {
                Id = Guid.NewGuid(),
                Name = brandName,
                Sector = new Sector { Name = "Family Care", AgencyId = agencyId }
            };
            var brand3 = new Brand
            {
                Id = Guid.NewGuid(),
                Name = brandName,
                Sector = new Sector { Name = "Feminine", AgencyId = agencyId }
            };

            var brands = new List<Brand> { brand1, brand2, brand3 };
            _efContextMemory.Brand.AddRange(brands);
            _efContextMemory.SaveChanges();

            var inputtedBrand = brand2;
            var expectedBrand = brand2;

            //Act
            var brand = _efContextMemory.Brand.FirstOrDefault(x => x.Sector.AgencyId == inputtedBrand.Sector.AgencyId && x.Sector.Name == inputtedBrand.Sector.Name && x.Name == inputtedBrand.Name);

            //Assert
            brand.Name.Should().Be(expectedBrand.Name);
            brand.Sector.Name.Should().Be(expectedBrand.Sector.Name);
            brand.Sector.AgencyId.Should().Be(expectedBrand.Sector.AgencyId);
        }
        [Test]
        public async Task HandleA5EventObject_ProjectUpdated_BrandHasMultipleSectors_A5BuHasA4_Query()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var projectFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(projectFilePath, false);
            var agencyFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency_a4brandfield.json";
            var a5Agency = await _jsonReader.GetObject<A5Agency>(agencyFilePath, true);
            JObject item = JObject.Parse(a5Project._cm.Common.ToString());
            var expectedOutput = "Brand";

            //Act
            var sectorName = string.Empty;
            if (string.IsNullOrWhiteSpace(sectorName))
            {
                if (a5Agency._cm.A4 == null || string.IsNullOrEmpty(a5Agency._cm.A4.Sector_field))
                {
                    // there is another case that sector is stored in Brand field and Brand Name is stored in Sub - Brand.
                    sectorName = item.GetPropertyValue("brand", true, true);
                }
                else
                {
                    sectorName = item.GetPropertyValue(RemoveCmCommon(a5Agency._cm.A4.Sector_field), true, true);
                }
            }
            //Assert
            sectorName.Should().NotBeNullOrEmpty();
            sectorName.Should().Be(expectedOutput);
        }
        [Test]
        public async Task HandleA5EventObject_ProjectUpdated_BrandHasMultipleSectors_A5BuHasNoA4_Query()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var projectFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(projectFilePath, false);
            var agencyFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency.json";
            var a5Agency = await _jsonReader.GetObject<A5Agency>(agencyFilePath, true);
            JObject item = JObject.Parse(a5Project._cm.Common.ToString());
            var expectedOutput = "Brand";

            //Act
            var sectorName = string.Empty;
            if (string.IsNullOrWhiteSpace(sectorName))
            {
                if (a5Agency._cm.A4 == null || string.IsNullOrEmpty(a5Agency._cm.A4.Sector_field))
                {
                    // there is another case that sector is stored in Brand field and Brand Name is stored in Sub - Brand.
                    sectorName = item.GetPropertyValue("brand", true, true);
                }
                else
                {
                    sectorName = item.GetPropertyValue(RemoveCmCommon(a5Agency._cm.A4.Sector_field), true, true);
                }
            }
            //Assert
            sectorName.Should().NotBeNullOrEmpty();
            sectorName.Should().Be(expectedOutput);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectUpdated_BrandHasMultipleSectors_A5BuHasA4AndSector_Query()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var projectFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(projectFilePath, false);
            var agencyFilePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency_a4brandfield.json";
            var a5Agency = await _jsonReader.GetObject<A5Agency>(agencyFilePath, true);
            JObject item = JObject.Parse(a5Project._cm.Common.ToString());
            var expectedOutput = "Sector";

            //Act
            var sectorName = item.GetPropertyValue("sector", true, true);
            if (string.IsNullOrWhiteSpace(sectorName))
            {
                if (a5Agency._cm.A4 == null || string.IsNullOrEmpty(a5Agency._cm.A4.Sector_field))
                {
                    // there is another case that sector is stored in Brand field and Brand Name is stored in Sub - Brand.
                    sectorName = item.GetPropertyValue("brand", true, true);
                }
                else
                {
                    sectorName = item.GetPropertyValue(RemoveCmCommon(a5Agency._cm.A4.Sector_field), true, true);
                }
            }
            //Assert
            sectorName.Should().NotBeNullOrEmpty();
            sectorName.Should().Be(expectedOutput);
        }

        [Test, AutoData]
        public async Task HandleA5EventObject_ProjectCreated_Not_Saved(A5Project project)
        {
            //Setup
            _gdamClientMock.Setup(a => a.FindProjectById(project._id)).ReturnsAsync(project);
            _permissionServiceMock.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() });
            var agency = new Agency { Id = Guid.NewGuid(), GdamAgencyId = project.Agency._id, Labels = new[] { "stuff" } };
            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                GdamUserId = project.CreatedBy._id,
                ParentId = Guid.NewGuid()
            };

            var agencies = new List<Agency> { agency }.AsQueryable();
            var costUsers = new List<CostUser> { costUser }.AsQueryable();
            var projectsList = new List<Project>().AsQueryable();

            _efContextMock.MockAsyncQueryable(agencies, c => c.Agency);
            _efContextMock.MockAsyncQueryable(costUsers, c => c.CostUser);
            _efContextMock.MockAsyncQueryable(projectsList, c => c.Project);

            //Act
            await _projectService.AddProjectToDb(project);

            //Assert
            _efContextMock.Verify(a => a.Add(It.IsAny<Project>()), Times.Never);

        }
        /// <summary>
        /// Remove _cm.common from the field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private static string RemoveCmCommon(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            return field.Replace("_cm.common.", string.Empty);
        }

    }
}
