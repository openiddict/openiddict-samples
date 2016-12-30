import { OpenIdConnectConfiguration, UserManagerSettings } from "aurelia-open-id-connect";

const oidcConfig: OpenIdConnectConfiguration = {
    LoginRedirectModuleId: "login",
    LogoutRedirectModuleId: "login",
    UserManagerSettings: <UserManagerSettings>{
        authority: "http://localhost:12345",
        client_id: "aurelia",
        post_logout_redirect_uri: "http://localhost:9000/signout-oidc",
        redirect_uri: "http://localhost:9000/signin-oidc",
        response_type: "id_token token",
        scope: "openid email roles profile",
        filterProtocolClaims: true,
        loadUserInfo: true,
    }
};

export default oidcConfig;
