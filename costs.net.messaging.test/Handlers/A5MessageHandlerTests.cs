namespace costs.net.messaging.test.Handlers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using core.ExternalResource.Gdam;
    using core.Models.AMQ;
    using core.Models.Gdam;
    using core.Services.Agency;
    using core.Services.Brand;
    using core.Services.Project;
    using core.Services.User;
    using messaging.Handlers;
    using Moq;
    using NUnit.Framework;
    using Serilog;
    using tests.common.Extensions;

    [TestFixture]
    public class A5MessageHandlerTests
    {
        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger>();
            _userServiceMock = new Mock<IUserService>();
            _projectServiceMock = new Mock<IProjectService>();
            _gdamClientMock = new Mock<IGdamClient>();
            _agencyServiceMock = new Mock<IAgencyService>();
            _brandServiceMock = new Mock<IBrandService>();

            _handler = new A5MessageHandler(
                _loggerMock.Object,
                _gdamClientMock.Object,
                _agencyServiceMock.Object,
                _projectServiceMock.Object,
                _userServiceMock.Object,
                _brandServiceMock.Object);

            _jsonReader = new JsonTestReader();
        }

        private JsonTestReader _jsonReader;
        private Mock<ILogger> _loggerMock;
        private Mock<IGdamClient> _gdamClientMock;
        private Mock<IBrandService> _brandServiceMock;
        private Mock<IAgencyService> _agencyServiceMock;
        private Mock<IProjectService> _projectServiceMock;
        private Mock<IUserService> _userServiceMock;
        private A5MessageHandler _handler;

        [Test]
        public async Task HandleA5EventObject_BusinessUnitCreated()
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency.json";
            var a5Agency = await _jsonReader.GetObject<A5Agency>(filePath, true);
            _gdamClientMock.Setup(a => a.FindAgencyById(a5Agency._id)).ReturnsAsync(a5Agency);
            filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}agency_created.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            await _handler.Handle(a5EventObject);

            _gdamClientMock.Verify(a => a.FindAgencyById(a5Agency._id), Times.Once);
            _agencyServiceMock.Verify(a => a.AddAgencyToDb(a5Agency), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectCreated()
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_project.json";
            var a5Project = await _jsonReader.GetObject<A5Project>(filePath, false);
            _gdamClientMock.Setup(a => a.FindProjectById(a5Project._id)).ReturnsAsync(a5Project);
            filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}project_created.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            await _handler.Handle(a5EventObject);

            _gdamClientMock.Verify(a => a.FindProjectById(a5Project._id), Times.Once);
            _projectServiceMock.Verify(a => a.AddProjectToDb(a5Project), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectDeleted()
        {
            // Setup
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}project_deleted.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            _gdamClientMock.Setup(a => a.FindProjectById(It.IsAny<string>())).ReturnsAsync(new A5Project());

            // Act
            await _handler.Handle(a5EventObject);

            // Assert
            _gdamClientMock.Verify(a => a.FindProjectById(It.IsAny<string>()), Times.Never);
            _projectServiceMock.Verify(a => a.DeleteProject(a5EventObject.Object.Id, a5EventObject.Subject.Id), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_ProjectSchemaUpdated()
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency.json";
            var a5Agency = await _jsonReader.GetObject<A5Agency>(filePath, true);
            _gdamClientMock.Setup(a => a.FindAgencyById(a5Agency._id)).ReturnsAsync(a5Agency);
            filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}schema_changed.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            await _handler.Handle(a5EventObject);

            _gdamClientMock.Verify(a => a.FindAgencyById(a5Agency._id), Times.Once);
            _brandServiceMock.Verify(a => a.SyncBusinessUnitBrands(a5Agency), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_UserCreated()
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_user.json";
            var a5User = await _jsonReader.GetObject<GdamUser>(filePath, false);
            _gdamClientMock.Setup(a => a.FindUser(a5User._id)).ReturnsAsync(a5User);
            filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}user_created.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            await _handler.Handle(a5EventObject);

            _gdamClientMock.Verify(a => a.FindUser(a5User._id), Times.Once);
            _userServiceMock.Verify(a => a.AddUserToDb(a5User), Times.Once);
        }

        [Test]
        public async Task HandleA5EventObject_UserUpdated()
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_user.json";
            var a5User = await _jsonReader.GetObject<GdamUser>(filePath, false);
            _gdamClientMock.Setup(a => a.FindUser(a5User._id)).ReturnsAsync(a5User);
            filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}user_updated.json";
            var a5EventObject = await _jsonReader.GetObject<A5EventObject>(filePath);

            await _handler.Handle(a5EventObject);

            _gdamClientMock.Verify(a => a.FindUser(a5User._id), Times.Once);
            _userServiceMock.Verify(a => a.AddUserToDb(a5User), Times.Once);
        }
    }
}