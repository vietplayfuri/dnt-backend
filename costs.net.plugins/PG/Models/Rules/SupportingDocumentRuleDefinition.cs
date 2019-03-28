
namespace costs.net.plugins.PG.Models.Rules
{
    public class SupportingDocumentRuleDefinition
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public bool Mandatory { get; set; } = true;
        public bool CanManuallyUpload { get; set; } = true;
    }
}
