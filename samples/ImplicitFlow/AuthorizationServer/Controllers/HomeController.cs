using Microsoft.AspNetCore.Mvc;

namespace AuthorizationServer.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return View();
        }
        public IActionResult Error() {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
