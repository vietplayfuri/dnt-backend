namespace costs.net.plugins.PG.Models.Rules
{
    using System.Collections.Generic;

    public class PgStageRuleDefinition
    {
        public PgStageRuleDefinition()
        {
            Add = new AddStage();
            Remove = new RemoveStage();
        }

        public AddStage Add { get; set; }

        public RemoveStage Remove { get; set; }

        public class Stage
        {
            public string Name { get; set; }

            public bool IsRequired { get; set; }

            public bool IsCalculatingPayment { get; set; }
        }

        public class AddStage
        {
            public AddStage()
            {
                Stages = new Dictionary<string, Stage>();
                Transitions = new Dictionary<string, IEnumerable<string>>();
            }

            public Dictionary<string, Stage> Stages { get; set; }

            public Dictionary<string, IEnumerable<string>> Transitions { get; set; }
        }

        public class RemoveStage
        {
            public RemoveStage()
            {
                Stages = new string[0];
                Transitions = new Dictionary<string, IEnumerable<string>>();
            }

            public string[] Stages { get; set; }

            public Dictionary<string, IEnumerable<string>> Transitions { get; set; }
        }
    }
}