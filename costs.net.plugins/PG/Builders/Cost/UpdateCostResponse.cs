namespace costs.net.plugins.PG.Builders.Cost
{
    using System.Collections.Generic;
    using core.Builders.Response;
    using core.Builders.Response.Cost;
    using dataAccess.Entity;

    public class UpdateCostResponse : IUpdateCostResponse
    {
        public CustomFormData ProductionDetails { get; set; }

        public CustomFormData StageDetails { get; set; }

        public List<ApprovalModel> Approvals { get; set; }

        public CostStageModel CurrentCostStageModel { get; set; }

        public Currency NewCurrency { get; set; }

        public bool DpvSelected { get; set; }

        public bool AipeSelected { get; set; }
    }
}