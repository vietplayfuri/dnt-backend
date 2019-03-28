using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;
using static costs.net.plugins.Constants;
using costs.net.plugins.PG.Models.Stage;
using FluentAssertions;
using costs.net.dataAccess.Entity;
using Moq;
using System;

namespace costs.net.integration.tests.Plugins.PG.SupportingDocumentRules
{
    [TestFixture]
    public class Supplier_Winning_Bid : SupportingDocumentsTestBase
    {
        [Test]
        public async Task SupportingDocumentRule_OE_PostProduction_Except_NorthAmerica_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);            

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)", "P&G Communication Brief" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_FullProduction_Except_NorthAmerica_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);            

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)" , "Approved Creative (storyboard/layout/script)", "P&G Communication Brief"});
        }

        [Test]
        public async Task SupportingDocumentRule_OE_FullProduction_Except_NorthAmerica_Video()
        {
            var stageDetails = BuildStageDetails(ContentType.Video, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);            

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)", "Approved Creative (storyboard/layout/script)" , "P&G Communication Brief" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_FullProduction_Except_NorthAmerica_Digital()
        {
            var stageDetails = BuildStageDetails(ContentType.Digital, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)" , "P&G Communication Brief" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_FullProduction_Except_NorthAmerica_Still()
        {
            var stageDetails = BuildStageDetails(ContentType.Photography, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)" , "P&G Communication Brief" });
        }


        [Test]
        public async Task SupportingDocumentRule_OE_Production_NorthAmerica_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.NorthAmerica, dataAccess.Entity.CostType.Production, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Approved Creative (storyboard/layout/script)", "P&G Communication Brief", "Supplier winning bid (budget form)" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_Usage_Except_NorthAmerica()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Buyout, ProductionType.FullProduction, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new[] { CostStages.OriginalEstimate.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo( new[] { "Brief/Call for work" });
        }
    }
}
