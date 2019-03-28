namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Threading.Tasks;
    using Models;

    public interface IPgLedgerMaterialCodeService
    {
        Task UpdateLedgerMaterialCodes(Guid costStageRevisionId);

        Task<PgLedgerMaterialCodeModel> GetLedgerMaterialCodes(Guid costStageRevisionId);
    }
}