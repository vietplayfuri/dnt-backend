namespace costs.net.core.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using Builders.Response;
    using Builders.Response.Cost;
    using Builders.Workflow;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using core.Models.Costs;
    using core.Models.User;
    using Moq;
    using NUnit.Framework;
    using core.Services;
    using core.Services.Search;
    using Castle.Components.DictionaryAdapter;
    using ExternalResource.Paperpusher;
    using Serilog;
    using core.Models;
    using core.Models.ACL;
    using core.Models.BusinessRole;
    using core.Models.Response;
    using core.Models.Workflow;
    using core.Services.ActivityLog;
    using core.Services.Notifications;
    using core.Services.PostProcessing;
    using core.Services.User;
    using net.tests.common.Stubs.EFContext;
    using CostStageModel = Builders.Response.Cost.CostStageModel;
    using Newtonsoft.Json.Linq;
    using costs.net.core.Services.Module;

    public class CostServiceTests
    {
        [TestFixture]
        public abstract class CostServiceTest
        {
            protected Mock<IPaperpusherClient> PaperPusherClientMock;
            protected Mock<ICostBuilder> CostBuilderMock;
            protected Mock<IPgUserService> PgUserServiceMock;
            protected Mock<IEventService> EventService;
            protected Mock<IPermissionService> PermissionService;
            protected Mock<IElasticSearchService> SearchServiceMock;
            protected Mock<ILogger> Logger;
            protected List<Lazy<ICostBuilder, PluginMetadata>> CostBuilders;
            protected UserIdentity User;
            protected CostService CostService;
            protected Lazy<IActionBuilder, PluginMetadata>[] ActionBuilders;
            protected Mock<IActionBuilder> ActionBuilderMock;
            protected List<Lazy<IPgUserService, PluginMetadata>> UserServices;
            protected Mock<IMapper> Mapper;
            protected Mock<IActivityLogService> ActivityLogServiceMock;
            protected IEnumerable<Lazy<IActionPostProcessor, PluginMetadata>> PostProcessors;
            protected Mock<IActionPostProcessor> PostProcessorMock;
            protected Mock<ICostStageRevisionPermissionService> CostStageRevisionPermissionServiceMock;
            protected Mock<ISupportNotificationService> SupportNotificationServiceMock;
            protected Mock<IModuleService> ModuleService;
            protected EFContext EFContext;

            [SetUp]
            public void Setup()
            {
                Logger = new Mock<ILogger>();
                PaperPusherClientMock = new Mock<IPaperpusherClient>();
                CostBuilderMock = new Mock<ICostBuilder>();
                PgUserServiceMock = new Mock<IPgUserService>();
                EventService = new Mock<IEventService>();
                PermissionService = new Mock<IPermissionService>();
                SearchServiceMock = new Mock<IElasticSearchService>();
                ActionBuilderMock = new Mock<IActionBuilder>();
                ModuleService = new Mock<IModuleService>();
                ActionBuilders = new[]
                {
                    new Lazy<IActionBuilder, PluginMetadata>(
                        () => ActionBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
                };
                UserServices = new EditableList<Lazy<IPgUserService, PluginMetadata>>
                {
                    new Lazy<IPgUserService, PluginMetadata>(
                        () => PgUserServiceMock.Object, new PluginMetadata { BuType = BuType.Pg })
                };
                var costApprovalServiceMock = new Mock<ICostApprovalService>();
                Mapper = new Mock<IMapper>();
                ActivityLogServiceMock = new Mock<IActivityLogService>();

                CostBuilders = new EditableList<Lazy<ICostBuilder, PluginMetadata>>
                {
                    new Lazy<ICostBuilder, PluginMetadata>(
                        () => CostBuilderMock.Object, new PluginMetadata { BuType = BuType.Pg })
                };
                PostProcessorMock = new Mock<IActionPostProcessor>();
                PostProcessors = new EditableList<Lazy<IActionPostProcessor, PluginMetadata>>
                {
                    new Lazy<IActionPostProcessor, PluginMetadata>(
                        () => PostProcessorMock.Object, new PluginMetadata { BuType = BuType.Pg })
                };

                User = new UserIdentity
                {
                    Email = "UserName",
                    AgencyId = Guid.NewGuid(),
                    BuType = BuType.Pg,
                    Id = Guid.NewGuid()
                };
                CostStageRevisionPermissionServiceMock = new Mock<ICostStageRevisionPermissionService>();
                SupportNotificationServiceMock = new Mock<ISupportNotificationService>();
                EFContext = EFContextFactory.CreateInMemoryEFContext();

                CostService = new CostService(
                    CostBuilders,
                    UserServices,
                    EventService.Object,
                    PermissionService.Object,
                    PaperPusherClientMock.Object,
                    Logger.Object,
                    SearchServiceMock.Object,
                    EFContext,
                    ActionBuilders,
                    costApprovalServiceMock.Object,
                    Mapper.Object,
                    ActivityLogServiceMock.Object,
                    PostProcessors,
                    CostStageRevisionPermissionServiceMock.Object,
                    SupportNotificationServiceMock.Object,
                    ModuleService.Object
                );
            }
        }

        [TestFixture]
        public class CreateCostShould : CostServiceTest
        {
            private static CreateCostModel CreateCostModel(Guid templateId)
            {
                return new CreateCostModel
                {
                    TemplateId = templateId,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "costNumber", "XYZZZZZZ" },
                            { "agencyName", "XYZZZZZZ" },
                            { "projectId", "123456789" },
                        }
                    }
                };
            }

            [Test]
            public async Task InsertCost()
            {
                //Setup
                var adCostNumber = "adCostNumber";
                var templateId = Guid.NewGuid();
                var requestModel = CreateCostModel(templateId);
                var costModel = new CostBuilderModel
                {
                    Stages = new[]
                    {
                        new CostStageModel
                        {
                            Revisions = new []
                            {
                                new CostStageRevisionModel
                                {
                                    SupportingDocuments = new List<SupportingDocumentModel>()
                                }
                            }
                        }
                    }
                };

                var costUser = new CostUser
                {
                    Id = User.Id,
                    Agency = new Agency
                    {
                        Id = User.AgencyId,

                    }
                };

                var responseMock = new Mock<ICreateCostResponse>();
                responseMock.SetupGet(f => f.Cost).Returns(costModel);
                PgUserServiceMock.Setup(a => a.GrantUsersAccess(It.IsAny<Cost>(), It.IsAny<CreateCostModel>())).Returns(Task.CompletedTask);
                CostBuilderMock.Setup(e => e.CreateCost(It.IsAny<CostUser>(), It.IsAny<CreateCostModel>()))
                    .ReturnsAsync(responseMock.Object);

                EFContext.Project.Add(new Project { Id = Guid.NewGuid(), AdCostNumber = adCostNumber });
                EFContext.CostUser.Add(costUser);
                await EFContext.SaveChangesAsync();

                PermissionService.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(new[] { Guid.NewGuid().ToString() });
                //Act
                await CostService.CreateCost(User, requestModel);

                //Assert
                CostBuilderMock.Verify(e => e.CreateCost(costUser, requestModel), Times.Once);
                EFContext.Cost.Should().HaveCount(1);
                EFContext.NotificationSubscriber.Should().HaveCount(0);
                Mapper.Verify(m => m.Map<CostModel>(It.Is<Cost>(c => c != null && c.Status == CostStageRevisionStatus.Draft)));
            }

            [Test]
            public async Task InsertDependentItems()
            {
                // Arrange
                var adCostNumber = "adCostNumber";
                var templateId = Guid.NewGuid();
                var requestModel = CreateCostModel(templateId);

                var supportingDocumentModel = new SupportingDocumentModel();
                var costStageRevisionModel = new CostStageRevisionModel
                {
                    StageDetails = @"{ ""abc"": ""abc"" }",
                    SupportingDocuments = new[] { supportingDocumentModel }
                };
                var costStageModel = new CostStageModel
                {
                    Revisions = new[] { costStageRevisionModel }
                };
                var costModel = new CostBuilderModel
                {
                    Stages = new[] { costStageModel }
                };
                var costUser = new CostUser
                {
                    Id = User.Id,
                    Agency = new Agency
                    {
                        Id = User.AgencyId,

                    }
                };

                var responseMock = new Mock<ICreateCostResponse>();
                responseMock.SetupGet(f => f.Cost).Returns(costModel);
                CostBuilderMock.Setup(e => e.CreateCost(It.IsAny<CostUser>(), It.IsAny<CreateCostModel>()))
                    .ReturnsAsync(responseMock.Object);

                EFContext.Project.Add(new Project { Id = Guid.NewGuid(), AdCostNumber = adCostNumber });
                EFContext.CostUser.Add(costUser);
                await EFContext.SaveChangesAsync();

                // Act
                await CostService.CreateCost(User, requestModel);

                // Assert
                EFContext.Cost.Should().HaveCount(1);
                var cost = EFContext.Cost.First();

                EFContext.CostStage.Should().HaveCount(1);
                var costStage = EFContext.CostStage.First();
                costStage.CostId.Should().Be(cost.Id);

                EFContext.CostStageRevision.Should().HaveCount(1);
                var costStageRevision = EFContext.CostStageRevision.First();
                costStageRevision.CostStageId.Should().Be(costStage.Id);
                costStageRevision.StageDetails.Should().NotBeNull();
                costStageRevision.ProductDetails.Should().NotBeNull();
                costStageRevision.SupportingDocuments.Should().NotBeNull();
            }

            [Test]
            public async Task GrandEditPermissionToOwner()
            {
                //Setup
                var adCostNumber = "adCostNumber";
                var templateId = Guid.NewGuid();
                var requestModel = CreateCostModel(templateId);
                var costModel = new CostBuilderModel
                {
                    Stages = new[]
                    {
                        new CostStageModel
                        {
                            Revisions = new []
                            {
                                new CostStageRevisionModel
                                {
                                    SupportingDocuments = new List<SupportingDocumentModel>()
                                }
                            }
                        }
                    }
                };

                var costUser = new CostUser
                {
                    Id = User.Id,
                    Agency = new Agency
                    {
                        Id = User.AgencyId,

                    }
                };

                var responseMock = new Mock<ICreateCostResponse>();
                responseMock.SetupGet(f => f.Cost).Returns(costModel);
                PgUserServiceMock.Setup(a => a.GrantUsersAccess(It.IsAny<Cost>(), It.IsAny<CreateCostModel>())).Returns(Task.CompletedTask);
                CostBuilderMock.Setup(e => e.CreateCost(It.IsAny<CostUser>(), It.IsAny<CreateCostModel>()))
                    .ReturnsAsync(responseMock.Object);

                EFContext.Project.Add(new Project { Id = Guid.NewGuid(), AdCostNumber = adCostNumber });
                EFContext.CostUser.Add(costUser);
                await EFContext.SaveChangesAsync();

                PermissionService.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(new[] { Guid.NewGuid().ToString() });
                //Act
                await CostService.CreateCost(User, requestModel);

                //Assert
                CostStageRevisionPermissionServiceMock.Verify(p =>
                    p.GrantCostPermission(It.IsAny<Guid>(), Roles.CostEditor, It.Is<IEnumerable<CostUser>>(c => c.Any(i => i == costUser)), BuType.Pg, It.IsAny<Guid?>(), true),
                    Times.Once);
            }
        }

        public class GetCostWatchersShould : CostServiceTest
        {
            [Test]
            public async Task Get_watchers_by_costId()
            {
                // Arrange
                var costId = Guid.NewGuid();
                var firstUserId = Guid.NewGuid();
                var secondUserId = Guid.NewGuid();

                var notificationSubscribers = new List<NotificationSubscriber>
                {
                    new NotificationSubscriber
                    {
                        Id = Guid.NewGuid(),
                        CostId = costId,
                        CostUserId = firstUserId,
                        CostUser = new CostUser
                        {
                            Id = firstUserId,
                            FirstName = "Clark",
                            LastName = "Kent",
                            UserBusinessRoles = new List<UserBusinessRole>
                            {
                                new UserBusinessRole
                                {
                                    BusinessRole = new BusinessRole
                                    {
                                        Value = "Super Man"
                                    }
                                }
                            }
                        }
                    },
                    new NotificationSubscriber
                    {
                        Id = Guid.NewGuid(),
                        CostId = costId,
                        CostUserId = secondUserId,
                        CostUser = new CostUser
                        {
                            Id = secondUserId,
                            FirstName = "Bruce",
                            LastName = "Wayne",
                            UserBusinessRoles = new List<UserBusinessRole>
                            {
                                new UserBusinessRole
                                {
                                    BusinessRole = new BusinessRole
                                    {
                                        Value = "Bat Man"
                                    }
                                }
                            }
                        }
                    }
                };

                EFContext.NotificationSubscriber.AddRange(notificationSubscribers);
                await EFContext.SaveChangesAsync();

                Mapper.Setup(m => m.Map<List<CostWatcherModel>>(It.IsAny<List<NotificationSubscriber>>())).Returns(new List<CostWatcherModel>
                {
                    new CostWatcherModel
                    {
                        BusinessRoles = new List<BusinessRoleModel>
                        {
                            new BusinessRoleModel
                            {
                                Value = "Super Man"
                            }
                        },
                        CostId = costId,
                        FullName = "Clark Kent",
                        UserId = firstUserId,
                        Owner = true
                    },
                    new CostWatcherModel
                    {
                        BusinessRoles = new List<BusinessRoleModel>
                        {
                            new BusinessRoleModel
                            {
                                Value = "Bat Man"
                            }
                        },
                        CostId = costId,
                        FullName = "Bruce Wayne",
                        UserId = secondUserId,
                        Owner = false
                    }
                });
                // Act
                var watchers = await CostService.GetCostWatchers(costId);

                // Assert
                watchers.Count.Should().Be(2);
                watchers.First().BusinessRoles.First().Value.Should().Be("Super Man");
                watchers.Last().BusinessRoles.First().Value.Should().Be("Bat Man");
                watchers.First().Owner.Should().Be(true);
                watchers.Last().Owner.Should().Be(false);
            }
        }

        public class RemoveCostWatchersShould : CostServiceTest
        {
            [Test]
            public async Task Remove_watchers_by_costId()
            {
                // Arrange
                var costId = Guid.NewGuid();
                var cost = new Cost
                {
                    Id = costId,
                    CostNumber = "Test101"
                };
                var firstUserId = Guid.NewGuid();
                var secondUserId = Guid.NewGuid();

                var notificationSubscribers = new List<NotificationSubscriber>
                {
                    new NotificationSubscriber
                    {
                        Id = Guid.NewGuid(),
                        CostId = costId,
                        CostUserId = firstUserId,
                        CostUser = new CostUser
                        {
                            Id = firstUserId,
                            FirstName = "Clark",
                            LastName = "Kent",
                            UserBusinessRoles = new List<UserBusinessRole>
                            {
                                new UserBusinessRole
                                {
                                    BusinessRole = new BusinessRole
                                    {
                                        Value = "Super Man"
                                    }
                                }
                            }
                        }
                    },
                    new NotificationSubscriber
                    {
                        Id = Guid.NewGuid(),
                        CostId = costId,
                        CostUserId = secondUserId,
                        CostUser = new CostUser
                        {
                            Id = secondUserId,
                            FirstName = "Bruce",
                            LastName = "Wayne",
                            UserBusinessRoles = new List<UserBusinessRole>
                            {
                                new UserBusinessRole
                                {
                                    BusinessRole = new BusinessRole
                                    {
                                        Value = "Bat Man"
                                    }
                                }
                            }
                        }
                    }
                };

                EFContext.Cost.Add(cost);
                EFContext.NotificationSubscriber.AddRange(notificationSubscribers);
                await EFContext.SaveChangesAsync();

                Mapper.Setup(m => m.Map<List<CostWatcherModel>>(It.IsAny<List<NotificationSubscriber>>())).Returns(new List<CostWatcherModel>
                {
                    new CostWatcherModel
                    {
                        BusinessRoles = new List<BusinessRoleModel>
                        {
                            new BusinessRoleModel
                            {
                                Value = "Bat Man"
                            }
                        },
                        CostId = costId,
                        FullName = "Bruce Wayne",
                        UserId = secondUserId,
                        Owner = false
                    }
                });

                // Act
                var watchers = await CostService.RemoveCostWatchers(costId, new List<Guid> { firstUserId }, User);

                // Assert
                watchers.Count.Should().Be(1);
                watchers.First().CostId.Should().Be(costId);
                watchers.First().UserId.Should().Be(secondUserId);

                EFContext.NotificationSubscriber.First().CostId.Should().Be(costId);
                EFContext.NotificationSubscriber.First().CostUserId.Should().Be(secondUserId);
            }
        }

        public class SearchShould : CostServiceTest
        {
            [Test]
            public async Task Not_return_deleted_costs()
            {
                // Arrange
                var costId = Guid.NewGuid();
                var cost = new Cost
                {
                    Id = costId,
                    Deleted = true
                };
                var userId = Guid.NewGuid();
                var user = new UserIdentity { Id = userId };
                ActionBuilderMock.Setup(a => a.GetActions(It.IsAny<IEnumerable<Guid>>(), user))
                    .ReturnsAsync(new Dictionary<Guid, CostActionsModel> {
                            {
                                costId,
                                new CostActionsModel(cost, new Dictionary<string, ActionModel>())
                            }
                        }
                    );

                EFContext.Cost.Add(cost);
                await EFContext.SaveChangesAsync();

                var costQuery = new CostQuery();
                SearchServiceMock.Setup(ss => ss.SearchCosts(costQuery, user.Id)).ReturnsAsync((
                    new[] {
                        new CostSearchItem
                        {
                            Id = costId.ToString(),
                            CreatedBy = userId.ToString()
                        }
                    },
                    1)
                );

                // Act
                var searchResult = await CostService.Search(costQuery, user);

                // Assert
                searchResult.Count.Should().Be(0);
                searchResult.Costs.Count.Should().Be(0);
            }
        }

        public class ChangeOwnerShould : CostServiceTest
        {
            [Test]
            public void ChangeOwner_WhenOwnerIsNull_ShouldThrowException()
            {
                // Arrange
                var cost = new Cost { Owner = new CostUser() };
                CostUser owner = null;

                // Act
                // Assert
                CostService.Awaiting(s => s.ChangeOwner(User, cost, owner)).ShouldThrow<ArgumentNullException>();
            }

            [Test]
            public void ChangeOwner_WhenCostIsNull_ShouldThrowException()
            {
                // Arrange
                Cost cost = null;
                CostUser owner = new CostUser();

                // Act
                // Assert
                CostService.Awaiting(s => s.ChangeOwner(User, cost, owner)).ShouldThrow<ArgumentNullException>();
            }

            [Test]
            public void ChangeOwner_WhenUserIdentityIsNull_ShouldThrowException()
            {
                // Arrange
                Cost cost = new Cost { Owner = new CostUser() };
                CostUser owner = new CostUser();

                // Act
                // Assert
                CostService.Awaiting(s => s.ChangeOwner(null, cost, owner)).ShouldThrow<ArgumentNullException>();
            }

            [Test]
            public void ChangeOwner_WhenCostOwnerIsNull_ShouldThrowException()
            {
                // Arrange
                var cost = new Cost();
                CostUser owner = null;

                // Act
                // Assert
                CostService.Awaiting(s => s.ChangeOwner(User, cost, owner)).ShouldThrow<ArgumentNullException>();
            }

            [Test]
            public async Task ChangeOwner_WhenUserAndCostAreValid_ShouldChageOwnerOfTheCost()
            {
                // Arrange
                var cost = new Cost { Owner = new CostUser(), CostOwners = new List<CostOwner>() };
                var ownerId = Guid.NewGuid();
                var owner = new CostUser { Id = ownerId };

                // Act
                var result = await CostService.ChangeOwner(User, cost, owner);

                // Assert
                result.Owner.Should().Be(owner);
                result.OwnerId.Should().Be(ownerId);
            }

            [Test]
            public async Task ChangeOwner_WhenUserAndCostAreValid_ShouldSaveHistoricalOwner()
            {
                // Arrange
                var oldOwnerId = Guid.NewGuid();
                var oldOwner = new CostUser { Id = oldOwnerId };
                var newOwnerId = Guid.NewGuid();
                var newOwner = new CostUser { Id = newOwnerId };
                var cost = new Cost { Owner = oldOwner, OwnerId = oldOwner.Id, Id = Guid.NewGuid() };
                cost.CostOwners = new List<CostOwner>();

                // Act
                var result = await CostService.ChangeOwner(User, cost, newOwner);

                // Assert
                result.CostOwners.Should().HaveCount(1);
                Assert.AreEqual(result.CostOwners[0].UserId, oldOwner.Id);
                Assert.AreEqual(result.CostOwners[0].CostId, cost.Id);
            }

            [Test]
            public async Task ChangeOwner_WhenUserAndCostAreValid_RevokeAccessForPreviousOwner()
            {
                // Arrange
                var previousOwner = new CostUser
                {
                    Id = Guid.NewGuid()
                };
                var costId = Guid.NewGuid();
                var cost = new Cost
                {
                    Id = costId,
                    Owner = previousOwner,
                    CostOwners = new List<CostOwner>(),
                };
                var ownerId = Guid.NewGuid();
                var owner = new CostUser { Id = ownerId };

                // Act
                await CostService.ChangeOwner(User, cost, owner);

                // Assert
                CostStageRevisionPermissionServiceMock.Verify(p =>
                    p.RevokeCostPermission(costId, Roles.CostEditor, new[] { previousOwner }, null, false));
            }

            [Test]
            public async Task ChangeOwner_WhenUserAndCostAreValid_GrantAccessToNewOwner()
            {
                // Arrange
                var costId = Guid.NewGuid();
                var cost = new Cost
                {
                    Id = costId,
                    Owner = new CostUser(),
                    CostOwners = new List<CostOwner>(),
                };
                var ownerId = Guid.NewGuid();
                var owner = new CostUser { Id = ownerId };

                // Act
                await CostService.ChangeOwner(User, cost, owner);

                // Assert
                CostStageRevisionPermissionServiceMock.Verify(p =>
                    p.GrantCostPermission(costId, Roles.CostEditor, new[] { owner }, User.BuType, null, false));
            }
        }

        public class UpdateCostShould : CostServiceTest
        {
            [Test]
            public async Task UpdateCost_WhenSupportingDocumentsHaveChanged()
            {
                // Arrange
                var costId = Guid.NewGuid();
                var stageRevisionId = Guid.NewGuid();
                var documentId = Guid.NewGuid();
                string data = "{\"smoId\": null, \"title\": \"asdf\", \"isAIPE\": false, \"campaign\": \"15xStronger (Pantene)\", \"costType\": \"Production\", \"projectId\": \"5aa6958836d1a002c68a1424\", \"costNumber\": \"PGI0005D0000001\", \"contentType\": {\"id\": \"0cbdb306-f3bd-4d2e-a70c-6cfe3b3dc41b\", \"key\": \"Digital\", \"value\": \"Digital\", \"created\": \"2018-02-13T14:42:56.684202\", \"visible\": true, \"modified\": \"2018-02-13T14:42:56.684201\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"a97b2773-66eb-4d07-9bc5-8af5bf784d53\"}, \"budgetRegion\": {\"id\": \"7291b9c2-92cd-488b-8c68-f03f339b3c18\", \"key\": \"EUROPE AREA\", \"name\": \"Europe\"}, \"organisation\": {\"id\": \"fe8f1631-0c15-44be-a5e5-16d42fb62d2f\", \"key\": \"BFO\", \"value\": \"BFO\", \"created\": \"2018-02-13T14:42:56.696786\", \"visible\": true, \"modified\": \"2018-02-13T14:42:56.696785\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"f1ce7c45-c502-488f-b33c-15554b60c1f1\"}, \"initialBudget\": 50000, \"agencyCurrency\": \"USD\", \"agencyProducer\": [\"Abby Jenkins (Leo Burnett)\"], \"IsCurrencyChanged\": false}";
                var cost = new Cost
                {
                    Id = costId,
                    Owner = new CostUser(),
                    LatestCostStageRevision = new CostStageRevision()
                    {
                        Id = stageRevisionId,
                        SupportingDocuments = new List<SupportingDocument>()
                        {
                            new SupportingDocument()
                            {
                                Id = documentId,
                                CostStageRevisionId = stageRevisionId,
                                Key = "file1 key",
                                Name = "file1 name",
                            },
                            new SupportingDocument()
                            {
                                Id = Guid.NewGuid(),
                                CostStageRevisionId = stageRevisionId,
                            }
                        },
                        StageDetails = new CustomFormData()
                        {
                            Data = data,
                        },
                        ProductDetails = new CustomFormData()
                        {
                            Data = data,
                        },
                        CostStage = new CostStage()
                        {
                            Key = String.Empty,
                            Name = String.Empty,
                        },
                    },
                };

                CostBuilderMock.Setup(cb => cb.UpdateCost(It.IsAny<UserIdentity>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CostType>(), It.IsAny<StageDetails>(), It.IsAny<ProductionDetail>()))
                    .ReturnsAsync(new plugins.PG.Builders.Cost.UpdateCostResponse()
                    {
                        StageDetails = cost.LatestCostStageRevision.StageDetails,
                        ProductionDetails = cost.LatestCostStageRevision.ProductDetails,
                        CurrentCostStageModel = new CostStageModel()
                        {
                            Key = "FirstStage Key",
                            Name = "FirstStage Name",
                            Order = 1,
                            Revisions = new[]
                            {
                                new CostStageRevisionModel
                                {
                                    Name = "FirstStage Key",
                                    Status = CostStageRevisionStatus.Draft,
                                    StageDetails = data,
                                    SupportingDocuments = new []
                                    {
                                        new SupportingDocumentModel()
                                        {
                                            Key = "file1 key",
                                            Name = "file1 name",
                                        },
                                        new SupportingDocumentModel(),
                                    }
                                }
                            }
                        }
                    });

                EFContext.Cost.Add(cost);
                EFContext.SaveChanges();
                cost.LatestCostStageRevision.SupportingDocuments.Add(new SupportingDocument());

                // Act
                await CostService.UpdateCost(costId, User, new UpdateCostModel());

                // Assert
                var documents = EFContext.SupportingDocument.Where(sd => sd.CostStageRevisionId == stageRevisionId).ToList();
                documents.Count.Should().Be(3);
                documents.Count(sd => sd.Id == documentId).Should().Be(1);
            }
        }

        public class IsValidForSubmissionShould : CostServiceTest
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public async Task IsValidForSubmission_Always_Should_GetResultFromCostBuilder(bool isValid)
            {
                // Arrange
                var costId = Guid.NewGuid();
                const string message = "Any message";
                EFContext.Add(new Cost { Id = costId });
                EFContext.SaveChanges();

                CostBuilderMock.Setup(cb => cb.IsValidForSubmittion(costId)).ReturnsAsync(new OperationResponse(isValid, message));

                // Act
                var result = await CostService.IsValidForSubmission(User, costId);

                // Assert
                result.Should().NotBeNull();
                result.Success.Should().Be(isValid);
            }

            [Test]
            public async Task IsValidForSubmission_When_Invalid_Should_SendNotificationToSupportTeam()
            {
                // Arrange
                var costId = Guid.NewGuid();
                const string message = "Any message";
                const string costNumber = "PO12312313";
                EFContext.Add(new Cost
                {
                    Id = costId,
                    CostNumber = costNumber
                });
                EFContext.SaveChanges();

                CostBuilderMock.Setup(cb => cb.IsValidForSubmittion(costId)).ReturnsAsync(new OperationResponse(false, message));

                // Act
                var result = await CostService.IsValidForSubmission(User, costId);

                // Assert
                result.Should().NotBeNull();
                result.Success.Should().Be(false);
                SupportNotificationServiceMock.Verify(sn =>
                    sn.SendSupportSubmissionFailedNotification(costNumber, null), Times.Once);
            }
        }

        [TestFixture]
        public class ViewCostShould : CostServiceTest
        {
            private async Task CreateExchangeRate()
            {
                var currencies = new[]
                {
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "USD",
                        Description = "US Dollar",
                        Symbol = "$",
                        DefaultCurrency = true
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "AUD",
                        Description = "Australian Dollar",
                        Symbol = "$"
                    },
                    new Currency
                    {
                        Id = Guid.NewGuid(),
                        Code = "CAD",
                        Description = "Canadian Dollar",
                        Symbol = "$"
                    }
                };
                EFContext.Currency.AddRange(currencies);
                await EFContext.SaveChangesAsync();


                var effectiveFrom = DateTime.UtcNow;
                var exchangeRates = new List<ExchangeRate> {
                    new ExchangeRate
                    {
                        FromCurrency = currencies[1].Id,
                        ToCurrency = currencies[0].Id,
                        EffectiveFrom = effectiveFrom,
                        Rate = decimal.Parse("1.31")
                    },
                    new ExchangeRate
                    {
                        FromCurrency = currencies[2].Id,
                        ToCurrency = currencies[0].Id,
                        EffectiveFrom = effectiveFrom.AddDays(-20),
                        Rate = decimal.Parse("0.715")
                    },
                    new ExchangeRate
                    {
                        FromCurrency = currencies[2].Id,
                        ToCurrency = currencies[0].Id,
                        EffectiveFrom = effectiveFrom.AddDays(-11),
                        Rate = decimal.Parse("0.72")
                    },
                    new ExchangeRate
                    {
                        FromCurrency = currencies[2].Id,
                        ToCurrency = currencies[0].Id,
                        EffectiveFrom = effectiveFrom.AddDays(-4),
                        Rate = decimal.Parse("0.719")
                    },
                    new ExchangeRate
                    {
                        FromCurrency = currencies[2].Id,
                        ToCurrency = currencies[0].Id,
                        EffectiveFrom = effectiveFrom.AddDays(-2),
                        Rate = decimal.Parse("0.73926")
                    }
                };

                EFContext.ExchangeRate.AddRange(exchangeRates);
                await EFContext.SaveChangesAsync();
            }

            private async Task<Guid> CreateCostTest(Currency paymentCurrency, Currency costLineItemCurrency)
            {
                var cost = new Cost
                {
                    PaymentCurrency = paymentCurrency,
                    CostStages = new List<CostStage>() {
                        new CostStage {
                            CostStageRevisions = new List<CostStageRevision> {
                                new CostStageRevision {
                                    CostLineItems = new List<CostLineItem> {
                                        new CostLineItem {
                                            LocalCurrencyId = costLineItemCurrency.Id
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                EFContext.Cost.Add(cost);
                await EFContext.SaveChangesAsync();

                return cost.Id;
            }

            /// <summary>
            /// This test covers SPG-2991
            /// </summary>
            [Test]
            public async Task GetExchangeRatesOfCurrenciesInCost_Should_Include_DefaultCurrency()
            {
                //Setup
                await CreateExchangeRate();

                var paymentCurrency = EFContext.Currency.First(c => c.Code == "AUD");
                var costLineItemCurrency = EFContext.Currency.First(c => c.Code == "CAD");

                var costId = await CreateCostTest(paymentCurrency, costLineItemCurrency);

                var cost = EFContext.Cost.First(c => c.Id == costId);
                //Act
                var lstExchangeRates = await CostService.GetExchangeRatesOfCurrenciesInCost(cost);

                //Assert
                lstExchangeRates.Should().Contain(c=> c.FromCurrency == paymentCurrency.Id);
                lstExchangeRates.Should().Contain(c => c.FromCurrency == costLineItemCurrency.Id);
            }
        }
    }
}
