import environment from "./environment";
import { OpenIdConnectConfiguration } from "aurelia-open-id-connect";
import { UserManagerSettings, WebStorageStateStore } from "oidc-client";

const oidcConfig: OpenIdConnectConfiguration = {
  loginRedirectRoute: "/home",
  logoutRedirectRoute: "/home",
  userManagerSettings: <UserManagerSettings>{
    // number of seconds in advance of access token expiry
    // to raise the access token expiring event
    accessTokenExpiringNotificationTime: 1,
    authority: environment.urls.authority,
    automaticSilentRenew: false, // true,
    // interval in milliseconds to check the user's session
    checkSessionInterval: 10000,
    client_id: "Aurelia.OpenIdConnect",
    filterProtocolClaims: true,
    loadUserInfo: false,
    post_logout_redirect_uri: `${environment.urls.host}/signout-oidc`,
    redirect_uri: `${environment.urls.host}/signin-oidc`,
    response_type: "id_token token",
    scope: "openid email roles profile",
    // number of millisecods to wait for the authorization
    // server to response to silent renew request
    silentRequestTimeout: 10000,
    silent_redirect_uri: `${environment.urls.host}/signin-oidc`,
    userStore: new WebStorageStateStore({
      prefix: "oidc",
      store: window.localStorage,
    }),
  },
} as OpenIdConnectConfiguration;

export default oidcConfig;
