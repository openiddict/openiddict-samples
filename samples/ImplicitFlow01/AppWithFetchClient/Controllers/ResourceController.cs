using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation;

[Route("api")]
public class ResourceController : Controller
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationDefaults.AuthenticationScheme)]
    public ActionResult GetAsync()
    {
        return Json(new
        {
            foo = "foo",
            bar = "bar"
        });
    }

}