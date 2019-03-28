
namespace costs.net.plugins.PG.Services.BillingExpenses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders.BillingExpenses;
    using core.Models;
    using core.Models.BillingExpenses;
    using core.Services;
    using core.Services.BillingExpenses;
    using core.Services.Costs;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using Form.UsageBuyout;

    public class BillingExpensesService : IBillingExpensesService
    {
        private static readonly ILogger Logger = Log.ForContext<BillingExpensesService>();

        private readonly EFContext _efContext;
        private readonly IMapper _mapper;
        private readonly IFinancialYearService _financialYearService;
        private readonly ICostFormService _costFormService;
        private readonly IBillingExpenseBuilder _builder;
        private readonly IBillingExpenseCalculator _calculator;
        private readonly IBillingExpenseInterpolator _interpolator;
        private readonly ICostStageRevisionService _costStageRevisionService;

        public BillingExpensesService(EFContext efContext,
            IMapper mapper, 
            IFinancialYearService financialYearService,
            ICostFormService costFormService,
            IBillingExpenseBuilder builder,
            IBillingExpenseCalculator calculator,
            IBillingExpenseInterpolator interpolator,
            ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _mapper = mapper;
            _financialYearService = financialYearService;
            _costFormService = costFormService;
            _builder = builder;
            _calculator = calculator;
            _interpolator = interpolator;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<ServiceResult> Upsert(Guid costId, Guid costStageRevisionId, 
            Guid userId, IEnumerable<BillingExpenseItem> billingExpenses)
        {
            var entities = _efContext.BillingExpense.Where(be => be.CostStageRevisionId == costStageRevisionId);

            foreach (var billingExpense in billingExpenses)
            {
                //ignore bad data
                if (string.IsNullOrEmpty(billingExpense.SectionKey) || string.IsNullOrEmpty(billingExpense.Key))
                {
                    continue;
                }

                //ignore dynamically-calculated data
                if (billingExpense.SectionKey == Constants.BillingExpenseSection.Header && 
                    billingExpense.Key == Constants.BillingExpenseItem.BalancePrepaid)
                {
                    continue;
                }

                BillingExpense entity;
                if (billingExpense.Id.HasValue)
                {
                    //Update
                    entity = entities.First(i => i.Id == billingExpense.Id);
                    _mapper.Map(billingExpense, entity);
                    entity.SetModifiedNow();
                }
                else
                {
                    // Create
                    entity = new BillingExpense { Id = Guid.NewGuid() };
                    entity.SetCreatedNow(userId);
                    _mapper.Map(billingExpense, entity);
                    entity.CostStageRevisionId = costStageRevisionId;
                    _efContext.BillingExpense.Add(entity);
                }
            }

            await _efContext.SaveChangesAsync();

            return new ServiceResult(true);
        }        

        public async Task<ServiceResult<BillingExpenseViewModel>> Get(Guid costStageRevisionId)
        {
            //Get the contract period
            var usageForm = await _costFormService.GetCostFormDetails<BuyoutDetails>(costStageRevisionId);
            var contract = usageForm.Contract;
            var startDate = contract.StartDate;
            var endDate = contract.EndDate ?? DateTime.UtcNow;

            //Build the financial years based on start and end date
            var financialYears = await _financialYearService.Calculate(BuType.Pg, startDate, endDate);
            
            var model = new BillingExpenseViewModel();

            //Get the payment currency from the stage details form
            var stageForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            var paymentCurrency = await _efContext.Currency.FirstOrDefaultAsync(c => c.Code == stageForm.AgencyCurrency);
            model.PaymentCurrency = paymentCurrency;

            var billingExpenses = await _efContext.BillingExpense.Where(be => be.CostStageRevisionId == costStageRevisionId).ToListAsync();

            //Build the billing expenses
            var costStage = await _efContext.CostStageRevision
                .Where(c => c.Id == costStageRevisionId)
                .Select(c => c.CostStage).FirstOrDefaultAsync();
            model.Data = _builder.BuildExpenses(costStage, billingExpenses, financialYears.Result);
            model.Modified = billingExpenses.Count > 0 ? billingExpenses.Max(b => b.Modified) : null;
            model.Saved = model.Modified != null;

            //Perform any calculations
            await _calculator.CalculateExpenses(usageForm.Contract.ContractTotal, model.Data, billingExpenses);
           
            return ServiceResult<BillingExpenseViewModel>.CreateSuccessfulResult(model);
        }

        public async Task<ServiceResult<CalculationResponse>> Calculate(Guid costStageRevisionId, CalculationRequest request)
        {
            if (request == null)
            {
                return ServiceResult<CalculationResponse>.CreateFailedResult("Invalid request.");
            }

            //Get the contract period
            var usageForm = await _costFormService.GetCostFormDetails<BuyoutDetails>(costStageRevisionId);
            var contract = usageForm.Contract;
            var startDate = contract.StartDate;
            var endDate = contract.EndDate ?? DateTime.UtcNow;

            //Build the financial years based on start and end date
            var contractPeriodResult = await _financialYearService.Calculate(BuType.Pg, startDate, endDate);

            var model = new CalculationResponse();
            
            var costStageRevision = await _efContext.CostStageRevision
                .Include(c => c.CostLineItems)
                .Include(c => c.BillingExpenses)
                .Include(c => c.CostStage)
                .FirstOrDefaultAsync(c => c.Id == costStageRevisionId);
            
            var billingExpenses = costStageRevision.BillingExpenses;

            //Build the Billing Expense
            model.Data = _builder.BuildExpenses(costStageRevision.CostStage, request.Cells, contractPeriodResult.Result);

            //Calculate items in the Billing Expense
            await _calculator.CalculateExpenses(usageForm.Contract.ContractTotal, model.Data, billingExpenses);

            //Update/Interpolate any related cost line items
            model.CostLineItems = await _interpolator.InterpolateCostLineItems(costStageRevision.CostStage.CostId, costStageRevisionId, model.Data);

            return ServiceResult<CalculationResponse>.CreateSuccessfulResult(model);
        }
    }
}
