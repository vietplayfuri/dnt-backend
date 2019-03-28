namespace costs.net.core.tests.Services.Vendors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using Builders.Rules;
    using core.Events.Vendor;
    using core.Services.Currencies;
    using core.Services.Vendors;
    using dataAccess;
    using dataAccess.Entity;
    using core.Models;
    using core.Models.Rule;
    using core.Models.User;
    using core.Models.Vendor;
    using core.Services.Events;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Rules;
    using Castle.Core.Logging;
    using costs.net.core.Models.Response;

    [TestFixture]
    public class VendorServiceTests
    {
        private VendorsService _service;
        private EFContext _efContext;
        private Mock<ICurrencyService> _currencyServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IVendorRuleBuilder> _vendorRuleBuilderMock;
        private const string RuleName = "test rule 1";
        private const string CategoryName = "Category1";
        private const string VendorName = "VendorName_UnitTest";

        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _mapperMock = new Mock<IMapper>();
            _eventServiceMock = new Mock<IEventService>();
            _vendorRuleBuilderMock = new Mock<IVendorRuleBuilder>();
            var vendorRuleBuilders = new List<Lazy<IVendorRuleBuilder, PluginMetadata>>
            {
                new Lazy<IVendorRuleBuilder, PluginMetadata>(() =>
                    _vendorRuleBuilderMock.Object, new PluginMetadata
                    {
                        BuType =  BuType.Pg
                    })
            };
            _service = new VendorsService(_currencyServiceMock.Object, _mapperMock.Object, _efContext, vendorRuleBuilders, _eventServiceMock.Object);
        }

        [Test]
        public async Task Upsert_ifAnyPaymentRules_shouldPersistPaymentRules()
        {
            // Arrange            
            var request = new SaveVendorModel
            {
                VendorCategoryModels = new[]
                {
                    new VendorCategoryModel
                    {
                        PaymentRules = new[]
                        {
                            new VendorRuleModel
                            {
                                Name = RuleName,
                                Criteria = new Dictionary<string, CriterionValueModel>
                                {
                                    {
                                        nameof(PgPaymentRule.BudgetRegion),
                                        new CriterionValueModel
                                        {
                                            FieldName = nameof(PgPaymentRule.BudgetRegion),
                                            Value = Constants.BudgetRegion.China,
                                            Operator = ExpressionType.Equal.ToString()
                                        }
                                    }
                                }
                            }
                        },
                        Name = CategoryName,
                        HasDirectPayment = false,
                        IsPreferredSupplier = false
                    }
                }
            };
            var dbVendor = new Vendor();

            _currencyServiceMock.Setup(c => c.GetDefaultCurrency()).ReturnsAsync(new Currency());

            _vendorRuleBuilderMock.Setup(b =>
                b.ValidateAndGetVendorRules(It.IsAny<VendorRuleModel[]>(), It.IsAny<Vendor>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<VendorRule> { new VendorRule { VendorCategory = new VendorCategory
                {
                    Name = CategoryName,
                    HasDirectPayment = false,
                    IsPreferredSupplier = false,
                    Vendor = dbVendor
                }, Rule = new dataAccess.Entity.Rule() } });

            _mapperMock.Setup(m => m.Map<Vendor>(It.IsAny<SaveVendorModel>())).Returns(dbVendor);
            _mapperMock.Setup(m => m.Map(It.IsAny<Currency>(), It.IsAny<Vendor>())).Returns(new Vendor());
            _mapperMock.Setup(m => m.Map<VendorModel>(It.IsAny<Vendor>())).Returns(new VendorModel());
            _mapperMock.Setup(m => m.Map<VendorCategory>(It.IsAny<VendorCategoryModel>())).Returns(
                new VendorCategory
                {
                    Name = CategoryName,
                    IsPreferredSupplier = false,
                    HasDirectPayment = false
                }
            );

            // Act
            await _service.Upsert(request, new UserIdentity { BuType = BuType.Pg });

            // Assert
            _efContext.Vendor.Should().HaveCount(1);
            var vendor = _efContext.Vendor.First();
            //
            vendor.Categories.Should().HaveCount(1);
            vendor.Categories.First().VendorCategoryRules.Should().HaveCount(1);
            var rule = vendor.Categories.First().VendorCategoryRules.First().Rule;

            rule.Should().NotBeNull();
            _eventServiceMock.Verify(a => a.SendAsync(It.IsAny<VendorUpserted>()), Times.Once);
        }

        [Test]
        public void Query_WhenQueryIsNull_ShouldThrowException()
        {
            // Arrange

            // Act, Assert
            _service.Awaiting(s => s.Query(null)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void Query_WhenPageSizeIsLessThenOne_ShouldThrowException(int pageSize)
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageNumber = 1, PageSize = pageSize };

            // Act, Assert
            _service.Awaiting(s => s.Query(vendorQuery)).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void Query_WhenPageNumberIsLessThenOne_ShouldThrowException(int pageNumber)
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageNumber = pageNumber, PageSize = 1 };

            // Act, Assert
            _service.Awaiting(s => s.Query(vendorQuery)).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public async Task Query_WhenSearchTextIsNotEmpty_ShouldFilterBySearchText()
        {
            // Arrange
            const string searchText = "vend";
            var vendorQuery = new VendorQuery { SearchText = searchText, PageSize = 10, PageNumber = 1 };
            _efContext.Vendor.Add(new Vendor
            {
                Name = "vendor 1"
            });
            _efContext.Vendor.Add(new Vendor
            {
                Name = "vndor 1"
            });
            _efContext.SaveChanges();

            // Act
            var result = await _service.Query(vendorQuery);

            // Assert
            result.Vendors.Should().HaveCount(1);
        }

        [Test]
        public async Task Query_WhenSearchTextIsEmpty_ShouldNotFilterBySearchText()
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageSize = 10, PageNumber = 1 };
            _efContext.Vendor.Add(new Vendor
            {
                Name = "vendor 1"
            });
            _efContext.Vendor.Add(new Vendor
            {
                Name = "vndor 1"
            });
            _efContext.SaveChanges();

            // Act
            var result = await _service.Query(vendorQuery);

            // Assert
            result.Vendors.Should().HaveCount(2);
        }

        [Test]
        public async Task Query_WhenInutIsValid_ShouldOrderVendorsByName()
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageSize = 10, PageNumber = 1 };
            const string name1 = "v1";
            const string name2 = "v2";
            _efContext.Vendor.Add(new Vendor
            {
                Name = name2
            });
            _efContext.Vendor.Add(new Vendor
            {
                Name = name1
            });
            _efContext.SaveChanges();

            foreach (var vendor in _efContext.Vendor)
            {
                _mapperMock.Setup(m => m.Map<VendorModel>(vendor)).Returns(new VendorModel
                {
                    Name = vendor.Name
                });
            }

            // Act
            var result = await _service.Query(vendorQuery);

            // Assert
            result.Vendors.Should().HaveCount(2);
            result.Vendors[0].Name.Should().Be(name1);
            result.Vendors[1].Name.Should().Be(name2);
        }

        [Test]
        public async Task Query_WhenInutIsValid_ShouldTakeAmountOfVendorsEqualToPageSize()
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageSize = 1, PageNumber = 1 };
            const string name1 = "v1";
            const string name2 = "v2";
            _efContext.Vendor.Add(new Vendor
            {
                Name = name2
            });
            _efContext.Vendor.Add(new Vendor
            {
                Name = name1
            });
            _efContext.SaveChanges();

            foreach (var vendor in _efContext.Vendor)
            {
                _mapperMock.Setup(m => m.Map<VendorModel>(vendor)).Returns(new VendorModel
                {
                    Name = vendor.Name
                });
            }

            // Act
            var result = await _service.Query(vendorQuery);

            // Assert
            result.Vendors.Should().HaveCount(1);
            result.Vendors[0].Name.Should().Be(name1);
        }

        [Test]
        public async Task Query_WhenInutIsValid_ShouldSkipVendorsAccordingToToPageNumberAndPageSize()
        {
            // Arrange
            var vendorQuery = new VendorQuery { PageSize = 2, PageNumber = 2 };
            const string name1 = "v1";
            const string name2 = "v2";
            const string name3 = "v3";
            _efContext.Vendor.AddRange(new[]
            {
                new Vendor
                {
                    Name = name3
                },
                new Vendor
                {
                    Name = name2
                },
                new Vendor
                {
                    Name = name1
                }
            });
            _efContext.SaveChanges();

            foreach (var vendor in _efContext.Vendor)
            {
                _mapperMock.Setup(m => m.Map<VendorModel>(vendor)).Returns(new VendorModel
                {
                    Name = vendor.Name
                });
            }

            // Act
            var result = await _service.Query(vendorQuery);

            // Assert
            result.Vendors.Should().HaveCount(1);
            result.Vendors[0].Name.Should().Be(name3);
        }

        private async Task<OperationResponse> CreateSampleVendor(string vendorName = null)
        {
            var newVendor = new SaveVendorModel { };
            var dbVendor = new Vendor()
            {
                Name = vendorName
            };
            _currencyServiceMock.Setup(c => c.GetDefaultCurrency()).ReturnsAsync(new Currency());

            _vendorRuleBuilderMock.Setup(b =>
                b.ValidateAndGetVendorRules(It.IsAny<VendorRuleModel[]>(), It.IsAny<Vendor>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<VendorRule> { new VendorRule { VendorCategory = new VendorCategory
                {
                    Name = CategoryName,
                    HasDirectPayment = false,
                    IsPreferredSupplier = false,
                    Vendor = dbVendor
                }, Rule = new dataAccess.Entity.Rule() } });
                        
            _mapperMock.Setup(m => m.Map<SaveVendorModel, Vendor>(It.IsAny<SaveVendorModel>(), It.IsAny<Vendor>())).Returns(dbVendor);
            _mapperMock.Setup(m => m.Map(It.IsAny<Currency>(), It.IsAny<Vendor>())).Returns(new Vendor());
            _mapperMock.Setup(m => m.Map<VendorModel>(It.IsAny<Vendor>())).Returns(new VendorModel());
            _mapperMock.Setup(m => m.Map<VendorCategory>(It.IsAny<VendorCategoryModel>())).Returns(
                new VendorCategory
                {
                    Name = CategoryName,
                    IsPreferredSupplier = false,
                    HasDirectPayment = false
                }
            );
            return await _service.Upsert(newVendor, new UserIdentity { BuType = BuType.Pg });
        }

        [Test]
        public async Task Can_Create_New_Vendor_With_Same_Name_If_Previous_Vendor_Is_Deleted()
        {
            //Arrange
            //Create vendor at the first time
            await CreateSampleVendor(VendorName);
            var deletedVenfor = _efContext.Vendor.FirstOrDefault(v => v.Name == VendorName);
            await _service.Delete(deletedVenfor.Id);

            //Re-create vendor with the same name at the second time
            var result = await CreateSampleVendor(VendorName);

            //Assert
            result.Success.Should().BeTrue();
            _efContext.Vendor.Count(v=> v.Name == VendorName).Should().Equals(2);

            var vendors = _efContext.Vendor.Where(v => v.Name == VendorName).ToList();
            var deletedVendor = vendors.FirstOrDefault(v => v.Deleted == true);
            var activatedVendor = vendors.FirstOrDefault(v => v.Deleted == false);

            deletedVendor.Should().NotBeNull();
            activatedVendor.Should().NotBeNull();
        }

        [Test]
        public async Task Can_Not_Create_New_Vendor_With_Same_Name_If_Previous_Vendor_Is_Not_Deleted()
        {
            //Arrange
            var vendorName = VendorName + "Can_Not_Create";

            //Create vendor at the first time
            await CreateSampleVendor(vendorName);

            //Act
            //Re-create vendor with the same name at the second time
            var result = await CreateSampleVendor(vendorName);

            //Assert
            result.Success.Should().BeFalse();
            _efContext.Vendor.Count(v => v.Name == vendorName).Should().Equals(1);

            var vendors = _efContext.Vendor.Where(v => v.Name == vendorName).ToList();

            var activatedVendor = vendors.FirstOrDefault(v => v.Deleted == false);
            var deletedVendor = vendors.FirstOrDefault(v => v.Deleted == true);

            activatedVendor.Should().NotBeNull();            
            deletedVendor.Should().BeNull();            
        }
    }
}
