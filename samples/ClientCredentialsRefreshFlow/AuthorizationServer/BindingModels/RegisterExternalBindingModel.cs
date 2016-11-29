
namespace AuthorizationServer.BindingModels
{
    public class RegisterExternalBindingModel
    {
        public ExternalAuthProviders Provider { get; set; }
        public string AccessToken { get; set; }
    }
}
