
namespace costs.net.plugins.PG.Services.PostProcessing
{
    using System.Threading.Tasks;
    using core.Services.PostProcessing;
    using core.Models.ActivityLog;
    using core.Models.User;
    using core.Services.ActivityLog;
    using dataAccess.Entity;
    using Form;
    using Newtonsoft.Json;

    public class PgCostCreatedAipeSelected : IActionPostProcessor
    {
        private readonly IActivityLogService _activityLogService;

        public PgCostCreatedAipeSelected(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public bool CanProcess(PostProcessingAction action)
        {
            return action == PostProcessingAction.CostCreated;
        }

        public async Task Process(UserIdentity user, dynamic data)
        {
            if (data == null)
            {
                return;
            }
            if (!(data is Cost))
            {
                return;
            }

            var cost = (Cost)data;

            if (cost.CostType != CostType.Production)
            {
                return;
            }
            if (cost.LatestCostStageRevision?.StageDetails?.Data == null)
            {
                return;
            }

            var stageForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(cost.LatestCostStageRevision.StageDetails.Data);

            if (stageForm.IsAIPE)
            {
                await _activityLogService.Log(new AipeSelected(cost.CostNumber, user));
            }
        }
    }
}
