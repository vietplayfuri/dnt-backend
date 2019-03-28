namespace costs.net.core.tests.Services.Project
{
    using System;
    using System.Threading.Tasks;
    using core.Models.Projects;
    using core.Models.User;
    using core.Services.Project;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ProjectReportServiceTests
    {
        private Mock<IProjectDataService> _projectDataServiceMock;
        private ProjectReportService _projectReportService;

        [SetUp]
        public void Init()
        {
            _projectDataServiceMock = new Mock<IProjectDataService>();
            _projectReportService = new ProjectReportService(_projectDataServiceMock.Object);
        }

        [Test]
        public async Task DownloadCsv_Always_ShouldGetDataFromDataService()
        { 
            // Arrange
            var projectId = Guid.NewGuid();
            var userIdentity = new UserIdentity();
            _projectDataServiceMock.Setup(d => d.GetProjectTotals(It.IsAny<Guid>(), userIdentity))
                .ReturnsAsync(new ProjectTotals
                {
                    Summary = new ProjectTotalSummary()
                });

            // Act
            await _projectReportService.DownloadCsv(projectId, userIdentity);

            // Assert
            _projectDataServiceMock.Verify(d => d.GetProjectTotals(projectId, userIdentity), Times.Once);
        }

        [Test]
        public async Task DownloadCsv_Always_ShouldReturnReadableStream_AtStartPosition()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userIdentity = new UserIdentity();
            _projectDataServiceMock.Setup(d => d.GetProjectTotals(It.IsAny<Guid>(), userIdentity))
                .ReturnsAsync(new ProjectTotals
                {
                    Summary = new ProjectTotalSummary()
                });

            // Act
            var stream = await _projectReportService.DownloadCsv(projectId, userIdentity);

            // Assert
            stream.CanRead.Should().BeTrue();
            stream.Position.Should().Be(0);
            stream.Close();
        }

    }
}