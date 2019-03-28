namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Builders;
    using core.Models.ACL;
    using core.Models.Costs;    
    using core.Models.Workflow;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;
    using CostType = core.Models.CostTemplate.CostType;

    public class WorkflowTests
    {
        [TestFixture]
        public class GetAllStagesShould : WorkflowTestBase
        {
            [Test]
            public async Task ReturnSortedAIPEWorklowStages_when_VideoFullProduction()
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                builder.WithContentType(Constants.ContentType.Video);
                builder.WithBudgetRegion(Constants.BudgetRegion.NorthAmerica);
                builder.WithProductionType(Constants.ProductionType.FullProduction);

                var cost = await CreateCost(builder, AdminUser);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);

                // Assert
                stageModes.Should().HaveCount(7);
                stageModes.Should().ContainKey(CostStages.New.ToString());
                stageModes.Should().ContainKey(CostStages.Aipe.ToString());
                stageModes.Should().ContainKey(CostStages.OriginalEstimate.ToString());
                stageModes.Should().ContainKey(CostStages.OriginalEstimateRevision.ToString());
                stageModes.Should().ContainKey(CostStages.FirstPresentation.ToString());
                stageModes.Should().ContainKey(CostStages.FirstPresentationRevision.ToString());
                stageModes.Should().ContainKey(CostStages.FinalActual.ToString());

                stageModes.Skip(1).First().Key.Should().Be(CostStages.Aipe.ToString());
            }           

            [Test]
            [TestCase(CostType.Buyout, null, Constants.BudgetRegion.Latim, null)]
            [TestCase(CostType.Buyout, null, Constants.BudgetRegion.AsiaPacific, null)]
            [TestCase(CostType.Production, Constants.ContentType.Audio,Constants.BudgetRegion.Latim, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Audio, Constants.BudgetRegion.Europe, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Digital, Constants.BudgetRegion.Japan, null)]
            [TestCase(CostType.Production, Constants.ContentType.Digital, Constants.BudgetRegion.NorthAmerica, null)]
            public async Task SkipFirstPresentationIf_ContractDigitaAudioCosts(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);
                builder.WithApprovalStage(CostStages.FinalActual.ToString());
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);
                // Assert
                stageModes.Should().HaveCount(4);
                stageModes.Should().NotContainKeys(CostStages.FirstPresentation.ToString());
                stageModes[CostStages.New.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimate.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimateRevision.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
            }


            [Test]          
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.Latim, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.Europe, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.Japan, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.China, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.Latim, Constants.ProductionType.FullProduction)]
            public async Task SkipFirstPresentation_For_StillImage_If_(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);           
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);
                // Assert
                stageModes.Should().HaveCount(4);
                stageModes.Should().NotContainKeys(CostStages.FirstPresentation.ToString());
                stageModes[CostStages.New.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimate.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimateRevision.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
            }

            [Test]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.AsiaPacific, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.Europe, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.IndiaAndMiddleEastAfrica, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Photography, Constants.BudgetRegion.NorthAmerica, Constants.ProductionType.FullProduction)]
            public async Task DontSkipFirstPresentation_For_StillImage_If_(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);
                //   builder.WithApprovalStage(CostStages.FinalActual.ToString());
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);

                // Assert
                stageModes.Should().HaveCount(7);
                stageModes.Should().ContainKeys(CostStages.FirstPresentation.ToString());
            }

            [Test]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.AsiaPacific, Constants.ProductionType.CgiAnimation)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.China, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Japan, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Latim, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Latim, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Europe, Constants.ProductionType.PostProductionOnly)]
            public async Task SkipFirstPresentation_For_Video_No_DPV_If_(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);
                //   builder.WithApprovalStage(CostStages.FinalActual.ToString());
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);

                // Assert
                stageModes.Should().HaveCount(4);
                stageModes.Should().NotContainKeys(CostStages.FirstPresentation.ToString());
                stageModes[CostStages.New.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimate.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimateRevision.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
            }

            [Test]            
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.AsiaPacific, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Europe, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.IndiaAndMiddleEastAfrica, Constants.ProductionType.FullProduction)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.NorthAmerica, Constants.ProductionType.FullProduction)]
            public async Task DontSkipFirstPresentation_For_Video_No_DPV_If_(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);                
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);

                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);

                // Assert              
                stageModes.Should().HaveCount(7);
                stageModes.Should().ContainKeys(CostStages.FirstPresentation.ToString());
            }

            [Test]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.AsiaPacific, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Europe, Constants.ProductionType.PostProductionOnly)]
            [TestCase(CostType.Production, Constants.ContentType.Video, Constants.BudgetRegion.Japan, Constants.ProductionType.FullProduction)]
            public async Task SkipFirstPresentation_For_Video_DPV_If_(CostType costType, string contentType, string region, string productionType)
            {
                // Arrange
                var builder = new CreateCostModelBuilder();
                if (costType != CostType.Buyout)
                {
                    builder.WithContentType(contentType);
                }
                builder.WithBudgetRegion(region);                
                builder.WithProductionType(productionType);
                var cost = await CreateCost(builder, AdminUser, costType);              
                // Act
                var url = $"{CostWorkflowUrl(cost.Id)}/stages";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var stageModes = Deserialize<Dictionary<string, StageModel>>(response, HttpStatusCode.OK);

                // Assert               
                stageModes.Should().HaveCount(4);
                stageModes.Should().NotContainKeys(CostStages.FirstPresentation.ToString());
                stageModes[CostStages.New.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimate.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
                stageModes[CostStages.OriginalEstimateRevision.ToString()].Transitions.Should().ContainKey(CostStages.FinalActual.ToString());
            }
        }

        [TestFixture]
        public class GetActionsShould : WorkflowTestBase
        {
            [Test]
            public async Task ReturnCollectionOfAwailableActions()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
            }

            [Test]
            public async Task ReturnSubmitAction_WhenStatusIsDraftAndOwner()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Submit.ToString());
                actionModels[CostAction.Submit.ToString()].Key.Should().Be(CostAction.Submit);
            }

            [Test]
            public async Task ReturnNotReturnAnyAction_WhenStatusIsDraftAndNotOwner()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video)
                .WithProductionType(Constants.ProductionType.PostProductionOnly)
                .WithBudgetRegion(Constants.BudgetRegion.AsiaPacific)
                .WithInitialBudget(12312312312);

                var cost = await CreateCost(builder, AdminUser);
                await AddApprover(cost, AdminUser, Approver);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(Approver));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().BeEmpty();
            }

            [Test]
            public async Task ReturnApproveReject_WhenStatusIsPendingIPMApprovalAndBelowAuthLimit()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video)
                .WithProductionType(Constants.ProductionType.PostProductionOnly)
                .WithBudgetRegion(Constants.BudgetRegion.AsiaPacific)
                .WithInitialBudget(12312);

                await SetApproverAuthLimit(1000000);

                var cost = await CreateCost(builder, AdminUser);

                var latestStage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, AdminUser);

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                    {
                        new CostLineItemModel
                        {
                            Name = "Item1",
                            Value = 10000,
                            LocalCurrencyId = currency.Id,
                            TemplateSectionId = lineItemSection.Id
                        }
                    },
                    AdminUser);

                await AddApprover(cost, AdminUser, Approver);
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(Approver));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().HaveCount(2);
                actionModels.Should().ContainKey(CostAction.Approve.ToString());
                actionModels.Should().ContainKey(CostAction.Reject.ToString());
            }

            [Test]
            public async Task ReturnReject_WhenStatusIsPendingIPMApprovalAndAboveAuthLimit()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video)
                .WithProductionType(Constants.ProductionType.PostProductionOnly)
                .WithBudgetRegion(Constants.BudgetRegion.AsiaPacific)
                .WithApprovalStage(CostStages.FinalActual.ToString())
                .WithInitialBudget(12312);

                var cost = await CreateCost(builder, AdminUser);

                var latestStage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, AdminUser);

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                    {
                        new CostLineItemModel
                        {
                            Name = "Item1",
                            Value = 10000,
                            LocalCurrencyId = currency.Id,
                            TemplateSectionId = lineItemSection.Id
                        }
                    },
                    AdminUser);

                await AddApprover(cost, AdminUser, Approver);
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);

                await SetApproverAuthLimit(9999);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(Approver));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().HaveCount(1);
                actionModels.Should().ContainKey(CostAction.Reject.ToString());
            }

            [Test]
            public async Task ReturnDeleteAction_WhenOwner()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Delete.ToString());
            }


            [Test]
            public async Task ReturnNextStageAndCreateRevisionActions_WhenOwnerAndStatusApprovedAndNotFinalActual()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.NextStage.ToString());
                actionModels.Should().ContainKey(CostAction.CreateRevision.ToString());
            }

            [Test]
            public async Task ReturnCreateRevision_WhenOwnerButAlreadyRevision()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video)
                .WithApprovalStage(CostStages.OriginalEstimate.ToString());

                var cost = await CreateCost(builder, AdminUser);
                // Approve first stage
                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Create revision stage of current stage
                await ExecuteAction(cost.Id, CostAction.CreateRevision, AdminUser);

                // Approve revision stage
                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.CreateRevision.ToString());
                actionModels.Should().ContainKey(CostAction.NextStage.ToString());
            }

            [Test]
            public async Task NotReturn_NextStage_CreateRevision_ApproveReopen_RejectReopen_WhenFinalActualAndStatusIsDraft()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);
                CostAction[] invalidActions =
                {
                    CostAction.NextStage,
                    CostAction.CreateRevision,
                    CostAction.ApproveReopen,
                    CostAction.RejectReopen
                };

                var cost = await CreateCost(builder, AdminUser);
                var latestStage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, AdminUser);

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 412842,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                }, AdminUser);

                var stagesResponse = await Browser.Get($"{CostWorkflowUrl(cost.Id)}/stages", w => w.User(AdminUser));
                var stages = Deserialize<Dictionary<string, StageModel>>(stagesResponse, HttpStatusCode.OK);

                // Move to last stage ( 'FinalActual' )
                for (var i = 1; i < stages.Values.Count(s => !s.IsRequired); ++i)
                {
                    await SetCostStatus(cost, CostStageRevisionStatus.Approved);
                    await ExecuteAction(cost.Id, CostAction.NextStage, AdminUser);
                }

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().NotContainKeys(invalidActions.Select(a => a.ToString()));
            }

            [Test]
            public async Task ReturnReopenAction_WhenOwnerStatusIsRejected()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                await SetCostStatus(cost, CostStageRevisionStatus.Rejected);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Reopen.ToString());
                actionModels.Should().NotContainKey(CostAction.CreateRevision.ToString());
                actionModels.Should().NotContainKey(CostAction.NextStage.ToString());
            }

            [Test]
            public async Task ReturnRecallAction_WhenOwnerAndPendingTechnicalApproval()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                await SetCostStatus(cost, CostStageRevisionStatus.PendingTechnicalApproval);
                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Recall.ToString());
            }

            [Test]
            public async Task ReturnRecallAction_WhenOwnerAndPendingBrandApproval_AndNoPONumber_ShouldNotBeAvailable()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                await SetCostStatus(cost, CostStageRevisionStatus.PendingBrandApproval);
                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().NotContainKeys(CostAction.Recall.ToString());
            }

            [Test]
            public async Task ReturnEditValueReporting_WhenIPMApproverAndPendingBrandApproval()
            {
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                var latestStage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, AdminUser);

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 412842,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                }, AdminUser);
                var approver = await CreateUser($"{Guid.NewGuid()}approver_ValueReporting", Roles.CostApprover);

                await AddApprover(cost, AdminUser, approver);
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);
                await ExecuteAction(cost.Id, CostAction.Approve, approver);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(approver));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assert
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.EditValueReporting.ToString());
            }

            [Test]
            public async Task ReturnEditValueReporting_WhenCostIsAtFinalActualApproved()
            {
                var builder = new CreateCostModelBuilder()
                .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);
                var lateststage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, lateststage.Id, AdminUser);

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, lateststage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 1111111,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                }, AdminUser);

                var approver = await CreateUser($"{Guid.NewGuid()}approver_valueReporting", Roles.CostApprover);
                await AddApprover(cost, AdminUser, approver);
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);
                await ExecuteAction(cost.Id, CostAction.Approve, approver);

                var stageResponse = await Browser.Get($"{CostWorkflowUrl(cost.Id)}/stages", w => w.User(AdminUser));
                var stages = Deserialize<Dictionary<string, StageModel>>(stageResponse, HttpStatusCode.OK);

                for(var i = 1; i< stages.Values.Count(s=> !s.IsRequired); ++i)
                {
                    await SetCostStatus(cost, CostStageRevisionStatus.Approved);
                    await ExecuteAction(cost.Id, CostAction.NextStage, AdminUser);
                }
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);
                await ExecuteAction(cost.Id, CostAction.Approve, approver);

                cost = EFContext.Cost.First(c => c.Id == cost.Id);
                EFContext.Entry(cost).Reload();

                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(approver));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                actionModels.Should().ContainKey(CostAction.EditValueReporting.ToString());
            }


            [Test]
            public async Task ReturnCancellAction_WhenOwnerAndApproved()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assertcos
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Cancel.ToString());
            }

            [Test]
            public async Task ReturnCancellAction_WhenOwnerAndPendingBrandApproval_AndHasPONumber_AndHasExternalIntegration()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.PendingBrandApproval);
                await AddPONumber(cost, "test PO number");
                
                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assertcos
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Cancel.ToString());
            }

            [Test]
            public async Task ReturnCancellAction_WhenOwner_AndDraft_AndWasSubmittedForApprovalPreviously()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video);

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.Approved);
                await AddPONumber(cost);
                await ExecuteAction(cost.Id, CostAction.NextStage, AdminUser);

                // Act
                var url = $"{CostRevisionWorkflowUrl(cost.Id, GetLatestRevisionId(cost.Id))}/actions";
                var response = await Browser.Get(url, w => w.User(AdminUser));
                var actionModels = Deserialize<Dictionary<string, ActionModel>>(response, HttpStatusCode.OK);

                // Assertcos
                actionModels.Should().NotBeNull();
                actionModels.Should().ContainKey(CostAction.Cancel.ToString());
            }
        }

        [TestFixture]
        public class ExecuteActionShould : WorkflowTestBase
        {
            [Test]
            public async Task SubmitCost_when_submitRequestedAndEnoughPermissions()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithApprovalStage(CostStages.OriginalEstimate.ToString())
                    .WithProductionType(Constants.ProductionType.PostProductionOnly)
                    .WithBudgetRegion(Constants.BudgetRegion.China)
                    .WithInitialBudget(12312);

                var cost = await CreateCost(builder, AdminUser);
                var dbCost = await EFContext.Cost.Include(c => c.LatestCostStageRevision).FirstAsync(c => c.Id == cost.Id);

                var currency = await GetCurrencyByCode("CAD");
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateExchangeRate(currency, (decimal)1.5);
                await CreateCostLineItems(cost.Id, dbCost.LatestCostStageRevision.CostStageId, dbCost.LatestCostStageRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                       Name = "Item1",
                       Value = 41284,
                       LocalCurrencyId = currency.Id,
                       TemplateSectionId = lineItemSection.Id
                    }
                },
                AdminUser);
                await AddApprover(cost, AdminUser, Approver);

                // Act
                await ExecuteAction(cost.Id, CostAction.Submit, AdminUser);

                // Assert
                var latestStage = await GetCostLatestStage(cost.Id, AdminUser);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, AdminUser);
                latestRevision.Status.Should().Be(CostStageRevisionStatus.PendingTechnicalApproval);
            }

            [Test]
            public async Task MoveCostToNextStage_whenEnoughPermissionsAndAllowed()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithApprovalStage(CostStages.OriginalEstimate.ToString());

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Act
                await ExecuteAction(cost.Id, CostAction.NextStage, AdminUser);

                var browserResponse = await Browser.Get($"/v1/costs/{cost.Id}/stage", w => w.User(AdminUser));
                var stages = Deserialize<CostStageModel[]>(browserResponse, HttpStatusCode.OK);

                // Assert
                stages.Should().NotBeNull();
                stages.Should().HaveCount(2);

                stages[0].Key.Should().Be(CostStages.OriginalEstimate.ToString());
                stages[0].Revisions.Should().HaveCount(1);

                stages[1].Key.Should().Be(CostStages.FirstPresentation.ToString());
                stages[1].Revisions.Should().HaveCount(1);
            }

            [Test]
            public async Task CreateRevisionStageOfCurrentStage_whenEnoughPermissionsAndAllowed()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithApprovalStage(CostStages.OriginalEstimate.ToString());

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.Approved);

                // Act
                await ExecuteAction(cost.Id, CostAction.CreateRevision, AdminUser);

                var browserResponse = await Browser.Get($"/v1/costs/{cost.Id}/stage", w => w.User(AdminUser));
                var stages = Deserialize<CostStageModel[]>(browserResponse, HttpStatusCode.OK);

                // Assert
                stages.Should().NotBeNull();
                stages.Should().HaveCount(2);

                stages[0].Key.Should().Be(CostStages.OriginalEstimate.ToString());
                stages[0].Revisions.Should().HaveCount(1);

                stages[1].Key.Should().Be(CostStages.OriginalEstimateRevision.ToString());
                stages[1].Status.Should().Be(CostStageRevisionStatus.Draft);
                stages[1].Revisions.Should().HaveCount(1);
            }

            [Test]
            public async Task CreateVersionCurrentCostStage()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithApprovalStage(CostStages.OriginalEstimate.ToString());

                var cost = await CreateCost(builder, AdminUser);

                await SetCostStatus(cost, CostStageRevisionStatus.Rejected);

                // Act
                await ExecuteAction(cost.Id, CostAction.Reopen, AdminUser);

                var browserResponse = await Browser.Get($"/v1/costs/{cost.Id}/stage", w => w.User(AdminUser));
                var stages = Deserialize<CostStageModel[]>(browserResponse, HttpStatusCode.OK);

                // Assert
                stages.Should().NotBeNull();
                stages.Should().HaveCount(1);
                stages[0].Key.Should().Be(CostStages.OriginalEstimate.ToString());
                stages[0].Revisions.Should().HaveCount(2);
                stages[0].Revisions[1].Status.Should().Be(CostStageRevisionStatus.Draft);
            }

            [Test]
            public async Task DeleteCosts()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                     .WithContentType(Constants.ContentType.Video);
                var cost = await CreateCost(builder, AdminUser);

                // Act
                await ExecuteAction(cost.Id, CostAction.Delete, AdminUser);

                // Assert
                var browserResponse = await Browser.Get($"/v1/costs/{cost.Id}", w => w.User(AdminUser));
                browserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            [Test]
            public async Task Recall_When_PendingTechnicalApproval()
            {
                // Arrange
                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithProductionType(Constants.ProductionType.PostProductionOnly)
                    .WithBudgetRegion(Constants.BudgetRegion.AsiaPacific)
                    .WithInitialBudget(12312312312);

                var dbCost = await CreateCost(builder, AdminUser);
                await AddApprover(dbCost, AdminUser, Approver);
                await SetCostStatus(dbCost, CostStageRevisionStatus.PendingTechnicalApproval);

                // Act
                await ExecuteAction(dbCost.Id, CostAction.Recall, AdminUser);

                var browserResponse = await Browser.Get($"/v1/costs/{dbCost.Id}", w => w.User(AdminUser));
                var cost = Deserialize<Cost>(browserResponse, HttpStatusCode.OK);

                // Assert
                cost.Should().NotBeNull();
                cost.Status.Should().Be(CostStageRevisionStatus.Recalled);
            }

            [Test]
            public async Task Recall_When_PendingBrandApproval_AndCyclone_AndNorthAmerica()
            {
                // Arrange
                var agencyAbstractType = await CreateAgencyAbstractType(isCyclone: true, agencyName: "Advertiser_AGENCY");
                var owner = await CreateUser($"{Guid.NewGuid()}bob", Roles.CostOwner, agencyAbstractType.ObjectId);

                var builder = new CreateCostModelBuilder()
                    .WithContentType(Constants.ContentType.Video)
                    .WithProductionType(Constants.ProductionType.PostProductionOnly)
                    .WithBudgetRegion(Constants.BudgetRegion.NorthAmerica)
                    .WithInitialBudget(12312312312);

                var dbCost = await CreateCost(builder, owner);
                await AddApprover(dbCost, owner, Approver);
                await SetCostStatus(dbCost, CostStageRevisionStatus.PendingBrandApproval);

                // Act
                await ExecuteAction(dbCost.Id, CostAction.Recall, owner);

                var browserResponse = await Browser.Get($"/v1/costs/{dbCost.Id}", w => w.User(owner));
                var cost = Deserialize<Cost>(browserResponse, HttpStatusCode.OK);

                // Assert
                cost.Should().NotBeNull();
                cost.Status.Should().Be(CostStageRevisionStatus.Recalled);
            }
        }
    }
}
