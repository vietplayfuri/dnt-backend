namespace costs.net.integration.tests.Dictionary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    public class DictionaryTests
    {
        public abstract class DictionaryTest : BaseIntegrationTest
        {
            private CostUser _user;
            private readonly Random _random = new Random();

            [SetUp]
            public async Task Init()
            {
                _user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
            }

            protected Dictionary CreateDictionaryEntity()
            {
                return new Dictionary
                {
                    Name = "test_" + Guid.NewGuid().ToString().Substring(0, 10),
                    AbstractTypeId = EFContext.AbstractType.FirstOrDefault(a => a.Type == core.Constants.AccessObjectType.Module && a.ParentId != a.Id).Id,
                    IsNameEditable = true
                };
            }

            protected DictionaryEntry CreateDictionaryEntry(Guid dictionaryId)
            {
                return new DictionaryEntry
                {
                    DictionaryId = dictionaryId,
                    Key = $"key{_random.Next(1000)}",
                    Visible = true
                };
            }

            [TestFixture]
            public class CreateDictionaryShould : DictionaryTest
            {
                [Test]
                public async Task CreateDictionary()
                {
                    var url = "v1/dictionary";
                    var dictionary = CreateDictionaryEntity();

                    dictionary = Deserialize<Dictionary>(await Browser.Post(url, c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionary);
                    }), HttpStatusCode.OK);

                    var dictionaries = Deserialize<IEnumerable<Dictionary>>(await Browser.Get(url, c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);

                    dictionaries.Where(d => d.Id == dictionary.Id).Should().NotBeEmpty();
                }
            }

            [TestFixture]
            public class CreateDictionaryEntryShould : DictionaryTest
            {
                [Test]
                public async Task CreateDictionaryEntry()
                {
                    var dictionary = CreateDictionaryEntity();

                    var url = "v1/dictionary";

                    var createdDictionary = Deserialize<Dictionary>(await Browser.Post(url, c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionary);
                    }), HttpStatusCode.OK);

                    createdDictionary.Id.Should().NotBe(Guid.Empty);

                    var dictionaryEntry = CreateDictionaryEntry(createdDictionary.Id);
                    var dictionaryEntryResponse = await Browser.Post($"{url}/{dictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionaryEntry);
                    });

                    dictionaryEntryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var dictionaryEntries = Deserialize<IEnumerable<Dictionary>>(await Browser.Get($"{url}/{createdDictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);

                    dictionaryEntries.Should().HaveCount(1);
                }
            }

            [TestFixture]
            public class UpdateDictionaryEntryShould : DictionaryTest
            {
                [Test]
                public async Task UpdateDictionaryEntry()
                {
                    var dictionary = CreateDictionaryEntity();

                    var url = "v1/dictionary";

                    var createdDictionary = Deserialize<Dictionary>(await Browser.Post(url, c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionary);
                    }), HttpStatusCode.OK);

                    createdDictionary.Id.Should().NotBe(Guid.Empty);

                    var dictionaryEntry = CreateDictionaryEntry(createdDictionary.Id);
                    var dictionaryEntryResponse = await Browser.Post($"{url}/{dictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionaryEntry);
                    });

                    dictionaryEntryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var dictionaryEntries = Deserialize<IEnumerable<DictionaryEntry>>(await Browser.Get($"{url}/{createdDictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);

                    var createdDictionaryEntry = dictionaryEntries.First();
                    createdDictionaryEntry.Key = "testKey";
                    await Browser.Put($"{url}/{createdDictionary.Id}/dictionaryentries/{createdDictionaryEntry.Id}", c =>
                    {
                        c.User(_user);
                        c.JsonBody(createdDictionaryEntry);
                    });

                    dictionaryEntries = Deserialize<IEnumerable<DictionaryEntry>>(await Browser.Get($"{url}/{createdDictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);
                    dictionaryEntries.First().Should().NotBe(dictionaryEntry.Key);
                }
            }

            [TestFixture]
            public class DeleteDictionaryEntryShould : DictionaryTest
            {
                [Test]
                public async Task DeleteDictionaryEntry()
                {
                    var dictionary = CreateDictionaryEntity();

                    var url = "v1/dictionary";

                    var createdDictionary = Deserialize<Dictionary>(await Browser.Post(url, c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionary);
                    }), HttpStatusCode.OK);

                    createdDictionary.Id.Should().NotBe(Guid.Empty);

                    dictionary = createdDictionary;

                    var dictionaryEntry = CreateDictionaryEntry(createdDictionary.Id);
                    var dictionaryEntryResponse = await Browser.Post($"{url}/{dictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                        c.JsonBody(dictionaryEntry);
                    });

                    dictionaryEntryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var dictionaryEntries = Deserialize<IEnumerable<DictionaryEntry>>(await Browser.Get($"{url}/{dictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);

                    var createdDictionaryEntry = dictionaryEntries.First();
                    await Browser.Delete($"{url}/{dictionary.Id}/dictionaryentries/{createdDictionaryEntry.Id}", c =>
                    {
                        c.User(_user);
                        c.JsonBody(createdDictionaryEntry);
                    });

                    dictionaryEntries = Deserialize<IEnumerable<DictionaryEntry>>(await Browser.Get($"{url}/{dictionary.Id}/dictionaryentries", c =>
                    {
                        c.User(_user);
                    }), HttpStatusCode.OK);
                    dictionaryEntries.Any(d => d.Key != dictionaryEntry.Key).Should().BeFalse();
                }
            }
        }
    }
}