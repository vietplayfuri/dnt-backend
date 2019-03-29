namespace dnt.api.Controllers.Roles
{
    using System;
    using System.Threading.Tasks;
    using dnt.core.Models.Role;
    using dnt.core.Services;
    using dnt.core.Services.Role;
    using Microsoft.AspNetCore.Mvc;

    [Route("v1/[controller]")]
    public class RolesController : CostsControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService, IPermissionService permissionService)
            : base(permissionService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetSystemRoles()
        {
            var roles = await _roleService.Get();
            return Ok(roles);
        }

        //[HttpPost("save")]
        //public async Task<IActionResult> Create([FromBody]RoleModel insertData)
        //{
        //    //Check user's permission to see if user can create another user or not            
        //    //_permissionService.CheckHasAccess

        //    var user = await _userService.Get(insertData.Username);
        //    var result = new UserModel();
        //    if (user == null)
        //        result = await _userService.Create(insertData);
        //    else
        //        result = await _userService.Update(insertData);

        //    return Ok(result);
        //}
    }
}
