
namespace costs.net.plugins.tests.Builders.Notifications
{
    using core.Models.Notifications;
    using dataAccess.Entity;
    using plugins.PG.Models.Stage;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Regions;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;
    using Cost = dataAccess.Entity.Cost;
    using Project = dataAccess.Entity.Project;

    [TestFixture]
    public class InsuranceUserNotificationTests : EmailNotificationBuilderTestBase
    {
        [Test]
        public async Task Cost_Approved_Create_Notification_For_Insurance_User()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string insuranceUserParent = "insuranceuser";
            var costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var insuranceUserId = Guid.NewGuid();

            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;
            insuranceUser.Id = insuranceUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            costOwner.Agency = agency;

            //Act
            var result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(brandApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Approved_OE_Create_Notification_For_All_Insurance_Users()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            var costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            costOwner.Agency = agency;

            //Act
            var result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1, insuranceUserGdamUserId2);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(brandApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Approved_FP_Create_Notification_For_All_Insurance_Users()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            var costStageKey = CostStages.FirstPresentation.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            costOwner.Agency = agency;

            //Act
            var result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1, insuranceUserGdamUserId2);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(brandApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Approved_FA_Create_Notification_For_All_Insurance_Users()
        {
            //Arrange
            const string expectedActionType = "allApprovalsApproved";
            const string brandApproverName = "Jane Smith";
            const string brandApproverType = "Brand";
            const string expectedBrandApproverType = BrandManagerRoleValue;
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            var costStageKey = CostStages.FinalActual.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);
            var timestamp = DateTime.UtcNow;

            var cost = new Cost();

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId,
                costStage, brand, costId, costStageRevisionId, projectId);

            cost.Status = CostStageRevisionStatus.Approved;
            latestRevision.CostStage.Key = costStageKey;

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            costOwner.Agency = agency;

