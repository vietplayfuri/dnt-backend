
namespace costs.net.plugins.tests.PG.Services.BillingExpenses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.BillingExpenses;
    using core.Mapping;
    using core.Models.BillingExpenses;
    using core.Services.BillingExpenses;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Builders.BillingExpenses;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Services.BillingExpenses;

    [TestFixture]
    public class BillingExpensesServiceTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();

        private BillingExpensesService _target;

        [SetUp]
        public void Init()
        {
            var financialYearService = new Mock<IFinancialYearService>();
            var costFormService = new Mock<ICostFormService>();
            var costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();

            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(NotificationProfile),
                    typeof(PgPurchaseOrderResponse)
                )
            );
            var mapper = new Mapper(configuration);
            var billingExpenseBuilder = new BillingExpenseBuilder(mapper);
            var billingExpenseCalculator = new BillingExpenseCalculator();
            var interpolatorMock = new Mock<IBillingExpenseInterpolator>();

            _target = new BillingExpensesService(
                _efContextMock.Object,
                mapper, 
                financialYearService.Object, 
                costFormService.Object,
                billingExpenseBuilder, 
                billingExpenseCalculator,
                interpolatorMock.Object,
                costStageRevisionServiceMock.Object
                );
        }

        [Test]
        public async Task Upsert_No_Items_No_Existing()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingItems = new List<BillingExpense>();
            var upsertItems = new List<BillingExpenseItem>();

            _efContextMock.MockAsyncQueryable(existingItems.AsQueryable(), c => c.BillingExpense);

            // Act
            var result = await _target.Upsert(costId, costStageRevisionId, userId, upsertItems);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task Upsert_Ignore_CalculatedFields_New_Expenses()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingItems = new List<BillingExpense>();
            var upsertItems = new List<BillingExpenseItem>
            {
                new BillingExpenseItem
                {
                    SectionKey = Constants.BillingExpenseSection.Header,
                    Key = Constants.BillingExpenseItem.BalancePrepaid
                }
            };
            _efContextMock.MockAsyncQueryable(existingItems.AsQueryable(), c => c.BillingExpense);
            const int expectedCount = 0;

            // Act
            var result = await _target.Upsert(costId, costStageRevisionId, userId, upsertItems);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            existingItems.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Upsert_Ignore_CalculatedFields_Existing_Expenses()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingItems = new List<BillingExpense>
            {
                new BillingExpense
                {
                    CostStageRevisionId = costStageRevisionId,
                    CreatedById = userId,
                    Id = Guid.NewGuid(),
                    SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                    Key = Constants.BillingExpenseItem.AgencyFee,
                    Year = "2017/2018",
                    Value = 25,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                }
            };
            var upsertItems = new List<BillingExpenseItem>
            {
                new BillingExpenseItem
                {
                    SectionKey = Constants.BillingExpenseSection.Header,
                    Key = Constants.BillingExpenseItem.BalancePrepaid
                }
            };
            _efContextMock.MockAsyncQueryable(existingItems.AsQueryable(), c => c.BillingExpense);
            const int expectedCount = 1;

            // Act
            var result = await _target.Upsert(costId, costStageRevisionId, userId, upsertItems);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            existingItems.Should().HaveCount(expectedCount);
        }

        [Test]
        public async Task Upsert_Existing_Expenses()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var fy = "2017/2018";
            var existingValue = 25;
            var newValue = 20;
            var existingId = Guid.NewGuid();
            var existingItems = new List<BillingExpense>
            {
                new BillingExpense
                {
                    CostStageRevisionId = costStageRevisionId,
                    CreatedById = userId,
                    Id = existingId,
                    SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                    Key = Constants.BillingExpenseItem.AgencyFee,
                    Year = fy,
                    Value = existingValue,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                }
            };
            var upsertItems = new List<BillingExpenseItem>
            {
                new BillingExpenseItem
                {
                    SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                    Key = Constants.BillingExpenseItem.AgencyFee,
                    Year = fy,
                    Value = newValue,
                    Id = existingId
                }
            };
            _efContextMock.MockAsyncQueryable(existingItems.AsQueryable(), c => c.BillingExpense);
            const int expectedCount = 1;

            // Act
            var result = await _target.Upsert(costId, costStageRevisionId, userId, upsertItems);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            existingItems.Should().HaveCount(expectedCount);
            existingItems.First().Value.Should().Be(newValue);
        }


        [Test]
        public async Task Upsert_Add_New_Expenses()
        {
            // Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var fy = "2017/2018";
            var existingId = Guid.NewGuid();
            var existingValue = 25;
            var newItemValue = 20;

            var existingItems = new List<BillingExpense>
            {
                new BillingExpense
                {
                    CostStageRevisionId = costStageRevisionId,
                    CreatedById = userId,
                    Id = existingId,
                    SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                    Key = Constants.BillingExpenseItem.AgencyFee,
                    Year = fy,
                    Value = existingValue,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                }
            };
            var upsertItems = new List<BillingExpenseItem>
            {
                new BillingExpenseItem
                {
                    SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                    Key = Constants.BillingExpenseItem.AgencyFee,
                    Year = fy,
                    Value = newItemValue,
                }
            };
            _efContextMock.MockAsyncQueryable(existingItems.AsQueryable(), c => c.BillingExpense);

            // Act
            var result = await _target.Upsert(costId, costStageRevisionId, userId, upsertItems);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }
    }
}
