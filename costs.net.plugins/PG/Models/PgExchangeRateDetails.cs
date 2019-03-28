namespace costs.net.plugins.PG.Models
{
    using System;
    using core.Models.Common;

    public class PgExchangeRateDetails : IExchangeRateDetails
    {
        public Guid CurrencyId { get; set; }

        public decimal Rate { get; set; }

        public decimal? OldRate { get; set; }

        public string RateName { get; set; }

        public string RateType { get; set; }

        public string CurrencyCode { get; set; }
    }
}
