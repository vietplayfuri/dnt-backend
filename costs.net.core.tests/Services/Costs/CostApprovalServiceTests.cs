
namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using Builders.Request;
    using Builders.Response;
    using Builders.Response.Cost;
    using core.Models;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.User;
    using Castle.Components.DictionaryAdapter;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public class CostApprovalServiceTests
    {
        private CostApprovalService _target;

        private EFContext _efContext;
        private Mock<ICostBuilder> _costBuilderMock;

        [SetUp]
        public void Setup()
        {
            var approvalServiceMock = new Mock<IApprovalService>();
            var amqSettingsMock = new Mock<IOptions<AmqSettings>>();
            amqSettingsMock.SetupGet(o => o.Value).Returns(new AmqSettings());
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            var technicalFeeService = new Mock<ITechnicalFeeService>();
            var revisionPermissionService = new Mock<ICostStageRevisionPermissionService>();
            var costStageRevisionService = new Mock<ICostStageRevisionService>();

            _costBuilderMock = new Mock<ICostBuilder>();
            var costBuilders = new EditableList<Lazy<ICostBuilder, PluginMetadata>>
            {
                new Lazy<ICostBuilder, PluginMetadata>(
                    () => _costBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
            };
            var userServiceMock = new Mock<IUserService>();
            var activityLogServiceMock = new Mock<IActivityLogService>();
            var eventServiceMock = new Mock<IEventService>();

            _target = new CostApprovalService(approvalServiceMock.Object,
                technicalFeeService.Object,
                _efContext,
                costBuilders,
                userServiceMock.Object,
                activityLogServiceMock.Object,
                eventServiceMock.Object,
                revisionPermissionService.Object,
                costStageRevisionService.Object
            );
        }

        // ADC-2401
        [Test]
        public async Task UpdateApprovals_ExistingApprovals_ValidBusinessRole_Removed()
        {
            // Arrange
            var existingBusinessRoles = new[] { plugins.Constants.BusinessRole.Ipm, plugins.Constants.BusinessRole.CostConsultant };
            var expectedBusinessRoles = new[] { plugins.Constants.BusinessRole.Ipm };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = existingBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = expectedBusinessRoles
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            existingApprovals[0].ValidBusinessRoles.Should().NotBeNull();
            existingApprovals[0].ValidBusinessRoles.Should().BeEquivalentTo(expectedBusinessRoles);
        }

        [Test]
        public async Task UpdateApprovals_ExistingApprovals_DifferentType_DoesNothing()
        {
            // Arrange
            var existingBusinessRoles = new[] { plugins.Constants.BusinessRole.Ipm, plugins.Constants.BusinessRole.CostConsultant };
            var updatedBusinessRoles = existingBusinessRoles;
            var expectedBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = expectedBusinessRoles
                },
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = existingBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = expectedBusinessRoles
                },
                new ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = updatedBusinessRoles
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            // brand approvals remain unchanged
            existingApprovals.First(e => e.Type == ApprovalType.Brand).ValidBusinessRoles.Should().NotBeNull();
            existingApprovals.First(e => e.Type == ApprovalType.Brand).ValidBusinessRoles.Should().BeEquivalentTo(expectedBusinessRoles);
        }

        [Test]
        public async Task UpdateApprovals_ExistingApprovals_DifferentStatus_DoesNothing()
        {
            // Arrange
            var existingBusinessRoles = new[] { plugins.Constants.BusinessRole.Ipm, plugins.Constants.BusinessRole.CostConsultant };
            var expectedBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager };
            var ignoredBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager, plugins.Constants.BusinessRole.CentralAdaptationSupplier };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = expectedBusinessRoles
                },
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.Approved,
                    ValidBusinessRoles = existingBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = expectedBusinessRoles
                },
                new ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.Approved,
                    ValidBusinessRoles = ignoredBusinessRoles
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            existingApprovals.First(e => e.Type == ApprovalType.Brand).ValidBusinessRoles.Should().NotBeNull();
            existingApprovals.First(e => e.Type == ApprovalType.Brand).ValidBusinessRoles.Should().BeEquivalentTo(expectedBusinessRoles);
            existingApprovals.First(e => e.Type == ApprovalType.IPM).ValidBusinessRoles.Should().NotBeNull();
            existingApprovals.First(e => e.Type == ApprovalType.IPM).ValidBusinessRoles.Should().BeEquivalentTo(existingBusinessRoles);
        }

        [Test]
        public async Task UpdateApprovals_ExistingApprovals_OneApprovalRemoved_ShouldRemoveApprovalAndApprovalMembers()
        {
            // Arrange
            var taBusinessRoles = new[] { plugins.Constants.BusinessRole.Ipm, plugins.Constants.BusinessRole.CostConsultant };
            var brandBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>
                    {
                        new ApprovalMember()
                    },
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = brandBusinessRoles
                },
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>(),
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = taBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = taBusinessRoles
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            existingApprovals.Should().HaveCount(1);
            _efContext.Approval.Should().HaveCount(1);
            _efContext.ApprovalMember.Should().HaveCount(0);
        }

        [Test]
        public async Task UpdateApprovals_ExistingApprovals_MemberIsExternal_ShouldNotRemoveMember()
        {
            // Arrange
            var brandBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>
                    {
                        new ApprovalMember
                        {
                            IsExternal = true
                        }
                    },
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = brandBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = new[] { plugins.Constants.BusinessRole.FinanceManager }
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            existingApprovals.Should().HaveCount(1);
            _efContext.Approval.Should().HaveCount(1);
            _efContext.ApprovalMember.Should().HaveCount(1);
        }

        [Test]
        public async Task UpdateApprovals_ExistingApprovals_MemberIsNotExternalAndRoleDoesnNotMatch_ShouldRemoveMember()
        {
            // Arrange
            var brandBusinessRoles = new[] { plugins.Constants.BusinessRole.BrandManager };
            var existingApprovals = new List<Approval>
            {
                new Approval
                {
                    ApprovalMembers = new List<ApprovalMember>
                    {
                        new ApprovalMember
                        {
                            IsExternal = false,
                            CostUser = new CostUser()
                        }
                    },
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = brandBusinessRoles
                }
            };
            var updatedApprovals = new List<ApprovalModel>
            {
                new ApprovalModel
                {
                    Type = ApprovalType.Brand,
                    Status = ApprovalStatus.New,
                    ValidBusinessRoles = new[] { plugins.Constants.BusinessRole.FinanceManager }
                }
            };
            SetupRevision(out var user, out var costStageRevisionId, out _, out _, out _);
            _efContext.Approval.AddRange(existingApprovals);
            _efContext.SaveChanges();

            // Act
            await _target.UpdateApprovals(existingApprovals, updatedApprovals, costStageRevisionId, user.Id, user.BuType);

            // Assert
            existingApprovals.Should().NotBeNull();
            existingApprovals.Should().HaveCount(1);
            _efContext.Approval.Should().HaveCount(1);
            _efContext.ApprovalMember.Should().HaveCount(0);
        }

        private void SetupRevision(out UserIdentity user, out Guid costStageRevisionId, out Guid newCostStageId, out CostStageRevision currentCostStageRevision, out CostStage newCostStage)
        {
            // Arrange
            var userId = Guid.NewGuid();

            user = new UserIdentity
            {
                AgencyId = Guid.NewGuid(),
                BuType = BuType.Pg,
                Email = "newEmail@new.com",
                FirstName = "Test",
                FullName = "Test Test",
                LastName = "Test",
                GdamUserId = "58888a9b0c885449176a5fa5",
                Id = userId,
                ModuleId = Guid.NewGuid(),
            };
            var costId = Guid.NewGuid();
            var costStageId = Guid.NewGuid();
            costStageRevisionId = Guid.NewGuid();
            var stageDetailsId = Guid.NewGuid();
            var productionDetailsId = Guid.NewGuid();
            var costLineItemId = Guid.NewGuid();

            newCostStageId = Guid.NewGuid();
            var currentCostStage = new CostStage
            {
                CostId = costId,
                Created = DateTime.UtcNow,
                CreatedById = userId,
                Id = costStageId,
                Modified = DateTime.UtcNow,
                Key = CostStages.New.ToString(),
                StageOrder = 0,
                Cost = new Cost { CostType = CostType.Production }
            };

            var costLineItem = new CostLineItem
            {
                CostStageRevisionId = costStageRevisionId,
                Created = DateTime.UtcNow,
                CreatedById = user.Id,
                Id = costLineItemId,
                LocalCurrencyId = Guid.NewGuid(),
                Modified = DateTime.UtcNow,
                Name = "cast",
                TemplateSectionId = Guid.NewGuid(),
                ValueInDefaultCurrency = Decimal.Parse("1.2", CultureInfo.InvariantCulture),
                ValueInLocalCurrency = Decimal.Parse("1.0", CultureInfo.InvariantCulture),
            };

            currentCostStageRevision = new CostStageRevision
            {
                Status = CostStageRevisionStatus.Draft,
                CostStageId = costStageId,
                Created = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
                Id = costStageRevisionId,
                Modified = DateTime.UtcNow,
                Name = CostStageRevisionStatus.Draft.ToString(),
                ProductDetailsId = productionDetailsId,
                ProductDetails = new CustomFormData
                {
                    Id = productionDetailsId,
                    Data = JsonConvert.SerializeObject(new Dictionary<string, dynamic>())
                },
                StageDetails = new CustomFormData
                {
                    Id = stageDetailsId,
                    Data = JsonConvert.SerializeObject(new Dictionary<string, dynamic>())
                },
                StageDetailsId = stageDetailsId,
                CostStage = currentCostStage,
                CostLineItems = new List<CostLineItem> { costLineItem },
                SupportingDocuments = new List<SupportingDocument> {
                        new SupportingDocument(userId) {
                            Id = Guid.NewGuid(),
                            Name = "Name",
                            Key = "key",
                            SupportingDocumentRevisions = new List<SupportingDocumentRevision> {
                                new SupportingDocumentRevision(userId)
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = "FileName",
                                    GdnId = "GDNID"
                                }
                            }
                        }
                    },
                CostFormDetails = new List<CostFormDetails>(),
            };
            newCostStage = new CostStage
            {
                CostId = costId,
                Created = DateTime.UtcNow,
                CreatedById = userId,
                Id = newCostStageId,
                Modified = DateTime.UtcNow,
                Key = CostStages.New.ToString(),
                StageOrder = 0
            };
            // IStageDetails stageDetails, CostType costType, IEnumerable<string> stageKeys
            _efContext.CostStageRevision.Add(currentCostStageRevision);
            _efContext.CustomFormData.Add(new CustomFormData { Data = "emptyString", Id = Guid.NewGuid() });
            _costBuilderMock.Setup(s => s.BuildSupportingDocuments(It.IsAny<IStageDetails>(), It.IsAny<CostType>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                  .ReturnsAsync(new List<SupportingDocumentModel>());
            
            _efContext.SaveChanges();
        }
    }
}
