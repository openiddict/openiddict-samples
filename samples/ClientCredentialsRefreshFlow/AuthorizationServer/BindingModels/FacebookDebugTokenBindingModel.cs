using System.Collections.Generic;

namespace AuthorizationServer.BindingModels
{
    public class FacebookDebugTokenBindingModel
    {
        public FacebookDebugTokenData Data { get; set; }
    }

    public class FacebookDebugTokenData
    {
        public string App_Id { get; set; }
        public string Application { get; set; }
        public string Expires_At { get; set; }
        public bool Is_Valid { get; set; }
        public List<string> Scopes { get; set; }
        public string User_Id { get; set; }
    }
}
