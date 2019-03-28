namespace costs.net.plugins.PG.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders.Payments;
    using core.Models;
    using core.Models.Payments;
    using core.Models.Rule;
    using core.Models.User;
    using core.Models.Workflow;
    using core.Services.Costs;
    using core.Services.Payments;
    using core.Services.Rules;
    using core.Services.Workflow;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Models.Payments;
    using Serilog;
    using Models.Rules;
    using Models.Stage;
    using Newtonsoft.Json;
    using core.Services.CustomData;
    using Models;

    public class PgPaymentService : IPaymentService, IPgPaymentService
    {
        private static readonly Dictionary<string, string[]> DependentSections = new Dictionary<string, string[]>
        {
            { Constants.CostSection.Production, new [] { Constants.CostSection.TalentFees } }
        };

        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly ILogger _logger = Log.ForContext<PgPaymentService>();
        private readonly IRuleService _ruleService;
        private readonly IPgCurrencyService _pgCurrencyService;
        private readonly IPgCostSectionTotalsBuilder _pgTotalsBuilder;
        private readonly IPgCostStageRevisionTotalPaymentsBuilder _pgTotalPaymentsBuilder;
        private readonly ICustomObjectDataService _customObjectDataService;

        public PgPaymentService(
            EFContext efContext,
            ICostStageRevisionService costStageRevisionService,
            IRuleService ruleService,
            IStageService stageService,
            IPgCurrencyService pgCurrencyService,
            IPgCostSectionTotalsBuilder pgTotalsBuilder,
            IPgCostStageRevisionTotalPaymentsBuilder pgTotalPaymentsBuilder,
            ICustomObjectDataService customObjectDataService
           )
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
            _ruleService = ruleService;
            _stageService = stageService;
            _pgCurrencyService = pgCurrencyService;
            _pgTotalsBuilder = pgTotalsBuilder;
            _pgTotalPaymentsBuilder = pgTotalPaymentsBuilder;
            _customObjectDataService = customObjectDataService;
        }

        private readonly IStageService _stageService;

        public async Task<PaymentAmountResult> GetPaymentAmount(Guid costStageRevisionId, bool persist = true)
        {
            // first let's try and get the current calculation - no need to re-calculate if we've got everything
            var currentPaymentTotals = await _costStageRevisionService.GetCostStageRevisionPaymentTotals(costStageRevisionId);
            if (currentPaymentTotals != null && currentPaymentTotals.Count > 0)
            {
                return TransformPaymentResult(currentPaymentTotals);
            }

            return await CalculatePaymentAmount(costStageRevisionId, persist);
        }

        public async Task<PaymentAmountResult> GetPaymentAmount(CostStageRevision costStageRevision, bool persist = true)
        {
            // first let's try and get the current calculation - no need to re-calculate if we've got everything
            var currentPaymentTotals = await _costStageRevisionService.GetCostStageRevisionPaymentTotals(costStageRevision);
            if (currentPaymentTotals.Count > 0)
            {
                return TransformPaymentResult(currentPaymentTotals);
            }

            return await CalculatePaymentAmount(costStageRevision.Id, persist);
        }

        private PaymentAmountResult TransformPaymentResult(List<CostStageRevisionPaymentTotal> dbResults)
        {
            var result = new PaymentAmountResult
            {
                IsDetailedSplit = dbResults.Count > 1,
                TotalCostAmount = dbResults.First(x => x.LineItemTotalType == Constants.CostSection.CostTotal).LineItemFullCost
            };

            foreach (var p in dbResults)
            {
                switch (p.LineItemTotalType)
                {
                    case Constants.CostSection.CostTotal:
                        result.TotalCostAmountPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.InsuranceTotal:
                        result.InsuranceCostPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.Other:
                        result.OtherCostPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.PostProduction:
                        result.PostProductionCostPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.Production:
                        result.ProductionCostPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.TargetBudgetTotal:
                        result.TargetBudgetTotalPayment = p.LineItemTotalCalculatedValue;
                        break;
                    case Constants.CostSection.TechnicalFee:
                        result.TechnicalFeeCostPayment = p.LineItemTotalCalculatedValue;
                        break;
                }
            }

            return result;
        }

        public async Task<PaymentAmountResult> CalculatePaymentAmount(Guid costStageRevisionId, bool persist = true)
        {
            var revision = await _efContext.CostStageRevision
                .Include(csr => csr.CostFormDetails)
                .Include(r => r.CostStage).ThenInclude(cs => cs.Cost)
                .Include(r => r.CostStage).ThenInclude(cs => cs.CostStageRevisions)
                .Include(r => r.StageDetails)
                .Include(r => r.ProductDetails)
                .Include(r => r.CostStageRevisionPaymentTotals)
                //.AsNoTracking()
                .FirstOrDefaultAsync(csr => csr.Id == costStageRevisionId);

            var costStage = revision.CostStage;
            var cost = costStage.Cost;

            var stageDetailsForm = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(revision);
            var productionDetailsForm = _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(revision);

            //ADC-2690 revises paymentCostTotal calculation for re-opened Final Actual stage
            var previousPaymentCalculations = new List<CostStageRevisionPaymentTotal>();
            //get the latest costtotal from last approved final actual
            var previousPaymentCostTotal = new CostStageRevisionPaymentTotal();
            //flag to determine if this calculation is for the first FA or subsequent FAs
            bool isFirstFA = true;
            //check if the current cost has any Final Actual stage approved
            var approvedFinalActualStage = cost.CostStages.Find(x => x.Key == CostStages.FinalActual.ToString())?.CostStageRevisions.Find(a => a.Status == CostStageRevisionStatus.Approved);
            //if there is no final actual stage approve, then keep the current calculation as is, which is working correctly.
            if (approvedFinalActualStage == null)
            {
                previousPaymentCalculations = await _costStageRevisionService.GetAllCostPaymentTotals(cost.Id, revision.CostStage.Id);
            }
            else
            {
                //here is the area we do the calculation for re-opened FAs
                //Get All Cost Payment Totals for the current Final Actual stage
                previousPaymentCalculations = await _costStageRevisionService.GetAllCostPaymentTotalsFinalActual(cost.Id, revision.CostStage.Id);
                //extract values of CostTotal rowns of approved FA and order by calculated datetime
                var previousPaymentCostTotals = previousPaymentCalculations.Where(x => x.LineItemTotalType == Constants.CostSection.CostTotal
                   && x.CostStageRevision.Status == CostStageRevisionStatus.Approved)
                   .OrderBy(x => x.CalculatedAt).ToList();
                //check if there is at least 1 approved FA 
                if (previousPaymentCalculations.Any() && previousPaymentCostTotals.Any())
                {
                    //if there is an approved FA, it means there is an inprogress FA, and we shall need to get the last FA for subtraction later: Grand total at Final actual -II minus Grand total in Final actual -I
                    previousPaymentCostTotal = previousPaymentCostTotals[previousPaymentCostTotals.Count() - 1];
                    //flag up this is not the first FA
                    isFirstFA = false;
                }
                else
                {
                    //otherwise, keep the calculation as is
                    previousPaymentCalculations = await _costStageRevisionService.GetAllCostPaymentTotals(cost.Id, revision.CostStage.Id);
                }
            }

            var costLineItems = await _costStageRevisionService.GetCostLineItems(costStageRevisionId);

            var totals = _pgTotalsBuilder.Build(stageDetailsForm, costLineItems, costStage.Key);
            var previousPaymentTotals = _pgTotalPaymentsBuilder.Build(previousPaymentCalculations);

            // these are totals of remaining balance
            //changed for ADC-2690
            var totalRemainingPayment = new PgPaymentRule()
            {

                StageTotals = totals,
                BudgetRegion = stageDetailsForm.BudgetRegion?.Key,
                ContentType = stageDetailsForm.ContentType?.Key,
                CostType = cost.CostType.ToString(),
                CostStages = costStage.Key,
                ProductionType = Constants.ProductionType.ProductionTypeList.FirstOrDefault(a => a == stageDetailsForm.ProductionType?.Key),
                DirectPaymentVendorId = productionDetailsForm.DirectPaymentVendor?.Id,
                IsAIPE = stageDetailsForm.IsAIPE,

                // we need this to match with the rules' TotalCostAmount field
                TotalCostAmount = totals.TotalCostAmountTotal,

                // this is for detailed split
                InsuranceCost = totals.InsuranceCostTotal - previousPaymentTotals.InsuranceCostPayments,
                TechnicalFeeCost = totals.TechnicalFeeCostTotal - previousPaymentTotals.TechnicalFeeCostPayments,
                TalentFeeCost = totals.TalentFeeCostTotal - previousPaymentTotals.TalentFeeCostPayments,
                PostProductionCost = totals.PostProductionCostTotal - previousPaymentTotals.PostProductionCostPayments,
                ProductionCost = totals.ProductionCostTotal - previousPaymentTotals.ProductionCostPayments,
                OtherCost = totals.OtherCostTotal - previousPaymentTotals.OtherCostPayments,

                TargetBudgetTotalCost = totals.TargetBudgetTotal - previousPaymentTotals.TargetBudgetTotalCostPayments,
                CostCarryOverAmount = previousPaymentTotals.CarryOverAmount
            };
            //check if this is not the calculation for the first FA then do the subtraction: Grand total at Final actual -II minus Grand total in Final actual -I
            //if not keep as is
            if (!isFirstFA)
            {
                //if this is not the first FA, it means we would have to calculated TotalCost AKA CostTotal equal Grand total at Final actual -II minus Grand total in Final actual -I
                totalRemainingPayment.TotalCost = totals.TotalCostAmountTotal - previousPaymentCostTotal.LineItemFullCost;
            }
            else
            {
                // we use this to calculate the outstanding balance where there is no detailed split
                totalRemainingPayment.TotalCost = totals.TotalCostAmountTotal - previousPaymentTotals.TotalCostPayments;
            }
            _logger.Information($"Calculating payment amount for cost: {cost.CostNumber} at stage: {costStage.Key} revision: {revision.Id}");

            // these are actual payment splits (percentages of totals)
            var paymentAmount = await GetPaymentAmount(totalRemainingPayment, previousPaymentTotals, totals, productionDetailsForm.DirectPaymentVendor?.ProductionCategory);
            if (paymentAmount != null)
            {
                if (!persist)
                {
                    return paymentAmount;
                }
                var totalRemainingAmount = (PgPaymentRule)paymentAmount.TotalRemainingPayment;

                var nextStages = await _stageService.GetAllUpcomingStages(costStage.Key, BuType.Pg, cost.Id);
                if (nextStages != null)
                {
                    paymentAmount.ProjectedPayments = GetNextStagesPaymentAmounts(paymentAmount, totalRemainingPayment, nextStages);
                }

                var alreadySaved = await _costStageRevisionService.GetCostStageRevisionPaymentTotals(revision);
                if (alreadySaved == null || !alreadySaved.Any())
                {
                    await SaveTotals(costStageRevisionId, paymentAmount, totalRemainingAmount, isFirstFA);

                    foreach (var projectedPayment in paymentAmount.ProjectedPayments)
                    {
                        await SaveTotals(costStageRevisionId, projectedPayment, totalRemainingAmount, isFirstFA);
                    }
                }
                // UserIdentity in the parameters of below method is used only to log an activity when IO number gets changed.
                // Therefore we can pass any not null object here 
                await _customObjectDataService.Save(costStageRevisionId, CustomObjectDataKeys.PgPaymentRuleInput, totalRemainingPayment, new UserIdentity());
                await _customObjectDataService.Save(costStageRevisionId, CustomObjectDataKeys.PgMatchedPaymentRule, paymentAmount.MatchedPaymentRule, new UserIdentity());

                return paymentAmount;
            }

            _logger.Error($"Payment amount NOT calculated for cost: {cost.CostNumber} at stage: {costStage.Key} revision: {revision.Id} using rule: {totalRemainingPayment}!");
            return null;
        }

        public async Task<Currency> GetPaymentCurrency(Guid costStageRevisionId)
        {
            var stageDetailsForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            var productionDetailsForm = await _costStageRevisionService.GetProductionDetails<PgProductionDetailsForm>(costStageRevisionId);

            return await _pgCurrencyService.GetCurrency(stageDetailsForm.AgencyCurrency, productionDetailsForm);
        }

        private async Task<PaymentAmountResult> GetPaymentAmount(PgPaymentRule paymentRule, CostStageRevisionTotalPayments previousPayments, CostSectionTotals totals, string productionCategory = null)
        {
            var rules = new List<CompiledRule<PgPaymentRule>>();
            // Try match the rules one by one. First Vendor specific rules, then AIPE/Non-AIPE rules, and then standart rules if nothing matched.
            if (paymentRule.DirectPaymentVendorId.HasValue)
            {
                var vendorRules = await _ruleService.GetCompiledByVendorId<PgPaymentRule>(paymentRule.DirectPaymentVendorId.Value, RuleType.VendorPayment, productionCategory);
                rules.AddRange(vendorRules);
            }

            if (paymentRule.IsAIPE)
            {
                var aipeRules = await _ruleService.GetCompiledByRuleType<PgPaymentRule>(RuleType.AIPEPayment);
                rules.AddRange(aipeRules);
            }
            else
            {
                var nonAipeRules = await _ruleService.GetCompiledByRuleType<PgPaymentRule>(RuleType.NonAIPEPayment);
                rules.AddRange(nonAipeRules);
            }

            var commonRules = await _ruleService.GetCompiledByRuleType<PgPaymentRule>(RuleType.CommonPayment);
            rules.AddRange(commonRules);

            PaymentAmountResult MatchFunc(PgPaymentRule t, dataAccess.Entity.Rule r) =>
                CalculateStagePaymentAmountsWithRule(r, paymentRule.CostStages, t, false, previousPayments, false, totals);

            _ruleService.TryMatchRule(rules, paymentRule, MatchFunc, out var fResult);

            return fResult;
        }

        private decimal GetDisplayedValueOnPaymentSummaryPage(decimal? allocationAmmount, decimal? allocatedSplitPayment)
        {
            //if the total payment amount has value but there is no payment rule applied, we still need to display the amount on payment summary screen
            if (allocatedSplitPayment.HasValue && allocatedSplitPayment.Value > 0)
            {
                return allocatedSplitPayment ?? 0;
            }

            return allocationAmmount ?? 0;
        }

        private PaymentAmountResult CalculateStagePaymentAmountsWithRule(
            dataAccess.Entity.Rule paymentRule,
            string costStageKey,
            PgPaymentRule input,
            bool isProjection,
            CostStageRevisionTotalPayments previousPayments,
            bool isAipeProjection = false,
            CostSectionTotals rawTotals = null,
            bool isNextStageCalc = false
            )
        {
            var res = new PaymentAmountResult();
            var pgPaymentRuleData = JsonConvert.DeserializeObject<PgPaymentRuleDefinition>(paymentRule.Definition);
            res.IsDetailedSplit = pgPaymentRuleData.DetailedSplit;
            var costStage = (CostStages)Enum.Parse(typeof(CostStages), costStageKey);
            decimal? totalCalculatedValue;
            res.CostCarryOverAmount = 0;
            // do the specific calculations here: 
            // - sum up all the individual costs
            // -- if it is an AIPE projection (projection of any stage triggered in AIPE stage) - we only have TargetBudgetTotal, so use that anyways
            if (pgPaymentRuleData.DetailedSplit && !isAipeProjection)
            {
                // ADC-2243: run through the payment allocation rule to ensure the 'input' is correctly allocated
                //ADC-2711: check null and get 0
                res.InsuranceCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.InsuranceTotal, costStage) ?? 0;
                var talentFeeCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TalentFees, costStage);

                if (talentFeeCostPayment.HasValue)
                {
                    res.TalentFeeCostPayment = talentFeeCostPayment;
                }

                res.PostProductionCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.PostProduction, costStage) ?? 0;
                res.ProductionCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Production, costStage) ?? 0;
                res.TechnicalFeeCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TechnicalFee, costStage) ?? 0;
                res.OtherCostPayment = GetAllocatedSplitPayment(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Other, costStage) ?? 0;

                res.TargetBudgetTotalPayment = 0m;

                // AIPE specific handling of target budget
                if (costStage == CostStages.Aipe)
                {
                    res.TargetBudgetTotalPayment = input.TargetBudgetTotalCost * (pgPaymentRuleData.GetSplitByNameAndStage(Constants.CostSection.TargetBudgetTotal, costStage) ?? 0);
                }

                totalCalculatedValue =
                    res.InsuranceCostPayment
                    + res.OtherCostPayment
                    + res.PostProductionCostPayment
                    + res.ProductionCostPayment
                    + res.TargetBudgetTotalPayment
                    + res.TechnicalFeeCostPayment;

                //ADC-2711: if DPV then exclude postproduction cost due to it has already included into Othercost in GetAllocation method
                if (paymentRule.Type == RuleType.VendorPayment)
                {
                    totalCalculatedValue = totalCalculatedValue - res.PostProductionCostPayment;
                }

                // we only need carry over amounts for detailed splits - for non detailed splits all payments are already contained within TotalCost
                if (costStage == CostStages.Aipe)
                {
                    // we store it as a carry over for other stages to deduct from
                    // only if the calculated value is > 0 - otherwise no payment is made in this stage -> no carry over amount
                    res.CostCarryOverAmount = totalCalculatedValue > 0 ? input.TotalCostAmount - totalCalculatedValue : 0;
                }
                else if (input.IsAIPE)
                {
                    if (input.CostCarryOverAmount > 0)
                    {
                        if (input.CostCarryOverAmount - totalCalculatedValue > 0)
                        {
                            // deduct current payment from the "pot" and store the remainder
                            res.CostCarryOverAmount = input.CostCarryOverAmount - totalCalculatedValue;
                        }
                        else
                        {
                            res.CostCarryOverAmount = 0;
                        }
                    }

                    // we need to subtract current payment from previous target budget total payments to never lose them
                    totalCalculatedValue = totalCalculatedValue - input.CostCarryOverAmount;
                }
                else
                {
                    if (totalCalculatedValue < 0)
                    {
                        // carry over amount will be negative in non-aipe overpayment cases
                        res.CostCarryOverAmount = totalCalculatedValue;
                    }
                }
            }
            else if (isAipeProjection && costStage == CostStages.FinalActual)
            {
                // we pretend to pay the rest on AIPE FA stage since detailed rules won't have a split for Cost Total
                totalCalculatedValue = input.TotalCost;
            }
            else
            {
                totalCalculatedValue = input.TotalCost * (pgPaymentRuleData.GetSplitByNameAndStage(Constants.CostSection.CostTotal, costStage) ?? 0);
            }

            // - if the stage = Final Actual - return whatever result (even if negative)
            // -  otherwise - if <=0 return 0
            if (totalCalculatedValue < 0 && costStage != CostStages.FinalActual && costStage != CostStages.FinalActualRevision)
            {
                res.TotalCostAmountPayment = 0;
            }
            else
            {
                if ((costStage == CostStages.FinalActual || costStage == CostStages.FinalActualRevision) && input.CostCarryOverAmount < 0)
                {
                    // previous stage resulted in a negative calculation, despite us reporting 0, we still need to subtract that negative value
                    totalCalculatedValue += input.CostCarryOverAmount;
                }

                res.TotalCostAmountPayment = totalCalculatedValue;
            }

            res.TotalCostAmount = totalCalculatedValue;

            res.MatchedPaymentRule = paymentRule;
            res.IsProjection = isProjection;
            res.StageName = input.CostStages;

            CostSectionTotals totals = null;
            if (rawTotals != null)
            {
                //ADC-2711 overwrite line_item_full_cost values to display on payment summary screen
                if (paymentRule.Type == RuleType.VendorPayment && isNextStageCalc == false)
                {
                    var insuranceCostPaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.InsuranceTotal) ?? 0;
                    var postProductionCostPaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.PostProduction) ?? 0;
                    var productionCostPaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.Production) ?? 0;
                    var technicalFeeCostPaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.TechnicalFee) ?? 0;
                    var otherCostPaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.Other) ?? 0;
                    var talentFeePaymentAllocation = GetAllocation(input, paymentRule.Type, Constants.CostSection.TalentFees) ?? 0;


                    var insuranceCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.InsuranceTotal, costStage) ?? 0;
                    var postProductionCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.PostProduction, costStage) ?? 0;
                    var productionCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Production, costStage) ?? 0;
                    var technicalFeeCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TechnicalFee, costStage) ?? 0;
                    var otherCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Other, costStage) ?? 0;
                    var talentFeeCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TalentFees, costStage) ?? 0;

                    totals = new CostSectionTotals
                    {
                        // Recalculate allocated amount because it depends on the rule
                        InsuranceCostTotal = GetDisplayedValueOnPaymentSummaryPage(insuranceCostPaymentAllocation, insuranceCostTotal),
                        PostProductionCostTotal = GetDisplayedValueOnPaymentSummaryPage(postProductionCostPaymentAllocation, postProductionCostTotal),
                        ProductionCostTotal = GetDisplayedValueOnPaymentSummaryPage(productionCostPaymentAllocation, productionCostTotal),
                        TechnicalFeeCostTotal = GetDisplayedValueOnPaymentSummaryPage(technicalFeeCostPaymentAllocation, technicalFeeCostTotal),
                        OtherCostTotal = GetDisplayedValueOnPaymentSummaryPage(otherCostPaymentAllocation - postProductionCostPaymentAllocation, otherCostTotal - postProductionCostTotal),
                        TalentFeeCostTotal = GetDisplayedValueOnPaymentSummaryPage(talentFeePaymentAllocation, talentFeeCostTotal),
                        TargetBudgetTotal = input.TargetBudgetTotalCost ?? 0 + previousPayments.TargetBudgetTotalCostPayments,
                        TotalCostAmountTotal = input.TotalCostAmount
                    };
                }
                else
                {
                    totals = new CostSectionTotals
                    {
                        // Recalculate allocated amount because it depends on the rule
                        InsuranceCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.InsuranceTotal, costStage) ?? 0,
                        PostProductionCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.PostProduction, costStage) ?? 0,
                        ProductionCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Production, costStage) ?? 0,
                        TechnicalFeeCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TechnicalFee, costStage) ?? 0,
                        OtherCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Other, costStage) ?? 0,
                        TalentFeeCostTotal = GetAllocatedAmount(rawTotals, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TalentFees, costStage) ?? 0,

                        TargetBudgetTotal = input.TargetBudgetTotalCost ?? 0 + previousPayments.TargetBudgetTotalCostPayments,
                        TotalCostAmountTotal = input.TotalCostAmount
                    };
                }
            }

            var paymentTotals = new CostSectionTotals
            {
                // Recalculate allocated amount because it depends on the rule
                TalentFeeCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TalentFees, costStage) ?? 0,
                InsuranceCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.InsuranceTotal, costStage) ?? 0,
                PostProductionCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.PostProduction, costStage) ?? 0,
                ProductionCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Production, costStage) ?? 0,
                TechnicalFeeCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.TechnicalFee, costStage) ?? 0,
                OtherCostTotal = GetAllocatedAmount(input, paymentRule.Type, pgPaymentRuleData, Constants.CostSection.Other, costStage) ?? 0,

                TargetBudgetTotal = input.TargetBudgetTotalCost ?? 0 + previousPayments.TargetBudgetTotalCostPayments,
                TotalCostAmountTotal = input.TotalCostAmount
            };
            res.TotalRemainingPayment = new PgPaymentRule
            {
                BudgetRegion = input.BudgetRegion,
                ContentType = input.ContentType,
                CostType = input.CostType,
                CostStages = input.CostStages,
                ProductionType = input.ProductionType,
                DirectPaymentVendorId = input.DirectPaymentVendorId,
                IsAIPE = input.IsAIPE,
                TotalCostAmount = input.TotalCostAmount,
                TotalCost = input.TotalCost,
                TargetBudgetTotalCost = input.TargetBudgetTotalCost,
                CostCarryOverAmount = input.CostCarryOverAmount,

                // Recalculate remaining payment amount because allocated amount depends on macthed rule
                StageTotals = totals,
                InsuranceCost = paymentTotals.InsuranceCostTotal - previousPayments.InsuranceCostPayments,
                TalentFeeCost = paymentTotals.TalentFeeCostTotal - previousPayments.TalentFeeCostPayments,
                PostProductionCost = paymentTotals.PostProductionCostTotal - previousPayments.PostProductionCostPayments,
                ProductionCost = paymentTotals.ProductionCostTotal - previousPayments.ProductionCostPayments,
                TechnicalFeeCost = paymentTotals.TechnicalFeeCostTotal - previousPayments.TechnicalFeeCostPayments,
                OtherCost = paymentTotals.OtherCostTotal - previousPayments.OtherCostPayments
            };

            return res;
        }

        private static decimal? GetAllocatedAmount(PgPaymentRule input, RuleType ruleType, PgPaymentRuleDefinition paymentRuleDefinition, string splitName, CostStages costStage)
        {
            if (!paymentRuleDefinition.HasExplicitSplitForSectionAtStage(splitName, costStage))
            {
                return null;
            }

            // change the allocation based on detailed splits
            var allocation = GetAllocation(input, ruleType, splitName);

            if (DependentSections.ContainsKey(splitName))
            {
                foreach (var dependentSplit in DependentSections[splitName])
                {
                    if (paymentRuleDefinition.HasExplicitSplitForSectionAtStage(dependentSplit, costStage))
                    {
                        allocation -= GetAllocation(input, ruleType, dependentSplit);
                    }
                }
            }

            return allocation;
        }

        private static decimal? GetAllocatedAmount(CostSectionTotals rawTotals, RuleType ruleType, PgPaymentRuleDefinition paymentRuleDefinition, string splitName, CostStages costStage)
        {
            if (!paymentRuleDefinition.HasExplicitSplitForSectionAtStage(splitName, costStage))
            {
                return null;
            }

            // change the allocation based on detailed splits
            var allocation = GetAllocation(rawTotals, ruleType, splitName);

            if (DependentSections.ContainsKey(splitName))
            {
                foreach (var dependentSplit in DependentSections[splitName])
                {
                    if (paymentRuleDefinition.HasExplicitSplitForSectionAtStage(dependentSplit, costStage))
                    {
                        allocation -= GetAllocation(rawTotals, ruleType, dependentSplit);
                    }
                }
            }

            return allocation;
        }

        private static decimal? GetAllocatedSplitPayment(PgPaymentRule paymentInput, RuleType ruleType, PgPaymentRuleDefinition paymentRuleDefinition, string splitName, CostStages costStage)
        {
            var split = paymentRuleDefinition.GetSplitByNameAndStage(splitName, costStage);
            if (!split.HasValue)
            {
                return null;
            }

            // change the allocation based on detailed splits
            var allocation = GetAllocation(paymentInput, ruleType, splitName);

            decimal payment = 0;
            if (DependentSections.ContainsKey(splitName))
            {
                foreach (var dependentSplit in DependentSections[splitName])
                {
                    if (paymentRuleDefinition.HasExplicitSplitForSectionAtStage(dependentSplit, costStage))
                    {
                        allocation -= GetAllocation(paymentInput, ruleType, dependentSplit);
                        payment += GetAllocatedSplitPayment(paymentInput, ruleType, paymentRuleDefinition, dependentSplit, costStage) ?? 0;
                    }
                }
            }

            payment += (allocation ?? 0) * split.Value;

            return payment;
        }

        private static decimal? GetAllocation(PgPaymentRule paymentInput, RuleType ruleType, string splitName)
        {
            decimal? allocation;

            switch (splitName)
            {
                case Constants.CostSection.InsuranceTotal:
                    allocation = paymentInput.InsuranceCost;
                    break;
                case Constants.CostSection.PostProduction:
                    allocation = paymentInput.PostProductionCost;
                    break;
                case Constants.CostSection.Production:
                    allocation = paymentInput.ProductionCost;
                    break;
                case Constants.CostSection.TechnicalFee:
                    allocation = paymentInput.TechnicalFeeCost;
                    break;
                case Constants.CostSection.TalentFees:
                    allocation = paymentInput.TalentFeeCost;
                    break;
                case Constants.CostSection.Other:
                    if (ruleType == RuleType.VendorPayment)
                    {
                        // ADC-2243
                        allocation = paymentInput.OtherCost + paymentInput.PostProductionCost + paymentInput.InsuranceCost + paymentInput.TechnicalFeeCost;
                    }
                    else
                    {
                        allocation = paymentInput.OtherCost;
                    }

                    break;
                default:
                    allocation = paymentInput.OtherCost;
                    break;
            }

            return allocation;
        }

        private static decimal? GetAllocation(CostSectionTotals rawTotals, RuleType ruleType, string splitName)
        {
            decimal? allocation;

            switch (splitName)
            {
                case Constants.CostSection.InsuranceTotal:
                    allocation = rawTotals.InsuranceCostTotal;
                    break;
                case Constants.CostSection.PostProduction:
                    allocation = rawTotals.PostProductionCostTotal;
                    break;
                case Constants.CostSection.Production:
                    allocation = rawTotals.ProductionCostTotal;
                    break;
                case Constants.CostSection.TechnicalFee:
                    allocation = rawTotals.TechnicalFeeCostTotal;
                    break;
                case Constants.CostSection.TalentFees:
                    allocation = rawTotals.TalentFeeCostTotal;
                    break;
                case Constants.CostSection.Other:
                    if (ruleType == RuleType.VendorPayment)
                    {
                        // ADC-2243
                        allocation = rawTotals.OtherCostTotal + rawTotals.PostProductionCostTotal + rawTotals.InsuranceCostTotal + rawTotals.TechnicalFeeCostTotal;
                    }
                    else
                    {
                        allocation = rawTotals.OtherCostTotal;
                    }

                    break;
                default:
                    allocation = rawTotals.OtherCostTotal;
                    break;
            }

            return allocation;
        }

        private List<PaymentAmountResult> GetNextStagesPaymentAmounts(
            PaymentAmountResult fResult,
            PgPaymentRule paymentRule,
            IEnumerable<StageModel> nextStages)
        {
            var isAipeProjection = paymentRule.IsAIPE && fResult.StageName == CostStages.Aipe.ToString();
            var result = new List<PaymentAmountResult>();

            var currentResult = fResult;
            var currentInput = paymentRule;
            foreach (var nextStage in nextStages)
            {
                var input = new PgPaymentRule
                {
                    // we only need these values for next stage projection calculation
                    CostStages = nextStage.Key,
                    // paymentRule already has current total - old payments. We only need to subtract
                    // the freshly calculated values for this stage
                    InsuranceCost = currentInput.InsuranceCost - currentResult.InsuranceCostPayment,
                    OtherCost = currentInput.OtherCost - currentResult.OtherCostPayment,
                    PostProductionCost = currentInput.PostProductionCost - currentResult.PostProductionCostPayment,
                    ProductionCost = currentInput.ProductionCost - currentResult.ProductionCostPayment,
                    TargetBudgetTotalCost = currentInput.TargetBudgetTotalCost - currentResult.TargetBudgetTotalPayment,
                    TechnicalFeeCost = currentInput.TechnicalFeeCost - currentResult.TechnicalFeeCostPayment,
                    TalentFeeCost = currentInput.TalentFeeCost - currentResult.TalentFeeCostPayment,
                    IsAIPE = currentInput.IsAIPE,
                    CostCarryOverAmount = currentResult.CostCarryOverAmount,

                    TotalCost = currentInput.TotalCost - currentResult.TotalCostAmountPayment,
                    TotalCostAmount = currentResult.TotalCostAmount ?? 0m
                };
                var previousPayment = new CostStageRevisionTotalPayments
                {
                    InsuranceCostPayments = currentResult.InsuranceCostPayment ?? 0,
                    TalentFeeCostPayments = currentResult.TalentFeeCostPayment ?? 0,
                    TechnicalFeeCostPayments = currentResult.TechnicalFeeCostPayment ?? 0,
                    PostProductionCostPayments = currentResult.PostProductionCostPayment ?? 0,
                    ProductionCostPayments = currentResult.ProductionCostPayment ?? 0,
                    OtherCostPayments = currentResult.OtherCostPayment ?? 0,
                    CarryOverAmount = currentResult.CostCarryOverAmount ?? 0,
                    TargetBudgetTotalCostPayments = currentResult.TargetBudgetTotalPayment ?? 0,
                    TotalCostPayments = currentResult.TotalCostAmountPayment ?? 0
                };
                currentResult = CalculateStagePaymentAmountsWithRule(fResult.MatchedPaymentRule, nextStage.Key, input, true, previousPayment, isAipeProjection, isNextStageCalc: true);
                currentInput = input;

                result.Add(currentResult);
            }

            return result;
        }

        private async Task SaveTotals(Guid costStageRevisionId, PaymentAmountResult paymentAmount, PgPaymentRule payment, bool isFirstFA = true)
        {
            // save the totals here
            var toSave = new List<CostStageRevisionPaymentTotal>();
            if (paymentAmount.IsDetailedSplit)
            {
                toSave.AddRange(new List<CostStageRevisionPaymentTotal>
                {
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.InsuranceCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.InsuranceCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.InsuranceCostTotal,
                        LineItemTotalType = Constants.CostSection.InsuranceTotal,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName,
                        CalculatedAt = DateTime.UtcNow
                    },
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.OtherCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.OtherCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.OtherCostTotal,
                        LineItemTotalType = Constants.CostSection.Other,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    },
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.PostProductionCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.PostProductionCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.PostProductionCostTotal,
                        LineItemTotalType = Constants.CostSection.PostProduction,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    },
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.ProductionCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.ProductionCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.ProductionCostTotal,
                        LineItemTotalType = Constants.CostSection.Production,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    },
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.TargetBudgetTotalPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.TargetBudgetTotalCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.TargetBudgetTotal,
                        LineItemTotalType = Constants.CostSection.TargetBudgetTotal,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    },
                    new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.TechnicalFeeCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.TechnicalFeeCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.TechnicalFeeCostTotal,
                        LineItemTotalType = Constants.CostSection.TechnicalFee,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    }
                });

                if (paymentAmount.TalentFeeCostPayment.HasValue)
                {
                    toSave.Add(new CostStageRevisionPaymentTotal
                    {
                        CostStageRevisionId = costStageRevisionId,
                        LineItemTotalCalculatedValue = paymentAmount.TalentFeeCostPayment.GetValueOrDefault(0),
                        LineItemRemainingCost = payment.TalentFeeCost.GetValueOrDefault(0),
                        LineItemFullCost = payment.StageTotals.TalentFeeCostTotal,
                        LineItemTotalType = Constants.CostSection.TalentFees,
                        IsProjection = paymentAmount.IsProjection,
                        StageName = paymentAmount.StageName
                    });
                }
            }
            if (isFirstFA)
            {
                toSave.Add(new CostStageRevisionPaymentTotal
                {
                    CostStageRevisionId = costStageRevisionId,
                    LineItemTotalCalculatedValue = paymentAmount.TotalCostAmountPayment.GetValueOrDefault(0),
                    LineItemRemainingCost = paymentAmount.CostCarryOverAmount.GetValueOrDefault(0),
                    LineItemFullCost = payment.StageTotals.TotalCostAmountTotal,
                    LineItemTotalType = Constants.CostSection.CostTotal,
                    IsProjection = paymentAmount.IsProjection,
                    StageName = paymentAmount.StageName
                }
            );
            }
            else
            {
                toSave.Add(new CostStageRevisionPaymentTotal
                {
                    CostStageRevisionId = costStageRevisionId,
                    LineItemTotalCalculatedValue = payment.TotalCost.GetValueOrDefault(0),
                    LineItemRemainingCost = paymentAmount.CostCarryOverAmount.GetValueOrDefault(0),
                    LineItemFullCost = payment.StageTotals.TotalCostAmountTotal,
                    LineItemTotalType = Constants.CostSection.CostTotal,
                    IsProjection = paymentAmount.IsProjection,
                    StageName = paymentAmount.StageName
                }
            );
            }


            foreach (var total in toSave)
            {
                total.CalculatedAt = DateTime.UtcNow;
            }

            await _costStageRevisionService.SaveCostStageRevisionPaymentTotals(toSave);
        }
    }
}
