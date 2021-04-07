import { Action as ReduxAction, combineReducers, Reducer } from 'redux';

import { adminReducer, AdminState, getInitialAdminState } from './admin/adminReducer';
import { getInitialTestState, testReducer, TestState } from './test/testReducer';
import { getInitialUserState, userReducer, UserState } from './user/userReducer';

export interface Action<TAction = any, TPayload = any>
  extends ReduxAction<TAction> {
  type: TAction
  payload: TPayload
}

export interface AppState {
  user: UserState
  admin: AdminState
  test: TestState
}

export function getInitialAppState(): AppState {
  return {
    user: getInitialUserState(),
    admin: getInitialAdminState(),
    test: getInitialTestState(),
  } as AppState
}

export function createAppReducer(): Reducer<AppState, any> {
  var reducers = {
    user: userReducer,
    admin: adminReducer,
    test: testReducer,
  }

  return combineReducers<AppState>(reducers)
}
