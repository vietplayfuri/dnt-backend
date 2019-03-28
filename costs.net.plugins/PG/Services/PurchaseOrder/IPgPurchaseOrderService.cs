namespace costs.net.plugins.PG.Services.PurchaseOrder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Events.Cost;
    using Models.PurchaseOrder;

    public interface IPgPurchaseOrderService
    {
        Task<PgPurchaseOrder> GetPurchaseOrder(CostStageRevisionStatusChanged stageRevisionStatusChanged);

        Task<bool> NeedToSendPurchaseOrder(CostStageRevisionStatusChanged stageRevisionStatusChanged);

        Task<List<XMGOrder>> GetXMGOrder(string costNumber);
    }
}
