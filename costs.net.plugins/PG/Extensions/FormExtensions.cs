namespace costs.net.plugins.PG.Extensions
{
    using dataAccess.Entity;
    using Form;

    public static class FormExtensions
    {
        public static string GetContentType(this PgStageDetailsForm stageForm)
        {
            if (stageForm.CostType == CostType.Production.ToString())
            {
                return stageForm.ContentType.Key;
            }

            return string.Empty;
        }

        public static string GetProductionType(this PgStageDetailsForm stageForm)
        {
            if (stageForm.ContentType?.Key == Constants.ContentType.Digital)
            {
                return Constants.Miscellaneous.NotApplicable;
            }

            if (stageForm.CostType == CostType.Production.ToString())
            {
                return stageForm.ProductionType.Key;
            }

            if (stageForm.CostType == CostType.Buyout.ToString())
            {
                return stageForm.UsageBuyoutType.Key;
            }

            return string.Empty;
        }
    }
}
