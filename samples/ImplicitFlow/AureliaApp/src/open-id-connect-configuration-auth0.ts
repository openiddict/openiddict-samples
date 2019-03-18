import { OpenIdConnectConfiguration } from "aurelia-open-id-connect";
import { Log, UserManagerSettings, WebStorageStateStore } from "oidc-client";

const appHost = "http://localhost:9000";

export default {
  loginRedirectRoute: "private",
  logoutRedirectRoute: "index",
  unauthorizedRedirectRoute: "index",
  logLevel: Log.DEBUG,
  userManagerSettings: {
    // The number of seconds in advance of access token expiry
    // to raise the access token expiring event.
    accessTokenExpiringNotificationTime: 1,
    // Either host your own OpenID Provider or select a certified authority
    // from the list http://openid.net/certification/
    authority: "https://bigfont.auth0.com",
    automaticSilentRenew: false,
    // The interval in milliseconds between checking the user's session.
    checkSessionInterval: 10000,
    // The client or application ID that the authority issues.
    client_id: "VoHOI7uVmRSz5A0pDTnxRNNWZJU3nbY7",
    filterProtocolClaims: true,
    loadUserInfo: false,
    post_logout_redirect_uri: `${appHost}/signout-oidc`,
    redirect_uri: `${appHost}/signin-oidc`,
    response_type: "id_token",
    scope: "openid email profile",
    // number of millisecods to wait for the authorization
    // server to response to silent renew request
    silentRequestTimeout: 10000,
    silent_redirect_uri: `${appHost}/signin-oidc`,
    userStore: new WebStorageStateStore({
      prefix: "oidc",
      store: window.localStorage,
    }),
  } as UserManagerSettings,
} as OpenIdConnectConfiguration;
