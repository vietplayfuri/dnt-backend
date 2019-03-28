namespace costs.net.plugins.tests.PG.Builders.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Models.Rule;
    using core.Models.Workflow;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models.Rules;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public class PgCostBuilder_CreateCostTests : PgCostBuilderTests
    {
        [Test]
        [TestCase(true, Constants.BudgetRegion.NorthAmerica, false)]
        [TestCase(true, Constants.BudgetRegion.China, true)]
        [TestCase(false, Constants.BudgetRegion.NorthAmerica, true)]
        [TestCase(false, Constants.BudgetRegion.AsiaPacific, true)]
        public async Task CreateCost_whenCyclonAgencyAnd_shouldSetIsExternalPurchaseEnabledToFalse(bool isCycloneAgency, string budgetRegion, bool isExternalPurchasesEnabled)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var agencyId = Guid.NewGuid();
            var agencyAbstractTypeId = Guid.NewGuid();
            var projectId = "Gdam Porject Id";
            var agency = new Agency
            {
                Id = agencyId,
                Name = "Name",
                Labels = isCycloneAgency ? new[]
                    {
                        Constants.Agency.CycloneLabel, Constants.Agency.PAndGLabel, $"{core.Constants.BusinessUnit.CostModulePrimaryLabelPrefix}P&G"
                    } :
                    new[]
                    {
                        Constants.Agency.PAndGLabel, $"{core.Constants.BusinessUnit.CostModulePrimaryLabelPrefix}P&G"
                    },
                AbstractTypes = new List<AbstractType>
                {
                    new AbstractType
                    {
                        Id = Guid.NewGuid(),
                        ObjectId = agencyId,
                    }
                }
            };
            agency.AbstractTypes.First().Agency = agency;
            var costUser = new CostUser
            {
                Id = userId,
                Agency = agency
            };
            var abstractTypeAgency = new AbstractType
            {
                Id = agencyAbstractTypeId,
                Agency = agency,
                ObjectId = agencyId,
                Parent = new AbstractType
                {
                    Agency = agency
                }
            };

            EFContext.AbstractType.Add(abstractTypeAgency);
            EFContext.CostUser.Add(costUser);
            EFContext.SaveChanges();

            var costTemplateVersion = new CostTemplateVersion
            {
                Id = Guid.NewGuid(),
                CostTemplate = new CostTemplate
                {
                    Id = Guid.NewGuid(),
                    CostType = CostType.Production
                }
            };
            CostTemplateVersionServiceMock.Setup(ctv => ctv.GetLatestTemplateVersion(It.Is<Guid>(id => id == costTemplateVersion.CostTemplate.Id)))
                    .ReturnsAsync(costTemplateVersion);

            var stageDetailsForm = new PgStageDetailsForm
            {
                ProjectId = projectId,
                BudgetRegion = new AbstractTypeValue
                {
                    Key = budgetRegion
                },
                Agency = new PgStageDetailsForm.AbstractTypeAgency
                {
                    Id = abstractTypeAgency.ObjectId,
                    AbstractTypeId = abstractTypeAgency.Id,
                }
            };
            ProjectServiceMock.Setup(p => p.GetByGadmid(projectId)).ReturnsAsync(new Project
            {
                GdamProjectId = projectId,
                AgencyId = agencyId
            });
            var stageDetails = new StageDetails
            {
                Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(stageDetailsForm))
            };

            var createCostModel = new CreateCostModel
            {
                TemplateId = costTemplateVersion.CostTemplate.Id,
                StageDetails = stageDetails
            };

            PgStageBuilderMock.Setup(sb => sb.GetStages(It.IsAny<PgStageRule>(), null))
                .ReturnsAsync(new Dictionary<string, StageModel>
                {
                    {
                        CostStages.New.ToString(),
                        new StageModel
                        {
                            Key = CostStages.New.ToString(),
                            Transitions = new Dictionary<string, StageModel> { { CostStages.OriginalEstimate.ToString(), new StageModel() } }
                        }
                    },
                    {
                        CostStages.OriginalEstimate.ToString(),
                        new StageModel { Key = CostStages.OriginalEstimate.ToString() }
                    }
                });
            RuleServiceMock.Setup(r => r.GetCompiledByRuleType<SupportingDocumentRule>(RuleType.SupportingDocument)).ReturnsAsync(new List<CompiledRule<SupportingDocumentRule>>());

            // Act
            var result = await CostBuilder.CreateCost(costUser, createCostModel);

            // Assert
            result.Cost.IsExternalPurchasesEnabled.Should().Be(isExternalPurchasesEnabled);
        }
    }
}
