using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using openiddict_angular2.Models;
using openiddict_angular2.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace openiddict_angular2.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        public ResourceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("api/Resource"), HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            /*if (user == null) return BadRequest("No user - not logged in");// if Authorize is not applied*/
            return Ok(user);
        }
    }
}