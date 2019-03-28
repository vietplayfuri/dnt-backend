namespace costs.net.plugins.PG.Services.Notifications
{
    using System.Linq;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;

    public static class CostExtensions
    {
        public static IQueryable<Cost> IncludeCostOwner(this IQueryable<Cost> costs)
        {
            return costs
                .Include(c => c.Owner)
                    .ThenInclude(o => o.Agency)
                        .ThenInclude(a => a.Country);
        }
    }
}
