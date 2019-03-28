namespace costs.net.core.tests.Services.Dictionary
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Services.Dictionary;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Dictionary = dataAccess.Entity.Dictionary;

    [TestFixture]
    public class DictionaryServiceTests
    {
        private EFContext _efContext;
        private Mock<EFContext> _efContextMock;
        private readonly Mock<IEventService> _eventServiceMock = new Mock<IEventService>();
        private Mock<IMapper> _mapperMock;
        private DictionaryService _dictionaryService;
        private DictionaryService _dictionaryServiceMock;

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _efContextMock = new Mock<EFContext>();
            _mapperMock = new Mock<IMapper>();
            _dictionaryService = new DictionaryService(_efContext, _eventServiceMock.Object, _mapperMock.Object);
            _dictionaryServiceMock = new DictionaryService(_efContextMock.Object, _eventServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task QueryEntries_When_MaxEntries_ShouldLimitResult()
        {
            // Arrange
            var dictionaryId = Guid.NewGuid();
            var dictionaryEntryCount = 2;
            var maxEntries = 1;
            var entries = new bool[dictionaryEntryCount].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = true
            });
            _efContext.DictionaryEntry.AddRange(entries);
            _efContext.SaveChanges();

            // Act
            var result = await _dictionaryService.QueryEntries(dictionaryId, null, maxEntries);

            // Assert
            result.Should().HaveCount(maxEntries);
        }
        
        [Test]
        public async Task QueryEntries_always_ShouldReturnOnlyVisibleEntries()
        {
            // Arrange
            var dictionaryId = Guid.NewGuid();
            var visibleCount = 1;
            var invisibleCount = 2;
            _efContext.DictionaryEntry.AddRange(new bool[visibleCount].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = true
            }));
            _efContext.DictionaryEntry.AddRange(new bool[invisibleCount].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = false
            }));

            _efContext.SaveChanges();

            // Act
            var result = await _dictionaryService.QueryEntries(dictionaryId);

            // Assert
            result.Should().HaveCount(visibleCount);
        }

        [Test]
        public async Task QueryEntries_always_ShouldFilterEntriesBySpecifiedDictionaryId()
        {
            // Arrange
            var dictionaryId1 = Guid.NewGuid();
            var dictionaryId2 = Guid.NewGuid();
            var dictionary1Count = 1;
            var dictionary2Count = 2;
            _efContext.DictionaryEntry.AddRange(new bool[dictionary1Count].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId1,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = true
            }));
            _efContext.DictionaryEntry.AddRange(new bool[dictionary2Count].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId2,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = true
            }));

            _efContext.SaveChanges();

            // Act
            var result = await _dictionaryService.QueryEntries(dictionaryId1);

            // Assert
            result.Should().HaveCount(dictionary1Count);
        }

        [Test]
        public async Task QueryEntries_whenSearchTextProvided_ShouldFilterEntriesBySearchTextInValue()
        {
            // Arrange
            var dictionaryId = Guid.NewGuid();
            var count = 1;
            var searchText = "match";
            _efContext.DictionaryEntry.AddRange(new bool[count].Select((i, j) => new DictionaryEntry
            {
                DictionaryId = dictionaryId,
                Key = j.ToString(),
                Value = j.ToString(),
                Visible = true
            }));
            _efContext.DictionaryEntry.Add(new DictionaryEntry
            {
                DictionaryId = dictionaryId,
                Key = "any key",
                Value = "should be matched",
                Visible = true
            });

            _efContext.SaveChanges();

            // Act
            var result = await _dictionaryService.QueryEntries(dictionaryId, searchText);

            // Assert
            result.Should().HaveCount(1);
        }

        [Test]
        public async Task SearchDictionaries_filterByKey()
        {
            // Arrange
            var dictionaryId = Guid.NewGuid();
            var abstractTypeId = Guid.NewGuid();
            var dictionaryEntryIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
            var dictionaryName = "TestingDictionary_qwerty_abcd";
            var nameToSearch = "qwerty";
            var parentKey = "ParentElement";
            //resulting dictionary
            var entries = new DictionaryEntry[]
            {
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[0],
                    DictionaryId = dictionaryId,
                    Key = "element0",
                    Value = "element0",
                    Visible = true,
                },
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[1],
                    DictionaryId = dictionaryId,
                    Key = "element1",
                    Value = "element1",
                    Visible = true,
                },
                new DictionaryEntry()//not a match
                {
                    Id = Guid.NewGuid(),
                    DictionaryId = dictionaryId,
                    Key = "element2",
                    Value = "element2",
                    Visible = true,
                },
            };
            //parent dictionary entry
            _efContextMock.MockAsyncQueryable(new[] {
                new Dictionary()
                {
                    Id = dictionaryId,
                    AbstractTypeId = abstractTypeId,
                    Name = dictionaryName,
                    DictionaryEntries = entries.ToList(),
                } }.AsQueryable(), context=> context.Dictionary);
            _efContextMock.MockAsyncQueryable(new[]
            {
                entries[0],
                entries[1],
                entries[2],
                new DictionaryEntry()
                {
                    Id = dictionaryEntryIds[2],
                    Key = parentKey,
                    Value = parentKey,
                } }.AsQueryable(), context => context.DictionaryEntry);
            //relations
            _efContextMock.MockAsyncQueryable(new DependentItem[]
            {
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[0],
                    ParentId = dictionaryEntryIds[2],
                },
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[1],
                    ParentId = dictionaryEntryIds[2],
                },
            }.AsQueryable(), context => context.DependentItem);

            // Act
            var result = await _dictionaryServiceMock.SearchDictionaries(abstractTypeId, nameToSearch, parentKey);

            // Assert
            Assert.Multiple(() =>
            {
                result.Should().HaveCount(1);
                result[0].DictionaryEntries.Should().HaveCount(2);
            });
        }
        
        [Test]
        public async Task GetDictionariesByNames_filterbyKey_singleDictionary()
        {
            // Arrange
            var dictionaryIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var abstractTypeId = Guid.NewGuid();
            var dictionaryEntryIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };//entries0[0], entries0[1], entries0 parent, 
            var dictionaryNames = new[] { "TestingDictionary1" };
            var parentKey = "ParentElement";
            //resulting dictionary
            var entries0 = new DictionaryEntry[]
            {
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[0],
                    DictionaryId = dictionaryIds[0],
                    Key = "element0",
                    Value = "element0",
                    Visible = true,
                },
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[1],
                    DictionaryId = dictionaryIds[0],
                    Key = "element1",
                    Value = "element1",
                    Visible = true,
                },
                new DictionaryEntry()//not a match
                {
                    Id = Guid.NewGuid(),
                    DictionaryId = dictionaryIds[0],
                    Key = "element2",
                    Value = "element2",
                    Visible = true,
                },
            };
            //parent dictionary entry
            _efContextMock.MockAsyncQueryable(new[] {
                new Dictionary()
                {
                    Id = dictionaryIds[0],
                    AbstractTypeId = abstractTypeId,
                    Name = dictionaryNames[0],
                    DictionaryEntries = entries0.ToList(),
                },
            }.AsQueryable(), context => context.Dictionary);
            _efContextMock.MockAsyncQueryable(new[]
            {
                entries0[0],
                entries0[1],
                entries0[2],
                new DictionaryEntry()
                {
                    Id = dictionaryEntryIds[2],
                    Key = parentKey,
                    Value = parentKey,
                }
            }.AsQueryable(), context => context.DictionaryEntry);
            //relations
            _efContextMock.MockAsyncQueryable(new DependentItem[]
            {
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[0],
                    ParentId = dictionaryEntryIds[2],
                },
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[1],
                    ParentId = dictionaryEntryIds[2],
                },
            }.AsQueryable(), context => context.DependentItem);

            // Act
            var result = await _dictionaryServiceMock.GetDictionariesByNames(abstractTypeId, dictionaryNames, parentKey, true);

            // Assert
            Assert.Multiple(() =>
            {
                result.Should().HaveCount(1);
                result[0].DictionaryEntries.Should().HaveCount(2);
            });
        }

        [Test]
        public async Task GetDictionariesByNames_filterbyKey_multipleDictionaries()
        {
            // Arrange
            var dictionaryIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var abstractTypeId = Guid.NewGuid();
            var dictionaryEntryIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };//entries0[0], entries0[1], entries0 parent, //entries1[0], entries1[1], entries1 parent, 
            var dictionaryNames = new[] { "TestingDictionary1", "TestingDictionary2" };
            var parentKey = "ParentElement";
            //resulting dictionary
            var entries0 = new DictionaryEntry[]
            {
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[0],
                    DictionaryId = dictionaryIds[0],
                    Key = "element0",
                    Value = "element0",
                    Visible = true,
                },
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[1],
                    DictionaryId = dictionaryIds[0],
                    Key = "element1",
                    Value = "element1",
                    Visible = true,
                },
                new DictionaryEntry()//not a match
                {
                    Id = Guid.NewGuid(),
                    DictionaryId = dictionaryIds[0],
                    Key = "element2",
                    Value = "element2",
                    Visible = true,
                },
            };
            var entries1 = new DictionaryEntry[]
            {
                new DictionaryEntry()//match
                {
                    Id = dictionaryEntryIds[3],
                    DictionaryId = dictionaryIds[1],
                    Key = "element0",
                    Value = "element0",
                    Visible = true,
                },
                new DictionaryEntry()//not a match
                {
                    Id = dictionaryEntryIds[4],
                    DictionaryId = dictionaryIds[1],
                    Key = "element1",
                    Value = "element1",
                    Visible = true,
                },
                new DictionaryEntry()//not a match
                {
                    Id = Guid.NewGuid(),
                    DictionaryId = dictionaryIds[1],
                    Key = "element2",
                    Value = "element2",
                    Visible = true,
                },
            };
            //parent dictionary entry
            _efContextMock.MockAsyncQueryable(new[] {
                new Dictionary()
                {
                    Id = dictionaryIds[0],
                    AbstractTypeId = abstractTypeId,
                    Name = dictionaryNames[0],
                    DictionaryEntries = entries0.ToList(),
                },
                new Dictionary()
                {
                    Id = dictionaryIds[1],
                    AbstractTypeId = abstractTypeId,
                    Name = dictionaryNames[1],
                    DictionaryEntries = entries1.ToList(),
                },
            }.AsQueryable(), context => context.Dictionary);
            _efContextMock.MockAsyncQueryable(new[]
            {
                entries0[0],
                entries0[1],
                entries0[2],
                entries1[0],
                entries1[1],
                entries1[2],
                new DictionaryEntry()
                {
                    Id = dictionaryEntryIds[2],
                    Key = parentKey,
                    Value = parentKey,
                },
                new DictionaryEntry()
                {
                    Id = dictionaryEntryIds[5],
                    Key = parentKey,
                    Value = parentKey,
                },
            }.AsQueryable(), context => context.DictionaryEntry);
            //relations
            _efContextMock.MockAsyncQueryable(new DependentItem[]
            {
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[0],
                    ParentId = dictionaryEntryIds[2],
                },
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[1],
                    ParentId = dictionaryEntryIds[2],
                },
                new DependentItem()
                {
                    ChildId = dictionaryEntryIds[4],
                    ParentId = dictionaryEntryIds[5],
                },
            }.AsQueryable(), context => context.DependentItem);

            // Act
            var result = await _dictionaryServiceMock.GetDictionariesByNames(abstractTypeId, dictionaryNames, parentKey, true);

            // Assert
            Assert.Multiple(() =>
            {
                result.Should().HaveCount(2);
                result[0].DictionaryEntries.Should().HaveCount(2);
                result[1].DictionaryEntries.Should().HaveCount(1);
            });
        }
    }
}
