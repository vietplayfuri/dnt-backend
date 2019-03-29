namespace costs.net.messaging.Handlers
{
    using System.Threading.Tasks;
    using core.Messaging;
    using core.Models.AMQ;
    using core.Services.User;
    using Newtonsoft.Json;
    using Serilog;

    public class A5UserLoginHandler : IMessageHandler<UserLoginEvent>
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;

        public A5UserLoginHandler(ILogger logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task Handle(UserLoginEvent message)
        {
            /* Ignore this function temporary because of https://jira.adstream.com/browse/ADC-2823 */
            //if (message.action.Type == "logged" &&
            //    message.@object.sessionData != null &&
            //    message.@object.sessionData.buLabel == "pgSSO")
            //{
            //    _logger.Information($"Received userUpdate event for userId : {message.@object.userInfo._id["$oid"]}, message: {JsonConvert.SerializeObject(message)}");
            //    await _userService.UpdateSessionData(message.@object.sessionData, message.@object.userInfo._id["$oid"].ToString());
            //}
        }
    }
}
