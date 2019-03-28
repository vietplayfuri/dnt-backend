namespace costs.net.integration.tests.Plugins.PG.PaymentRules
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using dataAccess.Views;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;
    using plugins.PG.Models.Stage;

    public class PaymentRule_FA_Reopen_DecreaseCost_Recalculate : PaymentRuleIntegrationTestBase
    {

        [Test]
        public async Task FinalActual_ReduceCostAmount_RecalculatePaymentAmount()
        {
            const decimal originalCostTotal = 50000m;
            const decimal decreaseCostAmount = -3000m;
            var costUserId = Guid.NewGuid();
            var costStageRevisionPtfp = new CostStageRevision(costUserId)
            {
                CostStage = new CostStage(costUserId)
                {
                    Name = CostStages.FirstPresentation.ToString(),
                    Key = CostStages.FirstPresentation.ToString(),
                    StageOrder = 2
                },
                Name = CostStages.OriginalEstimate.ToString()
            };
            var costStageRevisionPtoe = new CostStageRevision(costUserId)
            {
                CostStage = new CostStage(costUserId)
                {
                    Name = CostStages.OriginalEstimate.ToString(),
                    Key = CostStages.OriginalEstimate.ToString(),
                    StageOrder = 1
                },
                Name = CostStages.OriginalEstimate.ToString()
            };
            var costLineItemsView = new List<CostLineItemView>
            {
                new CostLineItemView
                {
                    Name = "PandGInsurance",
                    TemplateSectionId = Guid.Parse("4e4bc257-fa2c-459d-9080-063c9458d3da"),
                    TemplateSectionName = "otherCosts"
                },
                new CostLineItemView
                {
                    Name = "taxImportation",
                    TemplateSectionId = Guid.Parse("4e4bc257-fa2c-459d-9080-063c9458d3da"),
                    TemplateSectionName = "otherCosts"
                },
                new CostLineItemView
                {
                    Name = "technicalFee",
                    TemplateSectionId = Guid.Parse("4e4bc257-fa2c-459d-9080-063c9458d3da"),
                    TemplateSectionName = "otherCosts"
                },
                new CostLineItemView
                {
                    Name = "foreignExchange",
                    TemplateSectionId = Guid.Parse("4e4bc257-fa2c-459d-9080-063c9458d3da"),
                    TemplateSectionName = "otherCosts"
                },
                new CostLineItemView
                {
                    Name = "agencyArtWorkPacks",
                    TemplateSectionId = Guid.Parse("e4930434-46b0-4ef8-832a-ee8c9a289ffd"),
                    TemplateSectionName = "agencyCosts"
                },
                new CostLineItemView
                {
                    Name = "insurance",
                    TemplateSectionId = Guid.Parse("e4930434-46b0-4ef8-832a-ee8c9a289ffd"),
                    TemplateSectionName = "agencyCosts"
                },
                new CostLineItemView
                {
                    Name = "music",
                    TemplateSectionId = Guid.Parse("e4930434-46b0-4ef8-832a-ee8c9a289ffd"),
                    TemplateSectionName = "agencyCosts"
                },
                new CostLineItemView
                {
                    Name = "agencyTravel",
                    TemplateSectionId = Guid.Parse("e4930434-46b0-4ef8-832a-ee8c9a289ffd"),
                    TemplateSectionName = "agencyCosts"
                },
                new CostLineItemView
                {
                    Name = "casting",
                    TemplateSectionId = Guid.Parse("e4930434-46b0-4ef8-832a-ee8c9a289ffd"),
                    TemplateSectionName = "agencyCosts"
                },
                new CostLineItemView
                {
                    Name = "audioFinalization",
                    TemplateSectionId = Guid.Parse("e6ad09f4-d8db-476b-ba21-d9ab529d845f"),
                    TemplateSectionName = "postProduction"
                },
                new CostLineItemView
                {
                    Name = "offlineEdits",
                    TemplateSectionId = Guid.Parse("e6ad09f4-d8db-476b-ba21-d9ab529d845f"),
                    TemplateSectionName = "postProduction"
                },
                new CostLineItemView
                {
                    Name = "onlineVideoFinalization",
                    TemplateSectionId = Guid.Parse("e6ad09f4-d8db-476b-ba21-d9ab529d845f"),
                    TemplateSectionName = "postProduction"
                },
                new CostLineItemView
                {
                    Name = "pphmarkup",
                    TemplateSectionId = Guid.Parse("e6ad09f4-d8db-476b-ba21-d9ab529d845f"),
                    TemplateSectionName = "postProduction"
                },
                new CostLineItemView
                {
                    Name = "cgiAnimation",
                    TemplateSectionId = Guid.Parse("e6ad09f4-d8db-476b-ba21-d9ab529d845f"),
                    TemplateSectionName = "postProduction"
                },
                new CostLineItemView
                {
                    Name = "travel",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "phmarkup",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "directorscut",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "transportcatering",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "crew",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "productionInsuranceNotCovered",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "directorsFees",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "talentfees",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production",
                    ValueInDefaultCurrency = 47000m,
                    ValueInLocalCurrency = 47000m
                },
                new CostLineItemView
                {
                    Name = "artDepartment",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "equipment",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                },
                new CostLineItemView
                {
                    Name = "preProduction",
                    TemplateSectionId = Guid.Parse("24e8b92d-2203-4326-8cc0-b4f41415d09e"),
                    TemplateSectionName = "production"
                }
            };
            var csrcli = new List<CostStageRevisionPaymentTotal>
            {
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.InsuranceTotal,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Other,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.PostProduction,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Production,
                    LineItemFullCost = originalCostTotal,
                    LineItemRemainingCost = originalCostTotal,
                    LineItemTotalCalculatedValue = originalCostTotal/2,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TargetBudgetTotal,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TechnicalFee,
                    CostStageRevision = costStageRevisionPtoe
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.CostTotal,
                    CostStageRevision = costStageRevisionPtoe,
                    LineItemFullCost = originalCostTotal,
                    LineItemRemainingCost = 0m,
                    LineItemTotalCalculatedValue = originalCostTotal/2
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.InsuranceTotal,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Other,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.PostProduction,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Production,
                    LineItemFullCost = originalCostTotal,
                    LineItemRemainingCost = originalCostTotal/2,
                    LineItemTotalCalculatedValue = originalCostTotal/2,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TargetBudgetTotal,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TechnicalFee,
                    CostStageRevision = costStageRevisionPtfp
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.CostTotal,
                    LineItemFullCost = originalCostTotal,
                    LineItemRemainingCost = 0m,
                    LineItemTotalCalculatedValue = originalCostTotal/2,
                    CostStageRevision = costStageRevisionPtfp
                }
            };
            InitData(false,CostStages.FinalActual.ToString(),"AAK (Asia)", costLineItemsView, "60000", csrcli, Constants.ContentType.Video, Constants.ProductionType.FullProduction, CostType.Production, null);
        
            // Act
            var result = await _pgPaymentService.GetPaymentAmount(_costStageRevisionId, true);

            // Assert
            result.CostCarryOverAmount.Should().Be(decreaseCostAmount);
            result.StageName.Should().Be(CostStages.FinalActual.ToString());
        }

        [Test]
        public async Task PaymentRule_AIPE_AAK_D_FP_stage_with_previous_payment()
        {
            // Arrange
            InitData(
                isAipe: true,
                stageKey: CostStages.FirstPresentation.ToString(),
                budgetRegion: Constants.BudgetRegion.AsiaPacific,
                targetBudget: "5555",
                items: new List<CostLineItemView>()
                {
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.Production, ValueInDefaultCurrency = 400 },//(incl. insurance)
                    new CostLineItemView() { Name = Constants.CostSection.ProductionInsurance, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.TechnicalFee, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { TemplateSectionName = Constants.CostSection.PostProduction, ValueInDefaultCurrency = 200 },
                    new CostLineItemView() { Name = Constants.CostSection.Other, ValueInDefaultCurrency = 200 }
                },
                payments: new List<CostStageRevisionPaymentTotal>()
                {
                    new CostStageRevisionPaymentTotal() { LineItemTotalType = Constants.CostSection.Production, LineItemTotalCalculatedValue = 100 }
                },
                contentType: Constants.ContentType.Photography
            );

            // Act
            var receipt = await _purchaseOrderService.GetPurchaseOrder(_costApprovedEvent);

            // Assert
            // 100% of Production
            // 100% of Insurance
            // 100% of Technical Fee
            // 50% of Post Production
            // -100 
            receipt.PaymentAmount.ShouldBeEquivalentTo(600);
        }

      
    }
}
