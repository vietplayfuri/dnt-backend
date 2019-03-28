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
    public class AccountCodeTests : PgPurchaseOrderServiceTests
    {
        [Test]
        public async Task GetPurchaseOrder_whenHasAccoundCodeFromXMG_shouldReturnValueAsIs()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);
            const string expectedAccountCode = "001-4880-US-G4P~K0--5000106342-S811419AF-0052470001";

            _customDataServiceMock.Setup(ds => 
                    ds.GetCustomData<PgPurchaseOrderResponse>(It.IsAny<Guid>(), CustomObjectDataKeys.PgPurchaseOrderResponse)
                )
                .ReturnsAsync(new PgPurchaseOrderResponse { AccountCode = expectedAccountCode });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.AccountCode.Should().Be(expectedAccountCode);
        }

        [Test]
        public async Task GetPurchaseOrder_whenNoAccoundCodeFromXMG_shouldReturnEmptyString()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.AccountCode.Should().Be(string.Empty);
        }
    }
}
