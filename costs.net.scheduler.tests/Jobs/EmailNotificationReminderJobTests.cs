using System;
using System.Collections.Generic;
using System.Linq;
using costs.net.core.Builders.Notifications;
using costs.net.core.ExternalResource.Paperpusher;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Notifications;
using costs.net.dataAccess;
using costs.net.dataAccess.Entity;
using costs.net.plugins.PG.Models.Stage;
using costs.net.scheduler.core.Jobs;
using Moq;
using NUnit.Framework;
using costs.net.tests.common.Stubs.EFContext;

namespace costs.net.scheduler.tests.Jobs
{
    [TestFixture]
    public class EmailNotificationReminderJobTests
    {
        private Mock<IEmailNotificationReminderService> _emailNotificationReminderServiceMock;
        private Mock<EFContext> _efContextMock;
        private Mock<IEmailNotificationBuilder> _emailNotificationBuilderMock;
        private Mock<IPaperpusherClient> _paperPusherClientMock;
        private readonly Guid _costIdOne = Guid.NewGuid();
        private readonly Guid _costIdTwo = Guid.NewGuid();

        [SetUp]
        public void Init()
        {
            _emailNotificationReminderServiceMock = new Mock<IEmailNotificationReminderService>();
            _efContextMock = new Mock<EFContext>();
            _emailNotificationBuilderMock = new Mock<IEmailNotificationBuilder>();
            _paperPusherClientMock = new Mock<IPaperpusherClient>();

            SetupDataSharedAcrossTests();
        }

        #region Private methods

        private void SetupDataSharedAcrossTests()
        {
            const string agencyLocation = "United Kingdom";
            const string projectName = "Pampers";
            const string projectGdamId = "57e5461ed9563f268ef4f19c";
            const string projectNumber = "PandG01";

            var projectId = Guid.NewGuid();
            var project = new dataAccess.Entity.Project();

            var costUsers = BuildCostUsers();
            var costOne = BuildCost(_costIdOne, costUsers, project);
            var costTwo = BuildCost(_costIdTwo, costUsers, project);


            var country = new Country();
            costOne.Parent.Agency.Country = country;
            costTwo.Parent.Agency.Country = country;

            project.Brand = costOne.Project.Brand;
            project.Id = projectId;
            project.Name = projectName;
            project.GdamProjectId = projectGdamId;
            project.AdCostNumber = projectNumber;
            country.Name = agencyLocation;

            var agencies = new List<dataAccess.Entity.Agency> { costOne.Parent.Agency };
            var brands = new List<dataAccess.Entity.Brand> { costOne.Project.Brand };
            var costs = new List<dataAccess.Entity.Cost> { costOne, costTwo };
            var costStages = new List<CostStageRevision> { costOne.LatestCostStageRevision, costTwo.LatestCostStageRevision };
            var countries = new List<Country> { country };
            var projects = new List<dataAccess.Entity.Project> { project };

            _efContextMock.MockAsyncQueryable(agencies.AsQueryable(), c => c.Agency);
            _efContextMock.MockAsyncQueryable(brands.AsQueryable(), c => c.Brand);
            _efContextMock.MockAsyncQueryable(costs.AsQueryable(), c => c.Cost);
            _efContextMock.MockAsyncQueryable(costStages.AsQueryable(), c => c.CostStageRevision);
            _efContextMock.MockAsyncQueryable(costUsers.AsQueryable(), c => c.CostUser);
            _efContextMock.MockAsyncQueryable(countries.AsQueryable(), c => c.Country);
            _efContextMock.MockAsyncQueryable(projects.AsQueryable(), c => c.Project);
        }

        private dataAccess.Entity.Cost BuildCost(Guid costId, CostUser[] costUsers, dataAccess.Entity.Project project)
        {
            const string agencyName = "Saatchi";
            const string brandName = "P&G";
            const string costNumber = "P101";
            const CostStages costStageName = CostStages.OriginalEstimate;

            var cost = new dataAccess.Entity.Cost
            {
                Parent = new AbstractType()
            };

            var costOwner = costUsers[0];

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var brand = new dataAccess.Entity.Brand();
            var agency = new dataAccess.Entity.Agency();

            cost.CostNumber = costNumber;
            cost.LatestCostStageRevision = latestRevision;
            cost.Project = project;
            cost.Parent.Agency = agency;
            costOwner.Agency = agency;
            costOwner.Id = Guid.NewGuid();
            latestRevision.CostStage = costStage;

            agency.Name = agencyName;
            brand.Name = brandName;
            cost.Id = costId;
            costStage.Name = costStageName.ToString();

            latestRevision.Id = Guid.NewGuid();

            return cost;
        }

        private CostUser[] BuildCostUsers()
        {
            const string costOwnerGdamUserId = "57e5461ed9563f268ef4f19d";
            const string costOwnerFullName = "Mr Cost Owner";

            var costOwner = new CostUser();
            var approverUser = new CostUser();
            var insuranceUser = new CostUser();

            approverUser.Id = Guid.NewGuid();
            costOwner.FullName = costOwnerFullName;
            costOwner.GdamUserId = costOwnerGdamUserId;
            costOwner.Id = Guid.NewGuid();
            insuranceUser.Id = Guid.NewGuid();

            return new[] { costOwner, approverUser, insuranceUser };
        }

        #endregion // Private methods
    }
}
