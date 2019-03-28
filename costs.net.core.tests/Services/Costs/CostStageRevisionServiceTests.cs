namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using core.Services.Costs;
    using core.Services.User;
    using Castle.Components.DictionaryAdapter;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Serilog;
    using Microsoft.Extensions.Options;
    using core.Models;
    using core.Models.Costs;
    using core.Models.User;
    using core.Models.Utils;
    using Moq;
    using NUnit.Framework;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using plugins.PG.Models.Stage;
    using CostFormDetails = dataAccess.Entity.CostFormDetails;
    using Builders.Request;
    using Builders.Response.Cost;
    using core.Mapping;
    using core.Models.ACL;
    using core.Models.Workflow;
    using core.Services.ActivityLog;
    using core.Services.Events;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;
    using plugins;

    public class CostStageRevisionServiceTests
    {
        [TestFixture]
        public abstract class CostStageRevisionServiceTest
        {
            private Mock<ITechnicalFeeService> _technicalFeeService;
            private Mock<IEventService> _eventServiceMock;
            protected Mock<ICostStageRevisionPermissionService> RevisionPermissionService;
            protected Mock<ICostStageRevisionService> CostStageRevisionService;
            protected Mock<IApprovalService> ApprovalServiceMock;
            protected Mock<IOptions<AmqSettings>> AmqSettingsMock;
            protected UserIdentity User;
            protected Mock<ILogger> LoggerMock;
            protected CostStageRevisionService Service;
            protected EFContext EFContext;
            protected Mock<ICostBuilder> CostBuilderMock;
            protected Mock<IActivityLogService> ActivityLogServiceMock;
            public ICostApprovalService CostApprovalService;

            [SetUp]
            public void Setup()
            {
                LoggerMock = new Mock<ILogger>();
                ApprovalServiceMock = new Mock<IApprovalService>();
                AmqSettingsMock = new Mock<IOptions<AmqSettings>>();
                AmqSettingsMock.SetupGet(o => o.Value).Returns(new AmqSettings());
                EFContext = EFContextFactory.CreateInMemoryEFContext();
                _technicalFeeService = new Mock<ITechnicalFeeService>();
                RevisionPermissionService = new Mock<ICostStageRevisionPermissionService>();
                CostStageRevisionService = new Mock<ICostStageRevisionService>();

                CostBuilderMock = new Mock<ICostBuilder>();
                var costBuilders = new EditableList<Lazy<ICostBuilder, PluginMetadata>>
                {
                    new Lazy<ICostBuilder, PluginMetadata>(
                        () => CostBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
                };
                var userServiceMock = new Mock<IUserService>();

                var configuration = new MapperConfiguration(config =>
                    config.AddProfiles(
                        typeof(AdminProfile),
                        typeof(SupportingDocumentProfile)
                    )
                );
                var mapper = new Mapper(configuration);
                ActivityLogServiceMock = new Mock<IActivityLogService>();
                _eventServiceMock = new Mock<IEventService>();

                CostApprovalService = new CostApprovalService(ApprovalServiceMock.Object,
                    _technicalFeeService.Object,
                    EFContext,
                    costBuilders,
                    userServiceMock.Object,
                    ActivityLogServiceMock.Object,
                    _eventServiceMock.Object,
                    RevisionPermissionService.Object,
                    CostStageRevisionService.Object
                    );

                User = new UserIdentity
                {
                    Email = "UserName",
                    AgencyId = Guid.NewGuid(),
                    Id = Guid.NewGuid()
                };

                Service = new CostStageRevisionService(
                    LoggerMock.Object,
                    mapper,
                    EFContext,
                    new[] { new Lazy<ICostBuilder, PluginMetadata>(
                                () => CostBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg }) }
                );
            }

            protected void SetupRevision(out UserIdentity user, out Guid costStageRevisionId, out Guid newCostStageId, out CostStageRevision currentCostStageRevision, out CostStage newCostStage)
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
                            Name = "Name1",
                            Key = "key1",
                            SupportingDocumentRevisions = new List<SupportingDocumentRevision> {
                                new SupportingDocumentRevision(userId)
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = "FileName1",
                                    GdnId = "GDNID1"
                                }
                            }
                        },
                        new SupportingDocument(userId) {
                            Id = Guid.NewGuid(),
                            Name = "Name2",
                            Key = "key2",
                            SupportingDocumentRevisions = new List<SupportingDocumentRevision> {
                                new SupportingDocumentRevision(userId)
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = "FileName2",
                                    GdnId = "GDNID2"
                                }
                            }
                        },
                        new SupportingDocument(userId) {
                            Id = Guid.NewGuid(),
                            Name = "Name3",
                            Key = "key3",
                            Required = true,
                            SupportingDocumentRevisions = new List<SupportingDocumentRevision> {
                                new SupportingDocumentRevision(userId)
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = "FileName3",
                                    GdnId = "GDNID3"
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
                EFContext.CostStageRevision.Add(currentCostStageRevision);
                EFContext.CustomFormData.Add(new CustomFormData { Data = "emptyString", Id = Guid.NewGuid() });
                CostBuilderMock.Setup(s => s.BuildSupportingDocuments(It.IsAny<IStageDetails>(), It.IsAny<CostType>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Guid>(), false))
                    .ReturnsAsync(new List<SupportingDocumentModel>());
                CostBuilderMock.Setup(s => s.BuildSupportingDocuments(It.IsAny<IStageDetails>(), It.IsAny<CostType>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Guid>(), true))
                    .ReturnsAsync(new List<SupportingDocumentModel>
                    {
                        new SupportingDocumentModel()
                        {
                            Name = "Name2",
                            Required = true,
                        },
                        new SupportingDocumentModel()
                        {
                            Name = "Name3",
                            Required = true,
                        }
                    });

                var approvals = new List<Approval>
                {
                    new Approval
                    {
                        Status = ApprovalStatus.Approved,
                        CostStageRevisionId = costStageRevisionId
                    },
                };

                EFContext.Approval.AddRange(approvals);
                EFContext.SaveChanges();
            }
        }

        public class UpdateApprovalsShould : CostStageRevisionServiceTest
        {
            private static List<Approval> CreateExistingApprovals(CostStageRevision costStageRevision, bool addIpmMember = false, bool addRequisitioner = false, bool addBrandMember = false)
            {
                var ipmApproval = new Approval
                {
                    Id = Guid.NewGuid(),
                    Type = ApprovalType.IPM,
                    CostStageRevision = costStageRevision,
                    ApprovalMembers = new List<ApprovalMember>(),
                    Requisitioners = new List<Requisitioner>(),
                    ValidBusinessRoles = new []{ Constants.BusinessRole.Ipm }
                };
                var brandApproval = new Approval
                {
                    Id = Guid.NewGuid(),
                    Type = ApprovalType.Brand,
                    CostStageRevision = costStageRevision,
                    ApprovalMembers = new List<ApprovalMember>(),
                    Requisitioners = new List<Requisitioner>(),
                    ValidBusinessRoles = new[] { Constants.BusinessRole.BrandManager }
                };

                var existingApprovals = new List<Approval> { ipmApproval, brandApproval };

                if (addIpmMember)
                {
                    ipmApproval.ApprovalMembers.Add(new ApprovalMember
                    {
                        CostUser = new CostUser
                        {
                            Id = Guid.NewGuid(),
                            UserBusinessRoles = new List<UserBusinessRole>
                                {
                                    new UserBusinessRole
                                    {
                                        BusinessRole = new BusinessRole
                                        {
                                            Key = Constants.BusinessRole.Ipm
                                        }
                                    }
                                }
                        },
                        IsExternal = true,
                        CreatedById = Guid.NewGuid(),
                        Approval = ipmApproval
                    });
                }

                if (addBrandMember)
                {
                    brandApproval.ApprovalMembers.Add(new ApprovalMember
                    {
                        CostUser = new CostUser
                        {
                            Id = Guid.NewGuid(),
                            UserBusinessRoles = new List<UserBusinessRole>
                            {
                                new UserBusinessRole
                                {
                                    BusinessRole = new BusinessRole
                                    {
                                        Key = Constants.BusinessRole.BrandManager
                                    }
                                }
                            }
                        },
                        IsExternal = true,
                        CreatedById = Guid.NewGuid(),
                        Approval = brandApproval
                    });
                }

                if (addRequisitioner)
                {
                    brandApproval.Requisitioners.Add(new Requisitioner
                    {
                        CostUserId = Guid.NewGuid()
                    });
                }

                return existingApprovals;
            }

            private static ApprovalModel[] CreateUpdateApprovalsModel()
            {
                var updateApprovalsModel = new[]
                {
                    new ApprovalModel
                    {
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = Guid.NewGuid(),
                                FullName = "Test"
                            }
                        },
                        ValidBusinessRoles = new [] { Constants.BusinessRole.Ipm }
                    },
                    new ApprovalModel
                    {
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = Guid.NewGuid(),
                                FullName = "Test"
                            }
                        },
                        ValidBusinessRoles = new [] { Constants.BusinessRole.BrandManager }
                    }
                };
                return updateApprovalsModel;
            }

            private CostStageRevision SetupCostStageRevision(Guid costStageRevisionId, Guid costId, bool mockApproval = false)
            {
                var costStageId = Guid.NewGuid();
                var revision = new CostStageRevision
                {
                    Id = costStageRevisionId,
                    StageDetails = new CustomFormData
                    {
                        Id = new Guid(),
                        Data = JsonConvert.SerializeObject(new Dictionary<string, dynamic>())
                    },
                    CostStageId = costStageId,
                    CostStage = new CostStage
                    {
                        Id = costStageId,

                        CostId = costId,
                        Cost = new Cost
                        {
                            Id = costId,
                            CostType = CostType.Buyout,
                        }
                    }
                };

                if (mockApproval)
                {
                    CostBuilderMock.Setup(s => s.GetApprovals(It.IsAny<CostType>(), It.IsAny<IStageDetails>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ReturnsAsync(new List<Builders.Response.ApprovalModel>());
                }
                return revision;
            }

            private void SetupMock(Guid oldRevisionId, bool addIpmMember, bool addRequisitioner, bool addBrandMember, Guid costStageRevisionId, Guid costId, Builders.Response.ApprovalModel approvalModels)
            {
                var oldRevision = SetupCostStageRevision(oldRevisionId, costId);

                var existingApprovals = CreateExistingApprovals(oldRevision, addIpmMember, addRequisitioner, addBrandMember);

                var updateApprovalsModel = CreateUpdateApprovalsModel();

                EFContext.Approval.AddRange(existingApprovals);
                EFContext.CostUser.AddRange(updateApprovalsModel.SelectMany(am => am.ApprovalMembers).Select(m => new CostUser { Id = m.Id }));

                EFContext.Role.Add(new Role { Id = Guid.NewGuid(), Name = "cost.approver" });

                var revision = SetupCostStageRevision(costStageRevisionId, costId);
                EFContext.CostStageRevision.AddRange(revision, oldRevision);

                CostBuilderMock.Setup(s => s.GetApprovals(It.IsAny<CostType>(), It.IsAny<IStageDetails>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(new List<Builders.Response.ApprovalModel> { approvalModels });

                EFContext.SaveChanges();
            }

            [Test]
            public async Task AddNewApprovalMembers()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var revision = SetupCostStageRevision(costStageRevisionId, costId, true);
                var existingApprovals = CreateExistingApprovals(revision);

                var updateApprovalsModel = CreateUpdateApprovalsModel();

                EFContext.CostUser.AddRange(updateApprovalsModel.SelectMany(am => am.ApprovalMembers).Select(m => new CostUser
                {
                    Id = m.Id,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.Ipm
                            }
                        },
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager
                            }
                        }
                    }
                }));
                EFContext.Approval.AddRange(existingApprovals);

                EFContext.Role.Add(new Role { Id = Guid.NewGuid(), Name = "cost.approver" });

                var oldRevision = SetupCostStageRevision(oldRevisionId, costId, true);
                EFContext.CostStageRevision.AddRange(oldRevision);
                
                ApprovalServiceMock.Setup(a => 
                        a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), false)
                    )
                .ReturnsAsync(existingApprovals.Select(a => new Approval
                {
                    Id = a.Id,
                    Type = a.Type,
                    Status = a.Status,
                    CostStageRevisionId = a.CostStageRevisionId,
                    CreatedById = a.CreatedById,
                    ApprovalMembers = new List<ApprovalMember>(),
                    Requisitioners = new List<Requisitioner>(),
                    Created = a.Created,
                    Modified = a.Modified,
                    ValidBusinessRoles = a.ValidBusinessRoles
                }).ToList());

                EFContext.Role.Add(new Role { Id = Guid.NewGuid(), Name = "cost.approver" });
                EFContext.SaveChanges();

                var ipmApprovalModel = new Builders.Response.ApprovalModel
                {
                    Type = ApprovalType.IPM,
                    ValidBusinessRoles = new [] { Constants.BusinessRole.Ipm }
                };
                var brandApprovalModel = new Builders.Response.ApprovalModel
                {
                    Type = ApprovalType.Brand,
                    ValidBusinessRoles = new [] { Constants.BusinessRole.BrandManager }
                };

                CostBuilderMock.Setup(b => 
                        b.GetApprovals(It.IsAny<CostType>(), It.IsAny<IStageDetails>(), It.IsAny<Guid>(), costStageRevisionId, costId))
                    .ReturnsAsync(new List<Builders.Response.ApprovalModel> { ipmApprovalModel, brandApprovalModel });

                // Act
                await CostApprovalService.UpdateApprovals(costId, costStageRevisionId, User, updateApprovalsModel);

                // Assert
                EFContext.Entry(revision).Collection(r => r.Approvals).Load();

                var ipmApproval = await EFContext.Approval
                        .Include(a => a.ApprovalMembers)
                    .FirstAsync(a => a.CostStageRevisionId == costStageRevisionId && a.Type == ApprovalType.IPM);

                var brandApproval = await EFContext.Approval
                        .Include(a => a.ApprovalMembers)
                    .FirstAsync(a => a.CostStageRevisionId == costStageRevisionId && a.Type == ApprovalType.Brand);
                ipmApproval.ApprovalMembers.Count.Should().Be(1);
                brandApproval.ApprovalMembers.Count.Should().Be(1);
            }

            [Test]
            public async Task UpdateApprovalAddIPMMembersFromPerviousRevision()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var approvalModels = new Builders.Response.ApprovalModel
                {
                    Status = ApprovalStatus.New,
                    Type = ApprovalType.IPM
                };
                const bool addIpmMember = true;
                const bool addRequisitioner = false;
                const bool addBrandMember = false;

                SetupMock(oldRevisionId, addIpmMember, addRequisitioner, addBrandMember, costStageRevisionId, costId, approvalModels);

                // Act
                await CostApprovalService.UpdateApprovals(costId, costStageRevisionId, User, new List<ApprovalModel>());

                // Assert
                var members = await EFContext.Approval
                    .Where(s => s.Type == ApprovalType.IPM && s.CostStageRevisionId == costStageRevisionId)
                    .Select(a => a.ApprovalMembers)
                    .ToListAsync();

                members.Count.Should().Be(1);
            }

            [Test]
            public async Task UpdateApprovalNotRemovingIPMMembersFromPerviousRevision()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var approvalModels = new Builders.Response.ApprovalModel
                {
                    Status = ApprovalStatus.New,
                    Type = ApprovalType.IPM
                };
                const bool addIpmMember = true;
                const bool addRequisitioner = false;
                const bool addBrandMember = false;

                SetupMock(oldRevisionId, addIpmMember, addRequisitioner, addBrandMember, costStageRevisionId, costId, approvalModels);

                // Act
                await CostApprovalService.UpdateApprovals(costId, costStageRevisionId, User, new List<ApprovalModel>());

                // Assert
                var members = await EFContext.Approval.Where(s => s.Type == ApprovalType.IPM && s.CostStageRevisionId == oldRevisionId).Select(a => a.ApprovalMembers).ToListAsync();
                Assert.AreEqual(members.Count, 1);
            }

            [Test]
            public async Task UpdateApprovalAddBrandMembersFromPerviousRevision()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var approvalModels = new Builders.Response.ApprovalModel
                {
                    Status = ApprovalStatus.New,
                    Type = ApprovalType.Brand
                };
                const bool addIpmMember = false;
                const bool addRequisitioner = false;
                const bool addBrandMember = true;

                SetupMock(oldRevisionId, addIpmMember, addRequisitioner, addBrandMember, costStageRevisionId, costId, approvalModels);

                // Act
                await CostApprovalService.UpdateApprovals(costId, costStageRevisionId, User, new List<ApprovalModel>());

                // Assert
                EFContext.Approval.Where(s => s.Type == ApprovalType.Brand && s.CostStageRevisionId == costStageRevisionId).Should().HaveCount(1);
            }

            [Test]
            public async Task UpdateApprovalAddRequisitionerFromPerviousRevision()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var approvalModels = new Builders.Response.ApprovalModel
                {
                    Status = ApprovalStatus.New,
                    Type = ApprovalType.Brand
                };

                const bool addIpmMember = false;
                const bool addRequisitioner = true;
                const bool addBrandMember = false;

                SetupMock(oldRevisionId, addIpmMember, addRequisitioner, addBrandMember, costStageRevisionId, costId, approvalModels);
                
                // Act
                await CostApprovalService.UpdateApprovals(costId, costStageRevisionId, User, new List<ApprovalModel>());

                // Assert
                EFContext.Approval.Where(s => s.Type == ApprovalType.Brand && s.CostStageRevisionId == costStageRevisionId).Should().HaveCount(1);
            }

            [Test]
            public void AddNewApprovals_Fails()
            {
                // Arrange
                var costId = Guid.NewGuid();

                var updateApprovalsModel = CreateUpdateApprovalsModel();

                EFContext.Role.Add(new Role { Id = Guid.NewGuid(), Name = "cost.approver" });
                EFContext.CostUser.AddRange(updateApprovalsModel.SelectMany(am => am.ApprovalMembers).Select(m => new CostUser { Id = m.Id }));
                EFContext.Cost.Add(new Cost { Id = costId });
                EFContext.SaveChanges();

                ApprovalServiceMock.Setup(a => a.GetApprovalsByCostStageRevisionId(It.IsAny<Guid>(), false)).ReturnsAsync(new List<Approval>());

                // Act, Assert
                Assert.ThrowsAsync<InvalidOperationException>(() => CostApprovalService.UpdateApprovals(costId, Guid.Empty, User, updateApprovalsModel));
            }

            [Test]
            public async Task UpdateApproval_Always_Should_GrantReadPermissionToNewApprovers()
            {
                // Arrange
                var costStageRevisionId = Guid.NewGuid();
                var oldRevisionId = Guid.NewGuid();
                var costId = Guid.NewGuid();
                var newIPMMemberId = Guid.NewGuid();
                var newBrandMemberId = Guid.NewGuid();
                var existingApprovalModels = new Builders.Response.ApprovalModel
                {
                    Status = ApprovalStatus.New,
                    Type = ApprovalType.IPM
                };
                var newApprovalModels = new List<ApprovalModel>
                {
                    new ApprovalModel
                    {
                        Status = ApprovalStatus.New,
                        Type = ApprovalType.IPM,
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = newIPMMemberId
                            }
                        }
                    },
                    new ApprovalModel
                    {
                        Status = ApprovalStatus.New,
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = newBrandMemberId
                            }
                        }
                    }
                };
                const bool addIpmMember = true;
                const bool addRequisitioner = false;
                const bool addBrandMember = true;

                SetupMock(oldRevisionId, addIpmMember, addRequisitioner, addBrandMember, costStageRevisionId, costId, existingApprovalModels);

                EFContext.CostUser.AddRange(new List<CostUser>
                {
                    new CostUser
                    {
                        Id = newIPMMemberId
                    },
                    new CostUser
                    {
                        Id = newBrandMemberId
                    }
                });
                EFContext.SaveChanges();

                // Act
                await CostApprovalService.UpdateApprovals(costId, oldRevisionId, User, newApprovalModels);

                // Assert
                RevisionPermissionService.Verify(rp => 
                    rp.GrantCostPermission(costId, Roles.CostViewer, It.Is<IEnumerable<CostUser>>(en => en.Count() == 2), BuType.Pg, It.IsAny<Guid?>(), true),
                    Times.Once // One call which includes 2 members: IPM and Brand
                );
            }

        }

        public class MoveToStageShoud : CostStageRevisionServiceTest
        {
            [Test]
            public async Task ReplicateRevisionStageOnReject()
            {
                // Arrange
                SetupRevision(out var user, out var costStageRevisionId, out var newCostStageId, out var currentCostStageRevision, out var newCostStage);
                
                // Act
                var result = await Service.MoveToStage(currentCostStageRevision, newCostStage, user.Id, BuType.Pg, CostAction.Reject);

                // Assert
                result.Should().NotBeNull();
                result.Should().NotBe(currentCostStageRevision);
                result.Id.Should().NotBe(costStageRevisionId);
                result.SupportingDocuments.SelectMany(s => s.SupportingDocumentRevisions).Should().BeEmpty();
                result.CostStageId.Should().Be(newCostStageId);
            }

            [Test]
            public async Task ReplicateRevisionStageOnReOpen()
            {
                // Arrange
                SetupRevision(out var user, out var costStageRevisionId, out var newCostStageId, out var currentCostStageRevision, out var newCostStage);
                
                // Act
                var result = await Service.MoveToStage(currentCostStageRevision, newCostStage, user.Id, BuType.Pg, CostAction.Reopen);
                
                // Assert
                result.Should().NotBeNull();
                result.Should().NotBe(currentCostStageRevision);
                result.Id.Should().NotBe(costStageRevisionId);
                result.SupportingDocuments.SelectMany(s => s.SupportingDocumentRevisions).Should().NotBeEmpty();
                result.CostStageId.Should().Be(newCostStageId);
            }
        }

        public class CreateVersionShould : CostStageRevisionServiceTest
        {
            [Test]
            public async Task CopyApprovalsWithStatusWhenRequestReopen()
            {
                // Arrange
                var currentRevision = new CostStageRevision
                {
                    CostStage = new CostStage { Cost = new Cost() },
                    Approvals = new List<Approval>
                    {
                        new Approval
                        {
                            Status = ApprovalStatus.Approved,
                            ApprovalMembers = new List<ApprovalMember>
                            {
                                new ApprovalMember
                                {
                                    Status = ApprovalStatus.Approved
                                }
                            }
                        }
                    }
                };
                var userId = Guid.NewGuid();
                var buType = BuType.Pg;
                var action = CostAction.RequestReopen;

                // Act
                var newRevision = await Service.CreateVersion(currentRevision, userId, buType, action);

                // Assert
                newRevision.Approvals.Should().HaveSameCount(currentRevision.Approvals);
                newRevision.Approvals[0].Status.Should().Be(currentRevision.Approvals[0].Status);
                newRevision.Approvals[0].ApprovalMembers.Should().HaveSameCount(currentRevision.Approvals[0].ApprovalMembers);
                newRevision.Approvals[0].ApprovalMembers[0].Status.Should().Be(currentRevision.Approvals[0].ApprovalMembers[0].Status);
            }
        }

        public class GetPreviousRevisionShould : CostStageRevisionServiceTest
        {
            private static readonly CostStageRevisionStatus[] ValidStatusesOfPreviousRevision = Enum.GetValues(typeof(CostStageRevisionStatus))
                .Cast<CostStageRevisionStatus>()
                .Where(c => 
                    c == CostStageRevisionStatus.Approved 
                    || c == CostStageRevisionStatus.PendingBrandApproval
                    || c == CostStageRevisionStatus.PendingTechnicalApproval)
                .ToArray();

            private static readonly CostStageRevisionStatus[] InvalidStatusesOfPreviousRevision = Enum.GetValues(typeof(CostStageRevisionStatus))
                .Cast<CostStageRevisionStatus>()
                .Where(c => !ValidStatusesOfPreviousRevision.Contains(c))
                .ToArray();

            [Test]
            [TestCaseSource(nameof(ValidStatusesOfPreviousRevision))]
            public async Task ReturnPreviousRevision_WhenStatusIsApprovedOrPendingApproval(CostStageRevisionStatus status)
            {
                // Arrange
                var costStageKey = CostStages.FirstPresentation.ToString();
                var costId = Guid.NewGuid();
                var createdById = Guid.NewGuid();
                var revision1 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow,
                    Status = status
                };
                var revision2 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow.AddMilliseconds(1)
                };
                var costStage = new CostStage
                {
                    Key = costStageKey,
                    Cost = new Cost
                    {
                        Id = costId
                    },
                    CostStageRevisions = new List<CostStageRevision> { revision1, revision2 }
                };
                EFContext.CostStage.Add(costStage);
                await EFContext.SaveChangesAsync();

                // Act
                var previousRevision = await Service.GetPreviousRevision(revision2.Id);

                // Assert
                previousRevision.Id.Should().Be(revision1.Id);
            }

            [Test]
            public async Task ReturnPreviousRevisionOfFromPreviousStage()
            {
                // Arrange
                var costStageKey1 = CostStages.OriginalEstimate.ToString();
                var costStageKey2 = CostStages.FirstPresentation.ToString();
                var costId = Guid.NewGuid();
                var createdById = Guid.NewGuid();
                var revision1 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow,
                    Status = CostStageRevisionStatus.Approved
                };
                var revision2 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow.AddMilliseconds(1)
                };
                var costStage1 = new CostStage
                {
                    Key = costStageKey1,
                    Cost = new Cost
                    {
                        Id = costId
                    },
                    CostStageRevisions = new List<CostStageRevision> { revision1 }
                };
                var costStage2 = new CostStage
                {
                    Key = costStageKey2,
                    Cost = new Cost
                    {
                        Id = costId
                    },
                    CostStageRevisions = new List<CostStageRevision> { revision2 }
                };
                EFContext.CostStage.Add(costStage1);
                EFContext.CostStage.Add(costStage2);
                await EFContext.SaveChangesAsync();

                // Act
                var previousRevision = await Service.GetPreviousRevision(revision2.Id);

                // Assert
                previousRevision.Id.Should().Be(revision1.Id);
            }

            [Test]
            [TestCaseSource(nameof(InvalidStatusesOfPreviousRevision))]
            public async Task ReturnNull_WhenPreviousRevisionOfTheSameStage_AndStatusIsNotApprovedOrPendingApproval(CostStageRevisionStatus status)
            {
                // Arrange
                var costStageKey = CostStages.FirstPresentation.ToString();
                var costId = Guid.NewGuid();
                var createdById = Guid.NewGuid();
                var revision1 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow,
                    Status = status
                };
                var revision2 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow.AddMilliseconds(1)
                };
                var costStage = new CostStage
                {
                    Key = costStageKey,
                    Cost = new Cost
                    {
                        Id = costId
                    },
                    CostStageRevisions = new List<CostStageRevision> { revision1, revision2 }
                };
                EFContext.CostStage.Add(costStage);
                await EFContext.SaveChangesAsync();

                // Act
                var previousRevision = await Service.GetPreviousRevision(revision2.Id);

                // Assert
                previousRevision.Should().BeNull();
            }

            [Test]
            public async Task ReturnNull_WhenNoPreviousRevision()
            {
                // Arrange
                var costStageKey = CostStages.FirstPresentation.ToString();
                var costId = Guid.NewGuid();
                var createdById = Guid.NewGuid();
                var revision2 = new CostStageRevision
                {
                    Id = Guid.NewGuid(),
                    CreatedById = createdById,
                    Created = DateTime.UtcNow
                };
                var costStage = new CostStage
                {
                    Key = costStageKey,
                    Cost = new Cost
                    {
                        Id = costId
                    },
                    CostStageRevisions = new List<CostStageRevision> { revision2 }
                };
                EFContext.CostStage.Add(costStage);
                await EFContext.SaveChangesAsync();

                // Act
                var previousRevision = await Service.GetPreviousRevision(revision2.Id);

                // Assert
                previousRevision.Should().BeNull();
            }
        }

        public class UpdateSupportingDocumentsForRevisionShould : CostStageRevisionServiceTest
        {
            [Test]
            public async Task SetRequiredToFalse_WhenFilesAreNotNeeded()
            {
                // Arrange
                SetupRevision(out var user, out var costStageRevisionId, out var newCostStageId, out var currentCostStageRevision, out var newCostStage);

                // Act
                var result = await Service.UpdateSupportingDocumentsForRevision(currentCostStageRevision, BuType.Pg, CostType.Buyout, false);

                // Assert
                result.Should().NotBeNull();
                result.Count.Should().Be(2);
                result.Count(sd => !sd.Required).Should().Be(2);
            }

            [Test]
            public async Task RemoveRequired_WhenRequiredConditionAreChanged()
            {
                // Arrange
                SetupRevision(out var user, out var costStageRevisionId, out var newCostStageId, out var currentCostStageRevision, out var newCostStage);

                // Act
                var result = await Service.UpdateSupportingDocumentsForRevision(currentCostStageRevision, BuType.Pg, CostType.Buyout, true);

                result.Should().NotBeNull();
                result.Count.Should().Be(3);
                result.Where(sd => sd.Required).Count().Should().Be(1);
                result.Where(sd => !sd.Required).Count().Should().Be(2);

                // second Act without total cost increased
                result = await Service.UpdateSupportingDocumentsForRevision(currentCostStageRevision, BuType.Pg, CostType.Buyout, false);

                // Assert
                result.Should().NotBeNull();
                result.Count.Should().Be(2);
                result.Where(sd => sd.Required).Count().Should().Be(0);
                result.Where(sd => !sd.Required).Count().Should().Be(2);
            }
        }
    }
}