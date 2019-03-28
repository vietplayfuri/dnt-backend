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
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using plugins.PG.Form;
    using dataAccess.Entity;
    using CostType = core.Models.CostTemplate.CostType;

    public class CostTests
    {
        public abstract class CostTest : BaseCostIntegrationTest
        {
        }

        [TestFixture]
        public class CreateCostShould : CostTest
        {
            [Test]
            public async Task CreateCost()
            {
                var cost = await CreateCostEntity(User);
                cost.Id.Should().NotBe(Guid.Empty);
            }

            [Test]
            public async Task CreateCost_MutipleUsersInParallel()
            {
                // Arrange
                const int usersCount = 100;
                const int costsCount = 5;
                var users = new List<CostUser>();
                for (var i = 0; i < usersCount; ++i)
                {
                    users.Add(await CreateUser($"{Guid.NewGuid()}bob_{i}", Roles.AgencyAdmin));
                }

                // Act
                var costs = new List<Cost>();
                for (var i = 0; i < costsCount; ++i)
                {
                    var cost = await CreateCostEntity(User);
                    costs.Add(cost);
                }

                // Assert
                costs.Should().HaveCount(costsCount);
            }

            [Test]
            public async Task CreateBuyoutCost()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
                var userAgency = await EFContext.AbstractType
                    .Include(at => at.Agency)
                    .FirstOrDefaultAsync(at => at.ObjectId == user.AgencyId);

                var costTemplate = await CreateTemplate(user, CostType.Buyout);
                var cost = Deserialize<Cost>(await CreateCost(user, new CreateCostModel
                {
                    TemplateId = costTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "budgetRegion", new AbstractTypeValue { Key = "AAK (Asia)" }},
                            { "targetBudget", "<10000" },
                            { "projectId", "123456789" },
                            { "contentType" , new { id= Guid.NewGuid(), value="Photography"} },
                            { "approvalStage", "OriginalEstimate" },
                            { "agency", new PgStageDetailsForm.AbstractTypeAgency
                                {
                                    Id = userAgency.ObjectId,
                                    AbstractTypeId = userAgency.Id,
                                    Name = userAgency.Agency.Name
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                cost.CostType.Should().Be(dataAccess.Entity.CostType.Buyout);
                cost.CostNumber.Should().Contain("AdCostNumberU");
            }

            [Test]
            public async Task CreateVideoCost()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
                var userAgency = await EFContext.AbstractType
                    .Include(at => at.Agency)
                    .FirstOrDefaultAsync(at => at.ObjectId == user.AgencyId);

                var costTemplate = await CreateTemplate(user, CostType.Production);
                var cost = Deserialize<Cost>(await CreateCost(user, new CreateCostModel
                {
                    TemplateId = costTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "budgetRegion", new AbstractTypeValue { Key = "AAK (Asia)" }},
                            { "targetBudget", "<10000" },
                            { "projectId", "123456789" },
                            { "contentType" , new { id= Guid.NewGuid(), value="Video"} },
                            { "approvalStage", "OriginalEstimate" },
                            { "agency", new PgStageDetailsForm.AbstractTypeAgency
                                {
                                    Id = userAgency.ObjectId,
                                    AbstractTypeId = userAgency.Id,
                                    Name = userAgency.Agency.Name
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                cost.CostType.Should().Be(dataAccess.Entity.CostType.Production);
                cost.CostNumber.Should().Contain("AdCostNumberV");
            }
                
            [Test]
            public async Task CreateTraffickingCost()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
                var userAgency = await EFContext.AbstractType
                    .Include(at => at.Agency)
                    .FirstOrDefaultAsync(at => at.ObjectId == user.AgencyId);

                var costTemplate = await CreateTemplate(user, CostType.Trafficking);
                var cost = Deserialize<Cost>(await CreateCost(user, new CreateCostModel
                {
                    TemplateId = costTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "budgetRegion", new AbstractTypeValue { Key = "AAK (Asia)" }},
                            { "targetBudget", "<10000" },
                            { "projectId", "123456789" },
                            { "approvalStage", "OriginalEstimate" },
                            { "agency", new PgStageDetailsForm.AbstractTypeAgency
                                {
                                    Id = userAgency.ObjectId,
                                    AbstractTypeId = userAgency.Id,
                                    Name = userAgency.Agency.Name
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                cost.CostType.Should().Be(dataAccess.Entity.CostType.Trafficking);
                cost.CostNumber.Should().Contain("AdCostNumberT");
            }
        }

        [TestFixture]
        public class UpdateCostShould : CostTest
        {
            [Test]
            public async Task UpdateWithProductionDetails()
            {
                var result = await CreateCostEntity(User);

                var updateModel = new UpdateCostModel
                {
                    ProductionDetails = new ProductionDetail
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "type", "AIPE" },
                            { "Prop", new { prop = "Test prop" } }
                        }
                    }
                };

                var updatedResult = await Browser.Put($"/v1/costs/{result.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                var updateResponse = Deserialize<OperationResponse>(updatedResult, HttpStatusCode.OK);

                updateResponse.Success.Should().BeTrue();
            }
        }

        [TestFixture]
        public class UpdateCostFormShould : CostTest
        {
            [Test]
            public async Task UpdateCostForm()
            {
                var result = await CreateCostEntity(User);

                var updateModel = new UpdateCostFormModel
                {
                    FormDefinitionId = CostTemplate.GetLatestVersion().Forms.First().Id,
                    CostFormDetails = new CostFormDetailsModel
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            ["testFieldOne"] = "test",
                            ["section"] = new { sectionOne = "test" },
                            ["name"] = "name",
                        }
                    }
                };

                var updatedResult = await Browser.Patch($"/v1/costs/{result.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                var updateResponse = Deserialize<OperationResponse>(updatedResult, HttpStatusCode.OK);

                updateResponse.Success.Should().BeTrue();
            }
        }
    }
}
