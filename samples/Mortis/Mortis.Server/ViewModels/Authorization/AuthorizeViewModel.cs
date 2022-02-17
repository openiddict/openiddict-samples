using System.Collections.Generic;
using System.Web.Mvc;

namespace Mortis.Server.ViewModels.Authorization
{
    [Bind(Exclude = nameof(Parameters))]
    public class LogoutViewModel
    {
        public IEnumerable<KeyValuePair<string, string>> Parameters { get; internal set; }
    }
}
