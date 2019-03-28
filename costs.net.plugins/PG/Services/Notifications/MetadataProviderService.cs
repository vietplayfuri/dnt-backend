
namespace costs.net.plugins.PG.Services.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Extensions;
    using core.Models.Notifications;
    using core.Services.Costs;
    using core.Services.Notifications;
    using costs.net.dataAccess.Extensions;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using MoreLinq;
    using Serilog;
    using Cost = dataAccess.Entity.Cost;

    public class MetadataProviderService : IMetadataProviderService
    {
        private static readonly ILogger Logger = Log.ForContext<MetadataProviderService>();

        private readonly EFContext _efContext;
        private readonly ICostStageRevisionService _costStageRevisionService;

        public MetadataProviderService(EFContext efContext, ICostStageRevisionService costStageRevisionService)
        {
            _efContext = efContext;
            _costStageRevisionService = costStageRevisionService;
        }

        public async Task<ICollection<MetadataItem>> Provide(Guid costId)
        {
            var metadataItems = new List<MetadataItem>();

            if (costId == Guid.Empty)
            {
                Logger.Warning("EN001: CostId is empty Guid.");
                return metadataItems;
            }

            var cost = await _efContext.Cost
                .AsNoTracking()
                .Include(c => c.Owner)
                    .ThenInclude(cu => cu.Agency)
                        .ThenInclude(a => a.Country)
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr => csr.Approvals)
                        .ThenInclude(a => a.ApprovalMembers)
                            .ThenInclude(am => am.CostUser)
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(csr=> csr.StageDetails)
                .Include(c => c.Project)
                    .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (cost == null)
            {
                Logger.Warning($"EN002: Cost with Id {costId} not found.");
                return metadataItems;
            }

            var costOwner = cost.Owner;

            if (costOwner == null)
            {
                Logger.Warning($"EN003: CostOwner with Id {cost.OwnerId} not found.");
                return metadataItems;
            }

            var stageDetails = _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(cost.LatestCostStageRevision);

            if (stageDetails == null)
            {
                Logger.Warning($"EN004: Stage Details for Cost Id {costId} not found.");
                return metadataItems;
            }

            AddMetadataItems(cost, costOwner, stageDetails, metadataItems);

            return metadataItems;
        }

        private void AddMetadataItems(Cost cost, CostUser costOwner, PgStageDetailsForm stageDetails, ICollection<MetadataItem> metadataItems)
        {
            var project = cost.Project;
            var agency = costOwner.Agency;
            var agencyCountry = agency.Country;

            // The items below are defined in ADC-2265. The order here is the same order they will appear in the email.
            metadataItems.Add(new MetadataItem
            {
                Label = "Cost Title",
                Value = stageDetails.Title
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Cost ID",
                Value = cost.CostNumber
            });

            AddCostTypeItems(metadataItems, stageDetails);

            metadataItems.Add(new MetadataItem
            {
                Label = "Status",
                Value = cost.Status.ToStringCustom().AddSpacesToSentence()
            });
            if (stageDetails.AgencyProducer != null)
            {
                metadataItems.Add(new MetadataItem
                {
                    Label = "Agency Producer",
                    Value = string.Join(',', stageDetails.AgencyProducer)
                });
            }
            metadataItems.Add(new MetadataItem
            {
                Label = "Agency Owner",
                Value = $"{costOwner.FullName} - {costOwner.Email}"
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Agency Name",
                Value = agency.Name
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Agency Location",
                Value = agencyCountry.Name
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Agency Tracking #",
                Value = stageDetails.AgencyTrackingNumber
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Project Name",
                Value = project.Name
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Project ID",
                Value = project.AdCostNumber
            });
            metadataItems.Add(new MetadataItem
            {
                Label = "Budget Region",
                Value = stageDetails.BudgetRegion.Name
            });

            AddApprovalTypeItems(metadataItems, cost.LatestCostStageRevision.Approvals);

            if (project.Brand != null)
            {
                metadataItems.Add(new MetadataItem
                {
                    Label = "Brand",
                    Value = project.Brand.Name
                });
            }
        }

        private static void AddApprovalTypeItems(ICollection<MetadataItem> metadataItems, List<Approval> approvals)
        {
            approvals.Where(a => a.Type == ApprovalType.IPM).ForEach(a =>
            {
                //get the first IPM only because the UI only allows one IPM
                var ipm = a.ApprovalMembers.FirstOrDefault();
                if (ipm != null)
                {
                    metadataItems.Add(new MetadataItem
                    {
                        Label = "Technical Approver",
                        Value = ipm.CostUser.FullName
                    });
                }
            });
        }

        private static void AddCostTypeItems(ICollection<MetadataItem> metadataItems, PgStageDetailsForm stageDetails)
        {
            switch (stageDetails.CostType)
            {
                case Constants.CostType.Production:
                    metadataItems.Add(new MetadataItem
                    {
                        Label = "Content Type",
                        Value = stageDetails.ContentType.Value
                    });
                    if (stageDetails.ProductionType != null)
                    {
                        // non-Digital
                        metadataItems.Add(new MetadataItem
                        {
                            Label = "Production Type",
                            Value = stageDetails.ProductionType.Value
                        });
                    }
                    break;
                case Constants.CostType.Buyout:
                    metadataItems.Add(new MetadataItem
                    {
                        Label = "Cost Type",
                        Value = stageDetails.UsageType.Value
                    });
                    metadataItems.Add(new MetadataItem
                    {
                        Label = "Usage/Buyout/Contract Type",
                        Value = stageDetails.UsageBuyoutType.Value
                    });
                    break;
                case Constants.CostType.Trafficking:
                    break;
                default:
                    break;
            }
        }
    }
}
