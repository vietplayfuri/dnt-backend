namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    using plugins.PG.Services.PurchaseOrder;

    [TestFixture]
    public class CommodityTests : PgPurchaseOrderServiceTests
    {
        public class TestParam
        {
            public CostType CostType { get; set; }

            public string ContentType { get; set; }

            public string UsageType { get; set; }

            public string Commodity { get; set; }
        }

        private static readonly List<TestParam> Params = new List<TestParam>
        {
            new TestParam
            {
                CostType = CostType.Production,
                ContentType = Constants.ContentType.Video,
                Commodity = PgPurchaseOrderService.CommodityConstants.VideoProduction
            },
            new TestParam
            {
                CostType = CostType.Production,
                ContentType = Constants.ContentType.Digital,
                Commodity = PgPurchaseOrderService.CommodityConstants.VideoProduction
            },
            new TestParam
            {
                CostType = CostType.Production,
                ContentType = Constants.ContentType.Audio,
                Commodity = PgPurchaseOrderService.CommodityConstants.RadioProduction
            },
            new TestParam
            {
                CostType = CostType.Production,
                ContentType = Constants.ContentType.Photography,
                Commodity = PgPurchaseOrderService.CommodityConstants.PrintAndImageProduction
            },

            new TestParam
            {
                CostType = CostType.Trafficking,
                Commodity = PgPurchaseOrderService.CommodityConstants.MultimediaDistributionAndTraffic
            },

            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Photography,
                Commodity = PgPurchaseOrderService.CommodityConstants.Music
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Music,
                Commodity = PgPurchaseOrderService.CommodityConstants.Music
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.VoiceOver,
                Commodity = PgPurchaseOrderService.CommodityConstants.Music
            },

            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.BrandResidual,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.CountryAiringRights,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Ilustrator,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Footage,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Actor,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Film,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Organization,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Athletes,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Model,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            },
            new TestParam
            {
                CostType = CostType.Buyout,
                UsageType = Constants.UsageType.Celebrity,
                Commodity = PgPurchaseOrderService.CommodityConstants.TalentAndCelebrity
            }
        };

        [Test]
        [TestCaseSource(nameof(Params))]
        public async Task Commodity_dependingOnContentType_shouldReturnCorrectCommodity(TestParam param)
        {
            // Arrange          
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.Draft);
            var costId = costSubmitted.AggregateId;
            var stageDetails = new Dictionary<string, dynamic>
            {
                { "contentType", new { key = param.ContentType } },
                { "usageType", new { key = param.UsageType } }
            };

            SetupPurchaseOrderView(costId, costType: param.CostType, stageDetails: stageDetails);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.Commodity.Should().Be(param.Commodity);
        }
    }
}