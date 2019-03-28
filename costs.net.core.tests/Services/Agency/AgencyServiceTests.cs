namespace costs.net.core.tests.Services.Agency
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.AMQ;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Agency = dataAccess.Entity.Agency;
    using Brand = dataAccess.Entity.Brand;

    public class AgencyServiceTests : BaseAgencyServiceTests
    {
        [Test]
        public async Task HandleA5EventObject_BU_AgencyCreated()
        {
            //Setup
            var a5Agency = await GetA5Agency();

            var labelz = a5Agency._cm.Common.Labels.ToList();
            labelz.Add("CM_Prime_P&G");
            a5Agency._cm.Common.Labels = labelz.ToArray();

            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                GdamUserId = a5Agency.CreatedBy._id,
                ParentId = Guid.NewGuid()
            };
            var agency = new Agency { Id = Guid.NewGuid(), GdamAgencyId = a5Agency._id };
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Brand", AdIdPrefix = "prefix" };
            var country = new Country { Iso = "GB", Id = Guid.NewGuid() };
            var currency = new Currency { DefaultCurrency = true, Code = "TEST", Description = "Test Currency" };

            var costUsers = new List<CostUser> { costUser }.AsQueryable();
            var brands = new List<Brand> { brand }.AsQueryable();

            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Type = AbstractObjectType.Agency.ToString(),
                    Id = Guid.NewGuid()
                },
                new AbstractType
                {
                    Type = AbstractObjectType.Module.ToString(),
                    Module = new Module
                    {
                        ClientType = ClientType.Pg
                    },
                    Id = Guid.NewGuid()
                }
            };

            EFContext.AbstractType.AddRange(abstractTypes);
            EFContext.Country.Add(country);
            EFContext.Agency.Add(new Agency
            {
                Name = "Media Agency",
                Version = 1,
                Labels = new string[] { }
            });
            EFContext.CostUser.AddRange(costUsers);
            EFContext.Brand.AddRange(brands);
            EFContext.Currency.Add(currency);

            EFContext.SaveChanges();

            PluginAgencyServiceMock.Setup(a => a.AddAgencyAbstractType(It.IsAny<Agency>(), It.IsAny<AbstractType>()))
                .ReturnsAsync(new AbstractType { Id = Guid.NewGuid(), ObjectId = Guid.NewGuid() });

            PgUserServiceMock.Setup(a => a.AddUsersToAgencyAbstractType(It.IsAny<AbstractType>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

            //Act
            var addedAgency = await AgencyService.AddAgencyToDb(a5Agency);

            //Assert
            PermissionServiceMock.Verify(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            EFContext.Agency.Should().HaveCount(2);
            EFContext.AbstractType.Should().HaveCount(3);
            EFContext.GlobalAgencyRegion.Should().HaveCount(0);
            EFContext.GlobalAgency.Should().HaveCount(0);
            addedAgency.Should().NotBeNull();
            addedAgency.Name.Should().Be("Saatchi");
            addedAgency.Labels.Length.Should().Be(7);
            addedAgency.GdamAgencyId.Should().Be( a5Agency._id);
        }

        [Test]
        public async Task HandleA5EventObject_Media_AgencyCreated()
        {
            //Setup
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}a5_agency.json";
            var a5Agency = await JsonReader.GetObject<A5Agency>(filePath, true);
            a5Agency._cm.Common.Labels = a5Agency._cm.Common.Labels.Where(a => !a.StartsWith("SMO_")).ToArray();

            PermissionServiceMock.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() });
            var costUser = new CostUser
            {
                Id = Guid.NewGuid(),
                GdamUserId = a5Agency.CreatedBy._id,
                ParentId = Guid.NewGuid()
            };
            var agency = new Agency { Id = Guid.NewGuid(), GdamAgencyId = a5Agency._id };
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Brand", AdIdPrefix = "prefix" };
            var country = new Country { Iso = "GB", Id = Guid.NewGuid() };
            var currency = new Currency { DefaultCurrency = true, Code = "TEST", Description = "Test Currency" };

            var costUsers = new List<CostUser> { costUser }.AsQueryable();
            var brands = new List<Brand> { brand }.AsQueryable();

            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Type = AbstractObjectType.Agency.ToString(),
                    Id = Guid.NewGuid()
                },
                new AbstractType
                {
                    Type = AbstractObjectType.Module.ToString(),
                    Module = new Module
                    {
                        ClientType = ClientType.Pg
                    },
                    Id = Guid.NewGuid()
                }
            };

            EFContext.AbstractType.AddRange(abstractTypes);
            EFContext.Country.Add(country);
            EFContext.CostUser.AddRange(costUsers);
            EFContext.Brand.AddRange(brands);
            EFContext.Currency.Add(currency);

            EFContext.SaveChanges();

            //Act
            await AgencyService.AddAgencyToDb(a5Agency);

            //Assert
            PermissionServiceMock.Verify(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            EFContext.Agency.Should().HaveCount(1);
            EFContext.GlobalAgencyRegion.Should().HaveCount(0);
            EFContext.GlobalAgency.Should().HaveCount(0);
        }
    }
}