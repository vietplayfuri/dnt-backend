
namespace costs.net.plugins.PG.Services.Budget
{
    using System;
    using System.Linq;
    using core.Models.CostTemplate;
    using core.Services;
    using core.Services.Budget;

    public class CostSectionFinder : ICostSectionFinder
    {
        public ServiceResult<ProductionDetailsFormDefinitionModel> GetCostSection(CostTemplateVersionModel templateModel, string contentType, string production)
        {
            if (templateModel == null)
            {
                throw new ArgumentNullException(nameof(templateModel));
            }

            if (templateModel.ProductionDetails == null)
            {
                throw new ArgumentNullException(nameof(templateModel.ProductionDetails));
            }

            if (production == "Contract")
            {
                // TODO: Why the cost_template contain duplicate versions of the same Usage/Buyout form for Video, Audio and so on??
                return new ServiceResult<ProductionDetailsFormDefinitionModel>(templateModel.ProductionDetails.First().Forms[0]);
            }

            if (string.IsNullOrEmpty(contentType))
            {
                return ServiceResult<ProductionDetailsFormDefinitionModel>.CreateFailedResult("ContentType is empty");
            }

            foreach (var pd in templateModel.ProductionDetails)
            {
                if (string.Compare(pd.Type, contentType, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    foreach (var pdf in pd.Forms)
                    {
                        if (string.Compare(pdf.ProductionType, production, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return new ServiceResult<ProductionDetailsFormDefinitionModel>(pdf);
                        }

                        if (production == Constants.Miscellaneous.NotApplicable &&
                            string.Compare(pdf.ProductionType, Constants.ProductionType.FullProduction, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return new ServiceResult<ProductionDetailsFormDefinitionModel>(pdf);
                        }
                    }
                }
            }
            string error = $"Production form not found for Content Type '{contentType}' and Production '{production}'";
            return ServiceResult<ProductionDetailsFormDefinitionModel>.CreateFailedResult(error);
        }
    }
}
