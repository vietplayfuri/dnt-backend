namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.Costs;
    using core.Models.Workflow;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Form;
    using E = dataAccess.Entity;

    public class CreateRevisionTests : BaseCostIntegrationTest
    {
        private const string ApproverName = "approver";

        private async Task<E.Cost> SetupCost(string costTitle, string contentType, string productionType)
        {
            await CreateCostEntity(User);


            var approver = await CreateUser($"{Guid.NewGuid()}{ApproverName}", Roles.CostApprover, businessRoleName: Constants.BusinessRole.Ipm);

            var cost = Deserialize<E.Cost>(await CreateCost(User, new CreateCostModel
            {
                TemplateId = CostTemplate.Id,
                StageDetails = new StageDetails
                {
                    Data = new Dictionary<string, dynamic>
                    {
                        { "title", costTitle },
                        { "agencyProducer", new[] { "bob" } },
                        { "description", "123" },
                        { "budgetRegion", new AbstractTypeValue { Key = Constants.BudgetRegion.China } },
                        { "organisation", new core.Builders.DictionaryValue { Key = "SMO" } },
                        { "smo", "OTHER EUROPE" },
                        { "contentType", new { id = Guid.NewGuid(), key = contentType, value = contentType } },
                        { "campaign", "test" },
                        { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                        { "agencyTrackingNumber", "122" },
                        { "agencyCurrency", "GBP" },

                        { "projectId", "123456789" },
                        { "costNumber", "AC123123456789" },

                        { "initialBudget", 123123123 },
                        { "targetBudget", 453453454 },
                        {
                            "agency", new PgStageDetailsForm.AbstractTypeAgency
                            {
                                Id = User.AgencyId,
                                AbstractTypeId = User.Agency.AbstractTypes.First().Id,
                                Name = User.Agency.Name
                            }
                        }
                    }
                }
            }), HttpStatusCode.Created);


            await GetDefaultCurrency();


            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            // latest revision id is not really latest revision any more - it's the previous one (AIPE)

            var currency = await GetDefaultCurrency();
            var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
            await CreateCostLineItems(cost.Id, stage1.Id, revision1.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 1000000,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                },
                User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);

            var approvals = new[]
            {
                new ApprovalModel
                {
                    Status = E.ApprovalStatus.New,
                    Type = E.ApprovalType.IPM,
                    ValidBusinessRoles = new[] { "s" },
                    ApprovalMembers = new List<ApprovalModel.Member>
                    {
                        new ApprovalModel.Member
                        {
                            Id = approver.Id,
                            Email = $"{ApproverName}@adstream.com"
                        }
                    }
                },
                new ApprovalModel
                {
                    Status = E.ApprovalStatus.New,
                    Type = E.ApprovalType.Brand,
                    ValidBusinessRoles = new[] { "s" },
                    ApprovalMembers = new List<ApprovalModel.Member>()
                }
            };

            await Browser.Post(approvalsUrl, r =>
            {
                r.User(User);
                r.JsonBody(approvals);
            });
            return cost;
        }

        [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
        public async Task Create_NestStage_KeepsOld_Approval(string costTitle, string contentType, string productionType, string[] expectedApprovals)
        {
            var cost = await SetupCost(costTitle, contentType, productionType);

            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);
            var approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying the approval before the creation of next stage
            (approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count).Should().Be(1);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
            // let's move to next stage
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.NextStage, User);

            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            //verifying old revision still has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            (approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count).Should().Be(1);
        }

        [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
        public async Task Create_Revision_KeepsOld_Approval(string costTitle, string contentType, string productionType, string[] expectedApprovals)
        {
            var cost = await SetupCost(costTitle, contentType, productionType);

            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);
            var approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying the approval before the creation of next stage
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
            // let's move to next stage
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.CreateRevision, User);

            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            //verifying old revision still has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);
        }

        [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
        public async Task Create_Version_KeepsOld_Approval(string costTitle, string contentType, string productionType, string[] expectedApprovals)
        {
            //Version is OE ==>OE Revision ==> Revision
            var cost = await SetupCost(costTitle, contentType, productionType);

            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);
            var approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying the approval before the creation of next stage
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
            // let's move to next stage
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.CreateRevision, User);

            var oerStage = await GetCostLatestStage(cost.Id, User);
            var oerRevision = await GetCostLatestRevision(cost.Id, oerStage.Id, User);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);

            // creating a new version
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.CreateRevision, User);

            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);


            approvalsUrl = ApprovalsUrl(cost.Id, oerStage.Id, oerRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying OE Revision has the approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);
        }

        [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
        public async Task FinalActual_Submit_Keeps_Approval(string costTitle, string contentType, string productionType, string[] expectedApprovals)
        {
            //Version is OE ==>OE Revision ==> Revision
            var cost = await SetupCost(costTitle, contentType, productionType);

            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);
            var approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying the approval before the creation of next stage
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
            // let's move to next stage
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.NextStage, User);

            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);


            // submitting the cost
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.Submit, User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);
        }

        [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
        public async Task ApprovalMembers_Are_Copying_From_Previous_Revision_During_Cost_Increase(string costTitle, string contentType, string productionType,
            string[] expectedApprovals)
        {
            //Version is OE ==>OE Revision ==> Revision
            var cost = await SetupCost(costTitle, contentType, productionType);

            var stage1 = await GetCostLatestStage(cost.Id, User);
            var revision1 = await GetCostLatestRevision(cost.Id, stage1.Id, User);

            var approvalsUrl = ApprovalsUrl(cost.Id, stage1.Id, revision1.Id);
            var approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying the approval before the creation of next stage
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.IPM)?.ApprovalMembers.Count.Should().Be(1);

            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
            // let's move to next stage
            await ExecuteActionAndValidateResponse(cost.Id, CostAction.NextStage, User);

            var latestStage = await GetCostLatestStage(cost.Id, User);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.Brand).ShouldBeEquivalentTo(null);

            var currency = await GetDefaultCurrency();
            var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
            await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 10000000,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                },
                User);

            approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);


            approvalsResponse = Deserialize<ApprovalModel[]>(
                await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK).ToList();

            // verifying current revision has approval
            approvalsResponse.FirstOrDefault(a => a.Type == E.ApprovalType.Brand).Should().NotBe(null);
        }

        [Test]
        public async Task MultipleInvokations_Shoudl_Not_Be_Allowed()
        {
            // Arrange
            // Version is OE ==>OE Revision ==> Revision
            var cost = await SetupCost("Multiple Invokations Test", Constants.ContentType.Video, Constants.ProductionType.PostProductionOnly);
            await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);

            // Act
            // Call CreateRevision 2 times without waiting for result
            var results = await Task.WhenAll(
                ExecuteAction(cost.Id, CostAction.CreateRevision, User),
                ExecuteAction(cost.Id, CostAction.CreateRevision, User)
                );

            // Assert
            results.Should().HaveCount(2);
            results.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
            results.Count(r => r.StatusCode == HttpStatusCode.InternalServerError).Should().Be(1, "Trying to Create Revision twice");

            var dbCost = await EFContext.Cost
                .Include(c => c.CostStages)
                .ThenInclude(cs => cs.CostStageRevisions)
                .FirstOrDefaultAsync(c => c.Id == cost.Id);

            await EFContext.Entry(dbCost).ReloadAsync();

            dbCost.CostStages.Should().HaveCount(2);
            dbCost.CostStages.ForEach(cs => cs.CostStageRevisions.Should().HaveCount(1));
        }
    }
}