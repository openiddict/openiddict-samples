using System.ComponentModel.DataAnnotations;

namespace Ralltiir.Server.ViewModels
{
    public class AuthorizeViewModel
    {
        public string ApplicationName { get; set; }
        public string Scope { get; set; }
        public string Code { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string State { get; set; }
        public string Nonce { get; set; }
    }
}
