
namespace costs.net.plugins.tests.PG.Extensions
{
    using AutoMapper;
    using costs.net.core.Builders;
    using costs.net.core.Builders.Workflow;
    using costs.net.core.Models;
    using costs.net.core.Services;
    using costs.net.core.Services.Rules;
    using costs.net.dataAccess;
    using costs.net.plugins.PG.Builders.Search;
    using costs.net.plugins.PG.Builders.Workflow;
    using costs.net.tests.common.Stubs.EFContext;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Extensions;
    using plugins.PG.Form;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class PgCostViewDetailsTests : PgCostViewDetailsTestsBase
    {
        [Test]
        public async Task CostDetails_Schema_Validation()
        {
            CostTemplateServiceMock.Setup(s => s.GetCostTemplateVersion(It.IsAny<Guid>()))
                .ReturnsAsync(new core.Models.CostTemplate.CostTemplateVersionModel {
                    
                });

            CostTemplateServiceMock.Setup(s => s.GetCostTemplate(It.IsAny<Guid>()))
                .ReturnsAsync(new core.Models.CostTemplate.CostTemplateDetailsModel
                {

                });

            CostServiceMock.Setup(s => s.GetExchangeRatesOfCurrenciesInCost(It.IsAny<Cost>()))
                .ReturnsAsync(new List<ExchangeRate>
                {
                    new ExchangeRate {
                        EffectiveFrom = DateTime.Now.AddMonths(-1),
                        FromCurrency = CurrencyUsdId,
                        Rate = 1,
                        ToCurrency = CurrencyUsdId
                    }
                });

            var costDetailModel = await CostViewDetails.GetCostDetails(CostId, UserIdentity, RevisionId);

            costDetailModel.Should().NotBeNull();
            costDetailModel.Cost.Should().NotBeNull();
            costDetailModel.Project.Should().NotBeNull();
            costDetailModel.SelectedRevision.Should().NotBeNull();
            costDetailModel.CostStages.Should().NotBeNull();
            foreach (var item in costDetailModel.CostStages)
            {
                item.cost.Should().NotBeNull();
            }

            costDetailModel.SelectedRevision.Approvals.Any(a => a.Type == ApprovalType.Brand && a.ApprovalMembers.Any() && a.Requisitioners.Any()).Should().BeFalse();
        }
    }
}
