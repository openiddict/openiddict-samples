import environment from "./environment";
import { OpenIdConnectConfiguration } from "aurelia-open-id-connect";
import { Log, UserManagerSettings, WebStorageStateStore } from "oidc-client";

const oidcConfig: OpenIdConnectConfiguration = {
    loginRedirectRoute: "/home",
    logoutRedirectRoute: "/home",
    unauthorizedRedirectRoute: "/home",
    logLevel: Log.INFO,
    userManagerSettings: <UserManagerSettings>{
        // number of seconds in advance of access token expiry
        // to raise the access token expiring event
        accessTokenExpiringNotificationTime: 3585,
        authority: environment.urls.authority,
        automaticSilentRenew: true,
        // interval in milliseconds to check the user's session
        checkSessionInterval: 10000,
        client_id: "aurelia",
        filterProtocolClaims: true,
        loadUserInfo: false,
        post_logout_redirect_uri: `${environment.urls.host}/signout-oidc`,
        redirect_uri: `${environment.urls.host}/signin-oidc`,
        response_type: "id_token token",
        scope: "openid email roles profile api1 api2",
        // number of millisecods to wait for the authorization
        // server to response to silent renew request
        silentRequestTimeout: 10000,
        silent_redirect_uri: `${environment.urls.host}/signin-oidc`
    },
};

export default oidcConfig;
