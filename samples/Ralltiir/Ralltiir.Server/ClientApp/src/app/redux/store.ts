import { routerMiddleware } from "connected-react-router/immutable";
import { History } from "history";
import { UserManagerSettings } from "oidc-client";
import { applyMiddleware, createStore, Store } from "redux";
import { composeWithDevTools } from "redux-devtools-extension";
import { createEpicMiddleware } from "redux-observable-es6-compat";

import {
  createOidcUserManager,
  registerOidcEvents,
} from "../utilities/oidcManagerFactory";
import { rootEpic } from "./rootEpic";
import { AppState, createAppReducer, getInitialAppState } from "./rootReducer";

export const configureStore = (
  history: History,
  oidcSettings: UserManagerSettings
): Store<AppState, any> => {
  const r = routerMiddleware(history);
  const epicMiddleware = createEpicMiddleware();

  const middlewares = [r, epicMiddleware];
  const middlewareEnhancer = applyMiddleware(...middlewares);

  const enhancers = [middlewareEnhancer];
  const composedEnhancers = composeWithDevTools(...enhancers);

  const oidcManager = createOidcUserManager(oidcSettings);

  const store = createStore(
    createAppReducer(history, oidcManager),
    getInitialAppState(),
    composedEnhancers
  );

  registerOidcEvents(oidcManager, store);

  epicMiddleware.run(rootEpic);

  return store;
};

export default configureStore;
