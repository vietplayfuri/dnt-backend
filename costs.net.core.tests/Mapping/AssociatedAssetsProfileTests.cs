namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using core.Mapping;
    using core.Models.AssociatedAsset;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class AssociatedAssetsProfileTests
    {
        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => m.AddProfile<AssociatedAssetsProfile>()));
        }

        private IMapper _mapper;

        [Test]
        public void AssociatedAsset_To_AssociatedAssetsProfile_IsValid()
        {
            var aa = new AssociatedAsset
            {
                Id = Guid.NewGuid(),
                CostStageRevisionId = Guid.NewGuid(),
                AdIdId = Guid.NewGuid(),
                CostStageRevision = new CostStageRevision(Guid.NewGuid())
                {
                    CostStage = new CostStage(Guid.NewGuid())
                    {
                        Cost = new Cost
                        {
                            Id = Guid.NewGuid(),
                            LatestCostStageRevisionId = Guid.NewGuid(),
                            CostNumber = "NUMBER"
                        },
                        CostId = Guid.NewGuid()
                    }
                },
                AdId = new ProjectAdId
                {
                    Id = Guid.NewGuid(),
                    Value = "VALUE",
                    ExpectedAssets = new List<ExpectedAsset>
                    {
                        new ExpectedAsset
                        {
                            Id = Guid.NewGuid(),
                            CostStageRevisionId = Guid.NewGuid(),
                            Title = "TITLE",
                            Initiative = "INITIATIVE",
                            CostStageRevision = new CostStageRevision(Guid.NewGuid())
                            {
                                Id = Guid.NewGuid(),
                                Status = CostStageRevisionStatus.Draft,
                                CostStage = new CostStage(Guid.NewGuid())
                                {
                                    Cost = new Cost
                                    {
                                        Id = Guid.NewGuid(),
                                        LatestCostStageRevisionId = Guid.NewGuid(),
                                        CostNumber = "NUMBER"
                                    },
                                    CostId = Guid.NewGuid()
                                }
                            }
                        }
                    }
                }
            };
            // Act
            var model = _mapper.Map<AssociatedAssetViewModel>(aa);
            aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().CostStageRevision.Status = CostStageRevisionStatus.Draft;
            // Assert
            model.Id.Should().Be(aa.Id);
            model.RevisionId.Should().Be(aa.CostStageRevisionId);
            model.CostId.Should().Be(aa.CostStageRevision.CostStage.CostId);
            model.CostNumber.Should().Be(aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().CostStageRevision.CostStage.Cost.CostNumber);
            model.LinkedCostId.Should().Be(aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().CostStageRevision.CostStage.CostId);
            model.LinkedRevisionId.Should().Be(aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().CostStageRevision.CostStage.Cost.LatestCostStageRevisionId.GetValueOrDefault());
            model.AdId.Should().Be(aa.AdId.Value);
            model.Title.Should().Be(aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().Title);
            model.Initiative.Should().Be(aa.AdId.ExpectedAssets.OrderByDescending(ea => ea.Modified).FirstOrDefault().Initiative);
            model.CostStatus.Should().Be(CostStageRevisionStatus.Draft.ToString());
        }

        [Test]
        public void AssociatedAssetsProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}