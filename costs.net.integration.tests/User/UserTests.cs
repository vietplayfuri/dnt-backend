namespace costs.net.integration.tests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.User;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using plugins;
    using Constants = core.Constants;

    public class UserTests
    {
        public abstract class UserTest : BaseIntegrationTest
        {
            public const string BaseUrl = "/v1/users";
            public string UsersUrl(Guid id) => $"{BaseUrl}/{id}";
        }

        [TestFixture]
        public class GetShould : UserTest
        {
            [Test]
            public async Task Get()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
                var res = await Browser.Get(UsersUrl(user.Id), with => with.User(user));
                var foundUser = Deserialize<CostUserModel>(res, HttpStatusCode.OK);

                foundUser.Id.Should().Be(user.Id);
            }

            [Test]
            public async Task ReturnNotFoundForNonExistentUser()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
                var res = await Browser.Get(UsersUrl(Guid.NewGuid()), with => with.User(user));

                res.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestFixture]
        public class GetByGdamUserIdShould : UserTest
        {
            [Test]
            public async Task GetByGdamUserId()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
                var url = $"{BaseUrl}?gdamUserId={user.GdamUserId}";
                var res = await Browser.Get(url, with => with.User(user));
                var foundUsers = Deserialize<CostUserModel>(res, HttpStatusCode.OK);

                foundUsers.Id.Should().Be(user.Id);
            }
        }

        [TestFixture]
        public class GetByAgencyIdShould : UserTest
        {
            [Test]
            public async Task GetByAgencyId()
            {
                var agencyId = await CreateAgencyIfNotExists(GenerateGdamId());
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin, agencyId);
                var url = $"{BaseUrl}?agencyId={agencyId}";
                var res = await Browser.Get(url, with => with.User(user));

                var foundUsers = Deserialize<List<CostUserModel>>(res, HttpStatusCode.OK);

                foundUsers.Count.Should().BeGreaterOrEqualTo(1);
                foundUsers.FirstOrDefault(c => c.Id == user.Id)?.Should().NotBeNull();
            }
        }

        [TestFixture]
        public class UpdateUserShould : UserTest
        {
            [Test]
            public async Task Update_Business_Roles()
            {
                var user = await CreateUser($"{Guid.NewGuid()}bob", Roles.ClientAdmin);
                var businessRoles = await EFContext.BusinessRole.Where(br => br.Id != user.UserBusinessRoles.First().BusinessRole.Id).ToListAsync();
                var updateUserModel = new UpdateUserModel
                {
                    ApprovalLimit = 100000,
                    AccessDetails = new List<AccessDetail>
                    {
                        new AccessDetail
                        {
                            ObjectType = Constants.AccessObjectType.Region,
                            BusinessRoleId = user.UserBusinessRoles.First().BusinessRole.Id,
                            LabelName = "RegionOne"
                        },
                        new AccessDetail
                        {
                            ObjectType = Constants.AccessObjectType.Client,
                            BusinessRoleId = businessRoles.First().Id
                        }
                    }
                };

                var url = UsersUrl(user.Id);
                var updateUserResult = await Browser.Put(url, w =>
                {
                    w.User(user);
                    w.JsonBody(updateUserModel);
                });

                updateUserResult.StatusCode.Should().Be(HttpStatusCode.OK);

                var res = await Browser.Get(url, with => with.User(user));
                var foundUser = Deserialize<CostUserModel>(res, HttpStatusCode.OK);

                foundUser.ApprovalLimit.Should().Be(updateUserModel.ApprovalLimit);
                foundUser.ApprovalLimit.Should().Be(100000);
                foundUser.BusinessRoles.Count.Should().Be(2);
            }

            [Test]
            public async Task Update_User_Business_Roles_Should_Not_Change()
            {
                var agencyId = await CreateAgencyIfNotExists("1refgvb");

                var user = await CreateUser($"{Guid.NewGuid()}Steve", Roles.CostViewer, agencyId);
                var businessRole = await EFContext.BusinessRole.FindAsync(user.UserBusinessRoles.First().BusinessRole.Id);

                var updateUserModel = new UpdateUserModel
                {
                    ApprovalLimit = 100000,
                    AccessDetails = new List<AccessDetail>
                    {
                        new AccessDetail
                        {
                            ObjectType = Constants.AccessObjectType.Client,
                            ObjectId = EFContext.AbstractType.First(a=>a.Type == Constants.AccessObjectType.Module).Id,
                            BusinessRoleId = businessRole.Id
                        }
                    }
                };

                var url = UsersUrl(user.Id);
                var updateUserResult = await Browser.Put(url, w =>
                {
                    w.User(user);
                    w.JsonBody(updateUserModel);
                });

                updateUserResult.StatusCode.Should().Be(HttpStatusCode.OK);

                var res = await Browser.Get(url, with => with.User(user));
                var foundUser = Deserialize<CostUserModel>(res, HttpStatusCode.OK);

                foundUser.ApprovalLimit.Should().Be(updateUserModel.ApprovalLimit);
                foundUser.BusinessRoles.Count.Should().Be(1);
            }

            [Test]
            public async Task Update_User_Business_Roles_Should_Add()
            {
                var agencyId = await CreateAgencyIfNotExists("1231eqwdsc");

                var user = await CreateUser($"{Guid.NewGuid()}Steve", Roles.CostViewer, agencyId);
                var businessRole = await EFContext.BusinessRole.FindAsync(user.UserBusinessRoles.First().BusinessRole.Id);

                var updateUserModel = new UpdateUserModel
                {
                    ApprovalLimit = 100000,
                    AccessDetails = new List<AccessDetail>
                    {
                        new AccessDetail
                        {
                            ObjectType = Constants.AccessObjectType.Client,
                            BusinessRoleId = businessRole.Id
                        }
                    }
                };

                var url = UsersUrl(user.Id);
                var updateUserResult = await Browser.Put(url, w =>
                {
                    w.User(user);
                    w.JsonBody(updateUserModel);
                });

                updateUserResult.StatusCode.Should().Be(HttpStatusCode.OK);

                var res = await Browser.Get(url, with => with.User(user));
                var foundUser = Deserialize<CostUserModel>(res, HttpStatusCode.OK);

                foundUser.ApprovalLimit.Should().Be(updateUserModel.ApprovalLimit);
                foundUser.BusinessRoles.Count.Should().Be(1);
            }
        }
    }
}
