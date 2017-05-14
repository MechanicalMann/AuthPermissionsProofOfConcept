using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Permissions.Models;

namespace Permissions.Controllers
{
    [Route("permissions/[controller]")]
    public class RolesController : Controller
    {
        private readonly IDictionary<string, IEnumerable<Role>> _appRoles = new Dictionary<string, IEnumerable<Role>> {
            { "Test", new List<Role> { new Role { Name = "Authorized Modifier", SystemName = "MOD", Description = "Permitted to modify data." } } }
        };

        private readonly IDictionary<string, Dictionary<string, List<Role>>> _roleAssignments = new Dictionary<string, Dictionary<string, List<Role>>>{
            { "test1", new Dictionary<string, List<Role>> { {"Test", new List<Role> { new Role { Name = "Authorized Modifier", SystemName = "MOD", Description = "Permitted to modify data." } } } } }
        };

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_appRoles);
        }

        [HttpGet("_current"), Authorize]
        public IActionResult GetCurrentRoles()
        {
            var username = Request.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            Dictionary<string, List<Role>> assignments;
            if (!_roleAssignments.TryGetValue(username, out assignments))
                return Unauthorized();
            return Ok(assignments);
        }
    }
}
