import { OpenIdConnectConfiguration } from "aurelia-open-id-connect";
import { Log, UserManagerSettings, WebStorageStateStore } from "oidc-client";

import environment from './environment';

export default {
  loginRedirectRoute: "/private",
  logoutRedirectRoute: "/home",
  unauthorizedRedirectRoute: "/home",
  logLevel: Log.DEBUG,
  userManagerSettings: {
    // The number of seconds in advance of access token expiry
    // to raise the access token expiring event.
    accessTokenExpiringNotificationTime: 1,
    // Either host your own OpenID Provider or select a certified authority
    // from the list http://openid.net/certification/
    authority: environment.urls.authority,
    automaticSilentRenew: false,
    // The interval in milliseconds between checking the user's session.
    checkSessionInterval: 10000,
    // The client or application ID that the authority issues.
    client_id: "aurelia",
    filterProtocolClaims: true,
    loadUserInfo: false,
    post_logout_redirect_uri: `${environment.urls.host}/signout-oidc`,
    redirect_uri: `${environment.urls.host}/signin-oidc`,
    response_type: "id_token",
    scope: "openid email profile",
    // number of millisecods to wait for the authorization
    // server to response to silent renew request
    silentRequestTimeout: 10000,
    silent_redirect_uri: `${environment.urls.host}/signin-oidc`,
    userStore: new WebStorageStateStore({
      prefix: "oidc",
      store: window.localStorage,
    }),
  } as UserManagerSettings,
} as OpenIdConnectConfiguration;
