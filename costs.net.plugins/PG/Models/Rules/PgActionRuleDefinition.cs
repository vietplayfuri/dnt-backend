namespace costs.net.plugins.PG.Models.Rules
{
    using System.Collections.Generic;

    public class PgActionRuleDefinition
    {
        public PgActionRuleDefinition()
        {
            Actions = new List<string>();
        }

        public List<string> Actions { get; private set; }
    }
}