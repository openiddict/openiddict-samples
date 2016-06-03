using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using newoidc.Data;
using newoidc.Models;
using System.Threading.Tasks;

namespace newoidc.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
       private ApplicationDbContext _context;
       private UserManager<ApplicationUser> _userManager;

        public TestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("api/test"), HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok("No user - not logged in");// if Authorize is not applied
            return Ok(user);
        }

     
        
    }
}