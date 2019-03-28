namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using core.Models;
    using core.Models.Payments;
    using core.Services.Costs;
    using core.Services.Payments;
    using core.Services.Workflow;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;
    using Serilog;

    public class CostStageServiceTests
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };

        private Mock<ICostStageRevisionService> CostStageRevisionServiceMock;
        private CostStageService CostStageService;

        private EFContext _efContext;
        private Mock<ILogger> LoggerMock;
        private Mock<IStageService> StageServiceMock;
        private Mock<IPaymentService> PgPaymentService;
        private Mock<ICostBuilder> costBuilderMock;

        [SetUp]
        public void Setup()
        {
            LoggerMock = new Mock<ILogger>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            CostStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            StageServiceMock = new Mock<IStageService>();
            costBuilderMock = new Mock<ICostBuilder>();
            var costbuilder = new List<Lazy<ICostBuilder, PluginMetadata>>
            {
                new Lazy<ICostBuilder, PluginMetadata>(
                    () => costBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
            };

            PgPaymentService = new Mock<IPaymentService>();
            PgPaymentService.Setup(x => x.CalculatePaymentAmount(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync((PaymentAmountResult) null);
            PgPaymentService.Setup(x => x.GetPaymentAmount(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync((PaymentAmountResult) null);

            CostStageService = new CostStageService(
                _efContext,
                LoggerMock.Object,
                CostStageRevisionServiceMock.Object,
                StageServiceMock.Object,
                costbuilder
            );
        }

        [Test]
        public async Task GetCostStagesLatestRevisions_OneStage()
        {
            // Arrange
            var currency = new Currency
            {
                Code = "AFN",
                DefaultCurrency = false,
                Description = "",
                Id = Guid.NewGuid(),
                Symbol = "symbol"
            };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                PaymentCurrency = currency
            };
            const string costNumber = "AC1489594599188";

            var costStage = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 2,
                Cost = cost
            };
            var costStageRevision = new CostStageRevision
            {
                StageDetails = new CustomFormData
                {
                    Data = JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        CostNumber = costNumber
                    }, _serializerSettings)
                },
                Id = Guid.NewGuid(),
                CostStage = costStage
            };

            _efContext.CostStageRevision.Add(costStageRevision);
            _efContext.SaveChanges();

            CostStageRevisionServiceMock.Setup(a => a.GetLatestRevisionWithPaymentCurrency(It.IsAny<Guid>())).ReturnsAsync(costStageRevision);

            // Act
            var result = await CostStageService.GetStagesLatestRevision(cost.Id, BuType.Pg);

            // Assert
            result.Should().HaveCount(1);
            result[0].Currency.Should().Be(currency);
            result[0].LatestRevision.Should().Be(costStageRevision);
        }

        [Test]
        public async Task GetCostStagesLatestRevisions_TwoStages()
        {
            // Arrange
            var currency = new Currency
            {
                Code = "AFN",
                DefaultCurrency = false,
                Description = "",
                Id = Guid.NewGuid(),
                Symbol = "symbol"
            };

            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                PaymentCurrency = currency
            };
            var costStageRevisionId = Guid.NewGuid();
            const string costNumber = "AC1489594599188";

            var costStageOe = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 2,
                Cost = cost
            };
            var costStageAipe = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.Aipe.ToString(),
                StageOrder = 1
            };
            var costStageRevision = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                StageDetailsId = costStageRevisionId,
                StageDetails = new CustomFormData
                {
                    Id = costStageRevisionId,
                    Data = JsonConvert.SerializeObject(new PgStageDetailsForm
                        {
                            CostNumber = costNumber
                        },
                        _serializerSettings)
                },
                CostStage = costStageOe
            };
            _efContext.CostStage.AddRange(costStageOe, costStageAipe);
            _efContext.CostStageRevision.Add(costStageRevision);
            _efContext.Currency.Add(currency);
            _efContext.SaveChanges();

            CostStageRevisionServiceMock.Setup(a => a.GetLatestRevisionWithPaymentCurrency(It.IsAny<Guid>())).ReturnsAsync(costStageRevision);

            // Act
            var result = await CostStageService.GetStagesLatestRevision(cost.Id, BuType.Pg);

            // Assert
            result.Should().HaveCount(2);
            result[0].Currency.Should().Be(currency);
            result[1].Currency.Should().Be(currency);
            ((string) result[0].StageDetails.costNumber).Should().Be(costNumber);
            result[0].LatestRevision.Should().Be(costStageRevision);
        }

        [Test]
        public async Task GetCostStagesLatestRevisions_2ApprovedFAs()
        {
            // Arrange
            var currency = new Currency
            {
                Code = "USD",
                DefaultCurrency = false,
                Description = "",
                Id = Guid.NewGuid(),
                Symbol = "$"
            };

            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                PaymentCurrency = currency
            };
            var costStageRevisionId = Guid.NewGuid();
            const string costNumber = "PGU0000001V0029";

            var costStageOe = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 1,
                Cost = cost
            };
            var costStageFP = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.FirstPresentation.ToString(),
                StageOrder = 2
            };


            var costStageFA = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.FinalActual.ToString(),
                StageOrder = 3
            };
            var costStageRevisionFA1 = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                StageDetailsId = costStageRevisionId,
                StageDetails = new CustomFormData
                {
                    Id = costStageRevisionId,
                    Data = JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        CostNumber = costNumber
                    },
                        _serializerSettings)
                },
                CostStage = costStageFA,
                Status = CostStageRevisionStatus.Approved,
                IsLineItemSectionCurrencyLocked=true,
                IsPaymentCurrencyLocked =true
            };
            var costStageRevisionFA2 = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                StageDetailsId = costStageRevisionId,
                StageDetails = new CustomFormData
                {
                    Id = costStageRevisionId,
                    Data = JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        CostNumber = costNumber
                    },
                        _serializerSettings)
                },
                CostStage = costStageFA,
                Status = CostStageRevisionStatus.PendingTechnicalApproval,
                IsLineItemSectionCurrencyLocked = true,
                IsPaymentCurrencyLocked = true
            };

            var costStageRevisionPaymentTotalFA1 = new CostStageRevisionPaymentTotal
            {
                Id = Guid.NewGuid(),
                CostStageRevision = costStageRevisionFA1,
                LineItemTotalType = "CostTotal",
                LineItemFullCost = 41000,
                LineItemTotalCalculatedValue = 25500,
                IsProjection = false,
                StageName = CostStages.FinalActual.ToString(),
                CalculatedAt = DateTime.Now
            };

            var costStageRevisionPaymentTotalFA2 = new CostStageRevisionPaymentTotal
            {
                Id = Guid.NewGuid(),
                CostStageRevision = costStageRevisionFA2,
                LineItemTotalType = "CostTotal",
                LineItemFullCost = 35000,
                LineItemTotalCalculatedValue = -6000,//35000-41000
                IsProjection = false,
                StageName = CostStages.FinalActual.ToString(),
                CalculatedAt = DateTime.Now
            };

            _efContext.CostStage.AddRange(costStageOe, costStageFP, costStageFA);
            _efContext.CostStageRevision.AddRange(costStageRevisionFA1, costStageRevisionFA2);
            _efContext.Currency.Add(currency);
            _efContext.SaveChanges();

            var costStageRevisionPaymentTotals = new List<CostStageRevisionPaymentTotal> {
                costStageRevisionPaymentTotalFA1,
                costStageRevisionPaymentTotalFA2
            };

            CostStageRevisionServiceMock.Setup(a => a.GetLatestRevisionWithPaymentCurrency(It.IsAny<Guid>())).ReturnsAsync(costStageRevisionFA2);
            CostStageRevisionServiceMock.Setup(a => a.GetAllCostPaymentTotalsFinalActual(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(costStageRevisionPaymentTotals);

            // Act
            var result = await CostStageService.GetStagesLatestRevision(cost.Id, BuType.Pg);

            // Assert
            result.Should().HaveCount(3);
            ((string)result[2].StageDetails.costNumber).Should().Be(costNumber);
            result[2].LatestRevision.Should().Be(costStageRevisionFA2);
            result[2].DisplayGRAmount.Should().Be(19500);
        }

        [Test]
        public async Task GetStages_Should_Get_Total_From_Latest_Approved_Revision()
        {
            // Arrange
            var cost = SetCostStage(out var approvedRevisionId);

            costBuilderMock.Setup(s =>
                s.GetRevisionTotals(It.Is<CostStageRevision>(r => r.Id == approvedRevisionId))
            ).ReturnsAsync((2000, 2000));

            // Act
            var result = await CostStageService.GetStages(cost.Id, BuType.Pg);

            // Assert
            result[0].cost.Total.ShouldBeEquivalentTo(2000);
        }

        [Test]
        public async Task GetApprovedOrLastRevision_Should_Get_Last_Approved_Revision()
        {
            // Arrange
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                LatestCostStageRevisionId = Guid.NewGuid()
            };
            var approvedRevisionId = Guid.NewGuid();
            var costStageOe = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 2,
                Cost = cost
            };
            var costStageRevisions = new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    CostStage = costStageOe,
                    Id = Guid.NewGuid(),
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Rejected
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 1000,
                            ValueInLocalCurrency = 1000
                        }
                    }
                },
                new CostStageRevision
                {
                    Id = approvedRevisionId,
                    CostStage = costStageOe,
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Approved
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 2000,
                            ValueInLocalCurrency = 2000
                        }
                    }
                },
                new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CostStage = costStageOe,
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Rejected
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 3000,
                            ValueInLocalCurrency = 3000
                        }
                    }
                }
            };

            _efContext.CostStageRevision.AddRange(costStageRevisions);
            _efContext.SaveChanges();

            // Act
            var result = await CostStageService.GetApprovedOrLastRevision(cost.Id, CostStages.OriginalEstimate.ToString());

            // Assert
            result.LatestRevision.Id.ShouldBeEquivalentTo(approvedRevisionId);
        }

        [Test]
        public async Task GetApprovedOrLastRevision_Should_Get_Last_Revision_For_Not_Approved_Stage()
        {
            // Arrange
            var lastRevisionId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                LatestCostStageRevisionId = lastRevisionId
            };
            var previsiousId = Guid.NewGuid();
            var costStageOe = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = cost.Id,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 2,
                Cost = cost
            };
            var firstRevisionCreated = DateTime.UtcNow;
            var costStageRevisions = new List<CostStageRevision>
            {
                new CostStageRevision
                {
                    CostStage = costStageOe,
                    Id = Guid.NewGuid(),
                    Created = firstRevisionCreated,
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Rejected
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 1000,
                            ValueInLocalCurrency = 1000
                        }
                    }
                },
                new CostStageRevision
                {
                    Id = previsiousId,
                    CostStage = costStageOe,
                    Created = firstRevisionCreated.AddDays(1),
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Rejected
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 2000,
                            ValueInLocalCurrency = 2000
                        }
                    }
                },
                new CostStageRevision
                {
                    Id = lastRevisionId,
                    CostStage = costStageOe,
                    Created = firstRevisionCreated.AddDays(2),
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Rejected
                        }
                    },
                    CostLineItems = new List<CostLineItem>
                    {
                        new CostLineItem
                        {
                            Name = "ABCD",
                            ValueInDefaultCurrency = 3000,
                            ValueInLocalCurrency = 3000
                        }
                    }
                }
            };

            _efContext.CostStageRevision.AddRange(costStageRevisions);
            _efContext.SaveChanges();

            // Act
            var result = await CostStageService.GetApprovedOrLastRevision(cost.Id, CostStages.OriginalEstimate.ToString());

            // Assert
            result.LatestRevision.Id.ShouldBeEquivalentTo(previsiousId);
        }

        private Cost SetCostStage(out Guid approvedRevisionId)
        {
            approvedRevisionId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage>(),
                PaymentCurrency = new Currency
                {
                    Code = "AFN",
                    DefaultCurrency = false,
                    Description = "",
                    Id = Guid.NewGuid(),
                    Symbol = "symbol"
                }
            };

            var costStageOe = new CostStage
            {
                Id = Guid.NewGuid(),
                Cost = cost,
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Key = CostStages.OriginalEstimate.ToString(),
                StageOrder = 2,
                CostStageRevisions = new List<CostStageRevision>
                {
                    new CostStageRevision
                    {
                        Id = Guid.NewGuid(),
                        Approvals = new List<Approval>
                        {
                            new Approval
                            {
                                Status = ApprovalStatus.Rejected
                            }
                        },
                        CostLineItems = new List<CostLineItem>
                        {
                            new CostLineItem
                            {
                                Name = "ABCD",
                                ValueInDefaultCurrency = 1000,
                                ValueInLocalCurrency = 1000
                            }
                        }
                    },
                    new CostStageRevision
                    {
                        Id = approvedRevisionId,
                        Approvals = new List<Approval>
                        {
                            new Approval
                            {
                                Status = ApprovalStatus.Approved
                            }
                        },
                        CostLineItems = new List<CostLineItem>
                        {
                            new CostLineItem
                            {
                                Name = "ABCD",
                                ValueInDefaultCurrency = 2000,
                                ValueInLocalCurrency = 2000
                            }
                        },
                        StageDetails = new CustomFormData
                        {
                            Data = JsonConvert.SerializeObject(new PgStageDetailsForm(), _serializerSettings)
                        }
                    },
                    new CostStageRevision
                    {
                        Id = Guid.NewGuid(),
                        Approvals = new List<Approval>
                        {
                            new Approval
                            {
                                Status = ApprovalStatus.Rejected
                            }
                        },
                        CostLineItems = new List<CostLineItem>
                        {
                            new CostLineItem
                            {
                                Name = "ABCD",
                                ValueInDefaultCurrency = 3000,
                                ValueInLocalCurrency = 3000
                            }
                        },
                        StageDetails = new CustomFormData
                        {
                            Data = JsonConvert.SerializeObject(new PgStageDetailsForm(), _serializerSettings)
                        }
                    }
                }
            };
            cost.CostStages.Add(costStageOe);

            _efContext.Cost.AddRange(cost);
            _efContext.SaveChanges();

            cost.LatestCostStageRevision = costStageOe.CostStageRevisions.Last();
            _efContext.SaveChanges();

            return cost;
        }

        [Test]
        public async Task GetStages_Should_Set_Status_As_Latest_Revision()
        {
            // Arrange
            var cost = SetCostStage(out var approvedRevisionId);
            cost.LatestCostStageRevision.Status = CostStageRevisionStatus.PendingTechnicalApproval;
            _efContext.SaveChanges();

            costBuilderMock.Setup(s =>
                s.GetRevisionTotals(It.Is<CostStageRevision>(r => r.Id == approvedRevisionId))
            ).ReturnsAsync((2000, 2000));

            // Act
            var result = await CostStageService.GetStages(cost.Id, BuType.Pg);

            // Assert
            result[0].Status.ShouldBeEquivalentTo(CostStageRevisionStatus.PendingTechnicalApproval);
        }

        [Test]
        public async Task GetPreviousCostStage_whenThereIsPreviousStage_shouldReturnPreviousCostStage()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var previousCostStage = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = costId,
                StageOrder = 0
            };
            var currentCostStage = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = costId,
                StageOrder = 1
            };

            _efContext.CostStage.AddRange(previousCostStage, currentCostStage);
            _efContext.SaveChanges();

            // Act
            var costStage = await CostStageService.GetPreviousCostStage(currentCostStage.Id);

            // Assert
            costStage.Should().NotBeNull();
            costStage.Id.Should().Be(previousCostStage.Id);
        }

        [Test]
        public async Task GetPreviousCostStage_whenThereIsNoPreviousStage_shouldReturnNull()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var currentCostStage = new CostStage
            {
                Id = Guid.NewGuid(),
                CostId = costId,
                StageOrder = 1
            };
            _efContext.CostStage.Add(currentCostStage);
            _efContext.SaveChanges();

            // Act
            var costStage = await CostStageService.GetPreviousCostStage(currentCostStage.Id);

            // Assert
            costStage.Should().BeNull();
        }
    }
}
