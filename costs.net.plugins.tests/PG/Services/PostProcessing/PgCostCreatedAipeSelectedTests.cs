
namespace costs.net.plugins.tests.PG.Services.PostProcessing
{
    using System;
    using System.Threading.Tasks;
    using core.Models;
    using core.Models.ActivityLog;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.PostProcessing;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Services.PostProcessing;

    [TestFixture]
    public class PgCostCreatedAipeSelectedTests
    {
        private PgCostCreatedAipeSelected _target;
        private UserIdentity _user;
        private Mock<IActivityLogService> _activityLogServiceMock;

        [SetUp]
        public void Init()
        {
            _user = new UserIdentity
            {
                Email = "UserName",
                AgencyId = Guid.NewGuid(),
                BuType = BuType.Pg,
                Id = Guid.NewGuid()
            };

            _activityLogServiceMock = new Mock<IActivityLogService>();
            _target = new PgCostCreatedAipeSelected(_activityLogServiceMock.Object);
        }

        [Test]
        public void CanProcess_None_ReturnsFalse()
        {
            var action = PostProcessingAction.None;

            var result = _target.CanProcess(action);

            result.Should().BeFalse();
        }

        [Test]
        public void CanProcess_CostCreated_ReturnsTrue()
        {
            var action = PostProcessingAction.CostCreated;

            var result = _target.CanProcess(action);

            result.Should().BeTrue();
        }

        [Test]
        public async Task Process_Null_DoesNothing()
        {
            Cost cost = null;

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_NonCost_DoesNothing()
        {
            string cost = "Not a cost object";

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_Cost_NonProduction_DoesNothing()
        {
            var cost = new Cost
            {
                CostType = CostType.Buyout
            };

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_Cost_Production_Aipe_False_DoesNothing()
        {
            var data = "{\"smoId\": null, \"title\": \"ADC - 2006 Aipe true(1)\", \"isAIPE\": false, \"campaign\": \"Aquafresh Campaign\", \"costType\": \"Production\", \"projectId\": \"5a16841442eba4001f6f5f6b\", \"contentType\": {\"id\": \"5c168fa6-8264-48db-9786-e19ecd6242e4\", \"key\": \"Video\", \"value\": \"Video\", \"visible\": true, \"projects\": null, \"dictionaryId\": \"27582bdd-fb57-4349-aa39-b992cac4add6\"}, \"description\": \"ADC - 2006 Aipe true(1)\", \"budgetRegion\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"NORTHERN AMERICA AREA\", \"name\": \"North America\"}, \"organisation\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"RBU\", \"value\": \"RBU\", \"visible\": true, \"projects\": null, \"dictionaryId\": \"27582bdd-fb57-4349-aa39-b992cac4add6\"}, \"initialBudget\": 65432, \"agencyCurrency\": \"GBP\", \"agencyProducer\": [\"Aaron Royer(Grey)\"], \"productionType\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"Full Production\", \"value\": \"Full Production\"}, \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"1234567890\"}";
            var customFormData = new CustomFormData
            {
                Data = data
            };
            var revision = new CostStageRevision
            {
                StageDetails = customFormData
            };
            var cost = new Cost
            {
                CostType = CostType.Production,
                LatestCostStageRevision = revision
            };

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_Cost_Production_Aipe_True_SendsActivity()
        {
            var data = "{\"smoId\": null, \"title\": \"ADC - 2006 Aipe true(1)\", \"isAIPE\": true, \"campaign\": \"Aquafresh Campaign\", \"costType\": \"Production\", \"projectId\": \"5a16841442eba4001f6f5f6b\", \"contentType\": {\"id\": \"5c168fa6-8264-48db-9786-e19ecd6242e4\", \"key\": \"Video\", \"value\": \"Video\", \"visible\": true, \"projects\": null, \"dictionaryId\": \"27582bdd-fb57-4349-aa39-b992cac4add6\"}, \"description\": \"ADC - 2006 Aipe true(1)\", \"budgetRegion\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"NORTHERN AMERICA AREA\", \"name\": \"North America\"}, \"organisation\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"RBU\", \"value\": \"RBU\", \"visible\": true, \"projects\": null, \"dictionaryId\": \"27582bdd-fb57-4349-aa39-b992cac4add6\"}, \"initialBudget\": 65432, \"agencyCurrency\": \"GBP\", \"agencyProducer\": [\"Aaron Royer(Grey)\"], \"productionType\": {\"id\": \"27582bdd-fb57-4349-aa39-b992cac4add6\", \"key\": \"Full Production\", \"value\": \"Full Production\"}, \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"1234567890\"}";
            var customFormData = new CustomFormData
            {
                Data = data
            };
            var revision = new CostStageRevision
            {
                StageDetails = customFormData
            };
            var cost = new Cost
            {
                CostType = CostType.Production,
                LatestCostStageRevision = revision
            };

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Once);
        }
    }
}
