namespace costs.net.messaging.Handlers
{
    using System.Threading.Tasks;
    using core.ExternalResource.Gdam;
    using core.Messaging;
    using core.Models.AMQ;
    using core.Services.Agency;
    using core.Services.Brand;
    using core.Services.Project;
    using core.Services.User;
    using Serilog;

    public class A5MessageHandler : IMessageHandler<A5EventObject>
    {
        private readonly IAgencyService _agencyService;
        private readonly IBrandService _brandService;
        private readonly IGdamClient _gdamClient;
        private readonly ILogger _logger;

        private readonly IProjectService _projectService;
        private readonly IUserService _userService;


        public A5MessageHandler(ILogger logger,
            IGdamClient gdamClient,
            IAgencyService agencyService,
            IProjectService projectService,
            IUserService userService,
            IBrandService brandService)
        {
            _logger = logger;
            _gdamClient = gdamClient;
            _agencyService = agencyService;
            _projectService = projectService;
            _userService = userService;
            _brandService = brandService;
        }

        public async Task Handle(A5EventObject eventObject)
        {
            switch (eventObject.Action.Type)
            {
                case "businessUnitUpdated":
                case "businessUnitCreated":
                    _logger.Information($"Received {eventObject.Action.Type} event for agencyId : {eventObject.Object.Id}");
                    await HandleForBusinessUnitUpdate(eventObject);
                    break;
                case "userUpdated":
                case "userCreated":
                    _logger.Information($"Received {eventObject.Action.Type} event for userId : {eventObject.Object.Id}");
                    await HandleForUserUpdate(eventObject);
                    break;
                case "projectCreated":
                case "projectCloned":
                case "projectUpdated":
                    _logger.Information($"Received {eventObject.Action.Type} event for userId : {eventObject.Object.Id}");
                    await HandleForProjectUpdate(eventObject);
                    break;
                case "projectDeleted":
                    _logger.Information($"Received {eventObject.Action.Type} event for userId : {eventObject.Object.Id}");
                    await HandleProjectDeletion(eventObject);
                    break;
                case "schemaChanged":
                    _logger.Information($"Received {eventObject.Action.Type} event for agencyId : {eventObject.Id}");
                    await SyncBusinessUnitBrands(eventObject.Object.Schema.Agency);
                    break;
            }
        }

        private async Task SyncBusinessUnitBrands(string buId)
        {
            var a5Bu = await _gdamClient.FindAgencyById(buId);
            if (a5Bu == null)
            {
                _logger.Information($"Can not find business unit with Gdam Id: {buId}, from event actionType: schemaChanged");
                return;
            }

            await _brandService.SyncBusinessUnitBrands(a5Bu);
        }

        private async Task HandleForProjectUpdate(A5EventObject project)
        {
            var a5Project = await _gdamClient.FindProjectById(project.Object.Id);

            await _projectService.AddProjectToDb(a5Project);
        }

        private async Task HandleProjectDeletion(A5EventObject project)
        {
            await _projectService.DeleteProject(project.Object.Id, project.Subject.Id);
        }

        private async Task HandleForBusinessUnitUpdate(A5EventObject businessUnit)
        {
            var gdamAgency = await _gdamClient.FindAgencyById(businessUnit.Object.Id);
            await _agencyService.AddAgencyToDb(gdamAgency);
        }

        private async Task HandleForUserUpdate(A5EventObject user)
        {
            var gdamUser = await _gdamClient.FindUser(user.Object.Id);
            await _userService.AddUserToDb(gdamUser);
        }
    }
}
