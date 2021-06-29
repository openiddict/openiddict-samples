import {
  connectRouter,
  RouterState as ConnectedRouterState,
} from "connected-react-router/immutable";
import { History } from "history";
import { UserManager } from "oidc-client";
import { Action as ReduxAction, combineReducers, Reducer } from "redux";

import {
  adminReducer,
  AdminState,
  getInitialAdminState,
} from "./admin/adminReducer";
import {
  getInitialTestState,
  testReducer,
  TestState,
} from "./test/testReducer";
import { createUserReducer, UserState } from "./user/userReducer";

export interface Action<TAction = any, TPayload = any>
  extends ReduxAction<TAction> {
  type: TAction;
  payload: TPayload;
}

export interface AppState {
  router: ConnectedRouterState<any>;
  user: UserState;
  admin: AdminState;
  test: TestState;
}

export function getInitialAppState(): AppState {
  return {
    router: (undefined as any) as ConnectedRouterState<any>,
    user: (undefined as any) as UserState,
    admin: getInitialAdminState(),
    test: getInitialTestState(),
  } as AppState;
}

export function createAppReducer(
  history: History,
  oidcUserManager: UserManager
): Reducer<AppState, any> {
  var reducers = {
    router: connectRouter<any>(history),
    user: createUserReducer(oidcUserManager),
    admin: adminReducer,
    test: testReducer,
  };

  return combineReducers<AppState>(reducers);
}
