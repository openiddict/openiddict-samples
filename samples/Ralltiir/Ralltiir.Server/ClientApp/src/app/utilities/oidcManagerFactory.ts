import axios from 'axios';
import Oidc, { UserManager, UserManagerSettings } from 'oidc-client';
import { Store } from 'redux';

import { createAuthorizeSuccessAction, createLogoutAction } from '../redux/user/userActions';

export class OidcUserManager {
  private _userManager: UserManager;

  constructor(settings: UserManagerSettings) {
    settings = {
      ...settings,
      iframeNavigator: new AxiosNavigator(),
      redirectNavigator: new AxiosNavigator(),
    } as UserManagerSettings;

    this._userManager = new UserManager(settings);
    this._userManager.clearStaleState();
  }

  get userManager(): UserManager {
    return this._userManager;
  }

  public registerOidcEvents(store: Store): void {
    this._userManager.getUser().then((user) => {
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
  }
}

export class AxiosNavigator {
  prepare() {
    const worker = new AxiosNavigatorWorker();
    return Promise.resolve(worker);
  }

  callback(url: string) {
    return Promise.resolve();
  }
}

export interface AxiosNavigatorWorkerParams {
  url: string;
  silentRequestTimeout: number | undefined;
}

export class AxiosNavigatorWorker {
  navigate(params: AxiosNavigatorWorkerParams) {
    return new Promise((res, rej) => {
      if (!params || !params.url) {
        rej("No url provided");
      } else {
        const urlParts = params.url.split("?");
        const url = urlParts[0];
        const query = urlParts[1] || "";

        const processResponse = (response: any): any => {
          if (response?.request?.responseURL === undefined) return response;

          const responseData = queryParamsToObject(
            response.request.responseURL
          );
          responseData["url"] = response.request.responseURL;

          const isSuccess = responseData?.error === undefined;

          if (isSuccess) {
            res(responseData);
          } else {
            rej(responseData);
          }

          return responseData;
        };

        axios
          .post(url, query, {
            headers: {
              "Content-Type": "application/x-www-form-urlencoded",
            },
            withCredentials:
              axios.defaults.headers.common["Authorization"] == undefined,
          })
          .then(processResponse, processResponse);
      }
    });
  }
}

function queryParamsToObject(path: string) {
  const queryString = path.split("?")[1] || "";
  const params = new URLSearchParams(queryString);

  return Object.fromEntries(params.entries());
}
