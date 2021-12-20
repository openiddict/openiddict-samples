using Microsoft.AspNetCore.Mvc;

namespace Zirku.Server.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml");
    }
}
