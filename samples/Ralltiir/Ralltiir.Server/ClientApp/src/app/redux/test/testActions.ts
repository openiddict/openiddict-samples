import { Action } from "../rootReducer";

export enum TestActionTypes {
  TEST_AUTH = "[TEST] TEST_AUTH",
  TEST_AUTH_SUCCESS = "[TEST] TEST_AUTH_SUCCESS",
  TEST_AUTH_ERROR = "[TEST] TEST_AUTH_ERROR",
  TEST_MODERATOR = "[TEST] TEST_MODERATOR",
  TEST_MODERATOR_SUCCESS = "[TEST] TEST_MODERATOR_SUCCESS",
  TEST_MODERATOR_ERROR = "[TEST] TEST_MODERATOR_ERROR",
  TEST_ADMIN = "[TEST] TEST_ADMIN",
  TEST_ADMIN_SUCCESS = "[TEST] TEST_ADMIN_SUCCESS",
  TEST_ADMIN_ERROR = "[TEST] TEST_ADMIN_ERROR",
}

export interface TestAuthAction extends Action<TestActionTypes.TEST_AUTH> {}
export interface TestAuthSuccessAction
  extends Action<TestActionTypes.TEST_AUTH_SUCCESS, string> {}
export interface TestAuthErrorAction
  extends Action<TestActionTypes.TEST_AUTH_ERROR, string> {}

export interface TestModeratorAction
  extends Action<TestActionTypes.TEST_MODERATOR> {}
export interface TestModeratorSuccessAction
  extends Action<TestActionTypes.TEST_MODERATOR_SUCCESS, string> {}
export interface TestModeratorErrorAction
  extends Action<TestActionTypes.TEST_MODERATOR_ERROR, string> {}

export interface TestAdminAction extends Action<TestActionTypes.TEST_ADMIN> {}
export interface TestAdminSuccessAction
  extends Action<TestActionTypes.TEST_ADMIN_SUCCESS, string> {}
export interface TestAdminErrorAction
  extends Action<TestActionTypes.TEST_ADMIN_ERROR, string> {}

export const createTestAuthAction = (): TestAuthAction => {
  return {
    type: TestActionTypes.TEST_AUTH,
  } as TestAuthAction;
};

export const createTestAuthSuccessAction = (
  message: string
): TestAuthSuccessAction => {
  return {
    type: TestActionTypes.TEST_AUTH_SUCCESS,
    payload: message,
  } as TestAuthSuccessAction;
};

export const createTestAuthErrorAction = (
  errorMessage: string
): TestAuthErrorAction => {
  return {
    type: TestActionTypes.TEST_AUTH_ERROR,
    payload: errorMessage,
  } as TestAuthErrorAction;
};

export const createTestModeratorAction = (): TestModeratorAction => {
  return {
    type: TestActionTypes.TEST_MODERATOR,
  } as TestModeratorAction;
};

export const createTestModeratorSuccessAction = (
  message: string
): TestModeratorSuccessAction => {
  return {
    type: TestActionTypes.TEST_MODERATOR_SUCCESS,
    payload: message,
  } as TestModeratorSuccessAction;
};

export const createTestModeratorErrorAction = (
  errorMessage: string
): TestModeratorErrorAction => {
  return {
    type: TestActionTypes.TEST_MODERATOR_ERROR,
    payload: errorMessage,
  } as TestModeratorErrorAction;
};

export const createTestAdminAction = (): TestAdminAction => {
  return {
    type: TestActionTypes.TEST_ADMIN,
  } as TestAdminAction;
};

export const createTestAdminSuccessAction = (
  message: string
): TestAdminSuccessAction => {
  return {
    type: TestActionTypes.TEST_ADMIN_SUCCESS,
    payload: message,
  } as TestAdminSuccessAction;
};

export const createTestAdminErrorAction = (
  errorMessage: string
): TestAdminErrorAction => {
  return {
    type: TestActionTypes.TEST_ADMIN_ERROR,
    payload: errorMessage,
  } as TestAdminErrorAction;
};
