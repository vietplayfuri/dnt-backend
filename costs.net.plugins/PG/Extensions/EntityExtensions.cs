
namespace costs.net.plugins.PG.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using core.Models.Regions;
    using core.Services.Regions;
    using costs.net.core.Extensions;
    using costs.net.core.Models;
    using costs.net.core.Models.Costs;
    using costs.net.plugins.PG.Services.Costs;
    using costs.net.plugins.PG.Models.PurchaseOrder;
    using dataAccess.Entity;
    using Newtonsoft.Json;
    using Serilog;

    public static class EntityExtensions
    {
        private static readonly ILogger Logger = Log.ForContext<Agency>();

        public static bool IsCyclone(this Agency agency)
        {
            if (agency == null)
            {
                throw new ArgumentException(nameof(agency));
            }

            return agency.Labels?.Any(l => l.StartsWith(Constants.Agency.CycloneLabel)) ?? false;
        }

        public static bool IsNorthAmericanAgency(this Agency agency, IRegionsService regionsService)
        {
            Country country = agency.Country;

            if (country.GeoRegionId == null)
            {
                Logger.Warning($"Country {country.Iso} does not have region. This is required to check for North American agencies.");
                return false;
            }

            var region = regionsService.GetGeoRegion(country.GeoRegionId.Value).Result;

            return region.Name == Constants.AgencyRegion.NorthAmerica;
        }

        public static bool IsEuropeanAgency(this Agency agency, IRegionsService regionsService)
        {
            Country country = agency.Country;

            if (country.GeoRegionId == null)
            {
                Logger.Warning($"Country {country.Iso} does not have region. This is required to check for European agencies.");
                return false;
            }

            var region = regionsService.GetGeoRegion(country.GeoRegionId.Value).Result;

            return region.Name == Constants.AgencyRegion.Europe;
        }

        public static bool IsCostModuleOwner(this Agency agency)
        {
            if (agency == null)
            {
                throw new ArgumentNullException(nameof(agency));
            }
            return agency.Labels != null && agency.Labels.Any(a => a.StartsWith(core.Constants.BusinessUnit.CostModulePrimaryLabelPrefix));
        }

        public static string GetSapVendorCode(this string[] labels)
        {
            return ((labels ?? new string[0])
                    .FirstOrDefault(l => l.StartsWith(Constants.PurchaseOrder.VendorSapIdLabelPrefix)) ?? ""
                    )
                .Replace(Constants.PurchaseOrder.VendorSapIdLabelPrefix, "");
        }

        /// <summary>
        /// Get revision of costs by revision id
        /// <para>Required: Cost has data of all Cost Stages / Revisions </para>
        /// </summary>
        public static CostStageRevision GetSelectedRevision(this Cost cost, Guid revisionId)
        {
            var revisions = cost.CostStages.SelectMany(cs => cs.CostStageRevisions);
            return revisions.FirstOrDefault(r => r.Id == revisionId);
        }

        /// <summary>
        /// Convert from CostStage to CostStageModel - correct data used for frontend to view
        /// <para>Required: Cost Stage has data of Revisions</para>
        /// </summary>
        public static CostStageModel ToCostStageModel(this CostStage costStage, List<ExchangeRate> exchangeRates, Guid defaultPaymentCurrencyId)
        {
            var model = new CostStageModel
            {
                Id = costStage.Id,
                CostId = costStage.CostId,
                Key = costStage.Key,
                Name = costStage.Name,
            };

            if (costStage.CostStageRevisions.Any())
            {
                var ordered = costStage.CostStageRevisions.OrderBy(csr => csr.Created).Where(r => r.Status != CostStageRevisionStatus.ReopenRejected).ToArray();
                var latest = ordered.LastOrDefault(r => r.Approvals.All(a => a.Status == ApprovalStatus.Approved)) ?? ordered.Last();
                model.Status = ordered.Last().Status;

                model.cost = new CostStageModel.Cost();

                var totals = latest.GetRevisionTotals(exchangeRates, defaultPaymentCurrencyId);
                model.cost.Total = totals.total;
                model.cost.TotalInLocalCurrency = totals.totalInLocalCurrency;
                model.cost.LocalCurrencySymbol = costStage.Cost.PaymentCurrency?.Symbol;
                model.cost.LocalCurrencyCode = costStage.Cost.PaymentCurrency?.Code;

                model.Revisions = ordered
                    .Select(csr => new CostStageModel.CostStageRevision
                    {
                        Id = csr.Id,
                        Name = csr.Name,
                        Status = csr.Status
                    })
                    .ToList();
            }

            return model;
        }

        /// <summary>
        /// Get previous revision of current revision in cost with corresponding status
        /// <para>Required: Cost has data of all Cost Stages / Revisions</para>
        /// </summary>
        public static CostStageRevision GetPreviousRevision(this CostStageRevision currentRevision, Cost cost)
        {
            if (cost == null || !cost.CostStages.Any() || currentRevision == null)
            {
                return null;
            }

            return cost.CostStages.SelectMany(cs => cs.CostStageRevisions)
                    .Where(csr =>
                        csr.Id != currentRevision.Id
                        && Constants.ApprovedStatuses.Contains(csr.Status)
                        && csr.CostStage.CostId == cost.Id
                        && csr.Created < currentRevision.Created
                    )
                    .OrderByDescending(csr => csr.Created)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Get latest revision in stage by stage name in cost
        /// <para>Required: Cost has data of all Cost Stages / Revisions / Approvals </para>
        /// </summary>
        public static CostStageRevision GetLatestRevisionByStage(this Cost cost, string stageName)
        {
            if (cost == null || !cost.CostStages.Any(cs => cs.Key == stageName))
            {
                return null;
            }

            var allRevisionsInStage = cost.CostStages.Where(cs => cs.Key == stageName).SelectMany(cs => cs.CostStageRevisions).ToList();

            return allRevisionsInStage.FirstOrDefault(r => r.Approvals.All(a => a.Status == ApprovalStatus.Approved))
                    ?? allRevisionsInStage.OrderByDescending(c => c.Created).FirstOrDefault(r => r.CostStage.Cost.LatestCostStageRevisionId != r.Id);
        }

        /// <summary>
        /// Get latest cost stage of costs
        /// <para>Required: Cost has data of Cost Stages / Revisions </para>
        /// </summary>
        public static CostStage GetLatestStage(this Cost cost)
        {
            if (cost == null || !cost.CostStages.Any())
            {
                return null;
            }

            return cost.CostStages.FirstOrDefault(cs => cs.CostStageRevisions.Any(csr => csr.Id == cost.LatestCostStageRevisionId));
        }

        /// <summary>
        /// Get latest revision of Cost Stage with corresponding status
        /// <para>Required: Cost Stage has data of Revisions</para>
        /// </summary>
        public static CostStageRevision LatestRevision(this CostStage costStage)
        {
            return costStage.CostStageRevisions
                .Where(csr => csr.Status != CostStageRevisionStatus.ReopenRejected)
                .OrderByDescending(csr => csr.Created)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get list of CostStageRevisionPaymentTotals of latest revision of Cost Stage with corresponding status
        /// <para>Required: Cost Stage has data of Revisions / Payments</para>
        /// </summary>
        public static List<CostStageRevisionPaymentTotal> GetLatestAvailablePayments(this CostStage costStage, bool includeProjections = false)
        {
            var latestPaymentRevision = costStage.CostStageRevisions
                .Where(r => !Constants.SkippedStatusForPayment.Contains(r.Status))
                .OrderByDescending(r => r.Created)
                .FirstOrDefault();

            return latestPaymentRevision.CostStageRevisionPaymentTotals.Where(p => includeProjections || !p.IsProjection).ToList();
        }

        /// <summary>
        /// Get all payments of all cost revisions in cost stage with corresponding status
        /// <para>Required: Cost Stage has data of Revisions / Payments</para>
        /// </summary>
        public static List<CostStageRevisionPaymentTotal> GetAllCostPaymentTotalsFinalActual(this CostStage costStage)
        {
            var payments = costStage.CostStageRevisions.SelectMany(csr => csr.CostStageRevisionPaymentTotals).ToList();
            return payments
                .Where(pm => Constants.ApprovedStatuses.Contains(pm.CostStageRevision.Status) && !pm.IsProjection)
                .ToList();
        }


        /// <summary>
        /// Get latest revision and its payments for all cost stages of Cost with corresponding status
        /// <para>Required: Cost has data of Cost Stages / Revisions / Payments / Currency </para>
        /// </summary>
        public static List<PaymentsViewModel> GetPaymentsViewModel(this Cost cost)
        {
            var result = new List<PaymentsViewModel>();
            if (cost == null || !cost.CostStages.Any())
            {
                return result;
            }

            var rateMultiplier = cost.ExchangeRate ?? 1m;

            foreach (var costStage in cost.CostStages.OrderBy(cs => cs.StageOrder))
            {
                var latestRevision = costStage.LatestRevision();
                var payments = costStage.GetLatestAvailablePayments(true);
                var deserialisedStageDetails = JsonConvert.DeserializeObject<dynamic>(latestRevision.StageDetails.Data);

                //ADC-2690 Add GR Amount to just display on UI, not the one that would be sent to COUPA
                var revisionCostTotals = costStage.GetAllCostPaymentTotalsFinalActual().Where(a => a.LineItemTotalType == Constants.CostSection.CostTotal).ToList();
                var displayGRAmount = revisionCostTotals.Sum(x => x.LineItemTotalCalculatedValue);

                result.Add(new PaymentsViewModel
                {
                    StageDetails = deserialisedStageDetails,
                    Currency = new CurrencyModel(cost.PaymentCurrency),
                    Key = costStage.Key,
                    Name = costStage.Name,
                    Payments = payments.Select(pm => new CostStageRevisionPaymentTotalModel
                    {
                        IsProjection = pm.IsProjection,
                        LineItemFullCost = pm.LineItemFullCost,
                        LineItemRemainingCost = pm.LineItemRemainingCost,
                        LineItemTotalCalculatedValue = pm.LineItemTotalCalculatedValue,
                        LineItemTotalType = pm.LineItemTotalType,
                        StageName = pm.StageName
                    }).ToList(),
                    ExchangeRate = rateMultiplier,
                    DisplayGRAmount = displayGRAmount
                });
            }

            return result;
        }

        /// <summary>
        /// Parse form from Custom object data by name
        /// <para>Required: must have data of custom object data </para>
        /// </summary>
        public static T GetForm<T>(this List<CustomObjectData> customObjectDatas, string name) where T : class
        {
            if (customObjectDatas != null && customObjectDatas.Any())
            {
                var data = customObjectDatas.FirstOrDefault(cod => cod.Name == name);
                if (data != null)
                {
                    return JsonConvert.DeserializeObject<T>(data.Data);
                }
            }
            return null;
        }

        /// <summary>
        /// Get total line item amount and total line item calculated amount of this revision
        /// <para>Required: revision has data of Payments </para>
        /// </summary>
        public static (decimal TotalAmount, decimal PaymentAmount) GetTotalCalculatedPayment(this CostStageRevision revision, decimal exchangeRate)
        {
            if (revision == null
                || !revision.CostStageRevisionPaymentTotals.Any(x => !x.IsProjection && x.LineItemTotalType == Constants.CostSection.CostTotal))
            {
                return (0, 0);
            }

            var calculatedPayments = revision.CostStageRevisionPaymentTotals.Where(x => !x.IsProjection).ToList();
            var totalCostAmount = calculatedPayments.First(x => x.LineItemTotalType == Constants.CostSection.CostTotal).LineItemFullCost / exchangeRate;
            var totalCostPayment = calculatedPayments.First(x => x.LineItemTotalType == Constants.CostSection.CostTotal).LineItemTotalCalculatedValue / exchangeRate;

            return (totalCostAmount, totalCostPayment);
        }

        /// <summary>
        /// Get total amount which is paid (shown in payment summary screen - only OE / FP / FA - not count their revisions) for this cost until this revision
        /// <para>Required: Cost has data of Cost Stages / Revisions / Payments </para>
        /// </summary>
        public static decimal GetAccumulatedAmount(this Cost cost, CostStageRevision currentRevision)
        {
            var calculatedRevisions = GetAccumulatedRevisions(cost, currentRevision);
            decimal accumulatedAmount = 0;
            foreach (var revision in calculatedRevisions)
            {
                accumulatedAmount += GetTotalCalculatedPayment(revision, cost.ExchangeRate ?? 1m).PaymentAmount;
            }

            return accumulatedAmount;
        }

        /// <summary>
        /// Get all revisions which are paid (shown in payment summary screen - only OE / FP / FA - not count their revisions) for this cost until this revision
        /// <para>Required: Cost has data of Cost Stages / Revisions / Payments </para>
        /// </summary>
        public static List<CostStageRevision> GetAccumulatedRevisions(this Cost cost, CostStageRevision currentRevision)
        {
            var calculatedRevisions = cost.CostStages.SelectMany(cs => cs.CostStageRevisions)
                .Where(csr => csr.Created <= currentRevision.Created &&
                    csr.Status == CostStageRevisionStatus.Approved &&
                    core.Constants.CostStageConstants.GrStatuses.Contains(csr.Name))
                .OrderBy(csr => csr.Created)
                .ToList();

            return calculatedRevisions;
        }

        /// <summary>
        /// Get total amount which is paid for this cost until this revision
        /// <para>Required: Cost has data of Cost Stages / Revisions / Payments </para>
        /// </summary>
        public static List<XMGPaidStep> GetPaidSteps(this Cost cost, CostStageRevision currentRevision)
        {
            var result = new List<XMGPaidStep>();
            var calculatedRevisions = GetAccumulatedRevisions(cost, currentRevision);

            foreach (var revision in calculatedRevisions)
            {
                result.Add(new XMGPaidStep {
                    Name = revision.Name,
                    Amount = GetTotalCalculatedPayment(revision, cost.ExchangeRate ?? 1m).PaymentAmount
                });
            }

            return result;
        }
    }
}
