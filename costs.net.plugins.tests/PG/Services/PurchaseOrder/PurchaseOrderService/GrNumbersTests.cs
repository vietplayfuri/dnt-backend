namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;

    [TestFixture]
    public class GrNumbersTests : PgPurchaseOrderServiceTests
    {
        [Test]
        public async Task GrNumbers_whenNoGrNumbers_shouldReturnEmptyArray()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingCancellation);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.GrNumbers.Should().NotBeNull();
            purchase.GrNumbers.Should().HaveCount(0);
        }

        [Test]
        public async Task GrNumbers_whenMultipleVresionOfCurrentStage_shouldReturnGrNumberForEachVersion()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingCancellation);
            var costId = costSubmitted.AggregateId;
            var cost = SetupPurchaseOrderView(costId);
            var costStage = cost.LatestCostStageRevision.CostStage;
            var revision1 = cost.LatestCostStageRevision;
            var grNumber1 = "gr-number-1";

            var revision2 = new CostStageRevision { Id = Guid.NewGuid() };
            costStage.CostStageRevisions.Add(revision2);
            var grNumber2 = "gr-number-2";

            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPurchaseOrderResponse>(
                    It.Is<IEnumerable<Guid>>(ids => ids.Contains(revision1.Id) && ids.Contains(revision2.Id)), CustomObjectDataKeys.PgPurchaseOrderResponse))
                .ReturnsAsync(new List<PgPurchaseOrderResponse>
                {
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber1
                    },
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber2
                    }
                });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.GrNumbers.Should().NotBeNull();
            purchase.GrNumbers.Should().HaveCount(2);
            purchase.GrNumbers.Should().Contain(grNumber1);
            purchase.GrNumbers.Should().Contain(grNumber2);
        }

        [Test]
        public async Task GrNumbers_whenMultipleStages_shouldReturnGrNumberForEachVersionOfEachStage()
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingCancellation);
            var costId = costSubmitted.AggregateId;
            var cost = SetupPurchaseOrderView(costId);
            var costStage1 = cost.LatestCostStageRevision.CostStage;
            var revision1 = cost.LatestCostStageRevision;
            var grNumber1 = "gr-number-1";

            var revision2 = new CostStageRevision { Id = Guid.NewGuid() };
            costStage1.CostStageRevisions.Add(revision2);
            var grNumber2 = "gr-number-2";

            var costStage2 = new CostStage();
            var revision3 = new CostStageRevision { Id = Guid.NewGuid(), CostStage = costStage2 };
            costStage2.CostStageRevisions.Add(revision3);
            var grNumber3 = "gr-number-3";

            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPurchaseOrderResponse>(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Contains(revision1.Id)
                        && ids.Contains(revision2.Id)
                        && ids.Contains(revision2.Id)), CustomObjectDataKeys.PgPurchaseOrderResponse)
                )
                .ReturnsAsync(new List<PgPurchaseOrderResponse>
                {
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber1
                    },
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber2
                    },
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber3
                    }
                });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.GrNumbers.Should().NotBeNull();
            purchase.GrNumbers.Should().HaveCount(3);
            purchase.GrNumbers.Should().Contain(grNumber1);
            purchase.GrNumbers.Should().Contain(grNumber2);
            purchase.GrNumbers.Should().Contain(grNumber3);
        }

        [Test]
        [TestCaseSource(nameof(NonCancelledAndNotApprovedStatuses))]
        public async Task GrNumbers_whenMultipleVresionOfCurrentStageAndStageIsNotPendingCancellation_shouldReturnEmptyArray(CostStageRevisionStatus status)
        {
            // Arrange           
            var costSubmitted = GetCostRevisionStatusChanged(status);
            var costId = costSubmitted.AggregateId;
            var cost = SetupPurchaseOrderView(costId);
            var costStage = cost.LatestCostStageRevision.CostStage;
            var revision1 = cost.LatestCostStageRevision;
            var grNumber1 = "gr-number-1";

            var revision2 = new CostStageRevision { Id = Guid.NewGuid() };
            costStage.CostStageRevisions.Add(revision2);
            var grNumber2 = "gr-number-1";

            _customDataServiceMock.Setup(cd => cd.GetCustomData<PgPurchaseOrderResponse>(
                    It.Is<IEnumerable<Guid>>(ids => ids.Contains(revision1.Id) && ids.Contains(revision2.Id)), CustomObjectDataKeys.PgPurchaseOrderResponse))
                .ReturnsAsync(new List<PgPurchaseOrderResponse>
                {
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber1
                    },
                    new PgPurchaseOrderResponse
                    {
                        GrNumber = grNumber2
                    }
                });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.GrNumbers.Should().NotBeNull();
            purchase.GrNumbers.Should().HaveCount(0);
        }
    }
}
