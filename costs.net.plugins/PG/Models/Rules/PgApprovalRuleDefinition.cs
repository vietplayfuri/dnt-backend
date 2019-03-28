namespace costs.net.plugins.PG.Models.Rules
{
    public class PgApprovalRuleDefinition
    {
        public bool CostConsultantIpmAllowed { get; set; }
        public bool IpmApprovalEnabled { get; set; }
        public bool BrandApprovalEnabled { get; set; }
        public bool HasExternalIntegration { get; set; } = true;
    }
}