namespace costs.net.plugins.PG.Form
{
    using System;
    using core.Builders;

    public class PgStageDetailsForm
    {
        public class AbstractTypeAgency
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public Guid AbstractTypeId { get; set; }
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? InitialBudget { get; set; }

        public string AgencyName { get; set; }

        public string AgencyLocation { get; set; }

        public string[] AgencyProducer { get; set; }

        public AbstractTypeValue BudgetRegion { get; set; }

        public string Campaign { get; set; }

        public DictionaryValue ContentType { get; set; }

        public DictionaryValue ProductionType { get; set; }

        public string AgencyTrackingNumber { get; set; }

        public DictionaryValue Organisation { get; set; }

        public string AgencyCurrency { get; set; }

        public string IoNumber { get; set; }

        public string SmoId { get; set; }
        public string SmoName { get; set; }

        public DictionaryValue UsageType { get; set; }

        public bool IsUsage { get; set; }

        public bool IsAIPE { get; set; }

        public DictionaryValue UsageBuyoutType { get; set; }

        public string ApprovalStage { get; set; }

        public string ProjectId { get; set; }

        public string CostType { get; set; }

        public string CostNumber { get; set; }

        public AbstractTypeAgency Agency { get; set; }
        public DateTime AirInsertionDate { get; set; }
    }
}