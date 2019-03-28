namespace costs.net.plugins.PG.Builders.Cost
{
    using System.Collections.Generic;
    using core.Builders.Response;
    using core.Builders.Response.Cost;
    using dataAccess.Entity;

    public class UpdateCostFormResponse : IUpdateCostFormResponse
    {
        public UpdateCostFormResponse()
        {
            Errors = new List<string>();
        }

        public CostFormDetails Details { get; set; }

        public CustomFormData Form { get; set; }

        public List<string> Errors { get; }

        public IEnumerable<ApprovalModel> Approvals { get; set; }
    }
}