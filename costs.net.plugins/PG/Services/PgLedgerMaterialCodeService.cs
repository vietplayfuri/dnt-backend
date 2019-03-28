namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Builders.Response;
    using core.Models.CostTemplate;
    using core.Models.User;
    using core.Services.CustomData;
    using dataAccess;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json;

    public class PgLedgerMaterialCodeService : IPgLedgerMaterialCodeService
    {
        private const string MultipleOptions = "Multiple";
        private readonly EFContext _efContext;
        private readonly ICustomObjectDataService _customDataService;

        public PgLedgerMaterialCodeService(
            EFContext efContext,
            ICustomObjectDataService customDataService)
        {
            _efContext = efContext;
            _customDataService = customDataService;
        }

        public async Task UpdateLedgerMaterialCodes(Guid costStageRevisionId)
        {
            var costStageRevision = await _efContext.CostStageRevision
                .Include(csr => csr.StageDetails)
                .Include(csr => csr.ExpectedAssets)
                .Include(csr => csr.CostStage)
                    .ThenInclude(cs => cs.Cost)
                .FirstOrDefaultAsync(csr => csr.Id == costStageRevisionId);

            var costType = costStageRevision.CostStage.Cost.CostType.ToString();

            var multipleOvalTypesOption = await _efContext.DictionaryEntry
                .Where(d => d.Dictionary.Name == Constants.DictionaryNames.OvalType && d.Key == MultipleOptions)
                .FirstAsync();

            var multipleMediaTypesOption = await _efContext.DictionaryEntry
                .Where(d => d.Dictionary.Name == Constants.DictionaryNames.MediaType && d.Key == MultipleOptions)
                .FirstAsync();

            var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(costStageRevision.StageDetails.Data);

            // Eligible for Production cost
            var contentTypeId = stageDetailsForm.ContentType?.Id; 
            var productionTypeId = stageDetailsForm.ProductionType?.Id;
            var expectedAssets = costStageRevision.ExpectedAssets;
            Guid? mediaTypeId = null;
            Guid? ovalTypeId = null;
            
            var mediaTypeIds = expectedAssets.Where(a => a.MediaTypeId != null).Select(a => a.MediaTypeId).Distinct().ToArray();
            if (mediaTypeIds.Length > 0)
            {
                mediaTypeId = mediaTypeIds.Length == 1 ? mediaTypeIds[0] : multipleMediaTypesOption.Id;
            }
            var ovalTypeIds = expectedAssets.Where(a => a.OvalTypeId != null).Select(a => a.OvalTypeId).Distinct().ToArray();
            if (ovalTypeIds.Length > 0)
            {
                ovalTypeId = ovalTypeIds.Length == 1 ? ovalTypeIds[0] : multipleOvalTypesOption.Id;
            }

            // Eligible for Usage/Buyout cost
            var usageTypeId = stageDetailsForm.UsageType?.Id;

            var ledgerMaterialCode = await _efContext.PgLedgerMaterialCode
                .OrderBy(c => c.ProductionTypeId)
                .FirstOrDefaultAsync(c =>
                    (c.CostType == costType
                    && c.ContentTypeId == contentTypeId
                    && c.ProductionTypeId == productionTypeId
                    && c.MediaTypeId == mediaTypeId
                    && c.OvalId == ovalTypeId
                    && c.UsageTypeId == usageTypeId
                    )
                    ||
                    //Default values needed for AIPE
                    (costType == CostType.Production.ToString()
                     && c.ContentTypeId == contentTypeId
                     && c.ProductionTypeId == null
                     && c.MediaTypeId == null
                     && c.OvalId == null
                     && c.UsageTypeId == null
                     )
                     ||
                     (c.CostType == CostType.Trafficking.ToString() &&
                        costType == CostType.Trafficking.ToString())
                );

            var mgCode = ledgerMaterialCode?.MaterialGroupCode;
            var glCode = ledgerMaterialCode?.GeneralLedgerCode;

            var codeModel = new PgLedgerMaterialCodeModel { GlCode = glCode, MgCode = mgCode };

            var adminUser = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Email == ApprovalMemberModel.BrandApprovalUserEmail);
            var adminUserIdentity = new SystemAdminUserIdentity(adminUser);
            await _customDataService.Save(costStageRevisionId, CustomObjectDataKeys.PgMaterialLedgerCodes, codeModel, adminUserIdentity);
        }

        public async Task<PgLedgerMaterialCodeModel> GetLedgerMaterialCodes(Guid costStageRevisionId)
        {
            var codes = await _customDataService.GetCustomData<PgLedgerMaterialCodeModel>(costStageRevisionId, CustomObjectDataKeys.PgMaterialLedgerCodes);
            return codes;
        }
    }
}
