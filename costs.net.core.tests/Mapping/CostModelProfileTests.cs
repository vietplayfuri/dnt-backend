namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Diagnostics;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Costs;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class CostModelProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.AddProfile<CostModelProfile>();
                m.AddProfile<UserProfile>();
            }));
        }

        [Test]
        public void Cost_To_CostModel_IsValid()
        {
            // Arrange
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now.AddDays(1),
                UserGroups = new[] { "asdasd", "sdfsdf" },
                ParentId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                CostTemplateVersionId = Guid.NewGuid(),
                CostType = CostType.Production,
                Status = CostStageRevisionStatus.Draft,
                LatestCostStageRevisionId = Guid.NewGuid(),
                CreatedBy = new CostUser { Id = Guid.NewGuid() },
                Owner = new CostUser { Id = Guid.NewGuid() },
                CostNumber = "23470923429",
                Version = 100,
                IsExternalPurchases = true,
                ExchangeRateDate = DateTime.Now.AddDays(2)
            };

            // Act
            var model = _mapper.Map<Cost, CostModel>(cost);

            // Assert
            model.Id.Should().Be(cost.Id);
            model.CreatedById.Should().Be(cost.CreatedById);
            model.OwnerId.Should().Be(cost.OwnerId);
            model.Created.Should().Be(cost.Created);
            model.Modified.Should().Be(cost.Modified.Value);
            model.UserGroups.Should().BeEquivalentTo(cost.UserGroups);
            model.ParentId.Should().Be(cost.ParentId);
            model.ProjectId.Should().Be(cost.ProjectId);
            model.CostTemplateVersionId.Should().Be(cost.CostTemplateVersionId);
            model.CostType.Should().Be(cost.CostType);
            model.Status.Should().Be(cost.Status);
            model.LatestCostStageRevisionId.Should().Be(cost.LatestCostStageRevisionId.Value);
            model.CreatedBy.Id.Should().Be(cost.CreatedBy.Id);
            model.Owner.Id.Should().Be(cost.Owner.Id);
            model.CostNumber.Should().Be(cost.CostNumber);
            model.Version.Should().Be(cost.Version);
            model.IsExternalPurchases.Should().Be(cost.IsExternalPurchases);
            model.ExchangeRateDate.Should().Be(cost.ExchangeRateDate);
        }

        [Test]
        public void CostModelProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void CostStageRevision_To_RevisionModel_IsValid()
        {
            // Arrange
            var costStageRevision = new CostStageRevision
            {
                Id = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                Created = DateTime.Now,
                Modified = DateTime.Now.AddDays(1),
                Status = CostStageRevisionStatus.Draft,
                CreatedBy = new CostUser { Id = Guid.NewGuid() }
            };

            // Act
            var model = _mapper.Map<CostStageRevision, RevisionModel>(costStageRevision);

            // Assert
            model.Id.Should().Be(costStageRevision.Id);
            model.CostStageId.Should().Be(costStageRevision.CostStageId);
            model.Name.Should().Be(costStageRevision.Name);
            model.Status.Should().Be(costStageRevision.Status);
            model.StageDetailsId.Should().Be(costStageRevision.StageDetailsId);
            model.ProductDetailsId.Should().Be(costStageRevision.ProductDetailsId);
            model.CreatedById.Should().Be(costStageRevision.CreatedById);
            model.IsPaymentCurrencyLocked.Should().Be(costStageRevision.IsPaymentCurrencyLocked);
            model.IsLineItemSectionCurrencyLocked.Should().Be(costStageRevision.IsLineItemSectionCurrencyLocked);
            model.Created.Should().Be(costStageRevision.Created);
            model.Modified.Should().Be(costStageRevision.Modified.Value);
            model.CreatedBy.Id.Should().Be(costStageRevision.CreatedBy.Id);
        }
    }
}