namespace dnt.core.Models.Utils
{
    public class AdIdSettings
    {
        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string BankId { get; set; }

        public string Advertiser { get; set; }

        /// <summary>
        /// This is used when the Brand Prefix fails with status code 4XX.
        /// </summary>
        public string FallbackBrandPrefix { get; set; }
    }
}
