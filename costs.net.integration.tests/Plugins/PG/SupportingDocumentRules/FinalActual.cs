using costs.net.plugins.PG.Models.Stage;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using static costs.net.plugins.Constants;

namespace costs.net.integration.tests.Plugins.PG.SupportingDocumentRules
{
    public class FinalActual : SupportingDocumentsTestBase
    {
        [Test]
        public async Task SupportingDocumentRule_FA_PostProduction()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.FinalActual.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Final (online) version approval from brand" });
        }

        [Test]
        public async Task SupportingDocumentRule_FA_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new[] { CostStages.FinalActual.ToString() }, new Guid(PreviousStageId));

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Signed contract or letter of extension" });
        }

        [Test]
        public async Task SupportingDocumentRule_FA_on_FirstStage_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new[] { CostStages.FinalActual.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Signed contract or letter of extension", "Brief/Call for work" });
        }
    }
}
