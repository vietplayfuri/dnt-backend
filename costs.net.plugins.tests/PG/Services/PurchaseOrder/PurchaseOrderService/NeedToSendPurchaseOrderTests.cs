namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    public class NeedToSendPurchaseOrderTests : PgPurchaseOrderServiceTestsBase
    {
        [Test]
        public async Task NeedToSendPurchaseOrder_whenNotExternalPurchase_shouldReturnFalse()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            SetupPurchaseOrderView(costId, isExternalPurchase:false);

            // Act
            var needToSend = await PgPurchaseOrderService.NeedToSendPurchaseOrder(costSubmitted);

            // Assert
            needToSend.Should().BeFalse();
        }

        [Test]
        public async Task NeedToSendPurchaseOrder_whenApprovedAndExternalPurchase_shouldReturnTrue()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.Approved);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId, isExternalPurchase:true);

            // Act
            var gr = await PgPurchaseOrderService.NeedToSendPurchaseOrder(costSubmitted);

            // Assert
            gr.Should().BeTrue();
        }
    }
}