using System;
using OpenIddict;

namespace AuthorizationServer.Models {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : OpenIddictUser<Guid> { }
}
