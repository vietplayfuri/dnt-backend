namespace costs.net.plugins.PG.Services.PurchaseOrder
{
    using core.Events.Cost;
    using core.Models.Utils;
    using core.Services.Costs;
    using core.Services.CustomData;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Models;
    using Models.PurchaseOrder;
    using Models.Stage;
    using Newtonsoft.Json;
    using Serilog;
    using Services;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;
    using System.Collections.Generic;

    public class PgPurchaseOrderService : IPgPurchaseOrderService
    {
        private readonly IPgLedgerMaterialCodeService _ledgerMaterialCode;
        private readonly IPgCurrencyService _currencyService;
        private readonly ILogger _logger;
        private readonly IPgPaymentService _pgPaymentService;
        private readonly ICostExchangeRateService _costExchangeRateService;
        private const string BasketNameTemplate = "ADCOST{0}{1}:{2}";
        private const string DesciptionTemplate = "{0}/{1} {2}";
        private readonly AppSettings _appSettings;
        private readonly EFContext _efContext;
        private readonly ICustomObjectDataService _customDataService;

        public static class CommodityConstants
        {
            public const string VideoProduction = "Video Production";
            public const string RadioProduction = "Radio Production";
            public const string PrintAndImageProduction = "Print and Image Production";
            public const string MultimediaDistributionAndTraffic = "Multimedia Distribution and Traffic";
            public const string Music = "Music";
            public const string TalentAndCelebrity = "Talent and Celebrity";
        }

        public PgPurchaseOrderService(EFContext efContext,
            IOptions<AppSettings> appSettings,
            IPgLedgerMaterialCodeService ledgerMaterialCode,
            ICustomObjectDataService customDataService,
            IPgCurrencyService pgCurrencyService,
            ILogger logger,
            IPgPaymentService pgPaymentService,
            ICostExchangeRateService costExchangeRateService)
        {
            _efContext = efContext;
            _ledgerMaterialCode = ledgerMaterialCode;
            _customDataService = customDataService;
            _appSettings = appSettings.Value;
            _currencyService = pgCurrencyService;
            _logger = logger;
            _pgPaymentService = pgPaymentService;
            _costExchangeRateService = costExchangeRateService;
        }

        public async Task<PgPurchaseOrder> GetPurchaseOrder(CostStageRevisionStatusChanged stageRevisionStatusChanged)
        {
            var costData = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr => csr.CostStage)
                .Where(c => c.Id == stageRevisionStatusChanged.AggregateId)
                .Include(c => c.Parent)
                    .ThenInclude(p => p.Agency)
                .Select(c => new
                {
                    BrandName = c.Project.Brand != null ? c.Project.Brand.Name : string.Empty,
                    CostNumber = c.CostNumber,
                    CostId = c.Id,
                    StageDetailsData = c.LatestCostStageRevision.StageDetails.Data,
                    ProductionDetailsData = c.LatestCostStageRevision.ProductDetails.Data,
                    LatestCostStageRevisionId = c.LatestCostStageRevision.Id,
                    AgencyLabels = c.Parent.Agency.Labels,
                    CostStageRevisionKey = c.LatestCostStageRevision.CostStage.Key,
                    CostStageRevisionName = c.LatestCostStageRevision.CostStage.Name,
                    Requisitioner = c.LatestCostStageRevision.Approvals
                        .Where(a => a.Type == ApprovalType.Brand && a.Requisitioners.Any())
                        .SelectMany(a => a.Requisitioners)
                        .Select(r => r.CostUser)
                        .FirstOrDefault(),
                    CostType = c.CostType,
                    ExchangeRate = c.ExchangeRate
                })
                .FirstOrDefaultAsync();

            if (costData == null)
            {
                _logger.Error($"Couldn't find cost with id {stageRevisionStatusChanged.AggregateId}");
                return null;
            }

            var purchaseOrderDto = new PgPurchaseOrderDTO
            {
                BrandName = costData.BrandName,
                CostNumber = costData.CostNumber,
                StageDetailsData = costData.StageDetailsData,
                ProductionDetailsData = costData.ProductionDetailsData,
                LatestCostStageRevisionId = costData.LatestCostStageRevisionId,
                AgencyLabels = costData.AgencyLabels,
                CostStageRevisionKey = costData.CostStageRevisionKey,
                CostStageRevisionName = costData.CostStageRevisionName,
                RequisitionerEmail = costData.Requisitioner?.Email,
                TNumber = costData.Requisitioner?.FederationId
            };

            var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(purchaseOrderDto.StageDetailsData);
            var productionDetailsForm = purchaseOrderDto.ProductionDetailsData != null ? JsonConvert.DeserializeObject<PgProductionDetailsForm>(purchaseOrderDto.ProductionDetailsData) : null;
            var paymentAmount = await GetTotals(stageRevisionStatusChanged.CostStageRevisionId, costData.ExchangeRate);

            var applicableCurrencyCode = await _currencyService.GetCurrencyCode(stageDetailsForm.AgencyCurrency, productionDetailsForm);
            var ledgerMaterialCode = await _ledgerMaterialCode.GetLedgerMaterialCodes(purchaseOrderDto.LatestCostStageRevisionId);
            var purchaseOrderResponse = await _customDataService.GetCustomData<PgPurchaseOrderResponse>(purchaseOrderDto.LatestCostStageRevisionId, CustomObjectDataKeys.PgPurchaseOrderResponse);
            var paymentDetails = await _customDataService.GetCustomData<PgPaymentDetails>(purchaseOrderDto.LatestCostStageRevisionId, CustomObjectDataKeys.PgPaymentDetails);

            var purchaseOrder = new PgPurchaseOrder
            {
                BasketName = GetBasketName(purchaseOrderDto, stageDetailsForm),
                Description = GetDescription(purchaseOrderDto, stageDetailsForm),
                TotalAmount = paymentAmount.TotalAmount,
                PaymentAmount = paymentAmount.PaymentAmount,
                StartDate = stageRevisionStatusChanged.TimeStamp,
                CostNumber = purchaseOrderDto.CostNumber ?? "",
                Currency = applicableCurrencyCode,
                CategoryId = ledgerMaterialCode?.MgCode ?? "",
                GL = ledgerMaterialCode?.GlCode ?? "",
                DeliveryDate = GetDeliveryDate(stageRevisionStatusChanged.TimeStamp),
                IONumber = !string.IsNullOrEmpty(paymentDetails?.IoNumber) ? $"00{paymentDetails.IoNumber}" : "",
                LongText = GetLongTextField(stageRevisionStatusChanged, purchaseOrderDto, stageDetailsForm, purchaseOrderResponse, paymentAmount.PaymentAmount, applicableCurrencyCode, costData.CostType),
                TNumber = purchaseOrderDto.TNumber ?? "",
                RequisitionerEmail = purchaseOrderDto.RequisitionerEmail ?? "",
                Vendor = GetVendor(purchaseOrderDto, productionDetailsForm) ?? "",
                PoNumber = paymentDetails?.PoNumber ?? "",
                AccountCode = purchaseOrderResponse?.AccountCode ?? "",
                ItemIdCode = purchaseOrderResponse?.ItemIdCode ?? "",
                GrNumbers = await GetGrNumbers(stageRevisionStatusChanged),
                Commodity = GetCommodity(costData.CostType, stageDetailsForm)
            };

            return purchaseOrder;
        }

        public async Task<bool> NeedToSendPurchaseOrder(CostStageRevisionStatusChanged stageRevisionStatusChanged)
        {
            var costData = await _efContext.Cost
                .Where(c => c.Id == stageRevisionStatusChanged.AggregateId)
                .Select(c => new
                {
                    IsExternalPurchases = c.IsExternalPurchases
                })
                .FirstOrDefaultAsync();

            if (costData == null)
            {
                _logger.Error($"Couldn't find cost with id {stageRevisionStatusChanged.AggregateId}");
                return false;
            }

            return costData.IsExternalPurchases;
        }

        private static string GetCommodity(CostType costType, PgStageDetailsForm stageDetails)
        {
            switch (costType)
            {
                case CostType.Production:
                    if (stageDetails.ContentType == null)
                    {
                        break;
                    }

                    switch (stageDetails.ContentType.Key)
                    {
                        case Constants.ContentType.Video:
                        case Constants.ContentType.Digital:
                            return CommodityConstants.VideoProduction;
                        case Constants.ContentType.Audio:
                            return CommodityConstants.RadioProduction;
                        case Constants.ContentType.Photography:
                            return CommodityConstants.PrintAndImageProduction;
                    }
                    break;
                case CostType.Trafficking:
                    return CommodityConstants.MultimediaDistributionAndTraffic;

                case CostType.Buyout:
                    if (stageDetails.UsageType == null)
                    {
                        break;
                    }

                    switch (stageDetails.UsageType.Key)
                    {
                        case Constants.UsageType.Photography:
                        case Constants.UsageType.Music:
                        case Constants.UsageType.VoiceOver:
                            return CommodityConstants.Music;
                        default:
                            return CommodityConstants.TalentAndCelebrity;
                    }
            }

            return string.Empty;
        }

        private async Task<string[]> GetGrNumbers(CostStageRevisionStatusChanged stageRevisionStatusChanged)
        {

            if (stageRevisionStatusChanged.Status != CostStageRevisionStatus.PendingCancellation)
            {
                return new string[0];
            }

            var costId = stageRevisionStatusChanged.AggregateId;
            var revisionIds = await _efContext.Cost
                .Where(c => c.Id == costId)
                .SelectMany(c => c.CostStages)
                .SelectMany(cs => cs.CostStageRevisions.Select(csr => csr.Id))
                .ToArrayAsync();

            var grNumbers = (await _customDataService.GetCustomData<PgPurchaseOrderResponse>(revisionIds, CustomObjectDataKeys.PgPurchaseOrderResponse))
                ?.Select(cd => cd.GrNumber)
                .ToArray();

            return grNumbers ?? new string[0];
        }

        private static string GetVendor(PgPurchaseOrderDTO purchaseDto, PgProductionDetailsForm productionDetails)
        {
            var code = productionDetails?.DirectPaymentVendor != null
                ? productionDetails.DirectPaymentVendor.SapVendorCode
                : purchaseDto.AgencyLabels.GetSapVendorCode();

            return code;
        }

        private PgPurchaseOrder.LongTextField GetLongTextField(CostStageRevisionStatusChanged stageRevisionStatusChanged,
            PgPurchaseOrderDTO purchaseOrderDto, PgStageDetailsForm stageDetailsForm, PgPurchaseOrderResponse purchaseOrderResponse,
            decimal paymentAmount, string currencyCode, CostType costType)
        {
            var longText = new PgPurchaseOrder.LongTextField();
            switch (stageRevisionStatusChanged.Status)
            {
                case CostStageRevisionStatus.PendingBrandApproval:
                    longText.VN.AddRange(new[] {
                            "Purchase order does not authorize committing funds without approved EPCAT sheet.",
                            "The services within this Purchase Order can only be ordered from 3rd parties after EPCAT approval."
                    });
                    var productionTypeIfProductionCost = stageDetailsForm.ProductionType != null ? $"{stageDetailsForm.ProductionType.Key} " : "";
                    longText.BN.Add($"{purchaseOrderDto.CostStageRevisionName} APPROVED {costType} {productionTypeIfProductionCost}{purchaseOrderResponse?.PoNumber}".TrimEnd());

                    longText.AN.Add($"{_appSettings.FrontendUrl.TrimEnd('/')}/#/cost/{stageRevisionStatusChanged.AggregateId}/review");
                    break;

                case CostStageRevisionStatus.PendingCancellation:
                    longText.VN.Add("PROJECT CANCELLED. PLEASE CANCEL PO AND REQUEST CN FOR ANY AMOUNTS PAID");
                    longText.BN.Add($"PROJECT CANCELLED. PLEASE CANCEL PO {purchaseOrderResponse?.PoNumber} AND REQUEST CN FOR ANY AMOUNTS PAID");
                    break;

                case CostStageRevisionStatus.Approved:
                    if (purchaseOrderDto.CostStageRevisionKey == CostStages.FinalActual.ToString() || purchaseOrderDto.CostStageRevisionKey == CostStages.FinalActualRevision.ToString())
                    {
                        //ADC-2412
                        //Strange issue when the payment amount looks something like -1560.00000000000000000000000000002
                        paymentAmount = Math.Round(paymentAmount, 2);
                        // Get calculated credit amount
                        if (paymentAmount < 0)
                        {
                            longText.BN.Add($"A credit note of { paymentAmount } {currencyCode} is needed, please update PO accordingly.");
                        }
                    }
                    break;
            }
            return longText;
        }

        private async Task<(decimal TotalAmount, decimal PaymentAmount)> GetTotals(Guid revisionId, Decimal? exchangeRate)
        {
            var totalPaymentAmountInApplicableCurrency = 0m;
            var totalAmountInApplicableCurrency = 0m;
            var paymentAmount = await _pgPaymentService.GetPaymentAmount(revisionId, false);
            if (paymentAmount != null)
            {
                var rateMultiplier = exchangeRate ?? 1m;

                totalAmountInApplicableCurrency = (paymentAmount.TotalCostAmount ?? 0) / rateMultiplier;
                totalPaymentAmountInApplicableCurrency = (paymentAmount.TotalCostAmountPayment ?? 0) / rateMultiplier;
            }

            return (TotalAmount: totalAmountInApplicableCurrency, PaymentAmount: totalPaymentAmountInApplicableCurrency);
        }

        private static DateTime GetDeliveryDate(DateTime purchaseViewStartDate)
        {
            return purchaseViewStartDate.AddMonths(3);
        }

        private static string GetBasketName(PgPurchaseOrderDTO purchaseOrderDto, PgStageDetailsForm stageDetailsForm)
        {
            var basketName = string.Format(BasketNameTemplate, purchaseOrderDto.CostNumber, purchaseOrderDto.BrandName, stageDetailsForm.Description);
            return Truncate(basketName);
        }

        private static string GetDescription(PgPurchaseOrderDTO purchaseOrderDto, PgStageDetailsForm stageDetailsForm)
        {
            var description = string.Format(DesciptionTemplate, stageDetailsForm.ContentType?.Value, stageDetailsForm.BudgetRegion?.Name, purchaseOrderDto.CostNumber);
            return Truncate(description, 50);
        }

        private static string Truncate(string str, int length = 40)
        {
            str = str.Replace("&", "and");
            if (str.Length > length)
            {
                str = str.Substring(0, length);
            }
            return str;
        }

        public async Task<List<XMGOrder>> GetXMGOrder(string costNumber)
        {
            var costData = await _efContext.Cost
                .Include(c => c.Project).ThenInclude(p => p.Brand)
                .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.StageDetails)
                .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.ProductDetails)
                .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.Approvals).ThenInclude(a => a.Requisitioners).ThenInclude(am => am.CostUser)
                .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CustomObjectData)
                .Include(c => c.CostStages).ThenInclude(cs => cs.CostStageRevisions).ThenInclude(csr => csr.CostStageRevisionPaymentTotals)
                .Include(c => c.Parent).ThenInclude(p => p.Agency)
                .Where(c => c.CostNumber == costNumber)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (costData == null)
            {
                _logger.Error($"Couldn't find cost number {costNumber}");
                return null;
            }

            var xmgOrders = new List<XMGOrder>();
            var revisionIds = costData.CostStages.SelectMany(cs => cs.CostStageRevisions).Select(csr => csr.Id).ToList();
            var grNumbers = (await _customDataService.GetCustomData<PgPurchaseOrderResponse>(revisionIds, CustomObjectDataKeys.PgPurchaseOrderResponse))?
                .Select(cd => cd.GrNumber)
                .Distinct()
                .ToArray();

            foreach (var costStage in costData.CostStages.OrderBy(cs => cs.StageOrder))
            {
                foreach (var revision in costStage.CostStageRevisions.OrderBy(cs => cs.Created))
                {
                    if (revision.CostStageRevisionPaymentTotals == null || !revision.CostStageRevisionPaymentTotals.Any())      //there is no payment - meaning it hasn't come to XMG yet
                    {
                        continue;
                    }

                    var requisitioner = revision.Approvals.Where(a => a.Type == ApprovalType.Brand && a.Requisitioners.Any())
                            .SelectMany(a => a.Requisitioners)
                            .Select(r => r.CostUser)
                            .FirstOrDefault();
                    var purchaseOrderDto = new PgPurchaseOrderDTO
                    {
                        BrandName = costData.Project.Brand.Name,
                        CostNumber = costData.CostNumber,
                        StageDetailsData = revision.StageDetails.Data,
                        ProductionDetailsData = revision.ProductDetails.Data,
                        LatestCostStageRevisionId = revision.Id,
                        AgencyLabels = costData.Parent.Agency.Labels,
                        CostStageRevisionKey = costStage.Key,
                        CostStageRevisionName = costStage.Name,
                        RequisitionerEmail = requisitioner?.Email,
                        TNumber = requisitioner?.FederationId
                    };

                    var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(purchaseOrderDto.StageDetailsData);
                    var productionDetailsForm = purchaseOrderDto.ProductionDetailsData != null ? JsonConvert.DeserializeObject<PgProductionDetailsForm>(purchaseOrderDto.ProductionDetailsData) : null;
                    var paymentAmount = revision.GetTotalCalculatedPayment(costData.ExchangeRate ?? 1m);

                    var applicableCurrencyCode = await _currencyService.GetCurrencyCode(stageDetailsForm.AgencyCurrency, productionDetailsForm);

                    var ledgerMaterialCode = revision.CustomObjectData.GetForm<PgLedgerMaterialCodeModel>(CustomObjectDataKeys.PgMaterialLedgerCodes);
                    var purchaseOrderResponse = revision.CustomObjectData.GetForm<PgPurchaseOrderResponse>(CustomObjectDataKeys.PgPurchaseOrderResponse);
                    var paymentDetails = revision.CustomObjectData.GetForm<PgPaymentDetails>(CustomObjectDataKeys.PgPaymentDetails);

                    var purchaseOrder = new XMGOrder
                    {
                        BasketName = GetBasketName(purchaseOrderDto, stageDetailsForm),
                        Description = GetDescription(purchaseOrderDto, stageDetailsForm),
                        TotalAmount = paymentAmount.TotalAmount,
                        PaymentAmount = paymentAmount.PaymentAmount,
                        StartDate = revision.Created,
                        CostNumber = purchaseOrderDto.CostNumber ?? "",
                        Currency = applicableCurrencyCode,
                        CategoryId = ledgerMaterialCode?.MgCode ?? "",
                        GL = ledgerMaterialCode?.GlCode ?? "",
                        DeliveryDate = GetDeliveryDate(revision.Created),
                        IONumber = !string.IsNullOrEmpty(paymentDetails?.IoNumber) ? $"00{paymentDetails?.IoNumber}" : "",
                        LongText = GetLongTextField(new CostStageRevisionStatusChanged
                        {
                            Status = revision.Status,
                            AggregateId = costData.Id
                        }, purchaseOrderDto, stageDetailsForm, purchaseOrderResponse, paymentAmount.PaymentAmount, applicableCurrencyCode, costData.CostType),
                        TNumber = purchaseOrderDto.TNumber ?? "",
                        RequisitionerEmail = purchaseOrderDto.RequisitionerEmail ?? "",
                        Vendor = GetVendor(purchaseOrderDto, productionDetailsForm) ?? "",
                        PoNumber = paymentDetails?.PoNumber ?? "",
                        AccountCode = purchaseOrderResponse?.AccountCode ?? "",
                        ItemIdCode = purchaseOrderResponse?.ItemIdCode ?? "",
                        GrNumbers = grNumbers ?? new string[0],
                        Commodity = GetCommodity(costData.CostType, stageDetailsForm),
                        StageName = revision.Name,
                        Status = revision.Status,
                        Created = revision.Created,
                        Modified = revision.Modified,
                        RequisitionId = purchaseOrderResponse?.Requisition,
                        AccumulatedAmount = costData.GetAccumulatedAmount(revision),
                        PaidSteps = costData.GetPaidSteps(revision)
                    };
                    xmgOrders.Add(purchaseOrder);
                }
            }

            return xmgOrders;
        }
    }
}
