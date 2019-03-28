namespace costs.net.integration.tests.Builders
{
    using System;
    using System.Collections.Generic;
    using core.Extensions;
    using core.Models.Costs;
    using dataAccess.Entity;
    using plugins;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;

    public class CreateCostModelBuilder : BaseBuilder<CreateCostModel>
    {
        private readonly Dictionary<string, dynamic> _stageDetails;

        public CreateCostModelBuilder()
        {
            _stageDetails = new Dictionary<string, dynamic>
            {
                {nameof(PgStageDetailsForm.AgencyCurrency).ToCamelCase(), "PHP"},
                { "budgetRegion", new AbstractTypeValue { Key = Constants.BudgetRegion.AsiaPacific }},
                { "contentType", new { id = Guid.NewGuid(), Key = Constants.ContentType.Video } },
                { "productionType", new { id = Guid.NewGuid(), Key = Constants.ProductionType.FullProduction } },
                { "targetBudget", "11000.0" },
                { "projectId", "123456789" },
                { "approvalStage",  CostStages.OriginalEstimate.ToString() },
                { "agency",  new PgStageDetailsForm.AbstractTypeAgency() },
            };
            Object.StageDetails = new StageDetails
            {
                Data = _stageDetails
            };
        }

        public CreateCostModelBuilder WithContentType(string contentType)
        {
            _stageDetails[nameof(PgStageDetailsForm.ContentType).ToCamelCase()] = new
            {
                id = Guid.NewGuid(),
                key = contentType,
                value = contentType
            };
            return this;
        }

        public CreateCostModelBuilder WithProductionType(string productionType)
        {
            _stageDetails[nameof(PgStageDetailsForm.ProductionType).ToCamelCase()] = new
            {
                id = Guid.NewGuid(),
                key = productionType,
                value = productionType
            };
            return this;
        }

        public CreateCostModelBuilder WithBudgetRegion(string budgetRegion)
        {
            _stageDetails[nameof(PgStageDetailsForm.BudgetRegion).ToCamelCase()] = new AbstractTypeValue { Key = budgetRegion };
            return this;
        }

        public CreateCostModelBuilder WithTemplateId(Guid costTemplateId)
        {
            Object.TemplateId = costTemplateId;
            return this;
        }

        public CreateCostModelBuilder WithInitialBudget(decimal initialBudget)
        {
            _stageDetails[nameof(PgStageDetailsForm.InitialBudget).ToCamelCase()] = initialBudget;
            return this;
        }

        public CreateCostModelBuilder WithApprovalStage(string approvalStage)
        {
            _stageDetails[nameof(PgStageDetailsForm.ApprovalStage).ToCamelCase()] = approvalStage;
            return this;
        }

        public CreateCostModelBuilder WithAgency(AbstractType agencyAbstractType)
        {
            _stageDetails[nameof(PgStageDetailsForm.Agency).ToCamelCase()] = new PgStageDetailsForm.AbstractTypeAgency
            {
                Id = agencyAbstractType.ObjectId,
                AbstractTypeId = agencyAbstractType.Id
            };
            return this;
        }
    }
}