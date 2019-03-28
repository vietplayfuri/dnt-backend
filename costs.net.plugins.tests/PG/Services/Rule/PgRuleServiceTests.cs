namespace costs.net.plugins.tests.PG.Services.Rule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Models.Stage;
    using plugins.PG.Services.Rule;

    [TestFixture]
    public class PgRuleServiceTests
    {
        private EFContext _efContext;
        private PgRuleService _pgRuleService;

        private static readonly CostStageRevisionStatus[] NonDraftStatuses = Enum.GetValues(typeof(CostStageRevisionStatus))
            .Cast<CostStageRevisionStatus>()
            .Where(c => c != CostStageRevisionStatus.Draft)
            .ToArray();

        private static readonly CostStages[] NonOEAIPEStages = Enum.GetValues(typeof(CostStages))
            .Cast<CostStages>()
            .Where(c => c != CostStages.OriginalEstimate && c != CostStages.Aipe)
            .ToArray();

        [SetUp]
        public void Init()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _pgRuleService = new PgRuleService(_efContext);
        }

        [Test]
        public async Task CanEditIONumber_When_OEStageAndStatusDraft_Should_ReturnTrue()
        {
            // Arrange
            var costStage = new CostStage { Key = CostStages.OriginalEstimate.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage>
                {
                    costStage
                },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStage
                },
                Status = CostStageRevisionStatus.Draft
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeTrue();
        }

        [Test]
        public async Task CanEditIONumber_When_AIPE_OEStageDraft_Should_ReturnFalse()
        {
            // Arrange
            var currentStage = new CostStage{ Key = CostStages.OriginalEstimate.ToString()};
            var cost =new Cost
            {
                Id = Guid.NewGuid(),
                CostStages =  new List<CostStage>
                {
                    new CostStage{ Key =  CostStages.Aipe.ToString()},
                    currentStage
                },
                LatestCostStageRevision =  new CostStageRevision
                {
                    CostStage = currentStage
                },
                Status =  CostStageRevisionStatus.Draft
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        public async Task CanEditIONumber_When_UsageAndBuyout_FAStageAndStatusDraft_Should_ReturnTrue()
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.FinalActual.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft,
                CostType = CostType.Buyout

            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeTrue();
        }

        [Test]
        public async Task CanEditIONumber_When_UsageAndBuyout_FAStageAndStatusDraft_TwoStages_Should_ReturnFalse()
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.FinalActual.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj , new CostStage { } },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft,
                CostType = CostType.Buyout

            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        public async Task CanEditIONumber_When_Production_FAStageAndStatusDraft_Should_ReturnFalse()
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.FinalActual.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft,
                CostType = CostType.Production
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        public async Task CanEditIONumber_When_Trafficking_FAStageAndStatusDraft_Should_ReturnTrue()
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.FinalActual.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft,
                CostType = CostType.Trafficking
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeTrue();
        }

        [Test]
        public async Task CanEditIONumber_When_Trafficking_FAStageAndStatusDraft_TwoStages_Should_ReturnFalse()
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.FinalActual.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj, new CostStage { } },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft,
                CostType = CostType.Trafficking
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        [TestCaseSource(nameof(NonDraftStatuses))]
        public async Task CanEditIONumber_When_OEStageAndStatusIsNotDraft_Should_ReturnFalse(CostStageRevisionStatus status)
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.OriginalEstimate.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = status
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        [TestCaseSource(nameof(NonDraftStatuses))]
        public async Task CanEditIONumber_When__UsageAndBuyout_FAStageAndStatusIsNotDraft_Should_ReturnFalse(CostStageRevisionStatus status)
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.OriginalEstimate.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = status,
                CostType = CostType.Buyout
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        [TestCaseSource(nameof(NonDraftStatuses))]
        public async Task CanEditIONumber_When__Trafficking_FAStageAndStatusIsNotDraft_Should_ReturnFalse(CostStageRevisionStatus status)
        {
            // Arrange
            var costStageObj = new CostStage { Key = CostStages.OriginalEstimate.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj },
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = status,
                CostType = CostType.Trafficking
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        [TestCaseSource(nameof(NonOEAIPEStages))]
        public async Task CanEditIONumber_When_NotOEAIPEStagesAndStatusIsDraft_Should_ReturnFalse(CostStages costStage)
        {
            // Arrange
            var costStageObj = new CostStage { Key = costStage.ToString() };
            var cost = new Cost
            {
                Id = Guid.NewGuid(),
                CostStages = new List<CostStage> { costStageObj},
                LatestCostStageRevision = new CostStageRevision
                {
                    CostStage = costStageObj
                },
                Status = CostStageRevisionStatus.Draft
            };
            _efContext.Add(cost);
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(cost.Id);

            // Assert
            canEditIONumber.Should().BeFalse();
        }

        [Test]
        public async Task CanEditIONumber_When_CostDoesNotExist_Should_ReturnFalse()
        {
            // Arrange
            _efContext.SaveChanges();

            // Act
            var canEditIONumber = await _pgRuleService.CanEditIONumber(Guid.NewGuid());

            // Assert
            canEditIONumber.Should().BeFalse();
        }
    }
}
