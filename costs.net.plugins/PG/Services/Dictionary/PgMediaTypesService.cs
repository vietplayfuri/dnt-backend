namespace costs.net.plugins.PG.Services.Dictionary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Services.Costs;
    using core.Services.Dictionary;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;

    public class PgMediaTypesService : IMediaTypesPluginService
    {
        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;

        public PgMediaTypesService(EFContext efContext, ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<IEnumerable<DictionaryEntry>> GetMediaTypes(Guid costStageRevisionId)
        {
            var costStageRevision = await _efContext.CostStageRevision
                .Include(csr => csr.StageDetails)
                .Include(csr => csr.CostStage)
                    .ThenInclude(cs => cs.Cost)
                .FirstOrDefaultAsync(csr => csr.Id == costStageRevisionId);

            if (costStageRevision == null)
            {
                throw new Exception($"Couldn't find costStageRevision with Id {costStageRevisionId}");
            }

            var cost = costStageRevision.CostStage.Cost;

            var stageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevision);

            List<DictionaryEntry> entries;
            switch (cost.CostType)
            {
                case CostType.Production:
                    entries = await _efContext.DependentItem
                        .Where(di => di.ParentId == stageDetails.ContentType.Id)
                        .Join(_efContext.DictionaryEntry, di => di.ChildId, de => de.Id, (di, de) => de)
                        .ToListAsync();
                    break;
                case CostType.Buyout:
                    entries = await _efContext.DependentItem
                        .Where(di => di.ParentId == stageDetails.UsageBuyoutType.Id)
                        .Join(_efContext.DictionaryEntry, di => di.ChildId, de => de.Id, (di, de) => de)
                        .ToListAsync();
                    break;
                default:
                    entries = await _efContext.DictionaryEntry
                        .Where(de =>
                            de.Dictionary.Name == Constants.DictionaryNames.MediaType
                            && de.Key == Constants.MediaType.NA)
                    .ToListAsync();
                    break;
            }

            return entries;
        }
    }
}