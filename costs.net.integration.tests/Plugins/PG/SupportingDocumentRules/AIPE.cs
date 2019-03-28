using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Models.Stage;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using static costs.net.plugins.Constants;

namespace costs.net.integration.tests.Plugins.PG.SupportingDocumentRules
{
    public class AIPE : SupportingDocumentsTestBase
    {
        [Test]
        public async Task SupportingDocumentRule_AIPE_PostProduction()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new[] { CostStages.Aipe.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "Supplier winning bid (budget form)", "P&G Communication Brief" });            
        }

        [Test]
        public async Task SupportingDocumentRule_AIPE_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new[] { CostStages.Aipe.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(Array.Empty<string>());
        }
    }
}
