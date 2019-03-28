namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;

    [TestFixture]
    public class ItemIdCodeTests : PgPurchaseOrderServiceTests
    {
        [Test]
        public async Task ItemIdCode_whenNotExists_shouldReturnEmptyString()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.ItemIdCode.Should().Be(string.Empty);
        }

        [Test]
        public async Task ItemIdCode_whenExists_shouldReturnItemIdCode()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);
            const string itemIdCode = "item-id-code";

            _customDataServiceMock.Setup(s => s.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), CustomObjectDataKeys.PgPurchaseOrderResponse))
                .ReturnsAsync(new PgPurchaseOrderResponse { ItemIdCode = itemIdCode });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.ItemIdCode.Should().Be(itemIdCode);
        }
    }
}
