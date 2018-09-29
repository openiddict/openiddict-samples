/* global fetch, Oidc, window */

//
// Disclaimer:
//
// This is not production code.
// Its purpose is to demonstrate the OpenId Connect Implicit Flow with OpenIddict.
// The code lacks JavaScript modules, mobile browser support, logging, and error handling.
//

const { origin } = window.location;

//
// Define helper functions.
//
const writeInnerText = (id, message) => {
  window.document.getElementById(id).innerText = message;
};

const fetchUserResources = (accessToken) => {
  const url = `${origin}/api`;
  const options = {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  };

  fetch(url, options)
    .then((response) => {
      if (response.status !== 200) {
        throw new Error(response.statusText);
      }

      return response.json();
    })
    .then((obj) => {
      const json = JSON.stringify(obj, null, 4);
      writeInnerText('fetch_response', json);
    })
    .catch((err) => {
      writeInnerText('fetch_response', err);
    });
};

const displayUserData = (user) => {
  const json = JSON.stringify(user, null, 4);
  writeInnerText('signin_response', json);
};

const signinSilentError = (err) => {
  writeInnerText('signin_response', err);
};

//
// Configure the oidc-client.js UserManager.
//
const settings = {
  authority: origin,
  silent_redirect_uri: origin,
  client_id: 'my-client',
  response_type: 'id_token token',
  scope: 'openid',
  loadUserInfo: false,
  // A long timeout can be helpful during debugging.
  // silentRequestTimeout: 10000,
};

const userManager = new Oidc.UserManager(settings);

//
// Run the demo.
//
if (window !== window.parent) {
  // We are in an iframe, so only process the silent sign in callback.
  // Silent sign in always happens in child iframe.
  userManager.signinSilentCallback().then(() => {
    // Optional: communicate from the child iframe to its parent.
    // The userManager already does that, and we can optionally do more.
    window.parent.postMessage('signinSilentCallback -> complete', '*');
  });
} else {
  // Optional: receive message from the child iframe.
  window.addEventListener('message', (event) => {
    writeInnerText('iframe_message', event.data);
  });

  userManager.signinSilent()
    .then((user) => {
      displayUserData(user);
      fetchUserResources(user.access_token);
    })
    .catch(signinSilentError);
}