            //Act
            var result = await EmailNotificationBuilder.BuildCostApprovedNotification(costUsers, cost,
                latestRevision, brandApproverName, brandApproverType, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1, insuranceUserGdamUserId2);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(brandApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(expectedBrandApproverType);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Rejected_Create_Notification_For_Insurance_User_For_NorthAmerica()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string insuranceUserParent = "insuranceuser";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            agency.Country.GeoRegionId = NorthAmericanRegionId;

            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            insuranceUserMessage.Object.Comments.Should().NotBeNull();
            insuranceUserMessage.Object.Comments.Should().Be(comments);

            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Rejected_Create_Notification_For_Insurance_User_For_Europe()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string insuranceUserParent = "insuranceuser";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            agency.Country.GeoRegionId = EuropeRegionId;

            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];

            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            insuranceUserMessage.Object.Comments.Should().NotBeNull();
            insuranceUserMessage.Object.Comments.Should().Be(comments);

            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Rejected_Create_Notification_For_All_Insurance_Users_For_NorthAmerica()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string insuranceUserParent = "insuranceuser";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            agency.Country.GeoRegionId = NorthAmericanRegionId;

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);
            EFContext.Add(rejecter);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            // Both users are in the Recipients field
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            insuranceUserMessage.Object.Comments.Should().NotBeNull();
            insuranceUserMessage.Object.Comments.Should().Be(comments);

            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Rejected_Create_Notification_For_All_Insurance_Users_For_Europe()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string insuranceUserParent = "insuranceuser";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            agency.Country.GeoRegionId = EuropeRegionId;

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);
            EFContext.Add(rejecter);

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            var insuranceUserMessage = notifications[1];

            // Both users are in the Recipients field
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);

            insuranceUserMessage.Object.Approver.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Type.Should().NotBeNull();
            insuranceUserMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            insuranceUserMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            insuranceUserMessage.Object.Comments.Should().NotBeNull();
            insuranceUserMessage.Object.Comments.Should().Be(comments);

            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Rejected_Do_Not_Create_Notification_For_Insurance_User_For_Not_America_And_Europe()
        {
            //Arrange
            const string expectedActionType = "notifyRejected";
            const string comments = "My Comments";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string technicalApproverGdamUserId = "57e5461ed9563f268ef4f1ta";
            const string technicalApproverName = "Jane Smith";
            const string technicalApproverType = "Technical";
            const string costOwnerParent = "costowner";

            var costId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();
            var rejecter = new CostUser();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            agency.Country.GeoRegionId = AsiaRegionId;

            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            rejecter.GdamUserId = technicalApproverGdamUserId;
            rejecter.FullName = technicalApproverName;
            rejecter.Id = userId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostRejectedNotification(costUsers, cost, latestRevision, rejecter, technicalApproverType, comments, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = notifications[0];

            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);

            costOwnerMessage.Object.Approver.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Type.Should().NotBeNull();
            costOwnerMessage.Object.Approver.Name.Should().Be(technicalApproverName);
            costOwnerMessage.Object.Approver.Type.Should().Be(technicalApproverType);
            costOwnerMessage.Object.Comments.Should().NotBeNull();
            costOwnerMessage.Object.Comments.Should().Be(comments);

            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);
        }

        [Test]
        public async Task Cost_Cancelled_OE_Create_Notification_For_Insurance_User_EuropeAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_FP_Create_Notification_For_Insurance_User_EuropeAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.FirstPresentation.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_FA_Create_Notification_For_Insurance_User_EuropeAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.FinalActual.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_OE_Create_Notification_For_Insurance_User_NorthAmericanAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            costStage.Key = costStageKey;
            country.GeoRegionId = NorthAmericanRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_FP_Create_Notification_For_Insurance_User_NorthAmericanAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.FirstPresentation.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            costStage.Key = costStageKey;
            country.GeoRegionId = NorthAmericanRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_FA_Create_Notification_For_Insurance_User_NorthAmericanAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.FinalActual.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            costStage.Key = costStageKey;
            country.GeoRegionId = NorthAmericanRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_Create_Notification_For_All_Insurance_Users_EuropeAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId1 = "57e5461ed9563f268ef4fiu1";
            const string insuranceUserGdamUserId2 = "57e5461ed9563f268ef4fiu2";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUserRole = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    BusinessRole = new BusinessRole
                    {
                        Key = Constants.BusinessRole.InsuranceUser,
                        Value = Constants.BusinessRole.InsuranceUser
                    },
                    ObjectType = core.Constants.AccessObjectType.Client
                }
            };
            var insuranceUser1 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId1,
                UserBusinessRoles = insuranceUserRole
            };

            var insuranceUser2 = new CostUser
            {
                GdamUserId = insuranceUserGdamUserId2,
                UserBusinessRoles = insuranceUserRole
            };
            EFContext.Add(insuranceUser1);
            EFContext.Add(insuranceUser2);

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });

            costStage.Key = costStageKey;
            country.GeoRegionId = EuropeRegionId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId1, insuranceUserGdamUserId2 },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId1);
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId2);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_Create_Notification_For_All_Insurance_Users_NorthAmericanAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string insuranceUserParent = "insuranceuser";
            const string costOwnerParent = "costowner";
            string costStageKey = CostStages.OriginalEstimate.ToString();

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            costStage.Key = costStageKey;
            country.GeoRegionId = NorthAmericanRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                InsuranceUsers = new List<string> { insuranceUserGdamUserId },
                Watchers = new List<string> { CostOwnerGdamUserId }
            };
            RegionsServiceMock.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });
            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(2);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);

            EmailNotificationMessage<CostNotificationObject> insuranceUserMessage = notifications[1];
            TestCommonMessageDetails(insuranceUserMessage, expectedActionType, insuranceUserGdamUserId);
            insuranceUserMessage.Object.Parents.Should().Contain(insuranceUserParent);
        }

        [Test]
        public async Task Cost_Cancelled_Do_Not_Create_Notification_For_Insurance_User_Non_NorthAmerican_Nor_EuropeAgency()
        {
            //Arrange
            const string expectedActionType = "notifyCancelled";
            const string insuranceUserGdamUserId = "57e5461ed9563f268ef4f1iu";
            const string costOwnerParent = "costowner";

            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var costOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var timestamp = DateTime.UtcNow;

            var cost = new Cost();
            var costOwner = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.AgencyOwner,
                            Value = Constants.BusinessRole.AgencyOwner
                        }
                    }
                }
            };
            var insuranceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        }
                    }
                }
            };

            var latestRevision = new CostStageRevision();
            var costStage = new CostStage();
            var project = new Project();
            var brand = new Brand();
            var agency = new Agency();
            var country = new Country();

            SetupDataSharedAcrossTests(agency, country, cost, latestRevision, project, costOwner, costOwnerId, costStage, brand, costId, costStageRevisionId, projectId);

            country.GeoRegionId = AsiaRegionId;
            insuranceUser.GdamUserId = insuranceUserGdamUserId;

            var costUsers = new CostNotificationUsers
            {
                CostOwner = costOwner,
                Watchers = new List<string> { CostOwnerGdamUserId }
            };

            //Act
            var result = await EmailNotificationBuilder.BuildCostCancelledNotification(costUsers, cost, latestRevision, timestamp);

            //Assert
            result.Should().NotBeNull();
            var notifications = result.ToArray();
            notifications.Should().HaveCount(1);

            EmailNotificationMessage<CostNotificationObject> costOwnerMessage = result.First();
            TestCommonMessageDetails(costOwnerMessage, expectedActionType, CostOwnerGdamUserId);
            costOwnerMessage.Object.Parents.Should().Contain(costOwnerParent);
        }

        
    }
}
