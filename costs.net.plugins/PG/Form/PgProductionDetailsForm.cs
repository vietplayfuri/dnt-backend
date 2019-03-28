namespace costs.net.plugins.PG.Form
{
    using System;

    public class PgProductionDetailsForm
    {
        public string Type { get; set; }

        public DateTimeOffset? FirstShootDate { get; set; }

        public Vendor ProductionCompany { get; set; }

        public string DirectorsName { get; set; }

        public string ShootDays { get; set; }

        public Country PrimaryShootCountry { get; set; }

        public City PrimaryShootCity { get; set; }

        public Vendor PostProductionCompany { get; set; }

        public Vendor MusicCompany { get; set; }

        public string Airing { get; set; }

        public Vendor CgiAnimationCompany { get; set; }

        public Vendor CastingCompany { get; set; }

        public Vendor TalentCompany { get; set; }

        public Vendor DistributionTrafficking { get; set; }

        public Vendor UsageBuyoutContract { get; set; }

        public Vendor DirectPaymentVendor { get; set; }

        public class Vendor
        {
            public Guid Id { get; set; }

            public Guid? CurrencyId { get; set; }

            public string SapVendorCode { get; set; }

            public string ProductionCategory { get; set; }
        }

        public class City
        {
            public Guid Id { get; set; }

            public Guid CountryId { get; set; }

            public string Name { get; set; }
        }

        public class Country
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
        }
    }
}
