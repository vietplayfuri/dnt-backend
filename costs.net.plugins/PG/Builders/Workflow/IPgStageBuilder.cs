namespace costs.net.plugins.PG.Builders.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Models.Workflow;
    using Models.Rules;

    public interface IPgStageBuilder
    {
        Task<Dictionary<string, StageModel>> GetStages(PgStageRule testRule, Guid? vendorId = null);
    }
}