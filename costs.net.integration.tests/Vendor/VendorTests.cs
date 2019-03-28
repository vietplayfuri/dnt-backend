namespace costs.net.integration.tests.Vendor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.Rule;
    using core.Models.Vendor;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Rules;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public abstract class VendorBaseTest : BaseIntegrationTest
    {
        private static int _counter;
        private const string BaseUrl = "v1/vendors";
        private Guid _defaultCurrencyId;
        private CostUser _user;

        [SetUp]
        public async Task SetuUp()
        {
            _user = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
            var defaultCurrency = await EFContext.Currency.FirstOrDefaultAsync(c => c.DefaultCurrency);
            _defaultCurrencyId = defaultCurrency.Id;
        }

        public class CreateVendorShould : VendorBaseTest
        {
            [Test]
            public async Task Vendors_CreateNewVendor_withPaymentRules()
            {
                const string vendorName = "Vendor1_CreateNewVendor_withPaymentRules";
                const string productionCategory = "Production";
                const string sapVendorCode = "S_ALR_87012090";

                const string ruleName = "test rule 1";
                const string splitsKey = "splits";
                const string stageSplitsKey = "stageSplits";
                const string costTotalTypeKey = "costTotalType";
                const decimal originalEstimateSplitValue = (decimal)0.20;

                const string criteriaFieldName = nameof(PgPaymentRule.BudgetRegion);
                const string criteriaValue = Constants.BudgetRegion.China;
                var criteriaOperator = ExpressionType.Equal.ToString();

                var vendor = new SaveVendorModel
                {
                    Name = vendorName,
                    SapVendorCode = sapVendorCode,
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            HasDirectPayment = true,
                            IsPreferredSupplier = false,
                            Name = productionCategory,
                            DefaultCurrencyId = _defaultCurrencyId,
                            PaymentRules = new[]
                            {
                                new VendorRuleModel
                                {
                                    Name = ruleName,
                                    Criteria = new Dictionary<string, CriterionValueModel>
                                    {
                                        {
                                            criteriaFieldName,
                                            new CriterionValueModel
                                            {
                                                FieldName = criteriaFieldName,
                                                Value = criteriaValue,
                                                Operator = criteriaOperator
                                            }
                                        }
                                    },
                                    Definition = new Dictionary<string, dynamic>
                                    {
                                        {
                                            splitsKey,
                                            new []
                                            {
                                                new Dictionary<string, dynamic>
                                                {
                                                    {
                                                        costTotalTypeKey,
                                                        Constants.CostSection.CostTotal
                                                    },
                                                    {
                                                        stageSplitsKey,
                                                        new Dictionary<string, dynamic>
                                                        {
                                                            { CostStages.OriginalEstimate.ToString(), originalEstimateSplitValue }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                        }
                    }
                }
                };

                var vendorResult = await CreateVendor(vendor);

                vendorResult.VendorCategoryModels.Count.Should().Be(1);
                vendorResult.VendorCategoryModels.First().PaymentRules.Should().HaveCount(1);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Id.Should().NotBe(Guid.Empty);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Name.Should().Be(ruleName);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Definition.Should().ContainKey(splitsKey);
                ((decimal)vendorResult.VendorCategoryModels.First().PaymentRules[0].Definition[splitsKey][0][stageSplitsKey][CostStages.OriginalEstimate.ToString()])
                    .Should().Be(originalEstimateSplitValue);

                vendorResult.VendorCategoryModels.First().PaymentRules[0].Criteria.Should().ContainKey(criteriaFieldName);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Criteria[criteriaFieldName].FieldName.Should().Be(criteriaFieldName);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Criteria[criteriaFieldName].Value.Should().Be(criteriaValue);
                vendorResult.VendorCategoryModels.First().PaymentRules[0].Criteria[criteriaFieldName].Operator.Should().Be(criteriaOperator);
            }


            [Test]
            public async Task Vendors_CreateNewVendor()
            {
                const string url = "v1/vendors";
                const string vendorName = "Vendor1_CreateNewVendor";
                const string productionCategory = "Production";
                const string sapVendorCode = "S_ALR_87012086";

                var vendor = new SaveVendorModel
                {
                    Name = vendorName,
                    SapVendorCode = sapVendorCode,
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            IsPreferredSupplier = false,
                            HasDirectPayment = true,
                            Name = productionCategory,
                            DefaultCurrencyId = _defaultCurrencyId
                        }
                    }
                };
                var result = await Browser.Put(url, c =>
                {
                    c.User(_user);
                    c.JsonBody(vendor);
                });

                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var vendorResult = Deserialize<VendorModel>(result, HttpStatusCode.OK);

                vendorResult.Name.Should().Be(vendorName);
                vendorResult.SapVendorCode.Should().Be(sapVendorCode);
                vendorResult.VendorCategoryModels.Count.Should().Be(1);
                vendorResult.VendorCategoryModels.First().Name.Should().Be(productionCategory);
                vendorResult.VendorCategoryModels.First().HasDirectPayment.Should().BeTrue();
                vendorResult.VendorCategoryModels.First().IsPreferredSupplier.Should().BeFalse();
                vendorResult.VendorCategoryModels.First().PaymentRules.Should().BeNullOrEmpty();
            }

            [Test]
            public async Task Vendors_CreateNewVendor_WhenCurrencyIsNotProvided()
            {
                var url = "v1/vendors";
                const string vendorName = "Vendor_CreateNewVendor_WhenCurrencyIsNotProvide";
                const string productionCategory = "Production";
                const string sapVendorCode = "S_ALR_87012088";

                var vendor = new SaveVendorModel
                {
                    Name = vendorName,
                    SapVendorCode = sapVendorCode,
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            IsPreferredSupplier = false,
                            HasDirectPayment = true,
                            Name = productionCategory,
                                                    }
                    }
                };
                var result = await Browser.Put(url, c =>
                {
                    c.User(_user);
                    c.JsonBody(vendor);
                });

                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var vendorResult = Deserialize<VendorModel>(result, HttpStatusCode.OK);

                vendorResult.Id.Should().NotBe(Guid.Empty);
                vendorResult.Name.Should().Be(vendorName);

                vendorResult.SapVendorCode.Should().Be(sapVendorCode);

                vendorResult.VendorCategoryModels.Count.Should().Be(1);
                vendorResult.VendorCategoryModels.First().Name.Should().Be(productionCategory);
                vendorResult.VendorCategoryModels.First().HasDirectPayment.Should().BeTrue();
                vendorResult.VendorCategoryModels.First().IsPreferredSupplier.Should().BeFalse();
                vendorResult.VendorCategoryModels.First().DefaultCurrencyId.Should().Be(_defaultCurrencyId, "default currency has to be used if currency is not provided in the request.");
            }

            [Test]
            public async Task Vendors_InsertNewVendor_Correctly()
            {
                var url = "v1/vendors";
                var vendor = await CreateVendor(url);

                url = $"v1/vendors/{vendor.Name}/category/{vendor.VendorCategoryModels.First().Name}";
                var result = await Browser.Get(url, c => { c.User(_user); });
                var vendors = JsonConvert.DeserializeObject<List<VendorModel>>(result.Body.AsString());

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                vendors.Should().NotBeEmpty();
            }
        }

        public class UpdateVendorShould : VendorBaseTest
        {
            [Test]
            public async Task Vendors_UpdateExistingVendor_Correctly()
            {
                var url = "v1/vendors";
                var vendor = await CreateVendor(url);

                var updateVendorModel = new SaveVendorModel
                {
                    Id = vendor.Id,
                    Name = "TestVendor1",
                    SapVendorCode = "VendorCode1",
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            IsPreferredSupplier = false,
                            HasDirectPayment = false,
                            DefaultCurrencyId = _defaultCurrencyId,
                            Name = "ProductionCategory1"
                        }
                    }
                };
                var result = await Browser.Put(url, c =>
                {
                    c.User(_user);
                    c.JsonBody(updateVendorModel);
                });

                result.StatusCode.Should().Be(HttpStatusCode.OK);

                url = $"v1/vendors/{updateVendorModel.Name}/category/{updateVendorModel.VendorCategoryModels.First().Name}";
                result = await Browser.Get(url, c => { c.User(_user); });
                var vendors = JsonConvert.DeserializeObject<List<VendorModel>>(result.Body.AsString());

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                vendors.Should().NotBeEmpty();
            }
        }

        public class UpdateVendorWithPaymentRulesShould : VendorBaseTest
        {
            [Test]
            public void Vendors_update_withPaymentRules_shouldDeleteOldRulesAndCreateNewOnes()
            {
                const string vendorName = "Vendor1_update_withPaymentRules_shouldDeleteOldRulesAndCreateNewOnces";
                const string productionCategory = "Production";
                const string sapVendorCode = "S_ALR_87012089";

                const string ruleName = "test rule 2";
                const string splitsKey = "splits";
                const string stageSplitsKey = "stageSplits";
                const string costTotalTypeKey = "costTotalType";
                const decimal originalEstimateSplitValue = (decimal)0.20;

                const string criteriaFieldName = nameof(PgPaymentRule.BudgetRegion);
                const string criteriaValue = Constants.BudgetRegion.China;
                var criteriaOperator = ExpressionType.Equal.ToString();

                var vendor = new SaveVendorModel
                {
                    Name = vendorName,
                    SapVendorCode = sapVendorCode,
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            HasDirectPayment = true,
                            IsPreferredSupplier = false,
                            Name = productionCategory,
                            DefaultCurrencyId = _defaultCurrencyId,
                            PaymentRules = new[]
                            {
                                new VendorRuleModel
                                {
                                    Name = ruleName,
                                    Criteria = new Dictionary<string, CriterionValueModel>
                                    {
                                        {
                                            criteriaFieldName,
                                            new CriterionValueModel
                                            {
                                                FieldName = criteriaFieldName,
                                                Value = criteriaValue,
                                                Operator = criteriaOperator
                                            }
                                        }
                                    },
                                    Definition = new Dictionary<string, dynamic>
                                    {
                                        {
                                            splitsKey,
                                            new []
                                            {
                                                new Dictionary<string, dynamic>
                                                {
                                                    {
                                                        costTotalTypeKey,
                                                        Constants.CostSection.CostTotal
                                                    },
                                                    {
                                                        stageSplitsKey,
                                                        new Dictionary<string, dynamic>
                                                        {
                                                            { CostStages.OriginalEstimate.ToString(), originalEstimateSplitValue }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                var vendorResult = CreateVendor(vendor).Result;

                const string vendorNameUpdated = "Vendor1_update_withPaymentRules_shouldDeleteOldRulesAndCreateNewOnes_updated";

                const string ruleNameUpdated = "test rule 2_update";
                const decimal splitValueUpdated = 0.3m;

                var updateModel = new SaveVendorModel
                {
                    Id = vendorResult.Id,
                    Name = vendorNameUpdated,
                    SapVendorCode = sapVendorCode,
                    VendorCategoryModels = new[]
                    {
                        new VendorCategoryModel
                        {
                            HasDirectPayment = true,
                            IsPreferredSupplier = false,
                            Name = productionCategory,
                            DefaultCurrencyId = _defaultCurrencyId,
                            PaymentRules = new[]
                            {
                                new VendorRuleModel
                                {
                                    Name = ruleNameUpdated,
                                    Criteria = new Dictionary<string, CriterionValueModel>
                                    {
                                        {
                                            criteriaFieldName,
                                            new CriterionValueModel
                                            {
                                                FieldName = criteriaFieldName,
                                                Value = criteriaValue,
                                                Operator = criteriaOperator
                                            }
                                        }
                                    },
                                    Definition = new Dictionary<string, dynamic>
                                    {
                                        {
                                            splitsKey,
                                            new []
                                            {
                                                new Dictionary<string, dynamic>
                                                {
                                                    {
                                                        costTotalTypeKey,
                                                        Constants.CostSection.CostTotal
                                                    },
                                                    {
                                                        stageSplitsKey,
                                                        new Dictionary<string, dynamic>
                                                        {
                                                            { CostStages.OriginalEstimate.ToString(), splitValueUpdated }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // Act
                var updateVendorResult = UpdateVendor(updateModel).Result;

                // Assert
                updateVendorResult.VendorCategoryModels.First().PaymentRules.Should().HaveCount(1);
                var definition = updateVendorResult.VendorCategoryModels.First().PaymentRules[0].Definition;
                definition.Should().HaveCount(1);
                definition.Should().ContainKey(splitsKey);
                ((decimal)definition[splitsKey][0][stageSplitsKey][CostStages.OriginalEstimate.ToString()]).Should().Be(splitValueUpdated);
            }
        }

        public class DeleteVendorShould : VendorBaseTest
        {
            [Test]
            public async Task Vendors_Delete_Correctly()
            {
                var url = "v1/vendors";
                var vendor = await CreateVendor(url);
                url += "/" + vendor.Id;

                var result = await Browser.Delete(url, c => c.User(_user));

                result.StatusCode.Should().Be(HttpStatusCode.OK);

                url = $"v1/vendors/{vendor.Name}/category/{vendor.VendorCategoryModels.First().Name}";
                result = await Browser.Get(url, c => { c.User(_user); });
                var vendors = JsonConvert.DeserializeObject<List<VendorModel>>(result.Body.AsString());

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var updatedVendor = vendors.FirstOrDefault(v => v.Id == vendor.Id);
                updatedVendor.Should().Be(null);
            }
        }

        private async Task<VendorModel> CreateVendor(string url)
        {
            var updateVendorModel = new SaveVendorModel
            {
                Name = $"TestVendor_{Guid.NewGuid()}",
                SapVendorCode = $"SAP_{Interlocked.Increment(ref _counter)}",
                VendorCategoryModels = new[]
                {
                    new VendorCategoryModel
                    {
                        IsPreferredSupplier = false,
                        HasDirectPayment = false,
                        DefaultCurrencyId = _defaultCurrencyId,
                        Name = "ProductionCategory"
                    }
                }
            };
            var result = await Browser.Put(url, c =>
            {
                c.User(_user);
                c.JsonBody(updateVendorModel);
            });

            var vendorResult = Deserialize<VendorModel>(result, HttpStatusCode.OK);

            return vendorResult;
        }

        private async Task<VendorModel> CreateVendor(SaveVendorModel vendorModel)
        {
            var result = await Browser.Put(BaseUrl, c =>
            {
                c.User(_user);
                c.JsonBody(vendorModel);
            });

            var vendorResult = Deserialize<VendorModel>(result, HttpStatusCode.OK);

            return vendorResult;
        }

        private async Task<VendorModel> UpdateVendor(SaveVendorModel vendorModel)
        {
            var result = await Browser.Put(BaseUrl, c =>
            {
                c.User(_user);
                c.JsonBody(vendorModel);
            });

            var vendorResult = Deserialize<VendorModel>(result, HttpStatusCode.OK);

            return vendorResult;
        }
    }
}
