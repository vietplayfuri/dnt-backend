namespace costs.net.plugins.tests.PG.Services.PurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Events.Cost;
    using core.ExternalResource.Paperpusher;
    using core.Messaging.Messages;
    using costs.net.core.Services.Costs;
    using costs.net.plugins.PG.Models.Stage;
    using dataAccess.Entity;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Services.PurchaseOrder;
    using Serilog;

    public class PgPaperpusherNotifierTests
    {
        private static readonly CostStageRevisionStatus[] StatusesToNotifyPaperpusher =
        {
            CostStageRevisionStatus.PendingBrandApproval,
            CostStageRevisionStatus.PendingCancellation,
            CostStageRevisionStatus.PendingRecall,
            CostStageRevisionStatus.Approved
        };

        private static readonly CostStageRevisionStatus[] StatusesToNotNotifyPaperpusher =
            Enum.GetValues(typeof(CostStageRevisionStatus)).Cast<CostStageRevisionStatus>().Where(c => !StatusesToNotifyPaperpusher.Contains(c)).ToArray();

        private Guid _currentCostStageRevisionId;
        private Mock<ILogger> _loggerMock;

        private PgPaperpusherNotifier _notifier;
        private Mock<IPaperpusherClient> _paperpusherClientMock;
        private Mock<IPgPurchaseOrderService> _purchaseOrderServiceMock;
        private Mock<ICostService> _costServiceMock;

        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger>();
            _purchaseOrderServiceMock = new Mock<IPgPurchaseOrderService>();
            _paperpusherClientMock = new Mock<IPaperpusherClient>();
            _costServiceMock = new Mock<ICostService>();

            _notifier = new PgPaperpusherNotifier(
                _loggerMock.Object,
                _purchaseOrderServiceMock.Object,
                _paperpusherClientMock.Object,
                _costServiceMock.Object
                );
            _currentCostStageRevisionId = Guid.NewGuid();

            _purchaseOrderServiceMock.Setup(b => b.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>())).ReturnsAsync(new PgPurchaseOrder());
        }

        private CostStageRevisionStatusChanged GetStatusChangedEvent(CostStageRevisionStatus status)
        {
            var evnt = new CostStageRevisionStatusChanged
            {
                AggregateId = Guid.NewGuid(),
                CostStageRevisionId = _currentCostStageRevisionId,
                Status = status,
                TimeStamp = DateTime.UtcNow
            };

            _purchaseOrderServiceMock.Setup(po => po.NeedToSendPurchaseOrder(evnt)).ReturnsAsync(true);
            return evnt;
        }

        [Test]
        public async Task Notify_whenStatusPendingApproval_activityTypeShouldBeSubmitted()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(CostStageRevisionStatus.PendingBrandApproval);
            var expectedActivityType = ActivityTypes.Submitted;

            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.Is<string>(a => a == expectedActivityType)),
                Times.Once);
        }

        [Test]
        public async Task Notify_whenCancelled_activityTypeShouldBeCancelled()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(CostStageRevisionStatus.PendingCancellation);
            _paperpusherClientMock.Setup(a => a.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.Is<string>(a => a == ActivityTypes.Cancelled)),
                Times.Once);
        }

        [Test]
        public async Task Notify_whenRecalled_activityTypeShouldBeRecalled()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(CostStageRevisionStatus.PendingRecall);
            _paperpusherClientMock.Setup(a => a.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.Is<string>(a => a == ActivityTypes.Recalled)),
                Times.Once);
        }

        [Test]
        public async Task Notify_whenPendingBrandApproval_shoudGetPayloadFromPurchaseOrderService()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(CostStageRevisionStatus.PendingBrandApproval);
            _paperpusherClientMock.Setup(a => a.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _purchaseOrderServiceMock.Verify(p => p.GetPurchaseOrder(statusChanged), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(StatusesToNotifyPaperpusher))]
        public async Task Notify_whenCostApprovalStatusChanged_shouldNotifyPaperpusher(CostStageRevisionStatus status)
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(status);
            _paperpusherClientMock.Setup(a => a.SendMessage(It.IsAny<Guid>(), It.IsAny<PgPurchaseOrder>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            _purchaseOrderServiceMock.Setup(b => b.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>())).ReturnsAsync(new PgPurchaseOrder { PaymentAmount = 100 });

            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(StatusesToNotNotifyPaperpusher))]
        public async Task Notify_whenNonCostApprovalStatusChanged_shouldNotNotifyPaperpusher(CostStageRevisionStatus status)
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(status);
            statusChanged.CostStageRevisionId = Guid.NewGuid();
            _purchaseOrderServiceMock.Setup(a => a.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>()))
                .ReturnsAsync(new PgPurchaseOrder { CostNumber = "CostNumber", PoNumber = "poNumber", PaymentAmount = 0 });

            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Notify_whenNoNeedToSendPurchaseOrder_shouldNotRegisterMessageInPaperpusher()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(CostStageRevisionStatus.PendingBrandApproval);
            _purchaseOrderServiceMock.Setup(po => po.NeedToSendPurchaseOrder(statusChanged)).ReturnsAsync(false);

            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }


        [Test]
        public async Task Notify_Should_Send_Paperpusher()
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(StatusesToNotifyPaperpusher.First());
            statusChanged.CostStageRevisionId = Guid.NewGuid();

            _purchaseOrderServiceMock.Setup(a => a.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>()))
            .ReturnsAsync(new PgPurchaseOrder { CostNumber = "PRO000194A0006", PoNumber = "poNumber", PaymentAmount = 0 });

            // Act
            await _notifier.Notify(statusChanged);
            // Assert
            _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        [TestCase("PRO0001944V0006", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002065S0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002130V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is Original Estimate
        [TestCase("PRO0001945V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001828V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001755V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001949V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001750V0007", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is Original Estimate
        [TestCase("PRO0002086V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001567S0002", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0024V0000006", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001567V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001712S0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001944V0003", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001820V0004", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001864V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001944V0004", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001945V0002", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001742V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002213V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is Original Estimate
        [TestCase("PRO0002182S0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is Original Estimate
        [TestCase("PRO0001769V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001789V0002", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is Original Estimate
        [TestCase("PRO0002191V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002040S0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002043V0001", nameof(CostStages.OriginalEstimate), "false")] //do not sent, because target stage is First Presentation
        [TestCase("NORMALCASEV0001", nameof(CostStages.OriginalEstimate), "true")] //sent, because this cost isn't in excel file

        [TestCase("PRO0001944V0006", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002065S0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002130V0001", nameof(CostStages.FirstPresentation), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001945V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001828V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001755V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001949V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001750V0007", nameof(CostStages.FirstPresentation), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002086V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001567S0002", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0024V0000006", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001567V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001712S0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001944V0003", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001820V0004", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001864V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001944V0004", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001945V0002", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001742V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002213V0001", nameof(CostStages.FirstPresentation), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002182S0001", nameof(CostStages.FirstPresentation), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001769V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0001789V0002", nameof(CostStages.FirstPresentation), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002191V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002040S0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("PRO0002043V0001", nameof(CostStages.FirstPresentation), "false")] //do not sent, because target stage is First Presentation
        [TestCase("NORMALCASEV0001", nameof(CostStages.FirstPresentation), "true")] //sent, because this cost isn't in excel file

        [TestCase("PRO0001944V0006", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002065S0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002130V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001945V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001828V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001755V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001949V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001750V0007", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002086V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001567S0002", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0024V0000006", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001567V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001712S0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001944V0003", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001820V0004", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001864V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001944V0004", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001945V0002", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001742V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002213V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002182S0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001769V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001789V0002", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002191V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002040S0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002043V0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because target stage is First Presentation
        [TestCase("NORMALCASEV0001", nameof(CostStages.FirstPresentationRevision), "true")] //sent, because this cost isn't in excel file

        [TestCase("PRO0001944V0006", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002065S0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002130V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001945V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001828V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001755V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001949V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001750V0007", nameof(CostStages.FinalActual), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002086V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001567S0002", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0024V0000006", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001567V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001712S0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001944V0003", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001820V0004", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001864V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001944V0004", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001945V0002", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001742V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002213V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002182S0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0001769V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0001789V0002", nameof(CostStages.FinalActual), "true")] //sent, because target stage is Original Estimate
        [TestCase("PRO0002191V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002040S0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("PRO0002043V0001", nameof(CostStages.FinalActual), "true")] //sent, because target stage is First Presentation
        [TestCase("NORMALCASEV0001", nameof(CostStages.FinalActual), "true")] //sent, because this cost isn't in excel file
        public async Task Notify_ShouldNot_SentPaperpusher(string costNumber, string stage, string isSent)
        {
            // Arrange
            var statusChanged = GetStatusChangedEvent(StatusesToNotifyPaperpusher.First());
            statusChanged.CostStageRevisionId = Guid.NewGuid();

            _purchaseOrderServiceMock.Setup(a => a.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>()))
                .ReturnsAsync(new PgPurchaseOrder { CostNumber = costNumber, PoNumber = "poNumber", PaymentAmount = 0 });

            var cost = _costServiceMock.Setup(c => c.GetCostByCostNumber(costNumber))
                .ReturnsAsync(new core.Models.Costs.CostModel
                {
                    CostNumber = costNumber,
                    LatestCostStageRevision = new core.Models.Costs.RevisionModel
                    {
                        Name = stage
                    }
                });

            // Act
            await _notifier.Notify(statusChanged);

            // Assert
            if (bool.Parse(isSent))
            {
                _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
            }
            else
            {
                _paperpusherClientMock.Verify(p => p.SendMessage(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}
