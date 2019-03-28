namespace dnt.core.Models.Response
{
    using System.Collections.Generic;

    public class ElasticBootstrapResponse
    {
        public ElasticBootstrapResponse()
        {
            BootstrappedItems = new Dictionary<string, long>();
        }
        public bool Error { get; set; }
        public Dictionary<string, long> BootstrappedItems { get; }
    }
}