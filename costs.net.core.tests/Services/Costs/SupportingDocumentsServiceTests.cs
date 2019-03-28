namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Models.Common;
    using core.Models.Costs;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using ExternalResource.Gdam;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using SupportingDocumentUpdated = core.Events.SupportingDocument.SupportingDocumentUpdated;

    [TestFixture]
    public class SupportingDocumentsServiceTests
    {
        private Mock<IOptions<AppSettings>> _appSettingsOptionsMock;
        private EFContext _efContext;
        private Mock<IGdamClient> _gdamClientMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IActivityLogService> _activityLogServiceMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private Mock<ICostStageService> _costStageServiceMock;
        private SupportingDocumentsService _supportingDocumentsService;

        [SetUp]
        public void Init()
        {
            _appSettingsOptionsMock  = new Mock<IOptions<AppSettings>>();
            _appSettingsOptionsMock.Setup(s => s.Value).Returns(new AppSettings
            {
                GdnHost = "http://gdn.com",
                GdamCoreHost = "http://gdam.com"
            });

            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _gdamClientMock = new Mock<IGdamClient>();
            _mapperMock = new Mock<IMapper>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _eventServiceMock = new Mock<IEventService>();
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _costStageServiceMock = new Mock<ICostStageService>();

            _supportingDocumentsService = new SupportingDocumentsService(
                _appSettingsOptionsMock.Object,
                _efContext,
                _gdamClientMock.Object,
                _mapperMock.Object,
                _activityLogServiceMock.Object,
                _eventServiceMock.Object,
                _costStageRevisionServiceMock.Object,
                _costStageServiceMock.Object
                );
        }

        [Test]
        public async Task RegisterUpload_WhenValidInput_ShouldRegisterFileInGdam()
        {
            // Arrange
            const string fileName = "Test file.pdf";
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            var costId = Guid.NewGuid();
            var fileSize = 100;
            var request = new SupportingDocumentRegisterRequest
            {
                FileName = fileName,
                FileSize = fileSize
            };
            _gdamClientMock.Setup(c => c.RegisterFileUpload(gdamUserId, It.IsAny<RegisterFileUploadRequest>()))
                .ReturnsAsync(new RegisterFileUploadResponse
                {
                    Files = new[]
                    {
                        new RegisterFileUploadResponse.File
                        {
                            FileId = "234234234",
                            FileUri = "http://url.com/id",
                        }
                    }
                });

            // Act
            await _supportingDocumentsService.RegisterUpload(costId, request, userIdentity);

            // Assert
            _gdamClientMock.Verify(c => c.RegisterFileUpload(
                gdamUserId, It.Is<RegisterFileUploadRequest>(r => r.Files.Length > 0 && r.Files[0].Size.Equals(fileSize))
            ), Times.Once);
        }

        [Test]
        public async Task RegisterUpload_WhenValidInput_ShouldReturnFileUriAndFileIdTakenFromGDN()
        {
            // Arrange
            const string fileName = "Test file.pdf";
            const string fileId = "234234234";
            const string fileUri = "http://url.com/123123";
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            var costId = Guid.NewGuid();
            var request = new SupportingDocumentRegisterRequest
            {
                FileName = fileName
            };
            _gdamClientMock.Setup(c => c.RegisterFileUpload(gdamUserId, It.IsAny<RegisterFileUploadRequest>()))
                .ReturnsAsync(new RegisterFileUploadResponse
                {
                    Files = new[]
                    {
                        new RegisterFileUploadResponse.File
                        {
                            FileId = fileId,
                            FileUri = fileUri,
                        }
                    }
                });

            // Act
            var result = await _supportingDocumentsService.RegisterUpload(costId, request, userIdentity);

            // Assert
            result.FileUri.Should().Be(fileUri);
            result.FileId.Should().Be(fileId);
        }

        [Test]
        public async Task RegisterUpload_WhenNotAmazonS3Url_ShouldReturnIsGDNTrue()
        {
            // Arrange
            const string fileUri = "http://url.com/123123";
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            var costId = Guid.NewGuid();
            var request = new SupportingDocumentRegisterRequest();

            _gdamClientMock.Setup(c => c.RegisterFileUpload(gdamUserId, It.IsAny<RegisterFileUploadRequest>()))
                .ReturnsAsync(new RegisterFileUploadResponse
                {
                    Files = new[]
                    {
                        new RegisterFileUploadResponse.File
                        {
                            FileUri = fileUri
                        }
                    }
                });

            // Act
            var result = await _supportingDocumentsService.RegisterUpload(costId, request, userIdentity);

            // Assert
            result.IsGDN.Should().BeTrue();
        }

        [Test]
        public async Task RegisterUpload_WhenAmazonS3Url_ShouldReturnIsGDNFalse()
        {
            // Arrange
            const string fileUri = "http://url.com/123123?asd&X-Amz-Signature=234234234234";
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            var costId = Guid.NewGuid();
            var request = new SupportingDocumentRegisterRequest();
            _gdamClientMock.Setup(c => c.RegisterFileUpload(gdamUserId, It.IsAny<RegisterFileUploadRequest>()))
                .ReturnsAsync(new RegisterFileUploadResponse
                {
                    Files = new[]
                    {
                        new RegisterFileUploadResponse.File
                        {
                            FileUri = fileUri,
                        }
                    }
                });

            // Act
            var result = await _supportingDocumentsService.RegisterUpload(costId, request, userIdentity);

            // Assert
            result.IsGDN.Should().BeFalse();
        }

        [Test]
        public async Task CompleteUpload_WhenSupportingDocumentIdIsNull_ShouldCreateSupportingDocument()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var registerResult = new SupportingDocumentRegisterResult();
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult);

            // Assert
            _efContext.SupportingDocument.Should().HaveCount(1);
            _efContext.SupportingDocumentRevision.Should().HaveCount(1);
        }

        [Test]
        public async Task CompleteUpload_WhenSupportingDocumentIdIsNotNull_ShouldAddRevisiontoExistingSupportingDocument()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var supportingDocumentId = Guid.NewGuid();
            var registerResult = new SupportingDocumentRegisterResult();
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SupportingDocument.Add(new SupportingDocument
            {
                Id = supportingDocumentId,
                SupportingDocumentRevisions = new List<SupportingDocumentRevision>
                {
                    new SupportingDocumentRevision()
                }
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult, supportingDocumentId);

            // Assert
            _efContext.SupportingDocument.Should().HaveCount(1);
            _efContext.SupportingDocumentRevision.Should().HaveCount(2);
        }

        [Test]
        public async Task CompleteUpload_Always_ShouldCompleteUploadInGDN()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var fileId = "234234";
            var registerResult = new SupportingDocumentRegisterResult
            {
                FileId = fileId
            };
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult);

            // Assert
            _gdamClientMock.Verify(g => g.CompleteFileUpload(userIdentity.GdamUserId, fileId), Times.Once);
        }

        [Test]
        public async Task CompleteUpload_Always_ShouldEmitSupportingDocumentUpdatedEvent()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var fileId = "234234";
            var registerResult = new SupportingDocumentRegisterResult
            {
                FileId = fileId
            };
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult);

            // Assert
            _eventServiceMock.Verify(g => g.Add(It.Is<SupportingDocumentUpdated>(e => e.AggregateId == costId)), Times.Once);
            _eventServiceMock.Verify(g => g.SendAllPendingAsync(), Times.Once);
        }

        [Test]
        public async Task CompleteUpload_WhenNewSupportingDocument_ShouldAddEntryToActivityLog()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var supportingDocumentId = Guid.NewGuid();
            var fileId = "234234";
            var registerResult = new SupportingDocumentRegisterResult
            {
                FileId = fileId
            };
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SupportingDocument.Add(new SupportingDocument
            {
                Id = supportingDocumentId
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult, supportingDocumentId);

            // Assert
            _activityLogServiceMock.Verify(g => g.Log(It.IsAny<core.Models.ActivityLog.SupportingDocumentUpdated>()));
        }

        [Test]
        public async Task CompleteUpload_WhenExistingSupportingDocument_ShouldAddEntryToActivityLog()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var fileId = "234234";
            var registerResult = new SupportingDocumentRegisterResult
            {
                FileId = fileId
            };
            var gdamUserId = new string(Enumerable.Repeat('1', 24).ToArray());
            var userIdentity = new UserIdentity
            {
                Email = "test@test.com",
                GdamUserId = gdamUserId
            };
            _efContext.Cost.Add(new Cost
            {
                Id = costId
            });
            _efContext.SaveChanges();

            // Act
            await _supportingDocumentsService.CompleteUpload(costId, costStageRevisionId, userIdentity, registerResult);

            // Assert
            _activityLogServiceMock.Verify(g => g.Log(It.IsAny<core.Models.ActivityLog.SupportingDocumentAdded>()));
        }
    }
}
