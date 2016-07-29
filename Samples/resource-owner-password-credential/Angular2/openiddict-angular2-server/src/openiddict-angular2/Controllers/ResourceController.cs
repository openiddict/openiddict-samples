using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NgOidc.Models;
using NgOidc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace NgOidc.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        public ResourceController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Route("api/Resource"), HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            return Ok(user);
        }
    }
}