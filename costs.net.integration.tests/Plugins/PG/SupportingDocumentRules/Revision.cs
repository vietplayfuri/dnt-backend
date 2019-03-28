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
    public class Revision : SupportingDocumentsTestBase
    {
        [Test]
        public async Task SupportingDocumentRule_FP_Revision_PostProduction_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);
            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new String[] { CostStages.FirstPresentationRevision.ToString() }, Guid.Empty);

            var docsArray = docs.Select(s => s.Name).ToArray();

            docsArray.ShouldBeEquivalentTo<string[]>(new string[] { "Scope change approval form" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_Revision_PostProduction_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);
            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Production, new String[] { CostStages.OriginalEstimateRevision.ToString() }, Guid.Empty);

            var docsArray = docs.Select(s => s.Name).ToArray();

            docsArray.ShouldBeEquivalentTo<string[]>(new string[] { "Scope change approval form" });
        }

        [Test]
        public async Task SupportingDocumentRule_OE_Revision_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);
            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new String[] { CostStages.OriginalEstimateRevision.ToString() }, Guid.Empty);

            var docsArray = docs.Select(s => s.Name).ToArray();

            docsArray.ShouldBeEquivalentTo<string[]>(new string[] { "Scope change approval form" });
        }

        [Test]
        public async Task SupportingDocumentRule_FP_Revision_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, dataAccess.Entity.CostType.Production, ProductionType.PostProductionOnly, false);
            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, dataAccess.Entity.CostType.Buyout, new String[] { CostStages.FirstPresentationRevision.ToString() }, Guid.Empty);

            var docsArray = docs.Select(s => s.Name).ToArray();

            docsArray.ShouldBeEquivalentTo<string[]>(new string[] { "Scope change approval form" });
        }
    }
}
