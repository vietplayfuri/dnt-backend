

namespace costs.net.plugins.PG.Services.PostProcessing
{
    using System.Threading.Tasks;
    using core.Models.ActivityLog;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.PostProcessing;
    using dataAccess.Entity;
    using Models;
    using Newtonsoft.Json;

    public class PgCustomDataObjectIoNumberAdded : IActionPostProcessor
    {
        private readonly IActivityLogService _activityLogService;

        public PgCustomDataObjectIoNumberAdded(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public bool CanProcess(PostProcessingAction action)
        {
            return action == PostProcessingAction.CustomObjectDataSaved;
        }

        public async Task Process(UserIdentity user, dynamic data)
        {
            if (data == null)
            {
                return;
            }
            if (!(data is CustomObjectData))
            {
                return;
            }

            var cob = (CustomObjectData)data;
            if (string.IsNullOrEmpty(cob.Data))
            {
                return;
            }
            var paymentDetails = JsonConvert.DeserializeObject<PgPaymentDetails>(cob.Data);
            if (paymentDetails?.IoNumber != null)
            {
                await _activityLogService.Log(new IoNumberAdded(paymentDetails.IoNumber, user));
            }
        }
    }
}
