import axios from "axios";
import { UserManager, UserManagerSettings } from "oidc-client";
import { Store } from "redux";

import {
  createAuthorizeSuccessAction,
  createLogoutAction,
} from "../redux/user/userActions";

export const createOidcUserManager = (settings: UserManagerSettings) => {
  if (document.getElementById("") === undefined) {
    const frame = document.createElement("iframe");
    frame.setAttribute("id", "");
    frame.setAttribute("style", "display: 'none', position: 'absolute'");

    document.body.appendChild(frame);
  }

  var userManager = new UserManager(settings);
  userManager.clearStaleState();
  userManager.signinSilentCallback();

  return userManager;
};

export const registerOidcEvents = (userManager: UserManager, store: Store) => {
  userManager.getUser().then((user) => {
    if (user !== null) {
      if (user.expired) {
        store.dispatch(createLogoutAction());
        return;
      }

      store.dispatch(createAuthorizeSuccessAction(user));

      axios.defaults.headers.common[
        "Authorization"
      ] = `${user.token_type} ${user.access_token}`;
    }
  });
};
