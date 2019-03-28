namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Builders.Response;
    using core.Mapping;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class SearchItemsProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<SearchItemProfile>();
            }));
        }

        private CostUser GetUser()
        {
            var agency = GetAgency();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var email = "me@me.com";
            var businessRoleId = Guid.NewGuid();
            var objectId = Guid.NewGuid();
            return new CostUser
            {
                Id = userId,
                FirstName = "Clark",
                LastName = "Kent",
                FullName = "Clark Kent",
                GdamUserId = "gdam",
                Email = email,
                AgencyId = agency.Id,
                UserGroups = new[] { "One User Group" },
                Agency = agency,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        Id = Guid.NewGuid(),
                        BusinessRoleId = businessRoleId,
                        BusinessRole = new BusinessRole
                        {
                            Id = businessRoleId,
                            Key = "Owner",
                            Value = "Owner",
                            RoleId = roleId
                        },
                        ObjectId = objectId,
                        ObjectType = "Tent"
                    }
                },
                ApprovalLimit = 100,
                ApprovalBandId = Guid.NewGuid(),
                GdamBusinessUnits = new[] { $"{Guid.NewGuid()}" }
            };
        }

        private Agency GetAgency()
        {
            return new Agency
            {
                Name = "Agency Name",
                Id = Guid.NewGuid(),
                Version = 1,
                CountryId = Guid.NewGuid(),
                Disabled = false,
                GdamAgencyId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 24),
                Gid = "GID",
                PrimaryCurrency = Guid.NewGuid(),
                Labels = new[] { "label1", "label2", "CM_Prime_ADS" }
            };
        }

        private Project GetProject()
        {
            return new Project
            {
                AdCostNumber = "Number",
                Advertiser = "Advertiser",
                BrandId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Name = "Name",
                CreatedById = Guid.NewGuid(),
                CampaignId = Guid.NewGuid(),
                Created = DateTime.Now,
                GdamProjectId = Guid.NewGuid().ToString(),
                Modified = DateTime.Now,
                ShortId = "2",
                Version = 1,
                SubBrand = "subBrand",
                AgencyId = Guid.NewGuid()
            };
        }

        private AbstractType GetAbstractAgency()
        {
            var agency = GetAgency();
            return new AbstractType
            {
                Agency = agency,
                ObjectId = agency.Id,
                Id = Guid.NewGuid(),
                Type = AbstractObjectType.Agency.ToString(),
                ParentId = Guid.NewGuid(),
                UserGroups = new[] { $"{Guid.NewGuid()}" }
            };
        }

        private DictionaryEntry GetDictionaryEntry()
        {
            return new DictionaryEntry
            {
                Id = Guid.NewGuid(),
                DictionaryId = Guid.NewGuid(),
                Key = "A Key",
                Value = "A Value",
                Visible = false
            };
        }

        [Test]
        public void AbstractAgency_To_AgencySearchItem_IsValid()
        {
            // Arrange
            var abstractAgency = GetAbstractAgency();

            // Act
            var model = _mapper.Map<AbstractType, AgencySearchItem>(abstractAgency);

            // Assert
            model.Id.Should().Be(abstractAgency.Id.ToString());
            model.ObjectId.Should().Be(abstractAgency.Agency.Id.ToString());
            model.Name.Should().Be(abstractAgency.Agency.Name);
            model.DisplayName.Should().Be(abstractAgency.Agency.Name);
            model.ParentId.Should().Be(abstractAgency.ParentId.ToString());
            model.CurrencyId.Should().Be(abstractAgency.Agency.PrimaryCurrency.ToString());
            model.CountryId.Should().Be(abstractAgency.Agency.CountryId.ToString());
            model.GdamId.Should().Be(abstractAgency.Agency.GdamAgencyId);
            model.Labels.Count.Should().Be(abstractAgency.Agency.Labels.Count());
            model.UserGroups.Count.Should().Be(abstractAgency.UserGroups.Count());
            model.Disabled.Should().Be(abstractAgency.Agency.Disabled);
            model.Version.Should().Be(abstractAgency.Agency.Version);
        }

        [Test]
        public void CostUser_To_CostUserSearchItem_IsValid()
        {
            // Arrange
            var costUser = GetUser();

            // Act
            var model = _mapper.Map<CostUser, CostUserSearchItem>(costUser);

            // Assert
            model.Id.Should().Be(costUser.Id.ToString());
            model.FirstName.Should().Be(costUser.FirstName);
            model.LastName.Should().Be(costUser.LastName);
            model.Agency.Id.Should().Be(costUser.AgencyId.ToString());
            model.Agency.Name.Should().Be(costUser.Agency.Name);
            model.UserGroups.Count.Should().Be(1);
            model.UserGroups.First().Should().Be(costUser.UserGroups.First());
            model.ApprovalBandId.Should().Be(costUser.ApprovalBandId.ToString());
            model.ApprovalLimit.Should().Be(costUser.ApprovalLimit.ToString());
            model.Email.Should().Be(costUser.Email);
            model.GdamId.Should().Be(costUser.GdamUserId);
            model.Version.Should().Be(costUser.Version);
            model.Disabled.Should().Be(costUser.Disabled);
            model.GdamBusinessUnits.Count.Should().Be(1);
            model.GdamBusinessUnits.First().Should().Be(costUser.GdamBusinessUnits.First());

            model.BusinessRoles.First().Value.Should().Be(costUser.UserBusinessRoles.First().BusinessRole.Value);
            model.BusinessRoles.First().Id.Should().Be(costUser.UserBusinessRoles.First().BusinessRole.Id);
            model.BusinessRoles.First().Id.Should().Be(costUser.UserBusinessRoles.First().BusinessRoleId);
            model.BusinessRoles.First().ObjectId.Should().Be(costUser.UserBusinessRoles.First().ObjectId);
            model.BusinessRoles.First().ObjectId.Should().Be(costUser.UserBusinessRoles.First().ObjectId);
            model.BusinessRoles.First().ObjectType.Should().Be(costUser.UserBusinessRoles.First().ObjectType);
        }

        [Test]
        public void Project_To_ProjectSearchItem_IsValid()
        {
            // Arrange
            var project = GetProject();

            // Act
            var model = _mapper.Map<Project, ProjectSearchItem>(project);

            // Assert
            model.Id.Should().Be(project.Id.ToString());
            model.AgencyId.Should().Be(project.AgencyId.ToString());
            model.Name.Should().Be(project.Name);
            model.ProjectNumber.Should().Be(project.AdCostNumber);
            model.AgencyId.Should().Be(project.AgencyId.ToString());
            model.Advertiser.Should().Be(project.Advertiser);
            model.CampaignId.Should().Be(project.CampaignId?.ToString());
            model.SubBrand.Should().Be(project.SubBrand);
            model.ShortId.First().Should().Be(project.ShortId.First());
            model.GdamProjectId.Should().Be(project.GdamProjectId);
            model.CreatedBy.Should().Be(project.CreatedById.ToString());
            model.CreatedDate.Should().Be(project.Created);
            model.ModifiedDate.Should().Be(project.Modified.Value);
            model.Version.Should().Be(project.Version);
        }

        [Test]
        public void SearchItemProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void DictionaryEntry_To_DictionaryEntrySearchItem_IsValid()
        {
            var dictionaryEntry = GetDictionaryEntry();

            // Act
            var model = _mapper.Map<DictionaryEntry, DictionaryEntrySearchItem>(dictionaryEntry);

            // Assert
            model.Id.Should().Be(dictionaryEntry.Id.ToString());
            model.DictionaryId.Should().Be(dictionaryEntry.DictionaryId.ToString());
            model.Key.Should().Be(dictionaryEntry.Key);
            model.Value.Should().Be(dictionaryEntry.Value);
            model.Visible.Should().Be(dictionaryEntry.Visible);
            // unmapped
            model.Version.Should().Be(default(int));
        }
    }
}