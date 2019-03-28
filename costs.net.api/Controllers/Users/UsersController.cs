namespace dnt.api.Controllers.Users
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using dnt.api.Model;
    using dnt.api.Security.Token;
    using dnt.core.Models.CostUser;
    using dnt.core.Models.User;
    using dnt.core.Services;
    using dnt.core.Services.User;
    using Extensions;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("v1/[controller]")]
    public class UsersController : CostsControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService,
            IPermissionService permissionService)
            : base(permissionService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Search()
        {
            return Ok(AccountInMemory.ArrayAccount.ToList());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id, UserViewMode? viewMode = null)
        {
            return null;
        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserAuthenticationModel userParam)
        {
            var user = AccountInMemory.ArrayAccount.FirstOrDefault(a => a.UserName == userParam.Username && a.Password == userParam.Password);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            user = HandleToken(user);
            return Ok(user);
        }

        private Account HandleToken(Account user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("mysite_supersecret_secretkey!8050");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // remove password before returning
            // user.Password = null;

            return user;
        }
    }
}
