namespace costs.net.plugins.PG.Form.UsageBuyout
{
    using System;
    using core.Builders;

    public class Contract
    {
        public string Period { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Exclusivity { get; set; }

        public DictionaryValue[] ExclusivityCategoryValues { get; set; }

        public decimal ContractTotal { get; set; }
        /// <summary>        
        /// Check box No End date/Perpetuity is checked or not
        /// https://jira.adstream.com/browse/ADC-2594
        /// </summary>
        public bool IsPerpetuity { get; set; }
    }
}
