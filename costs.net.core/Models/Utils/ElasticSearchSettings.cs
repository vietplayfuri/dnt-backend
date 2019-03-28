namespace dnt.core.Models.Utils
{
    public class ElasticSearchSettings
    {
        public string DefaultIndex { get; set; }
        public bool IsLogged { get; set; }
        public int DefaultSearchSize { get; set; }
        // Comma-separated values
        public string Nodes { get; set; }
        public bool LogResponseBody { get; set; }
    }
}
