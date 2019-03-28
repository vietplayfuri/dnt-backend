
namespace costs.net.plugins.PG.Services.BillingExpenses
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Builders.BillingExpenses;
    using core.Models.BillingExpenses;
    using dataAccess.Entity;
    using MoreLinq;
    using System.Linq;

    public class BillingExpenseCalculator : IBillingExpenseCalculator
    {
        public async Task CalculateExpenses(decimal contractTotal, BillingExpenseData data, IList<BillingExpense> billingExpenses)
        {
            var summaryRows = data.Summary.Rows;
            var headerRows = data.Header.Rows;
            data.Sections.SelectMany(s => s.Rows).ForEach(r => r.Total = GetTotal(r.Cells));
            var financialYears = data.FinancialYears;

            foreach(var section in data.Sections)
            {
                for(var y = 0; y < financialYears.Count; y++)
                {
                    section.Totals?.Cells.Add(GetColumnTotal(section, y));
                }
                section.Totals.Total = section.Totals.Cells.Sum(c => c.Value);
            }

            var noOfMonthsInFy = data.Header.Rows.First(r => r.Key == Constants.BillingExpenseItem.NumberOfMonthsFY);
            var totalMonths = (int)noOfMonthsInFy.Cells.Sum(fy => fy.Value);

            for (var y = 0; y < financialYears.Count; y++)
            {
                foreach(var row in summaryRows)
                {
                    CalculateSummaryValue(row, y, (int)noOfMonthsInFy.Cells[y].Value, totalMonths, data, contractTotal);
                }
            }

            foreach (var row in summaryRows)
            {
                row.Total = GetTotal(row.Cells);
            }

            var balancePrepaid = headerRows.First(row => row.Key == Constants.BillingExpenseItem.BalancePrepaid);
            var balanceToBeMoved = summaryRows.First(row => row.Key == Constants.BillingExpenseSummaryItem.BalanceToBeMoved);
            var currentBalance = 0M;
            for(var y = 0; y < financialYears.Count; y++)
            {
                //Use the previous balance with the current balancePrepaid.
                var year = financialYears[y];
                balancePrepaid.Cells.Add(new BillingExpenseItem
                {
                    Value = currentBalance,
                    Year = year,
                    ReadOnly = true,
                    Key = Constants.BillingExpenseItem.BalancePrepaid,
                    SectionKey = Constants.BillingExpenseSection.Header
                });
                currentBalance = balanceToBeMoved.Cells[y].Value;
            }

            foreach (var row in headerRows)
            {
                row.Total = GetTotal(row.Cells);
            }
        }

        private static BillingExpenseCalculatedItem GetColumnTotal(BillingExpenseDataSection section, int yearIndex)
        {
            return new BillingExpenseCalculatedItem(section.Rows.Sum(r => r.Cells[yearIndex].Value));
        }

        private void CalculateSummaryValue(BillingExpenseCalculatedRow row, int yearIndex, int monthsInYear, int totalMonths, BillingExpenseData data, decimal targetBudgetAmount)        
        {
            var total = 0M;
            switch (row.Key)
            {
                case Constants.BillingExpenseSummaryItem.TotalContractTermsAndIncurredCosts:
                    total = CalculateContractTermsAndIncurredCosts(yearIndex, data.Sections);
                    break;
                case Constants.BillingExpenseSummaryItem.ExpensePerFY:
                    total = CalculateExpensePerFinancialYear(totalMonths, monthsInYear, targetBudgetAmount, yearIndex, data.Sections);
                    break;
                case Constants.BillingExpenseSummaryItem.BalanceToBeMoved:
                    total = CalculateBalanceToBeMoved(yearIndex, data.Summary);
                    break;
            }
            row.Cells.Add(new BillingExpenseCalculatedItem(total));
        }

        private static decimal CalculateContractTermsAndIncurredCosts(int yearIndex, IList<BillingExpenseDataSection> dataSections)
        {
            var totalContractTerms = GetTotal(Constants.BillingExpenseSectionTotal.ContractTerms, yearIndex, dataSections);
            var totalIncurredCosts = GetTotal(Constants.BillingExpenseSectionTotal.IncurredCosts, yearIndex, dataSections);

            return totalContractTerms + totalIncurredCosts;
        }

        private static decimal CalculateExpensePerFinancialYear(int totalMonths, int monthsInYear, decimal targetBudgetAmount, int yearIndex, IList<BillingExpenseDataSection> dataSections)
        {
            var totalIncurredCosts = GetTotal(Constants.BillingExpenseSectionTotal.IncurredCosts, yearIndex, dataSections);
            return totalMonths == 0 ? 0 : ((targetBudgetAmount / totalMonths) * monthsInYear) + totalIncurredCosts;
        }

        private static decimal CalculateBalanceToBeMoved(int yearIndex, BillingExpenseDataSummary summary)
        {
            var totalContractTermsAndIncurredCosts = GetTotal(Constants.BillingExpenseSummaryItem.TotalContractTermsAndIncurredCosts, yearIndex, summary);
            var expensePerFinancialYear = GetTotal(Constants.BillingExpenseSummaryItem.ExpensePerFY, yearIndex, summary);
            var previousBalance = 0m;
            if (yearIndex > 0)
            {
                previousBalance = GetTotal(Constants.BillingExpenseSummaryItem.BalanceToBeMoved, yearIndex-1, summary);
            }

            return previousBalance + totalContractTermsAndIncurredCosts - expensePerFinancialYear;
        }

        private static decimal GetTotal(IList<BillingExpenseItem> cells)
        {
            return cells.Sum(cell => cell.Value);
        }

        private static decimal GetTotal(IList<BillingExpenseCalculatedItem> cells)
        {
            return cells.Sum(c => c.Value);
        }

        private static decimal GetTotal(string totalKey, int yearIndex, IList<BillingExpenseDataSection> dataSections)
        {
            var total = 0M;

            dataSections.ForEach(section =>
            {
                if (section.Totals != null && section.Totals.Key == totalKey)
                {
                    total = section.Totals.Cells[yearIndex].Value;
                }
            });
            return total;
        }

        private static decimal GetTotal(string totalKey, int yearIndex, BillingExpenseDataSummary summary)
        {
            var row = summary.Rows.FirstOrDefault(r => r.Key == totalKey);
            var total = row.Cells[yearIndex].Value;
            return total;
        }
    }
}
