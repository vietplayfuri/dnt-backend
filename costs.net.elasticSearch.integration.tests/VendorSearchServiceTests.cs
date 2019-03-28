namespace costs.net.elasticSearch.integration.tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core;
    using core.Builders.Response;
    using core.Mapping;
    using core.Models.Vendor;
    using core.Services.Search;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;

    [TestFixture]
    public class VendorSearchServiceTests : BaseElasticSearchServiceTests
    {
        private IMapper _mapper;
        private VendorSearchService _vendorSearchService;

        protected override async Task CreateIndexes()
        {
            await ElasticSearchIndexService.CreateIndices(Constants.ElasticSearchIndices.VendorIndexName);
            await PopulatData();
        }

        private async Task PopulatData()
        {
            var vendors = await EFContext.Vendor.ToListAsync();
            var vendorsSearchItems = _mapper.Map<List<VendorSearchItem>>(vendors);
            foreach (var searchItem in vendorsSearchItems)
            {
                await AddToIndex(searchItem, Constants.ElasticSearchIndices.VendorIndexName);
            }
        }

        protected override async Task OneTimeSetup()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<VendorProfile>();
                m.AddProfile<SearchItemProfile>();
            }));

            _vendorSearchService = new VendorSearchService(ElasticClient);
            var currencies = GetCurrencies();
            EFContext.AddRange(currencies);
            EFContext.SaveChanges();

            var vendors = CreateVendors(10);
            EFContext.AddRange(vendors);
            EFContext.SaveChanges();
        }

        private List<Currency> GetCurrencies()
        {
            return new List<Currency>
            {
                new Currency
                {
                    Id = Guid.NewGuid(),
                    Code = "USD",
                    DefaultCurrency = true,
                    Description = "US Dollar",
                    Symbol = "$"
                }
            };
        }

        private List<Vendor> CreateVendors(int i)
        {
            var vendors = new List<Vendor>();
            for (var j = 0; j < i; j++)
            {
                vendors.Add(new Vendor
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    Name = $"Vendor_{j}",
                    Categories = new List<VendorCategory>(),
                    Deleted = false,
                    Version = 1,
                    SapVendor = $"sapCode_{j}",
                    Modified = DateTime.Now
                });
            }

            return vendors;
        }

        private async Task ReindexItem<T1, T2>(T2 entity, string indexName) where T1 : class, ISearchItem where T2 : Entity
        {
            var searchItem = _mapper.Map<T1>(entity);
            await AddToIndex(searchItem, indexName);
        }

        [Test, Order(1)]
        public async Task Get_All_Vendors_Should_return_10()
        {
            const int expectedCount = 10;
            var query = new VendorQuery
            {
                AutoComplete = false,
                PageNumber = 1,
                Limit = 20
            };
            var result = await _vendorSearchService.SearchVendors(query);
            result.Count.Should().Be(expectedCount);
        }

        [Test, Order(2)]
        public async Task Search_vendor_By_Category()
        {
            // Arrange
            await SetupVendorData();
            const int expectedCount = 1;
            const string expectedBudgetRegion = plugins.Constants.BudgetRegion.Japan;
            // Act
            var query = new VendorQuery
            {
                AutoComplete = false,
                PageNumber = 1,
                Limit = 20,
                BudgetRegion = expectedBudgetRegion
            };
            var result = await _vendorSearchService.SearchVendors(query);

            // Assert
            result.Count.Should().Be(expectedCount);
            var expectedVendor = result.Items.First();
            expectedVendor.VendorCategoryModels.Count.Should().Be(1);
            expectedVendor.VendorCategoryModels.First().RuleBudgetRegions.First().Should().Be(expectedBudgetRegion);
        }

        private async Task SetupVendorData()
        {
            var vendor1 = await EFContext.Vendor.FirstOrDefaultAsync(v => v.Name == "Vendor_1");
            var vendor2 = await EFContext.Vendor.FirstOrDefaultAsync(v => v.Name == "Vendor_2");
            var usCurrency = await EFContext.Currency.FirstOrDefaultAsync(a => a.Symbol == "$");
            vendor1.Categories.Add(new VendorCategory
            {
                Name = plugins.Constants.VendorCategory.DistributionTrafficking,
                IsPreferredSupplier = false,
                HasDirectPayment = false,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Currency = usCurrency,
                DefaultCurrencyId = usCurrency.Id,
                VendorCategoryRules = new List<VendorRule>
                {
                    //NORTH AMERICA AREA 
                    new VendorRule
                    {
                        Name = "Rule 1 Trafficking",
                        Rule = new Rule
                        {
                            Name = "1000 Volt_VendorPayment_Rule 1 Trafficking",
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            Criteria =
                                "{\"FieldName\":null,\"Operator\":\"And\",\"TargetValue\":null,\"Children\":[{\"FieldName\":\"BudgetRegion\",\"Operator\":\"Equal\",\"TargetValue\":\"NORTHERN AMERICA AREA\",\"Children\":[]},{\"FieldName\":\"CostType\",\"Operator\":\"Equal\",\"TargetValue\":\"Trafficking\",\"Children\":[]},{\"FieldName\":\"IsAIPE\",\"Operator\":\"Equal\",\"TargetValue\":\"False\",\"Children\":[]},{\"FieldName\":\"TotalCostAmount\",\"Operator\":\"GreaterThanOrEqual\",\"TargetValue\":\"0\",\"Children\":[]}]}",
                            Definition =
                                "{\"DetailedSplit\":false,\"Splits\":[{\"CostTotalName\":\"CostTotal\",\"AIPESplit\":0.0,\"OESplit\":0.0,\"FPSplit\":0.0,\"FASplit\":1.0}]}",
                            Priority = 0,
                            Type = RuleType.VendorPayment
                        }
                    }
                }
            });
            vendor2.Categories.Add(new VendorCategory
            {
                Name = plugins.Constants.VendorCategory.DistributionTrafficking,
                IsPreferredSupplier = false,
                HasDirectPayment = false,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Currency = usCurrency,
                DefaultCurrencyId = usCurrency.Id,
                VendorCategoryRules = new List<VendorRule>
                {
                    //JAPAN
                    new VendorRule
                    {
                        Name = "Rule 1 Trafficking",
                        Rule = new Rule
                        {
                            Name = "1000 Volt_VendorPayment_Rule 1 Trafficking",
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            Criteria =
                                "{\"FieldName\":null,\"Operator\":\"And\",\"TargetValue\":null,\"Children\":[{\"FieldName\":\"BudgetRegion\",\"Operator\":\"Equal\",\"TargetValue\":\"JAPAN\",\"Children\":[]},{\"FieldName\":\"CostType\",\"Operator\":\"Equal\",\"TargetValue\":\"Trafficking\",\"Children\":[]},{\"FieldName\":\"IsAIPE\",\"Operator\":\"Equal\",\"TargetValue\":\"False\",\"Children\":[]},{\"FieldName\":\"TotalCostAmount\",\"Operator\":\"GreaterThanOrEqual\",\"TargetValue\":\"0\",\"Children\":[]}]}",
                            Definition =
                                "{\"DetailedSplit\":false,\"Splits\":[{\"CostTotalName\":\"CostTotal\",\"AIPESplit\":0.0,\"OESplit\":0.0,\"FPSplit\":0.0,\"FASplit\":1.0}]}",
                            Priority = 0,
                            Type = RuleType.VendorPayment
                        }
                    }
                }
            });
            EFContext.Update(vendor1);
            EFContext.Update(vendor2);
            EFContext.SaveChanges();
            await ReindexItem<VendorSearchItem, Vendor>(vendor1, Constants.ElasticSearchIndices.VendorIndexName);
            await ReindexItem<VendorSearchItem, Vendor>(vendor2, Constants.ElasticSearchIndices.VendorIndexName);
        }

        [Test, Order(3)]
        public async Task Search_Vendor_By_Category_And_BudgetRegion()
        {
            // Arrange

            var vendor1 = await EFContext.Vendor.FirstOrDefaultAsync(v => v.Name == "Vendor_1");
            if (!vendor1.Categories.Any())
            {
                await SetupVendorData();
            }
            var usCurrency = await EFContext.Currency.FirstOrDefaultAsync(a => a.Symbol == "$");
            vendor1.Categories.Add(new VendorCategory
            {
                Name = plugins.Constants.VendorCategory.UsageBuyoutContract,
                IsPreferredSupplier = false,
                HasDirectPayment = false,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Currency = usCurrency,
                DefaultCurrencyId = usCurrency.Id,
                VendorCategoryRules = new List<VendorRule>
                {
                    //NORTH AMERICA AREA 
                    new VendorRule
                    {
                        Name = "Rule 1 Usage",
                        Rule = new Rule
                        {
                            Name = "12 Film Ltd._VendorPayment_Rule 1 Usage",
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            Criteria =
                            "{\"FieldName\":null,\"Operator\":\"And\",\"TargetValue\":null,\"Children\":[{\"FieldName\":\"BudgetRegion\",\"Operator\":\"Equal\",\"TargetValue\":\"NORTHERN AMERICA AREA\",\"Children\":[]},{\"FieldName\":\"CostType\",\"Operator\":\"Equal\",\"TargetValue\":\"Buyout\",\"Children\":[]},{\"FieldName\":\"IsAIPE\",\"Operator\":\"Equal\",\"TargetValue\":\"False\",\"Children\":[]},{\"FieldName\":\"TotalCostAmount\",\"Operator\":\"GreaterThanOrEqual\",\"TargetValue\":\"0\",\"Children\":[]}]}",
                            Definition =
                                "{\"DetailedSplit\":false,\"Splits\":[{\"CostTotalName\":\"CostTotal\",\"AIPESplit\":0.0,\"OESplit\":0.0,\"FPSplit\":0.0,\"FASplit\":1.0}]}",
                            Priority = 0,
                            Type = RuleType.VendorPayment
                        }
                    },
                    //GREATER CHINA AREA
                    new VendorRule
                    {
                        Name = "Rule 2 Usage",
                        Rule = new Rule
                        {
                            Name = "12 Film Ltd._VendorPayment_Rule 2 Usage",
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            Criteria =
                                "{\"FieldName\":null,\"Operator\":\"And\",\"TargetValue\":null,\"Children\":[{\"FieldName\":\"BudgetRegion\",\"Operator\":\"Equal\",\"TargetValue\":\"GREATER CHINA AREA\",\"Children\":[]},{\"FieldName\":\"CostType\",\"Operator\":\"Equal\",\"TargetValue\":\"Buyout\",\"Children\":[]},{\"FieldName\":\"IsAIPE\",\"Operator\":\"Equal\",\"TargetValue\":\"False\",\"Children\":[]},{\"FieldName\":\"TotalCostAmount\",\"Operator\":\"GreaterThanOrEqual\",\"TargetValue\":\"0\",\"Children\":[]}]}",
                            Definition =
                                "{\"DetailedSplit\":false,\"Splits\":[{\"CostTotalName\":\"CostTotal\",\"AIPESplit\":0.0,\"OESplit\":0.5,\"FPSplit\":0.0,\"FASplit\":1.0}]}",
                            Priority = 0,
                            Type = RuleType.VendorPayment
                        }
                    }
                }
            });

            EFContext.Update(vendor1);
            EFContext.SaveChanges();
            await ReindexItem<VendorSearchItem, Vendor>(vendor1, Constants.ElasticSearchIndices.VendorIndexName);
            const int expectedCount = 1;
            const string expectedBudgetRegion = plugins.Constants.BudgetRegion.China;
            // Act
            var query = new VendorQuery
            {
                AutoComplete = false,
                PageNumber = 1,
                Limit = 20,
                BudgetRegion = expectedBudgetRegion,
                Category = plugins.Constants.VendorCategory.UsageBuyoutContract
            };
            var result = await _vendorSearchService.SearchVendors(query);

            // Assert
            result.Count.Should().Be(expectedCount);
            var expectedVendor = result.Items.First();
            expectedVendor.VendorCategoryModels.Count.Should().Be(1);
            expectedVendor.VendorCategoryModels.First().RuleBudgetRegions.Should().Contain(expectedBudgetRegion);
        }
    }
}
