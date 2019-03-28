namespace costs.net.plugins.PG.Models.VendorRule
{
    using System.Collections.Generic;

    public class PgVendorRuleSplit
    {
        public string CostTotalType { get; set; }

        public Dictionary<string, decimal?> StageSplits { get; set; }
    }
}