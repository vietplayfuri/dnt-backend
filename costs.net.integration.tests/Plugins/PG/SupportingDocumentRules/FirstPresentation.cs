using static costs.net.plugins.Constants;

namespace costs.net.integration.tests.Plugins.PG.SupportingDocumentRules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Models.Stage;
    using Moq;

    public class FirstPresentation : SupportingDocumentsTestBase
    {
        [Test]
        public async Task SupportingDocumentRule_FP_PostProduction_Audio()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, CostType.Production, ProductionType.PostProductionOnly, false);


            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, CostType.Production, new[] { CostStages.FirstPresentation.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new string[0]);
        }

        [Test]
        public async Task SupportingDocumentRule_FP_PostProduction_Digital()
        {
            var stageDetails = BuildStageDetails(ContentType.Digital, BudgetRegion.AsiaPacific, CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, CostType.Production, new[] { CostStages.FirstPresentation.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new string[0]);
        }

        [Test]
        public async Task SupportingDocumentRule_FP_PostProduction_Video()
        {
            var stageDetails = BuildStageDetails(ContentType.Video, BudgetRegion.AsiaPacific, CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, CostType.Production, new[] { CostStages.FirstPresentation.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "First Presentation Confirmation Email" });
        }

        [Test]
        public async Task SupportingDocumentRule_FP_PostProduction_Image()
        {
            var stageDetails = BuildStageDetails(ContentType.Photography, BudgetRegion.AsiaPacific, CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, CostType.Production, new[] { CostStages.FirstPresentation.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(new[] { "First Presentation Confirmation Email" });
        }

        [Test]
        public async Task SupportingDocumentRule_FP_Usage()
        {
            var stageDetails = BuildStageDetails(ContentType.Audio, BudgetRegion.AsiaPacific, CostType.Production, ProductionType.PostProductionOnly, false);

            var docs = await CostBuilder.BuildSupportingDocuments(stageDetails, CostType.Buyout, new[] { CostStages.FirstPresentation.ToString() }, Guid.Empty);

            docs.Select(s => s.Name).ToArray().ShouldBeEquivalentTo(Array.Empty<string>());
        }
    }
}