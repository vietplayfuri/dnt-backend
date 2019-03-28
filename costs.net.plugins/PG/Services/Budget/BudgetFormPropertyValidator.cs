
namespace costs.net.plugins.PG.Services.Budget
{
    using System;
    using core.Models.Excel;
    using core.Services;
    using core.Services.Budget;

    /// <summary>
    /// Checks to see if the uploaded budget form is valid for the costs's content type and production.
    /// </summary>
    public class BudgetFormPropertyValidator : IBudgetFormPropertyValidator
    {
        private const string PropertiesInvalidMessage =
            "Please upload the correct type of Budget Form for this cost (Audio, Digital, Still Image, Video Full Production or Video Post Production)";

        public ServiceResult IsValid(ExcelProperties properties, string contentType, string production)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (!ArePropertiesValid(properties))
            {
                var result = new ServiceResult(false);
                result.AddError(PropertiesInvalidMessage);
                return result;
            }

            if (!IsContentTypeValid(properties, contentType))
            {
                var result = new ServiceResult(false);
                result.AddError($"Expected Content Type '{contentType}' but found '{properties[core.Constants.BudgetFormExcelPropertyNames.ContentType]}'.");
                return result;
            }

            if (IsAnyProductionContentType(properties[core.Constants.BudgetFormExcelPropertyNames.ContentType]))
            {
                return new ServiceResult(true);
            }

            if (!IsProductionValid(properties, contentType, production))
            {
                var result = new ServiceResult(false);
                result.AddError($"Expected '{production}' but found '{properties[core.Constants.BudgetFormExcelPropertyNames.Production]}'.");
                return result;
            }

            return new ServiceResult(true);
        }

        private bool ArePropertiesValid(ExcelProperties properties)
        {
            if (properties.Count == 0)
            {
                return false;
            }

            if (!properties.ContainsKey(core.Constants.BudgetFormExcelPropertyNames.LookupGroup))
            {
                return false;
            }

            if (!properties.ContainsKey(core.Constants.BudgetFormExcelPropertyNames.ContentType))
            {
                return false;
            }

            if (!properties.ContainsKey(core.Constants.BudgetFormExcelPropertyNames.Production))
            {
                return false;
            }

            return true;
        }

        private static bool IsContentTypeValid(ExcelProperties properties, string contentType)
        {
            return string.Compare(contentType, 
                properties[core.Constants.BudgetFormExcelPropertyNames.ContentType], StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static bool IsProductionValid(ExcelProperties properties, string contentType, string production)
        {
            if (string.Compare(contentType, Constants.ContentType.Video, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(production, Constants.ProductionType.CgiAnimation, StringComparison.OrdinalIgnoreCase) == 0)
            {
                production = Constants.ProductionType.PostProductionOnly; //For Video & CGI, use the Post Production mappings
            }

            return string.Compare(production, properties[core.Constants.BudgetFormExcelPropertyNames.Production], StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// For Video, the Production Type can be Full production or Post production. For the others, only
        /// one Excel format type is used. 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private static bool IsAnyProductionContentType(string contentType)
        {
            switch (contentType)
            {
                case Constants.ContentType.Audio:
                case Constants.ContentType.Digital:
                case Constants.ContentType.Photography:
                    return true;
            }

            return false;
        }

    }
}
