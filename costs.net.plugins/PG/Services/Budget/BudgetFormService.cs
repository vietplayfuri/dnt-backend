namespace costs.net.plugins.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Models.Budget;
    using core.Models.Excel;
    using core.Models.User;
    using core.Services;
    using core.Models.ActivityLog;
    using core.Models.Costs;
    using core.Services.ActivityLog;
    using core.Services.Budget;
    using core.Services.Costs;
    using core.Services.Excel;
    using dataAccess;
    using Extensions;
    using Form;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Serilog;

    public class BudgetFormService : IBudgetFormService
    {
        private static readonly ILogger Logger = Log.ForContext<BudgetFormService>();

        private readonly ISupportingDocumentsService _supportingDocumentsService;
        private readonly IExcelCellService _excelCellService;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly ICostLineItemUpdater _costLineItemUpdater;
        private readonly ICostCurrencyUpdater _costCurrencyUpdater;
        private readonly IBudgetFormPropertyValidator _budgetFormValidator;
        private readonly EFContext _efContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public BudgetFormService(
            EFContext efContext,
            IExcelCellService excelCellService,
            ICostStageRevisionService costStageRevisionService,
            ICostLineItemUpdater costLineItemUpdater,
            ISupportingDocumentsService supportingDocumentsService,
            ICostCurrencyUpdater costCurrencyUpdater,
            IBudgetFormPropertyValidator budgetFormValidator,
            ICostService costService,
            IActivityLogService activityLogService,
            IMapper mapper)
        {
            _efContext = efContext;
            _excelCellService = excelCellService;
            _costStageRevisionService = costStageRevisionService;
            _costLineItemUpdater = costLineItemUpdater;
            _supportingDocumentsService = supportingDocumentsService;
            _costCurrencyUpdater = costCurrencyUpdater;
            _budgetFormValidator = budgetFormValidator;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<BudgetFormUploadResult>> UploadBudgetForm(Guid costId, Guid costStageId, Guid costStageRevisionId, UserIdentity userIdentity, IFormFile file)
        {
            //Read the properties from the Properties excel sheet.
            ExcelProperties properties;
            using (var fileStream = file.OpenReadStream())
            {
                properties = _excelCellService.ReadProperties(file.FileName, fileStream);
            }

            //Validate the uploaded budget form is the correct format for the cost being updated.
            var stageForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(costStageRevisionId);
            string contentType = stageForm.GetContentType();
            string production = stageForm.GetProductionType();
            var validationResult = _budgetFormValidator.IsValid(properties, contentType, production);

            if (!validationResult.Success)
            {
                return ServiceResult<BudgetFormUploadResult>.CloneFailedResult(validationResult);
            }

            //Read the cost line items
            ExcelCellValueLookup entries;
            using (var fileStream = file.OpenReadStream())
            {
                entries = await _excelCellService.ReadEntries(properties[core.Constants.BudgetFormExcelPropertyNames.LookupGroup], file.FileName, fileStream);
            }

            //Update the currencies in the Cost
            var currencyResult = await _costCurrencyUpdater.Update(entries, userIdentity.Id, costId, costStageRevisionId);

            if (!currencyResult.Success)
            {
                return ServiceResult<BudgetFormUploadResult>.CloneFailedResult(currencyResult);
            }

            //Update the cost
            var costLineItemResult = await _costLineItemUpdater.Update(entries, userIdentity, costId, costStageRevisionId);

            if (!costLineItemResult.Success)
            {
                return ServiceResult<BudgetFormUploadResult>.CloneFailedResult(costLineItemResult);
            }

            //Save updates made by _costLineItemUpdater and _costCurrencyUpdater.
            await _efContext.SaveChangesAsync();

            //Add Budget form to GDN using Gdam Core.
            var supportingDocument = await _supportingDocumentsService.GetSupportingDocument(costStageRevisionId, core.Constants.SupportingDocuments.BudgetForm);
            await _supportingDocumentsService.UploadSupportingDocumentRevision(costId, supportingDocument, userIdentity, file);

            var supportingDocumentViewModel = _mapper.Map<SupportingDocumentViewModel>(supportingDocument);

            var costLineItemModels = _mapper.Map<List<CostLineItemViewModel>>(costLineItemResult.Result);

            var budgetFormUploadResult = new BudgetFormUploadResult
            {
                Currencies = currencyResult.Result,
                CostLineItems = costLineItemModels,
                SupportingDocument = supportingDocumentViewModel
            };

            var costNumber = await _efContext.Cost.Where(c => c.Id == costId).Select(c => c.CostNumber).SingleAsync();
            await _activityLogService.Log(new BudgetFormUploaded(costNumber, file.FileName, supportingDocument.Id, userIdentity));

            return new ServiceResult<BudgetFormUploadResult>(budgetFormUploadResult);
        }
    }
}
