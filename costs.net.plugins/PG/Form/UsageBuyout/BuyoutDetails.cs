namespace costs.net.plugins.PG.Form.UsageBuyout
{
    using System;
    using core.Builders;

    public class BuyoutDetails
    {
        public class Country
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }

        public string Name { get; set; }

        public Contract Contract { get; set; }

        public DictionaryValue[] Rights { get; set; }

        public DictionaryValue[] Touchpoints { get; set; }

        public string NameOfLicensor { get; set; }

        public Country[] AiringCountries { get; set; }
    }
}
