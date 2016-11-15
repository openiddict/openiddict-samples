import { OpenIdConnectConfiguration, UserManagerSettings, WebStorageStateStore } from "aurelia-open-id-connect";

let authority: string = "http://localhost:12345";
let host: string = "http://localhost:9000";

const oidcConfig: OpenIdConnectConfiguration = {
    loginRedirectModuleId: "home",
    logoutRedirectModuleId: "home",
    userManagerSettings: <UserManagerSettings> {
        // number of seconds in advance of access token expiry
        // to raise the access token expiring event
        accessTokenExpiringNotificationTime: "1",
        authority: authority,
        automaticSilentRenew: true,
        // interval in milliseconds to check the user's session
        checkSessionInterval: 10000,
        client_id: "Aurelia.OpenIdConnect",
        filterProtocolClaims: true,
        loadUserInfo: false,
        post_logout_redirect_uri: `${host}/signout-oidc`,
        redirect_uri: `${host}/signin-oidc`,
        response_type: "id_token token",
        scope: "openid email roles profile",
        // number of millisecods to wait for the authorization
        // server to response to silent renew request
        silentRequestTimeout: 10000,
        silent_redirect_uri: `${host}/signin-oidc`,
        userStore: new WebStorageStateStore("oidc", window.localStorage),
    },
};

export default oidcConfig;
