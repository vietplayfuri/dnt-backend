namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.Costs;
    using core.Models.Response;
    using core.Models.Workflow;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;
    using E = dataAccess.Entity;

    public class CostApprovalsTests
    {
        public abstract class CostApprovalsTest : BaseCostIntegrationTest
        { }

        // warn: assumes PG
        [TestFixture]
        public class UpdateCostShould : CostApprovalsTest
        {
            [TestCase("Cost1Video", Constants.ContentType.Video, Constants.ProductionType.PostProductionOnly, new string[0])]
            public async Task AddExpectedApprovals(string costTitle, string contentType, string productionType, string[] expectedApprovals)
            {
                await CreateCostEntity(User);

                var approverName = $"{Guid.NewGuid()}approver";
                var approver = await CreateUser(approverName, Roles.CostApprover);

                var cost = Deserialize<E.Cost>(await CreateCost(User, new CreateCostModel
                {
                    TemplateId = CostTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "title", costTitle },
                            { "agencyProducer",new [] {"bob" } },
                            { "description", "123" },
                            { "budgetRegion", new AbstractTypeValue {Key =  Constants.BudgetRegion.AsiaPacific }},
                            { "organisation", new core.Builders.DictionaryValue { Key = "SMO" } },
                            { "smo", "OTHER EUROPE" },
                            { "contentType", new { id = Guid.NewGuid(), key = contentType, value = contentType } },
                            { "campaign", "test" },
                            { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                            { "agencyTrackingNumber", "122" },
                            { "agencyCurrency", "GBP" },
                            { "isAIPE", "true" },
                            { "projectId", "123456789" },
                            { "costNumber", "AC123123456789" },
                            { "approvalStage",  CostStages.Aipe.ToString() },
                            { "initialBudget", 123123123 },
                            { "targetBudget", 453453454 },
                            { "agency", new PgStageDetailsForm.AbstractTypeAgency
                                {
                                    Id = User.AgencyId,
                                    AbstractTypeId = User.Agency.AbstractTypes.FirstOrDefault().Id,
                                    Name = User.Agency.Name
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                var updateModel = new UpdateCostModel
                {
                    ProductionDetails = new ProductionDetail
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "type", "AIPE" },
                            { "talentCompanies", new[] { "TalentCo1", "TalentCo2" } }
                        }
                    }
                };

                var updateResponse = Deserialize<OperationResponse>(await Browser.Put($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                }), HttpStatusCode.OK);

                // this should create a default currency
                await GetDefaultCurrency();

                updateResponse.Success.Should().BeTrue();

                // let's move to next stage
                await SetCostStatus(cost, E.CostStageRevisionStatus.Approved);
                await ExecuteActionAndValidateResponse(cost.Id, CostAction.NextStage, User);

                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                // latest revision id is not really latest revision any more - it's the previous one (AIPE)

                var currency = await GetDefaultCurrency();
                var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == Constants.CostSection.Production);
                await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 4128423942,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                },
                User);

                var approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);

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
                                Email = $"{approverName}@adstream.com"
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

                var approvalsResponse = Deserialize<ApprovalModel[]>(
                    await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK);

                approvalsResponse.Should().HaveCount(2);
                var ipmApproval = approvalsResponse.First(a => a.Type == E.ApprovalType.IPM);
                ipmApproval.ApprovalMembers.Should().HaveCount(1);
                ipmApproval.ApprovalMembers[0].Email.Should().Be($"{approverName}@adstream.com");
            }

            [TestCase("Cost2Video", Constants.ContentType.Video, Constants.ProductionType.FullProduction, new string[0])]
            public async Task AddExpectedApprovalsAIPE(string costTitle, string contentType, string productionType, string[] expectedApprovals)
            {
                await CreateCostEntity(User);

                var approverName = $"{Guid.NewGuid()}Brandapprover";
                var approver = await CreateUser(approverName, Roles.CostApprover, null, plugins.Constants.BusinessRole.BrandManager);

                var cost = Deserialize<E.Cost>(await CreateCost(User, new CreateCostModel
                {
                    TemplateId = CostTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "title", costTitle },
                            { "agencyProducer",new []{"bob" } },
                            { "description", "123" },
                            { "budgetRegion", new AbstractTypeValue { Key = Constants.BudgetRegion.AsiaPacific }},
                            { "organisation", new core.Builders.DictionaryValue { Key = "SMO" } },
                            { "smo", "OTHER EUROPE" },
                            { "contentType", new { id = Guid.NewGuid(), key = contentType, value = contentType } },
                            { "campaign", "test" },
                            { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                            { "agencyTrackingNumber", "122" },
                            { "agencyCurrency", "GBP" },
                            { "isAIPE", "true" },
                            { "projectId", "123456789" },
                            { "costNumber", "AC123123456789" },
                            { "approvalStage",  CostStages.Aipe.ToString() },
                            { "initialBudget", 123123123 },
                            { "targetBudget", 453453454 },
                            { "agency", new PgStageDetailsForm.AbstractTypeAgency
                                {
                                    Id = User.AgencyId,
                                    AbstractTypeId = User.Agency.AbstractTypes.FirstOrDefault().Id,
                                    Name = User.Agency.Name
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                var updateModel = new UpdateCostModel
                {
                    ProductionDetails = new ProductionDetail
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "type", "AIPE" },
                            { "talentCompanies", new[] { "TalentCo1", "TalentCo2" } }
                        }
                    }
                };

                var updateResponse = Deserialize<OperationResponse>(await Browser.Put($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                }), HttpStatusCode.OK);

                updateResponse.Success.Should().BeTrue();
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);

                var approvals = new[]
                {
                    new ApprovalModel
                    {
                        Status = E.ApprovalStatus.New,
                        Type = E.ApprovalType.Brand,
                        ValidBusinessRoles = new[] { "s" },
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = approver.Id,
                                Email = $"{approverName}@adstream.com"
                            }
                        }
                    }
                };

                await Browser.Post(approvalsUrl, r =>
                {
                    r.User(User);
                    r.JsonBody(approvals);
                });

                var approvalsResponse = Deserialize<ApprovalModel[]>(
                    await Browser.Get(approvalsUrl, w => w.User(User)), HttpStatusCode.OK);

                approvalsResponse.Should().HaveCount(1);
                var brandApproval = approvalsResponse.First(a => a.Type == E.ApprovalType.Brand);
                brandApproval.ApprovalMembers.Should().HaveCount(1);
                brandApproval.ApprovalMembers[0].Email.Should().Be($"{approverName}@adstream.com");
            }
        }

        [TestFixture]
        public class GetApprovalsShould : CostApprovalsTest
        {
            [Test]
            public async Task ReturnOnlyNotHiddenAprrovalMembers()
            {
                // Arrange
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                // Brand approval
                var approvalBrand = new E.Approval(User.Id)
                {
                    Type = E.ApprovalType.Brand,
                    CostStageRevisionId = latestRevision.Id,
                    Status = E.ApprovalStatus.New,
                    ApprovalMembers = new List<E.ApprovalMember>
                    {
                        new E.ApprovalMember(User.Id)
                        {
                            IsExternal = true,
                            MemberId = User.Id
                        }
                    }
                };
                EFContext.Approval.Add(approvalBrand);

                // IPM approval
                var approvalIpm = new E.Approval(User.Id)
                {
                    Type = E.ApprovalType.IPM,
                    CostStageRevisionId = latestRevision.Id,
                    Status = E.ApprovalStatus.New,
                    ApprovalMembers = new List<E.ApprovalMember>
                    {
                        new E.ApprovalMember(User.Id)
                        {
                            IsExternal = false,
                            MemberId = User.Id
                        }
                    }
                };
                EFContext.Approval.Add(approvalIpm);

                await EFContext.SaveChangesAsync();

                // Act
                var url = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);

                var approvalsResponse = await Browser.Get(url, w => w.User(User));
                var approvals = Deserialize<ApprovalModel[]>(approvalsResponse, HttpStatusCode.OK);

                // Assert
                approvals.Should().HaveCount(2);
                approvals.First(a => a.Type == E.ApprovalType.IPM).ApprovalMembers.Should().HaveCount(1);
                approvals.First(a => a.Type == E.ApprovalType.Brand).ApprovalMembers.Should().HaveCount(0);
            }
        }
    }
}