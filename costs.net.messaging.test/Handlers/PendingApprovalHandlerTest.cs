namespace costs.net.messaging.test.Handlers
{
    using System.Threading.Tasks;
    using core.Messaging.Messages;
    using core.Services.Costs;
    using messaging.Handlers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PendingApprovalHandlerTest
    {
        [SetUp]
        public void Init()
        {
            _costServiceMock = new Mock<ICostService>();
            _handler = new PendingApprovalHandler(_costServiceMock.Object);
        }

        private Mock<ICostService> _costServiceMock;

        private PendingApprovalHandler _handler;

        [Test]
        public async Task Handle_Request()
        {
            var request = new PendingApprovalsRequest { ClientName = "PG" };
            await _handler.Handle(request);
        }
    }
}