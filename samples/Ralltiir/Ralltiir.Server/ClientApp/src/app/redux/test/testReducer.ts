import { Record } from "immutable";
import { Reducer } from "redux";
import _ from "underscore";

import { Action } from "../rootReducer";
import { UserActionTypes } from "../user/userActions";
import { TestActionTypes } from "./testActions";

export interface BaseTestState {
  isLoading: boolean;
  testAuthMessage: string;
  testModeratorMessage: string;
  testAdminMessage: string;
}
export type TestState = Record<BaseTestState>;

export function getInitialTestState(): TestState {
  const result = {
    isLoading: false,
    testAuthMessage: "",
    testModeratorMessage: "",
    testAdminMessage: "",
  } as BaseTestState;

  return Record(result)();
}

export const testReducer: Reducer<TestState, Action> = (
  state: TestState | undefined,
  action: Action
) => {
  if (state === undefined) {
    state = getInitialTestState();
  }

  switch (action.type) {
    case UserActionTypes.LOGIN:
    case UserActionTypes.LOGOUT:
      return state.merge({
        testAuthMessage: "",
        testModeratorMessage: "",
        testAdminMessage: "",
      });
    case TestActionTypes.TEST_AUTH:
      return state.merge({
        isLoading: true,
        testAuthMessage: "",
      });
    case TestActionTypes.TEST_AUTH_SUCCESS:
    case TestActionTypes.TEST_AUTH_ERROR:
      return state.merge({
        isLoading: false,
        testAuthMessage: action.payload,
      });
    case TestActionTypes.TEST_MODERATOR:
      return state.merge({
        isLoading: true,
        testModeratorMessage: "",
      });
    case TestActionTypes.TEST_MODERATOR_SUCCESS:
    case TestActionTypes.TEST_MODERATOR_ERROR:
      return state.merge({
        isLoading: false,
        testModeratorMessage: action.payload,
      });
    case TestActionTypes.TEST_ADMIN:
      return state.merge({
        isLoading: true,
        testAdminMessage: "",
      });
    case TestActionTypes.TEST_ADMIN_SUCCESS:
    case TestActionTypes.TEST_ADMIN_ERROR:
      return state.merge({
        isLoading: false,
        testAdminMessage: action.payload,
      });

    default:
      return state;
  }
};
