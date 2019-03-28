namespace costs.net.core.tests.Services.ElasticSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using Builders.Response;
    using Builders.Search;
    using core.Models;
    using core.Models.Utils;
    using core.Services.Search;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Nest;
    using NUnit.Framework;
    using Serilog;

    [TestFixture]
    public class ElasticSearchIndexServiceTest
    {
        private Mock<ICostSearchBuilder> _costSearchBuilderMock;
        private Mock<ILogger> _logger;
        private EFContext _efContext;
        private Mock<IMapper> _mapperMock;
        private Mock<IElasticClient> _elasticClientMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IBulkResponse> _elasticBultResponseMock;
        private ElasticSearchIndexService _elasticSearchIndexService;

        [SetUp]
        public void Init()
        {
            _costSearchBuilderMock = new Mock<ICostSearchBuilder>();
            _costSearchBuilderMock.Setup(b => b.GetIndexDescriptors()).Returns(new[]
            {
                new IndexDescriptor
                {
                    Alias = Constants.ElasticSearchIndices.CostsIndexName
                }
            });
            var searchBuilders = new List<Lazy<ICostSearchBuilder, PluginMetadata>>
            {
                new Lazy<ICostSearchBuilder, PluginMetadata>(() => _costSearchBuilderMock.Object,
                    new PluginMetadata { BuType = BuType.Pg })
            };
            _logger = new Mock<ILogger>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _mapperMock = new Mock<IMapper>();
            _elasticClientMock = new Mock<IElasticClient>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings
            {
                ElasticBatchSize = 1000,
                AdminUser = "4ef31ce1766ec96769b399c0"
            });
            _elasticBultResponseMock = new Mock<IBulkResponse>();

            _elasticSearchIndexService = new ElasticSearchIndexService(
                searchBuilders,
                _logger.Object,
                _efContext,
                _mapperMock.Object,
                _elasticClientMock.Object,
                _appSettingsMock.Object
            );
        }

        [Test]
        public async Task ReIndex_when_no_issues_should_reIndexCosts()
        {
            //Setup
            const int costsCount = 2;

            var costs = new bool[costsCount].Select(i => new Cost
            {
                Created = DateTime.Now,
                Id = Guid.NewGuid()
            }).ToList();
            _mapperMock.Setup(m => m.Map<List<CostSearchItem>>(It.IsAny<List<Cost>>()))
                .Returns(new bool[costsCount]
                    .Select(i => new CostSearchItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Version = 1
                    }).ToList
                );
            var costSearchItems = _mapperMock.Object.Map<List<CostSearchItem>>(costs);

            _efContext.Cost.AddRange(costs);
            _efContext.SaveChanges();

            _costSearchBuilderMock.Setup(a => a.GetCostSearchItems(It.IsAny<List<Guid>>()))
                .ReturnsAsync(costSearchItems);

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            _elasticClientMock.Setup(a => a.IndexExists(It.IsAny<string>(), null).Exists).Returns(true);

            SetupValidAliasResponses(Constants.ElasticSearchIndices.CostsIndexName);

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.CostsIndexName);

            //Assert
            result.Error.Should().BeFalse();
            result.BootstrappedItems.Should().ContainKey(Constants.ElasticSearchIndices.CostsIndexName);
            result.BootstrappedItems[Constants.ElasticSearchIndices.CostsIndexName].Should().Be(costsCount);
        }

        [Test]
        public async Task ReIndex_when_index_is_costs_and_costIdsProvided_should_reIndexByCostIds()
        {
            //Setup
            var costId = Guid.NewGuid();

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.CostsIndexName, new [] { costId });

            //Assert
            result.Success.Should().BeTrue();
            _costSearchBuilderMock.Verify(b => b.GetCostSearchItems(It.Is<List<Guid>>(a => a.Contains(costId))), Times.Once);
        }

        [Test]
        public async Task ReIndex_when_no_issues_should_reIndexCostUsers()
        {
            //Setup
            const int costUsersCount = 3;

            _mapperMock.Setup(m => m.Map<List<CostUserSearchItem>>(It.IsAny<IEnumerable<CostUser>>()))
                .Returns(new bool[costUsersCount]
                    .Select(i => new CostUserSearchItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Version = 1
                    }).ToList
                );
            var costUsers = new bool[costUsersCount].Select(i => new CostUser
            {
                Id = Guid.NewGuid(),
                Agency = new Agency()
            }).ToList();

            _efContext.CostUser.AddRange(costUsers);
            _efContext.SaveChanges();

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            _elasticClientMock.Setup(a => a.IndexExists(It.IsAny<string>(), null).Exists).Returns(true);

            SetupValidAliasResponses(Constants.ElasticSearchIndices.CostUsersIndexName);

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.CostUsersIndexName);

            //Assert
            result.Error.Should().BeFalse();
            result.BootstrappedItems.Should().ContainKey(Constants.ElasticSearchIndices.CostUsersIndexName);
            result.BootstrappedItems[Constants.ElasticSearchIndices.CostUsersIndexName].Should().Be(costUsersCount);
        }

        [Test]
        public async Task ReIndex_when_index_is_cost_users_and_cost_userIdsProvided_should_reIndexByCostUserIds()
        {
            //Setup
            var costUserId = Guid.NewGuid();
            var users = new List<CostUser>
            {
                new CostUser
                {
                    Id = costUserId,
                    FullName = "requested user",
                    Agency = new Agency()
                },
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    FullName = "another user",
                    Agency = new Agency()
                }
            };

            _efContext.CostUser.AddRange(users);
            _efContext.SaveChanges();

            _mapperMock.Setup(m => m.Map<List<CostUserSearchItem>>(It.IsAny<IEnumerable<CostUser>>()))
                .Returns(new List<CostUserSearchItem>());

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.CostUsersIndexName, new[] { costUserId });

            //Assert
            result.Success.Should().BeTrue();
            _mapperMock.Verify(m => m.Map<List<CostUserSearchItem>>(It.Is<CostUser[]>(ul => ul.Length == 1 && ul.Any(u => u.Id == costUserId))));
        }

        [Test]
        public async Task ReIndex_when_no_issues_should_reIndexProjects()
        {
            //Setup
            const int projectsCount = 4;

            var projects = new bool[projectsCount].Select(i => new Project
            {
                Id = Guid.NewGuid()
            }).ToList();
            _mapperMock.Setup(m => m.Map<List<ProjectSearchItem>>(It.IsAny<IEnumerable<Project>>()))
                .Returns(new bool[projectsCount]
                    .Select(i => new ProjectSearchItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Version = 1
                    }).ToList
                );
            _efContext.Project.AddRange(projects);
            _efContext.SaveChanges();

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            _elasticClientMock.Setup(a => a.IndexExists(It.IsAny<string>(), null).Exists).Returns(true);

            SetupValidAliasResponses(Constants.ElasticSearchIndices.ProjectsIndexName);

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.ProjectsIndexName);

            //Assert
            result.Error.Should().BeFalse();
            result.BootstrappedItems.Should().ContainKey(Constants.ElasticSearchIndices.ProjectsIndexName);
            result.BootstrappedItems[Constants.ElasticSearchIndices.ProjectsIndexName].Should().Be(projectsCount);
        }

        [Test]
        public async Task ReIndex_when_index_is_projects_and_projectIdsProvided_should_reIndexByProjectIds()
        {
            //Setup
            var projectId = Guid.NewGuid();
            var projects = new List<Project>
            {
                new Project
                {
                    Id = projectId
                },
                new Project
                {
                    Id = Guid.NewGuid()
                }
            };

            _efContext.Project.AddRange(projects);
            _efContext.SaveChanges();

            _mapperMock.Setup(m => m.Map<List<ProjectSearchItem>>(It.IsAny<IEnumerable<Project>>()))
                .Returns(new List<ProjectSearchItem>());

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.ProjectsIndexName, new[] { projectId });

            //Assert
            result.Success.Should().BeTrue();
            _mapperMock.Verify(m => m.Map<List<ProjectSearchItem>>(It.Is<Project[]>(ul => ul.Length == 1 && ul.Any(u => u.Id == projectId))));
        }

        [Test]
        public async Task ReIndex_when_no_issues_should_reIndexAgencies()
        {
            //Setup
            const int agenciesCount = 5;

            var abstractTypes = new bool[agenciesCount].Select(i => new AbstractType
            {
                Id = Guid.NewGuid(),
                Agency = new Agency
                {
                    Id = Guid.NewGuid()
                }

            }).ToList();

            _mapperMock.Setup(m => m.Map<List<AgencySearchItem>>(It.IsAny<IEnumerable<AbstractType>>()))
                .Returns(new bool[agenciesCount]
                    .Select(i => new AgencySearchItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Version = 1
                    }).ToList
                );

            _efContext.AbstractType.AddRange(abstractTypes);
            _efContext.SaveChanges();

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            _elasticClientMock.Setup(a => a.IndexExists(It.IsAny<string>(), null).Exists).Returns(true);

            SetupValidAliasResponses(Constants.ElasticSearchIndices.AgencyIndexName);

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.AgencyIndexName);

            //Assert
            result.Error.Should().BeFalse();
            result.BootstrappedItems.Should().ContainKey(Constants.ElasticSearchIndices.AgencyIndexName);
            result.BootstrappedItems[Constants.ElasticSearchIndices.AgencyIndexName].Should().Be(agenciesCount);
        }

        [Test]
        public async Task ReIndex_when_index_is_agencies_and_agencyIdsProvided_should_reIndexByAgencyIds()
        {
            //Setup
            var agencyId = Guid.NewGuid();
            var agencies = new List<AbstractType>
            {
                new AbstractType
                {
                    Id = agencyId,
                    Agency = new Agency()
                },
                new AbstractType
                {
                    Id = Guid.NewGuid(),
                    Agency = new Agency()
                }
            };

            _efContext.AbstractType.AddRange(agencies);
            _efContext.SaveChanges();

            _mapperMock.Setup(m => m.Map<List<AgencySearchItem>>(It.IsAny<IEnumerable<AbstractType>>()))
                .Returns(new List<AgencySearchItem>());

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.AgencyIndexName, new[] { agencyId });

            //Assert
            result.Success.Should().BeTrue();
            _mapperMock.Verify(m => m.Map<List<AgencySearchItem>>(It.Is<AbstractType[]>(ul => ul.Length == 1 && ul.Any(u => u.Id == agencyId))));
        }

        [Test]
        public async Task ReIndex_when_no_issues_should_reIndexDictionaries()
        {
            //Setup
            const int dictionaryEntriesCount = 6;

            var dictionaryEntries = new bool[dictionaryEntriesCount].Select((i, j) => new DictionaryEntry
            {
                Id = Guid.NewGuid(),
                DictionaryId = Guid.NewGuid(),
                Key = $"{j}.Key",
                Value = $"{j}.Value",
                Visible = true
            }).ToList();
            _mapperMock.Setup(m => m.Map<List<DictionaryEntrySearchItem>>(It.IsAny<IEnumerable<DictionaryEntry>>()))
                .Returns(new bool[dictionaryEntriesCount]
                    .Select(i => new DictionaryEntrySearchItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Version = 1
                    }).ToList
                );
            _efContext.DictionaryEntry.AddRange(dictionaryEntries);
            _efContext.SaveChanges();

            _elasticBultResponseMock.Setup(r => r.IsValid).Returns(true);
            _elasticClientMock.Setup(a => a.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>(), CancellationToken.None))
                .ReturnsAsync(_elasticBultResponseMock.Object);

            _elasticClientMock.Setup(a => a.IndexExists(It.IsAny<string>(), null).Exists).Returns(true);

            SetupValidAliasResponses(Constants.ElasticSearchIndices.DictionaryEntriesIndexName);

            // Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.DictionaryEntriesIndexName);

            //Assert
            result.Error.Should().BeFalse();
            result.BootstrappedItems.Should().ContainKey(Constants.ElasticSearchIndices.DictionaryEntriesIndexName);
            result.BootstrappedItems[Constants.ElasticSearchIndices.DictionaryEntriesIndexName].Should().Be(dictionaryEntriesCount);
        }

        [Test]
        public async Task ReIndex_when_index_is_dictionary_entries_and_dictionaryEntryIdsProvided_should_reIndexByDictionaryEntryIds()
        {
            //Setup
            var dictionaryEntryId = Guid.NewGuid();
            var dictionaryEntries = new List<DictionaryEntry>
            {
                new DictionaryEntry
                {
                    Id = dictionaryEntryId,
                    Dictionary = new Dictionary()
                },
                new DictionaryEntry
                {
                    Id = Guid.NewGuid(),
                    Dictionary = new Dictionary()
                }
            };

            _efContext.DictionaryEntry.AddRange(dictionaryEntries);
            _efContext.SaveChanges();

            _mapperMock.Setup(m => m.Map<List<DictionaryEntrySearchItem>>(It.IsAny<IEnumerable<DictionaryEntry>>()))
                .Returns(new List<DictionaryEntrySearchItem>());

            //Act
            var result = await _elasticSearchIndexService.ReIndex(Constants.ElasticSearchIndices.DictionaryEntriesIndexName, new[] { dictionaryEntryId });

            //Assert
            result.Success.Should().BeTrue();
            _mapperMock.Verify(m => m.Map<List<DictionaryEntrySearchItem>>(It.Is<DictionaryEntry[]>(ul => ul.Length == 1 && ul.Any(u => u.Id == dictionaryEntryId))));
        }

        private void SetupValidAliasResponses(string indexName)
        {
            var getAliasResponseMock = new Mock<IGetAliasResponse>();
            var existingIndex = $"{indexName}_0";
            var newIndex = $"{indexName}_1";
            getAliasResponseMock.Setup(r => r.IsValid).Returns(true);
            getAliasResponseMock.Setup(r => r.Indices).Returns(new Dictionary<string, IReadOnlyList<AliasDefinition>>
            {
                {
                    existingIndex,
                    new List<AliasDefinition>()
                }
            });
            _elasticClientMock.Setup(e => e.GetAlias(It.IsAny<Func<GetAliasDescriptor, IGetAliasRequest>>()))
                .Returns(getAliasResponseMock.Object);

            var indexExistsMock = new Mock<IExistsResponse>();
            indexExistsMock.Setup(r => r.Exists).Returns(false);
            _elasticClientMock.Setup(e => e.IndexExists(newIndex, null)).Returns(indexExistsMock.Object);

            var createIndexMock = new Mock<ICreateIndexResponse>();
            createIndexMock.Setup(c => c.IsValid).Returns(true);
            _elasticClientMock.Setup(e => e.CreateIndex(newIndex, It.IsAny<Func<CreateIndexDescriptor, ICreateIndexRequest>>()))
                .Returns(createIndexMock.Object);

            var bulkAliasResponseMock = new Mock<IBulkAliasResponse>();
            bulkAliasResponseMock.Setup(a => a.IsValid).Returns(true);
            _elasticClientMock.Setup(e => e.Alias(It.IsAny<Func<BulkAliasDescriptor, IBulkAliasRequest>>()))
                .Returns(bulkAliasResponseMock.Object);

            var deleteIndexResponseMock = new Mock<IDeleteIndexResponse>();
            deleteIndexResponseMock.Setup(e => e.IsValid).Returns(true);
            _elasticClientMock.Setup(e => e
                    .DeleteIndexAsync(existingIndex, It.IsAny<Func<DeleteIndexDescriptor, IDeleteIndexRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteIndexResponseMock.Object);
        }
    }
}
