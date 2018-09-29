using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppWithFetchClient.Pages
{
    // disable client-side and server-side caching
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
