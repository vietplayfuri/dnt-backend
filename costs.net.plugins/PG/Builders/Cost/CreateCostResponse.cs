namespace costs.net.plugins.PG.Builders.Cost
{
    using core.Builders.Response.Cost;

    public class CreateCostResponse : ICreateCostResponse
    {
        public CostBuilderModel Cost { get; set; }
    }
}