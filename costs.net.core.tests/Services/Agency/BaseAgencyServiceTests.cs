namespace costs.net.core.tests.Services.Agency
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders;
    using core.Models;
    using core.Models.AMQ;
    using core.Models.Utils;
    using core.Services;
    using core.Services.Agency;
    using core.Services.User;
    using Castle.Components.DictionaryAdapter;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Extensions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Serilog;
    using Agency = dataAccess.Entity.Agency;

    [TestFixture]
    public class BaseAgencyServiceTests
    {
        [SetUp]
        public void Init()
        {
            EFContext = EFContextFactory.CreateInMemoryEFContext();
            LoggerAsMock = new Mock<ILogger>();
            AppSettings = new Mock<IOptions<AppSettings>>();
            AppSettings.Setup(a => a.Value).Returns(new AppSettings
            {
                AdminUser = "4ef31ce1766ec96769b399c0",
                CostsAdminUserId = "77681eb0-fc0d-44cf-83a0-36d51851e9ae"
            });
            PermissionServiceMock = new Mock<IPermissionService>();
            PgUserServiceMock = new Mock<IPgUserService>();
            PluginAgencyServiceMock = new Mock<IPluginAgencyService>();

            UserServices = new EditableList<Lazy<IPgUserService, PluginMetadata>>
            {
                new Lazy<IPgUserService, PluginMetadata>(
                    () => PgUserServiceMock.Object, new PluginMetadata { BuType = BuType.Pg })
            };
            PluginAgencyServices = new List<Lazy<IPluginAgencyService, PluginMetadata>>
            {
                new Lazy<IPluginAgencyService, PluginMetadata>(
                    () => PluginAgencyServiceMock.Object, new PluginMetadata { BuType = BuType.Pg })
            };

            AgencyService = new AgencyService(
                EFContext,
                LoggerAsMock.Object,
                PermissionServiceMock.Object,
                UserServices,
                PluginAgencyServices,
                AppSettings.Object);

            JsonReader = new JsonTestReader();
        }

        protected JsonTestReader JsonReader;
        protected Mock<IOptions<AppSettings>> AppSettings;
        protected Mock<ILogger> LoggerAsMock;
        protected Mock<IPgUserService> PgUserServiceMock;
        protected Mock<IPluginAgencyService> PluginAgencyServiceMock;

        protected List<Lazy<IPgUserService, PluginMetadata>> UserServices;
        protected List<Lazy<IPluginAgencyService, PluginMetadata>> PluginAgencyServices;
        protected EFContext EFContext;
        protected Mock<IPermissionService> PermissionServiceMock;
        protected AgencyService AgencyService;

        protected async Task<A5Agency> GetA5Agency(string jsonDataFileName = "a5_agency.json")
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}{jsonDataFileName}";
            var a5Agency = await JsonReader.GetObject<A5Agency>(filePath, true);
            return a5Agency;
        }

        protected async Task<A5Agency> PrepareTestData(params string[] labels)
        {
            var gdamAgencyId = "gdamAgencyId1";
            var a5Agency = await GetA5Agency();
            a5Agency._id = gdamAgencyId;
            if (labels != null)
            {
                var labelz = a5Agency._cm.Common.Labels.ToList();
                labelz.AddRange(labels);
                a5Agency._cm.Common.Labels = labelz.ToArray();
            }

            var costAgency = new Agency
            {
                Id = Guid.NewGuid(),
                GdamAgencyId = gdamAgencyId,
                Labels = a5Agency._cm.Common.Labels
            };
            var countryIso = a5Agency._cm.Common.Address.Country.FirstOrDefault();

            var agencyAbstractType = new AbstractType { ObjectId = costAgency.Id };
            var defaultCurrecny = new Currency { DefaultCurrency = true, Code = "USD" };
            var country = new Country { Iso = countryIso };

            EFContext.AbstractType.Add(agencyAbstractType);
            EFContext.Agency.Add(costAgency);
            EFContext.Currency.Add(defaultCurrecny);
            EFContext.Country.Add(country);
            EFContext.SaveChanges();

            return a5Agency;
        }
    }
}