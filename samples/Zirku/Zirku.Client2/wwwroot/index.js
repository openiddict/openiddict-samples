oidc.Log.setLogger(console);

const userManager = new oidc.UserManager({
    authority: 'https://localhost:44319',
    scope: 'openid offline_access api1',
    client_id: 'spa',
    redirect_uri: window.location.origin + '/signin-callback.html',
    response_type: 'code',
    userStore: new oidc.WebStorageStateStore({ store: window.localStorage }),
});

function login() {
    return userManager.signinRedirect({
        extraQueryParams: {
            identity_provider: 'Local',
            hardcoded_identity_id: '1',
        },
    });
}

function refreshToken() {
    return userManager.signinSilent();
}

