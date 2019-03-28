
namespace costs.net.plugins.tests.PG.Builders.Payments
{
    using System;
    using System.Collections.Generic;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Builders.Payments;

    [TestFixture]
    public class PgCostStageRevisionTotalPaymentsBuilderTests
    {
        private PgCostStageRevisionTotalPaymentsBuilder _target;

        [SetUp]
        public void Setup()
        {
            _target = new PgCostStageRevisionTotalPaymentsBuilder();
        }

        [Test]
        public void Null_Payments_Returns_Empty()
        {
            //Arrange
            List<CostStageRevisionPaymentTotal> payments = null;
            var expected = 0;

            //Act
            var result = _target.Build(payments);

            result.Should().NotBeNull();
            result.CarryOverAmount.Should().Be(expected);
            result.InsuranceCostPayments.Should().Be(expected);
            result.OtherCostPayments.Should().Be(expected);
            result.PostProductionCostPayments.Should().Be(expected);
            result.ProductionCostPayments.Should().Be(expected);
            result.TargetBudgetTotalCostPayments.Should().Be(expected);
            result.TechnicalFeeCostPayments.Should().Be(expected);
            result.TotalCostPayments.Should().Be(expected);
        }

        [Test]
        public void Normal_Parameters_Returns_TotalPayments()
        {
            //Arrange
            var expected = 10;
            var payments = new List<CostStageRevisionPaymentTotal>
            {
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.InsuranceTotal,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TechnicalFee,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.PostProduction,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Production,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.TargetBudgetTotal,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.Other,
                    LineItemTotalCalculatedValue = 10
                },
                new CostStageRevisionPaymentTotal
                {
                    LineItemTotalType = Constants.CostSection.CostTotal,
                    LineItemTotalCalculatedValue = 10,
                    CalculatedAt = DateTime.UtcNow,
                    LineItemRemainingCost = 10
                }
            };

            //Act
            var result = _target.Build(payments);

            result.Should().NotBeNull();
            result.CarryOverAmount.Should().Be(expected);
            result.InsuranceCostPayments.Should().Be(expected);
            result.OtherCostPayments.Should().Be(expected);
            result.PostProductionCostPayments.Should().Be(expected);
            result.ProductionCostPayments.Should().Be(expected);
            result.TargetBudgetTotalCostPayments.Should().Be(expected);
            result.TechnicalFeeCostPayments.Should().Be(expected);
            result.TotalCostPayments.Should().Be(expected);
        }
    }
}
