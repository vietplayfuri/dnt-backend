namespace dnt.api.Controllers.Roles
{
    using System;
    using System.Threading.Tasks;
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
        [Route("system")]
        public async Task<IActionResult> GetSystemRoles()
        {
            return new OkObjectResult(null);
        }

        [HttpGet]
        [Route("business")]
        public async Task<IActionResult> GetBusinessRoles()
        {
            return new OkObjectResult(null);
        }

        [HttpGet]
        [Route("business/{userId}")]
        public async Task<IActionResult> GetBusinessRoles(Guid userId)
        {
            return new OkObjectResult(null);
        }
    }
}
