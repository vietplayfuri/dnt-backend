
namespace costs.net.plugins.PG.Builders.BillingExpenses
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using core.Builders.BillingExpenses;
    using core.Models.BillingExpenses;
    using dataAccess.Entity;
    using Models.Stage;
    using MoreLinq;
    using Serilog;
    using FinancialYear = core.Models.BillingExpenses.FinancialYear;


    public class BillingExpenseBuilder : IBillingExpenseBuilder
    {
        private static readonly ILogger Logger = Log.ForContext<BillingExpenseBuilder>();

        private readonly IMapper _mapper;

        public BillingExpenseBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public BillingExpenseData BuildExpenses(CostStage costStage, IList<BillingExpense> billingExpenses, IList<FinancialYear> financialYears)
        {
            var billingExpensesLookup = CreateLookup(billingExpenses);
            var data = new BillingExpenseData();
            financialYears.ForEach(fy => data.FinancialYears.Add(fy.ShortName));

            data.Sections.Add(new BillingExpenseDataSection
            {
                Key = Constants.BillingExpenseSection.ContractTerms,
                Label = "Billing to P&G for contract terms"
            });
            data.Sections.Add(new BillingExpenseDataSection
            {
                Key = Constants.BillingExpenseSection.IncurredCosts,
                Label = "Incurred costs, pre, bonus payments, etc."
            });

            data.Header.Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.BalancePrepaid,
                Label = "BALANCE FROM PRIOR FY/PREPAID",
                Type = "currency"
            });
            var noOfMonthsRow = new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.NumberOfMonthsFY,
                Label = "No. of months in contract term per FY",
                Type = "number"
            };
            data.Header.Rows.Add(noOfMonthsRow);

            data.Sections[0].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.UsageBuyoutFee,
                Label = "Base Compensation",
                Type = "currency"
            });
            data.Sections[0].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.PensionAndHealth,
                Label = "Pension & Health",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.Bonus,
                Label = "Bonus",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.AgencyFee,
                Label = "Agency fee (PRE or other)",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.OtherCosts,
                Label = "Other incurred costs (including non-reclaimable taxes)",
                Type = "currency"
            });

            data.Sections[0].Totals = new BillingExpenseTotalRow
            {
                SectionKey = Constants.BillingExpenseSection.ContractTerms,
                Key = Constants.BillingExpenseSectionTotal.ContractTerms,
                Label = "Total billing for contract terms"
            };
            data.Sections[1].Totals = new BillingExpenseTotalRow
            {
                SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                Key = Constants.BillingExpenseSectionTotal.IncurredCosts,
                Label = "Total billing for incurred costs, bonuses, etc."
            };

            financialYears.ForEach(year =>
            {
                data.Sections.ForEach(section =>
                {
                    section.Rows.ForEach(row =>
                    {
                        if (row.Key != Constants.BillingExpenseItem.NumberOfMonthsFY)
                        {
                            row.Cells.Add(
                                GetOrCreateItem(billingExpensesLookup, section.Key, row.Key, year.ShortName)
                        );
                    }
                    });
                });                
            });
            var totalMonths = 0;
            financialYears.ForEach(year =>
            {
                noOfMonthsRow.Cells.Add(GetNumberOfMonthsItem(billingExpensesLookup, costStage, year));
                totalMonths += year.Months;
            });
            
            noOfMonthsRow.Total = totalMonths;
            
            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.TotalContractTermsAndIncurredCosts,
                Label = "Total billing for contract terms & bonuses, etc.",
                Type = "currency"
            });
            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.ExpensePerFY,
                Label = "Expense per FY",
                Type = "currency"
            });
            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.BalanceToBeMoved,
                Label = "Balance to be moved to prepaid",
                Type = "currency"
            });

            return data;
        }

        public BillingExpenseData BuildExpenses(CostStage costStage, IList<BillingExpenseItem> billingExpenses, IList<FinancialYear> financialYears)
        {
            var billingExpensesLookup = CreateLookup(billingExpenses);
            var data = new BillingExpenseData();
            financialYears.ForEach(fy => data.FinancialYears.Add(fy.ShortName));

            data.Sections.Add(new BillingExpenseDataSection
            {
                Key = Constants.BillingExpenseSection.ContractTerms,
                Label = "Billing to P&G for contract terms"
            });
            data.Sections.Add(new BillingExpenseDataSection
            {
                Key = Constants.BillingExpenseSection.IncurredCosts,
                Label = "Incurred costs, pre, bonus payments, etc."
            });

            data.Header.Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.BalancePrepaid,
                Label = "BALANCE FROM PRIOR FY/PREPAID",
                Type = "currency"
            });
            var noOfMonthsRow = new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.NumberOfMonthsFY,
                Label = "No. of months in contract term per FY",
                Type = "number"
            };
            data.Header.Rows.Add(noOfMonthsRow);
            data.Sections[0].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.UsageBuyoutFee,
                Label = "Base Compensation",
                Type = "currency"
            });
            data.Sections[0].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.PensionAndHealth,
                Label = "Pension & Health",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.Bonus,
                Label = "Bonus",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.AgencyFee,
                Label = "Agency fee (PRE or other)",
                Type = "currency"
            });
            data.Sections[1].Rows.Add(new BillingExpenseRow
            {
                Key = Constants.BillingExpenseItem.OtherCosts,
                Label = "Other incurred costs (including non-reclaimable taxes)",
                Type = "currency"
            });

            data.Sections[0].Totals = new BillingExpenseTotalRow
            {
                SectionKey = Constants.BillingExpenseSection.ContractTerms,
                Key = Constants.BillingExpenseSectionTotal.ContractTerms,
                Label = "Total billing for contract terms"
            };
            data.Sections[1].Totals = new BillingExpenseTotalRow
            {
                SectionKey = Constants.BillingExpenseSection.IncurredCosts,
                Key = Constants.BillingExpenseSectionTotal.IncurredCosts,
                Label = "Total billing for incurred costs, bonuses, etc."
            };

            financialYears.ForEach(year =>
            {
                noOfMonthsRow.Cells.Add(GetItem(billingExpensesLookup, Constants.BillingExpenseSection.Header, Constants.BillingExpenseItem.NumberOfMonthsFY, year.ShortName));

                data.Sections.ForEach(section =>
                {
                    section.Rows.ForEach(row =>
                    {
                        if (row.Key != Constants.BillingExpenseItem.NumberOfMonthsFY)
                        {
                            row.Cells.Add(
                                GetItem(billingExpensesLookup, section.Key, row.Key, year.ShortName)
                            );
                        }
                    });
                });
            });

            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.TotalContractTermsAndIncurredCosts,
                Label = "Total billing for contract terms & bonuses, etc.",
                Type = "currency"
            });
            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.ExpensePerFY,
                Label = "Expense per FY",
                Type = "currency"
            });
            data.Summary.Rows.Add(new BillingExpenseCalculatedRow
            {
                Key = Constants.BillingExpenseSummaryItem.BalanceToBeMoved,
                Label = "Balance to be moved to prepaid",
                Type = "currency"
            });

            return data;
        }

        private BillingExpenseItem GetNumberOfMonthsItem(IDictionary<string, BillingExpense> lookup, CostStage costStage, FinancialYear year)
        {
            var readOnly = false;

            if (costStage.Name == CostStages.FinalActual.ToString())
            {
                readOnly = true;
            }
            else if (year.IsPastYear())
            {
                readOnly = true;
            }
            var item = GetOrCreateItem(lookup, Constants.BillingExpenseSection.Header, Constants.BillingExpenseItem.NumberOfMonthsFY, year.ShortName, year.Months);
            item.ReadOnly = readOnly;

            return item;
        }

        private BillingExpenseItem GetItem(IDictionary<string, BillingExpenseItem> lookup, string sectionKey, string key, string year, decimal value = 0)
        {
            string lookupKey = CreateLookupKey(sectionKey, key, year);
            if (lookup.ContainsKey(lookupKey))
            {
                return lookup[lookupKey];
            }

            string message = $"Invalid Billing Expense sent from front end for calculation {sectionKey}-{key}-{year}";
            Logger.Error(message);
            throw new InvalidOperationException(message);
        }

        private BillingExpenseItem GetOrCreateItem(IDictionary<string, BillingExpense> lookup, string sectionKey, string key, string year, decimal value = 0)
        {
            string lookupKey = CreateLookupKey(sectionKey, key, year);

            if (lookup.ContainsKey(lookupKey))
            {
                return _mapper.Map<BillingExpenseItem>(lookup[lookupKey]);
            }

            return new BillingExpenseItem
            {
                SectionKey = sectionKey,
                Key = key,
                Year = year,
                Value = value
            };
        }

        private static IDictionary<string, BillingExpense> CreateLookup(IList<BillingExpense> billingExpenses)
        {
            var lookup = new Dictionary<string, BillingExpense>();

            foreach (var expense in billingExpenses)
            {
                string key = CreateLookupKey(expense.SectionKey, expense.Key, expense.Year);
                lookup[key] = expense;
            }

            return lookup;
        }

        private static IDictionary<string, BillingExpenseItem> CreateLookup(IList<BillingExpenseItem> billingExpenses)
        {
            var lookup = new Dictionary<string, BillingExpenseItem>();

            foreach (var expense in billingExpenses)
            {
                string key = CreateLookupKey(expense.SectionKey, expense.Key, expense.Year);
                lookup[key] = expense;
            }

            return lookup;
        }

        private static string CreateLookupKey(string sectionKey, string key, string year)
        {
            return $"{sectionKey}-{key}-{year}";
        }
    }
}
