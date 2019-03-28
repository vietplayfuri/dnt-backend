namespace costs.net.plugins.PG.Models.Rules
{
    public class PgPaymentRuleDefinitionSplit
    {
        public string CostTotalName { get; set; }
        public decimal? AIPESplit { get; set; }
        public decimal? OESplit { get; set; }
        public decimal? FPSplit { get; set; }
        public decimal? FASplit { get; set; }
    }
}