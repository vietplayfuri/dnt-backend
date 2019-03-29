namespace dnt.api.Controllers.Users
{
    using System.Threading.Tasks;
    using dnt.api.Model;
    using dnt.core.Models.User;
    using dnt.core.Services;
    using dnt.core.Services.User;
    using Extensions;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("v1/[controller]")]
    public class UsersController : CostsControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public UsersController(IUserService userService,
            IPermissionService permissionService)
            : base(permissionService)
        {
            _userService = userService;
            _permissionService = permissionService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]UserAuthenticationModel user)
        {
            var canLogin = await _userService.IsCorrectPassword(user.Username, user.Password);
            if (canLogin)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            var userdata = await _userService.Get(user.Username);
            var result = userdata.GenerateToken();
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Search()
        {
            var users = await _userService.Get();
            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long userId)
        {
            var user = await _userService.Get(userId);
            return Ok(user);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Create([FromBody]UserInsertModel insertData)
        {
            //Check user's permission to see if user can create another user or not            
            //_permissionService.CheckHasAccess

            var user = await _userService.Get(insertData.Username);
            var result = new UserModel();
            if (user == null)
                result = await _userService.Create(insertData);
            else
                result = await _userService.Update(insertData);

            return Ok(result);
        }
    }
}
