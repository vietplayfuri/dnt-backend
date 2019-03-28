namespace costs.net.core.tests.Services.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.Workflow;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using core.Models;
    using core.Models.User;
    using core.Models.Utils;
    using core.Models.Workflow;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Builders;
    using core.Models.Payments;
    using core.Models.Response;
    using core.Services.ActivityLog;
    using core.Services.Notifications;
    using core.Services.Payments;
    using costs.net.core.Models.ActivityLog;
    using costs.net.plugins.PG.Models.Stage;
    using System.Linq;

    public class CostActionServiceTests
    {
        public abstract class CostActionServiceTest
        {
            protected Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
            protected Mock<ICostStageService> _costStageServiceMock;
            protected Mock<IOptions<AppSettings>> _options;
            protected Mock<IValueReportingService> _valueReportingMock;
            protected Mock<IActivityLogService> _activityLogServiceMock;
            protected Mock<IEventService> _eventServiceMock;
            protected EFContext _eFContext;

            protected IEnumerable<Lazy<ICostBuilder, PluginMetadata>> CostBuilder;
            protected IEnumerable<Lazy<IPaymentService, PluginMetadata>> PaymentServiceBuilder;
            protected Mock<ICostBuilder> CostBuilderMock;
            protected Mock<IPaymentService> PaymentServiceMock;
            protected Mock<IApprovalService> ApprovalServiceMock;
            protected Mock<ICostStatusService> CostStatusServiceMock;
            protected Mock<ISupportNotificationService> SupportNotificationServiceMock;
            protected CostActionService CostActionService;
            protected CostUser CostUser;
            protected Mock<ICostApprovalService> CostApprovalServiceMock;

            protected UserIdentity User;

            [SetUp]
            public void Setup()
            {
                _eFContext = EFContextFactory.CreateInMemoryEFContext();
                CostStatusServiceMock = new Mock<ICostStatusService>();
                _options = new Mock<IOptions<AppSettings>>();
                _options.Setup(o => o.Value).Returns(new AppSettings());
                _eventServiceMock = new Mock<IEventService>();
                ApprovalServiceMock = new Mock<IApprovalService>();
                _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
                _costStageServiceMock = new Mock<ICostStageService>();
                CostBuilderMock = new Mock<ICostBuilder>();
                PaymentServiceMock = new Mock<IPaymentService>();
                _valueReportingMock = new Mock<IValueReportingService>();
                _activityLogServiceMock = new Mock<IActivityLogService>();

                CostBuilder = new List<Lazy<ICostBuilder, PluginMetadata>>
                {
                    new Lazy<ICostBuilder, PluginMetadata>(() => CostBuilderMock.Object, new PluginMetadata { BuType =  BuType.Pg })
                };

                PaymentServiceBuilder = new List<Lazy<IPaymentService, PluginMetadata>>
                {
                    new Lazy<IPaymentService, PluginMetadata>(() => PaymentServiceMock.Object, new PluginMetadata { BuType =  BuType.Pg })
                };

                User = new UserIdentity
                {
                    Email = "e@mail.com",
                    AgencyId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    BuType = BuType.Pg
                };

                CostStatusServiceMock.Setup(cs => cs.UpdateCostStageRevisionStatus(It.IsAny<Guid>(), It.IsAny<CostStageRevisionStatus>(), It.IsAny<BuType>()))
                    .ReturnsAsync(true);

                CostUser = new CostUser
                {
                    Id = User.Id,
                    Email = User.Email,
                    ParentId = User.AgencyId
                };
                _eFContext.CostUser.Add(CostUser);
                _eFContext.SaveChanges();

                CostApprovalServiceMock = new Mock<ICostApprovalService>();
                SupportNotificationServiceMock = new Mock<ISupportNotificationService>();

                CostActionService = new CostActionService(
                    _eFContext,
                    ApprovalServiceMock.Object,
                    _eventServiceMock.Object,
                    _costStageRevisionServiceMock.Object,
                    _costStageServiceMock.Object,
                    CostStatusServiceMock.Object,
                    CostBuilder,
                    CostApprovalServiceMock.Object,
                    PaymentServiceBuilder,
                    _valueReportingMock.Object,
                    _activityLogServiceMock.Object,
                    SupportNotificationServiceMock.Object
                    );
            }

            protected Cost MockCost()
            {
                var costId = Guid.NewGuid();
                var costStageRevisionId = Guid.NewGuid();
                var costStageRevision = new CostStageRevision
                {
                    Id = costStageRevisionId
                };
                var cost = new Cost
                {
                    Id = costId,
                    Status = CostStageRevisionStatus.Draft,
                    LatestCostStageRevisionId = costStageRevisionId,
                    LatestCostStageRevision = costStageRevision,
                    ExchangeRateDate = DateTime.UtcNow
                };

                _eFContext.Cost.Add(cost);
                _eFContext.SaveChanges();
                return cost;
            }
        }

        [TestFixture]
        public class CompleteRecallShould : CostActionServiceTest
        {
            [Test]
            public async Task SetStatusToRecall()
            {
                // Arrange
                var cost = MockCost();

                // Act
                var response = await CostActionService.CompleteRecall(cost.Id, User);

                // Assert 
                response.Should().NotBeNull();
                CostStatusServiceMock.Verify(cs => cs.UpdateCostStageRevisionStatus(cost.Id, CostStageRevisionStatus.Recalled, BuType.Pg), Times.Once);
            }
        }

        [TestFixture]
        public class RecallShould : CostActionServiceTest
        {
            [Test]
            public async Task SetStatusToRecall()
            {
                // Arrange
                var cost = MockCost();

                // Act
                await CostActionService.Recall(cost.Id, User);

                // Assert
                CostStatusServiceMock.Verify(cs => cs.UpdateCostStatus(User.BuType, cost.Id, CostAction.Recall), Times.Once);
            }
        }

        [TestFixture]
        public class CancelShould : CostActionServiceTest
        {
            [Test]
            public async Task SetStatusToCancel()
            {
                // Arrange 
                var cost = MockCost();

                // Act
                await CostActionService.Cancel(User.BuType, cost.Id);

                // Assert
                CostStatusServiceMock.Verify(cs => cs.UpdateCostStatus(User.BuType, cost.Id, CostAction.Cancel), Times.Once);
            }
        }

        [TestFixture]
        public class CompleteCancelShould : CostActionServiceTest
        {
            [Test]
            public async Task SetStatusToCancel()
            {
                // Arrange 
                var cost = MockCost();

                // Act
                await CostActionService.CompleteCancel(cost.Id, BuType.Pg);

                // Assert
                CostStatusServiceMock.Verify(cs => cs.UpdateCostStageRevisionStatus(cost.Id, CostStageRevisionStatus.Cancelled, BuType.Pg), Times.Once);
            }
        }

        [TestFixture]
        public class SubmitShould : CostActionServiceTest
        {
            [Test]
            public async Task UpdateCostStatusAndSendForApproval()
            {
                // Arrange 
                var cost = MockCost();
                CostBuilderMock.Setup(b => b.IsValidForSubmittion(cost.Id)).ReturnsAsync(new OperationResponse { Success = true });

                // Act
                await CostActionService.Submit(cost.Id, User);

                // Assert
                ApprovalServiceMock.Verify(cs => cs.SubmitApprovals(It.IsAny<Cost>(), It.IsAny<CostUser>(), It.IsAny<IEnumerable<Approval>>(), It.IsAny<BuType>()), Times.Once);
                CostStatusServiceMock.Verify(cs => cs.UpdateCostStatus(User.BuType, cost.Id, CostAction.Submit), Times.Once);
            }

            [Test]
            public async Task UpdateSubmittedAtOfTheCostStageRevision()
            {
                // Arrange 
                var cost = MockCost();
                var timeJustBeforeSubmittion = DateTime.UtcNow;
                CostBuilderMock.Setup(b => b.IsValidForSubmittion(cost.Id)).ReturnsAsync(new OperationResponse { Success = true });

                // Act
                await CostActionService.Submit(cost.Id, User);

                // Assert
                cost.LatestCostStageRevision.Submitted.Should().NotBeNull();
                Debug.Assert(cost.LatestCostStageRevision.Submitted != null, "cost.LatestCostStageRevision.Submitted != null");
                cost.LatestCostStageRevision.Submitted.Value.Should().BeOnOrAfter(timeJustBeforeSubmittion);
                cost.LatestCostStageRevision.Submitted.Value.Should().BeOnOrBefore(DateTime.UtcNow);
            }

            [Test]
            public async Task CalculatePaymentAmount_AfterSubmission()
            {
                // Arrange 
                CostBuilderMock = new Mock<ICostBuilder>(MockBehavior.Strict);
                PaymentServiceMock = new Mock<IPaymentService>(MockBehavior.Strict);

                var cost = MockCost();
                var sequence = new MockSequence();

                CostBuilderMock.InSequence(sequence).Setup(b => b.IsValidForSubmittion(cost.Id))
                    .ReturnsAsync(new OperationResponse { Success = true });

                CostBuilderMock.InSequence(sequence).Setup(b => b.SubmitCost(cost.Id))
                    .Returns(Task.CompletedTask);

                PaymentServiceMock.InSequence(sequence).Setup(p =>
                    p.CalculatePaymentAmount(cost.LatestCostStageRevision.Id, true))
                .ReturnsAsync(new PaymentAmountResult());

                CostBuilder = new List<Lazy<ICostBuilder, PluginMetadata>>
                {
                    new Lazy<ICostBuilder, PluginMetadata>(() => CostBuilderMock.Object, new PluginMetadata { BuType =  BuType.Pg })
                };
                PaymentServiceBuilder = new List<Lazy<IPaymentService, PluginMetadata>>
                {
                    new Lazy<IPaymentService, PluginMetadata>(() => PaymentServiceMock.Object, new PluginMetadata { BuType =  BuType.Pg })
                };
                CostActionService = new CostActionService(
                    _eFContext,
                    ApprovalServiceMock.Object,
                    _eventServiceMock.Object,
                    _costStageRevisionServiceMock.Object,
                    _costStageServiceMock.Object,
                    CostStatusServiceMock.Object,
                    CostBuilder,
                    CostApprovalServiceMock.Object,
                    PaymentServiceBuilder,
                    _valueReportingMock.Object,
                    _activityLogServiceMock.Object,
                    SupportNotificationServiceMock.Object
                );

                // Act
                await CostActionService.Submit(cost.Id, User);

                // Assert
                CostBuilderMock.Verify(b => b.IsValidForSubmittion(cost.Id), Times.Once);
                CostBuilderMock.Verify(b => b.SubmitCost(cost.Id), Times.Once);
                PaymentServiceMock.Verify(p => p.CalculatePaymentAmount(cost.LatestCostStageRevision.Id, true), Times.Once);
            }

            [Test]
            public async Task ReturnFailureReponse_WhenCostIsNotValidForSubmittion()
            {
                // Arrange
                var cost = MockCost();
                CostBuilderMock.Setup(b => b.IsValidForSubmittion(cost.Id)).ReturnsAsync(new OperationResponse { Success = false });

                // Act
                var response = await CostActionService.Submit(cost.Id, User);

                // Assert
                response.Success.Should().BeFalse();
            }


            private async Task<Cost> SetupCostStageData(string testCase)
            {
                var cost = new Cost();
                switch (testCase)
                {
                    case "OE_Stage_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        break;
                    case "OE_Stage_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_OE_Stage();
                        break;
                    case "OE_Current_Revision_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_OE_Stage_With_Revision(cost);
                        break;
                    case "OE_Current_Revision_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_OE_Stage_With_Revision(cost);
                        cost = await CreateCost_At_OE_Stage_With_Revision(cost);
                        break;

                    case "FP_Stage_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        break;
                    case "FP_Stage_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FP_Stage(cost);
                        break;
                    case "FP_Current_Revision_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FP_Stage_With_Revision(cost);
                        break;
                    case "FP_Current_Revision_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FP_Stage_With_Revision(cost);
                        cost = await CreateCost_At_FP_Stage_With_Revision(cost);
                        break;

                    case "FA_Stage_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FA_Stage(cost);
                        break;
                    case "FA_Stage_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FA_Stage(cost);
                        cost = await CreateCost_At_FA_Stage(cost);
                        break;
                    case "FA_Current_Revision_Reopen":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FA_Stage(cost);
                        cost = await CreateCost_At_FA_Stage_With_Revision(cost);
                        break;
                    case "FA_Current_Revision_Reopen_More_Than_1_Time":
                        cost = await CreateCost_At_OE_Stage();
                        cost = await CreateCost_At_FP_Stage(cost);
                        cost = await CreateCost_At_FA_Stage(cost);
                        cost = await CreateCost_At_FA_Stage_With_Revision(cost);
                        cost = await CreateCost_At_FA_Stage_With_Revision(cost);
                        break;
                    default:
                        break;
                }

                return cost;
            }

            #region create cost data for each stage
            private async Task<Cost> CreateCost_At_OE_Stage()
            {
                var costStageRevisionId = Guid.NewGuid();
                var costStageRevision = new CostStageRevision
                {
                    Id = costStageRevisionId,
                    Name = nameof(CostStages.OriginalEstimate),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };

                var cost = new Cost
                {
                    Id = Guid.NewGuid(),
                    LatestCostStageRevisionId = costStageRevisionId,
                    LatestCostStageRevision = costStageRevision,
                    ExchangeRateDate = DateTime.UtcNow,
                    ExchangeRate = 1m
                };

                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.OriginalEstimate)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.OriginalEstimate)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                cost.CostStages.Add(new CostStage
                {
                    Name = nameof(CostStages.OriginalEstimate)
                });
                cost.CostStages.LastOrDefault().CostStageRevisions.Add(costStageRevision);

                await _eFContext.Cost.AddAsync(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }

            private async Task<Cost> CreateCost_At_OE_Stage_With_Revision(Cost cost)
            {
                var newCostStageRevision = new CostStageRevision()
                {
                    Id = Guid.NewGuid(),
                    Name = nameof(CostStages.OriginalEstimateRevision),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };
                cost.ExchangeRateDate = DateTime.UtcNow;

                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.OriginalEstimate)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.OriginalEstimateRevision)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                cost.CostStages.LastOrDefault().CostStageRevisions.Add(newCostStageRevision);
                cost.LatestCostStageRevision = newCostStageRevision;
                cost.LatestCostStageRevisionId = newCostStageRevision.Id;

                _eFContext.Cost.Update(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }

            private async Task<Cost> CreateCost_At_FP_Stage(Cost cost)
            {
                if (!cost.CostStages.Any(cs => cs.Name == nameof(CostStages.FirstPresentation)))
                    cost.CostStages.Add(new CostStage { Name = nameof(CostStages.FirstPresentation) });

                //Approved previous stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.OriginalEstimate)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.OriginalEstimate)).Status = CostStageRevisionStatus.Approved;
                    if (cs.CostStageRevisions.Any(csr => csr.Name == nameof(CostStages.OriginalEstimateRevision)))
                        cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.OriginalEstimateRevision)).Status = CostStageRevisionStatus.Approved;
                });

                //Reject current stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.FirstPresentation)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.FirstPresentation)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                //Add new item for current stages
                var newCostStageRevisionId = Guid.NewGuid();
                var newCostStageRevision = new CostStageRevision()
                {
                    Id = newCostStageRevisionId,
                    Name = nameof(CostStages.FirstPresentation),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };

                //update cost
                cost.CostStages.LastOrDefault().CostStageRevisions.Add(newCostStageRevision);
                cost.LatestCostStageRevision = newCostStageRevision;
                cost.LatestCostStageRevisionId = newCostStageRevisionId;

                _eFContext.Cost.Update(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }

            private async Task<Cost> CreateCost_At_FP_Stage_With_Revision(Cost cost)
            {
                //Reject current stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.FirstPresentation)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.FirstPresentationRevision)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                //Add new item for current stages
                var newCostStageRevision = new CostStageRevision()
                {
                    Id = Guid.NewGuid(),
                    Name = nameof(CostStages.FirstPresentationRevision),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };
                cost.CostStages.LastOrDefault().CostStageRevisions.Add(newCostStageRevision);
                cost.LatestCostStageRevision = newCostStageRevision;
                cost.LatestCostStageRevisionId = newCostStageRevision.Id;

                _eFContext.Cost.Update(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }

            private async Task<Cost> CreateCost_At_FA_Stage(Cost cost)
            {
                if (!cost.CostStages.Any(cs => cs.Name == nameof(CostStages.FinalActual)))
                    cost.CostStages.Add(new CostStage { Name = nameof(CostStages.FinalActual) });

                //Approved previous stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.OriginalEstimate)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.OriginalEstimate)).Status = CostStageRevisionStatus.Approved;
                    if (cs.CostStageRevisions.Any(csr => csr.Name == nameof(CostStages.OriginalEstimateRevision)))
                        cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.OriginalEstimateRevision)).Status = CostStageRevisionStatus.Approved;
                });
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.FirstPresentation)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.FirstPresentation)).Status = CostStageRevisionStatus.Approved;
                    if (cs.CostStageRevisions.Any(csr => csr.Name == nameof(CostStages.FirstPresentationRevision)))
                        cs.CostStageRevisions.Last(csr => csr.Name == nameof(CostStages.FirstPresentationRevision)).Status = CostStageRevisionStatus.Approved;
                });

                //Reject current stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.FinalActual)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.FinalActual)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                //Add new item for current stages
                var newCostStageRevisionId = Guid.NewGuid();
                var newCostStageRevision = new CostStageRevision()
                {
                    Id = newCostStageRevisionId,
                    Name = nameof(CostStages.FinalActual),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };

                //update cost
                cost.CostStages.LastOrDefault().CostStageRevisions.Add(newCostStageRevision);
                cost.LatestCostStageRevision = newCostStageRevision;
                cost.LatestCostStageRevisionId = newCostStageRevisionId;

                _eFContext.Cost.Update(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }

            private async Task<Cost> CreateCost_At_FA_Stage_With_Revision(Cost cost)
            {
                //Reject current stages
                cost.CostStages.Where(cs => cs.Name == nameof(CostStages.FinalActual)).ToList().ForEach(cs =>
                {
                    cs.CostStageRevisions.Where(csr => csr.Name == nameof(CostStages.FinalActualRevision)).ToList().ForEach(csr =>
                    {
                        csr.Status = CostStageRevisionStatus.Rejected;
                    });
                });

                //Add new item for current stages
                var newCostStageRevision = new CostStageRevision()
                {
                    Id = Guid.NewGuid(),
                    Name = nameof(CostStages.FinalActualRevision),
                    Status = CostStageRevisionStatus.PendingTechnicalApproval
                };
                cost.CostStages.LastOrDefault().CostStageRevisions.Add(newCostStageRevision);
                cost.LatestCostStageRevision = newCostStageRevision;
                cost.LatestCostStageRevisionId = newCostStageRevision.Id;

                _eFContext.Cost.Update(cost);
                await _eFContext.SaveChangesAsync();
                return cost;
            }
            #endregion

            /// <summary>
            /// ADC-2607 - Tested with:
            /// 14 status of Enum: CostAction
            /// 12 cases: ( 3 x 4 = 12)
            ///     - We have 3 main stages - OE/FP/FA
            ///     - Each stage we have 4 cases: Reopen / Reopen more than 1 time / Current Revision Reopen / Current Revision Reopen more than 1 time
            /// So totally we have 12 x 14 = 168 cases
            /// There are only 2 cases that we will clear exchange rate date:
            /// OE_Stage_Reopen and OE_Stage_Reopen_More_Than_1_Time are Reopened
            /// </summary>
            [Test]
            [TestCase("OE_Stage_Reopen", "Submit", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("OE_Current_Revision_Reopen", "Submit", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("FP_Stage_Reopen", "Submit", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("FP_Current_Revision_Reopen", "Submit", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("FA_Stage_Reopen", "Submit", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("FA_Current_Revision_Reopen", "Submit", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Submit", "")]
            [TestCase("OE_Stage_Reopen", "NextStage", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("OE_Current_Revision_Reopen", "NextStage", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("FP_Stage_Reopen", "NextStage", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("FP_Current_Revision_Reopen", "NextStage", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("FA_Stage_Reopen", "NextStage", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("FA_Current_Revision_Reopen", "NextStage", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "NextStage", "")]
            [TestCase("OE_Stage_Reopen", "CreateRevision", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("OE_Current_Revision_Reopen", "CreateRevision", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("FP_Stage_Reopen", "CreateRevision", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("FP_Current_Revision_Reopen", "CreateRevision", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("FA_Stage_Reopen", "CreateRevision", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("FA_Current_Revision_Reopen", "CreateRevision", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "CreateRevision", "")]
            [TestCase("OE_Stage_Reopen", "Reopen", "1")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Reopen", "1")]
            [TestCase("OE_Current_Revision_Reopen", "Reopen", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Reopen", "")]
            [TestCase("FP_Stage_Reopen", "Reopen", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Reopen", "")]
            [TestCase("FP_Current_Revision_Reopen", "Reopen", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Reopen", "")]
            [TestCase("FA_Stage_Reopen", "Reopen", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Reopen", "")]
            [TestCase("FA_Current_Revision_Reopen", "Reopen", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Reopen", "")]
            [TestCase("OE_Stage_Reopen", "Recall", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("OE_Current_Revision_Reopen", "Recall", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("FP_Stage_Reopen", "Recall", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("FP_Current_Revision_Reopen", "Recall", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("FA_Stage_Reopen", "Recall", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("FA_Current_Revision_Reopen", "Recall", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Recall", "")]
            [TestCase("OE_Stage_Reopen", "Cancel", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("OE_Current_Revision_Reopen", "Cancel", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("FP_Stage_Reopen", "Cancel", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("FP_Current_Revision_Reopen", "Cancel", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("FA_Stage_Reopen", "Cancel", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("FA_Current_Revision_Reopen", "Cancel", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Cancel", "")]
            [TestCase("OE_Stage_Reopen", "Delete", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("OE_Current_Revision_Reopen", "Delete", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("FP_Stage_Reopen", "Delete", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("FP_Current_Revision_Reopen", "Delete", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("FA_Stage_Reopen", "Delete", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("FA_Current_Revision_Reopen", "Delete", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Delete", "")]
            [TestCase("OE_Stage_Reopen", "Approve", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("OE_Current_Revision_Reopen", "Approve", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("FP_Stage_Reopen", "Approve", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("FP_Current_Revision_Reopen", "Approve", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("FA_Stage_Reopen", "Approve", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("FA_Current_Revision_Reopen", "Approve", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Approve", "")]
            [TestCase("OE_Stage_Reopen", "Reject", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("OE_Current_Revision_Reopen", "Reject", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("FP_Stage_Reopen", "Reject", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("FP_Current_Revision_Reopen", "Reject", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("FA_Stage_Reopen", "Reject", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("FA_Current_Revision_Reopen", "Reject", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "Reject", "")]
            [TestCase("OE_Stage_Reopen", "RequestReopen", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("OE_Current_Revision_Reopen", "RequestReopen", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("FP_Stage_Reopen", "RequestReopen", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("FP_Current_Revision_Reopen", "RequestReopen", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("FA_Stage_Reopen", "RequestReopen", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("FA_Current_Revision_Reopen", "RequestReopen", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "RequestReopen", "")]
            [TestCase("OE_Stage_Reopen", "RejectReopen", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("OE_Current_Revision_Reopen", "RejectReopen", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("FP_Stage_Reopen", "RejectReopen", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("FP_Current_Revision_Reopen", "RejectReopen", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("FA_Stage_Reopen", "RejectReopen", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("FA_Current_Revision_Reopen", "RejectReopen", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "RejectReopen", "")]
            [TestCase("OE_Stage_Reopen", "ApproveReopen", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("OE_Current_Revision_Reopen", "ApproveReopen", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("FP_Stage_Reopen", "ApproveReopen", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("FP_Current_Revision_Reopen", "ApproveReopen", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("FA_Stage_Reopen", "ApproveReopen", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("FA_Current_Revision_Reopen", "ApproveReopen", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "ApproveReopen", "")]
            [TestCase("OE_Stage_Reopen", "EditValueReporting", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("OE_Current_Revision_Reopen", "EditValueReporting", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("FP_Stage_Reopen", "EditValueReporting", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("FP_Current_Revision_Reopen", "EditValueReporting", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("FA_Stage_Reopen", "EditValueReporting", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("FA_Current_Revision_Reopen", "EditValueReporting", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "EditValueReporting", "")]
            [TestCase("OE_Stage_Reopen", "EditIONumber", "")]
            [TestCase("OE_Stage_Reopen_More_Than_1_Time", "EditIONumber", "")]
            [TestCase("OE_Current_Revision_Reopen", "EditIONumber", "")]
            [TestCase("OE_Current_Revision_Reopen_More_Than_1_Time", "EditIONumber", "")]
            [TestCase("FP_Stage_Reopen", "EditIONumber", "")]
            [TestCase("FP_Stage_Reopen_More_Than_1_Time", "EditIONumber", "")]
            [TestCase("FP_Current_Revision_Reopen", "EditIONumber", "")]
            [TestCase("FP_Current_Revision_Reopen_More_Than_1_Time", "EditIONumber", "")]
            [TestCase("FA_Stage_Reopen", "EditIONumber", "")]
            [TestCase("FA_Stage_Reopen_More_Than_1_Time", "EditIONumber", "")]
            [TestCase("FA_Current_Revision_Reopen", "EditIONumber", "")]
            [TestCase("FA_Current_Revision_Reopen_More_Than_1_Time", "EditIONumber", "")]
            public async Task CreateVersion_Stages_Reopen(string testCase, string strCostAction, string shouldClearExchangeRateDate)
            {
                // Arrange
                var costHasExchangeRateDate = string.IsNullOrWhiteSpace(shouldClearExchangeRateDate)
                    ? true
                    : false;
                var cost = await SetupCostStageData(testCase);

                CostAction action = (CostAction)Enum.Parse(typeof(CostAction), strCostAction);
                var newRevision = new CostStageRevision { Name = cost.LatestCostStageRevision.Name };

                _costStageRevisionServiceMock.Setup(crs => crs.CreateVersion(It.IsAny<CostStageRevision>(), It.IsAny<Guid>(), It.IsAny<BuType>(), action))
                    .ReturnsAsync(newRevision);
                CostStatusServiceMock.Setup(b => b.UpdateCostStatus(It.IsAny<BuType>(), cost.Id, action))
                    .ReturnsAsync(new OperationResponse { });
                CostApprovalServiceMock.Setup(b => b.UpdateApprovals(It.IsAny<Guid>(), User.Id, It.IsAny<BuType>()))
                    .Returns(Task.FromResult(default(object)));
                _activityLogServiceMock.Setup(b => b.Log(It.IsAny<CostReopened>()))
                    .Returns(Task.FromResult(default(object)));

                // Act
                var response = await CostActionService.CreateVersion(cost.Id, User, action);

                // Assert
                var updatedCost = await _eFContext.Cost.FindAsync(cost.Id);
                updatedCost.ExchangeRateDate.HasValue.Should().Be(costHasExchangeRateDate);
                updatedCost.ExchangeRate.HasValue.Should().Be(costHasExchangeRateDate);
            }
        }
    }
}
