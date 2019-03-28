namespace costs.net.integration.tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Browser;
    using Builders;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.AspNetCore.TestHost;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using core.Models.Response;
    using core.Services.PurchaseOrder;
    using Microsoft.EntityFrameworkCore;
    using net.tests.common.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Nest;

    public class BaseIntegrationTest
    {
        protected readonly JsonTestReader JsonReader = new JsonTestReader();
        protected BrowserShim Browser { get; private set; }

        protected EFContext EFContext { get; private set; }
        protected Mock<IElasticClient> ElasticClient { get; private set; }
        protected IPurchaseOrderResponseConsumer PurchaseOrderResponseConsumer { get; private set; }
        private TestServer TestServer { get; set; }

        [OneTimeSetUp]
        public async Task BaseSetup()
        {
            var testContext = ApiTestContext.Instance;

            TestServer = testContext.TestServer;
            EFContext = TestServer.Host.Services.GetService<EFContext>();
            PurchaseOrderResponseConsumer = TestServer.Host.Services.GetService<IPurchaseOrderResponseConsumer>();
            ElasticClient = testContext.ElasticClient;
            Browser = testContext.Browser;

            JsonConvert.DefaultSettings = () => ApiTestContext.JsonSerializerSettings;

            await CreateAgencyIfNotExists(gdamId: null, agencyName: Constants.DefaultTestPGAgency);
        }

        protected static void ValidateStatusCode(BrowserResponse response, HttpStatusCode statusCode)
        {
            if (response.StatusCode != statusCode)
            {
                ErrorResponse errorModel = null;
                try
                {
                    errorModel = Deserialize<ErrorResponse>(response.Body);
                }
                catch (Exception)
                {
                    //nothing 
                }

                var error = $"Expected {statusCode} but got {response.StatusCode}. Body {response.Body.AsString()}. Request Uri: {response.RequestUri} Method: {response.RequestMethod}";
                if (errorModel != null)
                {
                    error += $"{Environment.NewLine}Details: {string.Join(",", errorModel.Messages)}\n";
                    error += $"{Environment.NewLine}Stacktrace: {errorModel.StackTrace}\n";
                    error += $"{Environment.NewLine}Body: {response.Body.AsString()}";
                }

                throw new AssertionException(error);
            }
        }

        protected async Task<CostUser> CreateUser(string name, string role, Guid? agencyId = null , string businessRoleName = null)
        {
            var email = $"{name}@adstream.com";

            var roleId = await GetRoleId(role);

            var userAgency = agencyId.HasValue
                ? await EFContext.Agency
                    .Include(a => a.AbstractTypes)
                    .FirstOrDefaultAsync(a => a.Id == agencyId.Value)
                : await EFContext.Agency
                    .Include(a => a.AbstractTypes)
                    .FirstOrDefaultAsync(a => a.Name == Constants.DefaultTestPGAgency);

            BusinessRole businessRole;
            if (businessRoleName is null)
            {
                businessRole = await EFContext.BusinessRole.FirstOrDefaultAsync(a => a.RoleId == roleId);
            }
            else
            {
                businessRole = await EFContext.BusinessRole.FirstOrDefaultAsync(a => a.Key == businessRoleName);
            }

            var root = await EFContext.AbstractType.FirstOrDefaultAsync(a => a.Id == a.ParentId);

            var costUser = new CostUser
            {
                GdamUserId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 24),
                Email = email,
                FirstName = name,
                LastName = name,
                FullName = name,
                Agency = userAgency,
                AbstractType = root,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = businessRole,
                        Labels = new []{"RegionOne"},
                        ObjectType = core.Constants.AccessObjectType.Region
                    }
                },
                GdamBusinessUnits = new[] { userAgency.GdamAgencyId }
            };

            EFContext.CostUser.Add(costUser);
            await EFContext.SaveChangesAsync();

            return costUser;
        }

        protected async Task<Guid> CreateAgencyIfNotExists(string gdamId = null, string agencyName = null)
        {
            gdamId = gdamId ?? "Test gdam agency id";
            agencyName = agencyName ?? "Test Agency";

            var existing = await EFContext.Agency.FirstOrDefaultAsync(a => a.GdamAgencyId == gdamId && a.Name == agencyName);
            if (existing != null)
            {
                return existing.Id;
            }

          var abstractType = EFContext.AbstractType.FirstOrDefault(a => a.Type == core.Constants.AccessObjectType.Agency && a.Parent.Type == core.Constants.AccessObjectType.Module);
            var country = new Country
            {
                Name = "Test coutry name",
                Iso = "USA"
            };
            EFContext.Country.Add(country);

            var agency = new Agency
            {
                Name = agencyName,
                CountryId = country.Id,
                GdamAgencyId = gdamId,
                Labels = new[] { "costPG", "CM_Prime_P&G", $"{plugins.Constants.PurchaseOrder.VendorSapIdLabelPrefix}TestSAPVendorCode" }
            };
            var agencyAbstractType = new AbstractType
            {
                Agency = agency,
                Type = AbstractObjectType.Agency.ToString(),
                ObjectId = agency.Id,
                ParentId = abstractType.Id
            };
            EFContext.Add(agencyAbstractType);
            await EFContext.SaveChangesAsync();
            return agency.Id;
        }

        private Agency CreateAgencyIfNotExists(bool isCyclone = false, string agencyName = null, string[] labels = null)
        {
            return new AgencyBuilder()
                 .WithCycloneLabel(isCyclone)
                 .WithPandGLabel()
                 .WithGdamAgencyId()
                 .WithLabels(labels)
                 .WithCountry(new Country
                 {
                     Name = "Test coutry name",
                     Iso = "USA"
                 })
                 .WithName(agencyName ?? "Test Agency")
                 .Build();
        }

        protected async Task<AbstractType> CreateAgencyAbstractType(bool isCyclone = false, string agencyName = null, string[] labels = null)
        {
            if (labels == null)
            {
                labels = new[] { $"{plugins.Constants.PurchaseOrder.VendorSapIdLabelPrefix}TestSAPVendorCore" };
            }

            var agency = CreateAgencyIfNotExists(isCyclone, agencyName, labels);

            EFContext.Agency.Add(agency);

            // Link agency to module defined by clientType
            var abstractType = EFContext.AbstractType.FirstOrDefault(a =>
                a.Type == core.Constants.AccessObjectType.Agency && 
                a.Parent.Type == core.Constants.AccessObjectType.Module);

            var abstractTypeAgency = new AbstractType
            {
                ParentId = abstractType.Id,
                Agency = agency,
                Type = AbstractObjectType.Agency.ToString()
            };
            EFContext.AbstractType.Add(abstractTypeAgency);

            await EFContext.SaveChangesAsync();

            return abstractTypeAgency;
        }

        protected static T Deserialize<T>(BrowserResponse response, HttpStatusCode statusCode)
        {
            ValidateStatusCode(response, statusCode);
            return Deserialize<T>(response.Body);
        }

        protected T GetService<T>()
        {
            return (T)TestServer.Host.Services.GetService(typeof(T));
        }

        private async Task<Guid> GetRoleId(string role)
        {
            var roles = await EFContext.Role
                .FirstOrDefaultAsync(r => r.Name == role);
            var roleId = roles.Id;
            return roleId;
        }

        protected static T Deserialize<T>(BrowserResponse.ResponseBody body)
        {
            return JsonConvert.DeserializeObject<T>(body.AsString());
        }

        protected string GenerateGdamId()
        {
            return new string(Guid.NewGuid().ToString().Take(24).ToArray());
        }
    }
}
