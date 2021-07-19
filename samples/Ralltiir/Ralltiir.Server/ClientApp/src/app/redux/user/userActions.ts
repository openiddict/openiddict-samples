import { User } from "oidc-client";

import { LoginRequest } from "../../models/loginRequest";
import { ProfileModel } from "../../models/profileModel";
import { RegistrationRequest } from "../../models/registrationRequest";
import { TokenResponse } from "../../models/tokenResponse";
import { Action } from "../rootReducer";

export enum UserActionTypes {
  REGISTER_USER = "[USER] REGISTER_USER",
  REGISTER_USER_SUCCESS = "[USER] REGISTER_USER_SUCCESS",
  REGISTER_USER_ERROR = "[USER] REGISTER_USER_ERROR",
  LOGIN = "[USER] LOGIN",
  LOGIN_SUCCESS = "[USER] LOGIN_SUCCESS",
  LOGIN_ERROR = "[USER] LOGIN_ERROR",
  AUTHORIZE = "[USER] AUTHORIZE",
  AUTHORIZE_SUCCESS = "[USER] AUTHORIZE_SUCCESS",
  AUTHORIZE_ERROR = "[USER] AUTHORIZE_ERROR",
  LOGOUT = "[USER] LOGOUT",
  LOGOUT_SUCCESS = "[USER] LOGOUT_SUCCESS",
  LOGOUT_ERROR = "[USER] LOGOUT_ERROR",
}

export interface RegisterUserAction
  extends Action<UserActionTypes.REGISTER_USER, RegistrationRequest> {}
export interface RegisterUserSuccessAction
  extends Action<UserActionTypes.REGISTER_USER_SUCCESS> {}
export interface RegisterUserErrorAction
  extends Action<UserActionTypes.REGISTER_USER_ERROR, any> {}
export interface LoginAction
  extends Action<UserActionTypes.LOGIN, LoginRequest> {}
export interface LoginSuccessAction
  extends Action<
    UserActionTypes.LOGIN_SUCCESS,
    { tokens: TokenResponse; profile: ProfileModel }
  > {}
export interface LoginErrorAction
  extends Action<UserActionTypes.LOGIN_ERROR, any> {}
export interface AuthorizeAction
  extends Action<UserActionTypes.AUTHORIZE, boolean | undefined> {}
export interface AuthorizeSuccessAction
  extends Action<UserActionTypes.AUTHORIZE_SUCCESS, User> {}
export interface AuthorizeErrorAction
  extends Action<UserActionTypes.AUTHORIZE_ERROR, any> {}
export interface LogoutAction extends Action<UserActionTypes.LOGOUT> {}
export interface LogoutSuccessAction
  extends Action<UserActionTypes.LOGOUT_SUCCESS> {}
export interface LogoutErrorAction
  extends Action<UserActionTypes.LOGOUT_ERROR, any> {}

export const createRegisterUserAction = (
  email: string,
  password: string,
  confirmPassword: string
): RegisterUserAction => {
  return {
    type: UserActionTypes.REGISTER_USER,
    payload: {
      email: email,
      password: password,
      confirmPassword: confirmPassword,
    } as RegistrationRequest,
  } as RegisterUserAction;
};

export const createRegisterUserSuccessAction = (): RegisterUserSuccessAction => {
  return {
    type: UserActionTypes.REGISTER_USER_SUCCESS,
  } as RegisterUserSuccessAction;
};

export const createRegisterUserErrorAction = (
  error: any
): RegisterUserErrorAction => {
  return {
    type: UserActionTypes.REGISTER_USER_ERROR,
    payload: error,
  } as RegisterUserErrorAction;
};

export const createLoginAction = (
  email: string,
  password: string
): LoginAction => {
  return {
    type: UserActionTypes.LOGIN,
    payload: {
      username: email,
      password: password,
    } as LoginRequest,
  } as LoginAction;
};

export const createLoginSuccessAction = (
  tokens: TokenResponse,
  profile: ProfileModel
): LoginSuccessAction => {
  return {
    type: UserActionTypes.LOGIN_SUCCESS,
    payload: { tokens, profile },
  } as LoginSuccessAction;
};

export const createLoginErrorAction = (error: any): LoginErrorAction => {
  return {
    type: UserActionTypes.LOGIN_ERROR,
    payload: error,
  } as LoginErrorAction;
};

export const createAuthorizeAction = (
  accept: boolean | undefined = undefined
): AuthorizeAction => {
  return {
    type: UserActionTypes.AUTHORIZE,
    payload: accept,
  } as AuthorizeAction;
};

export const createAuthorizeSuccessAction = (
  user: User
): AuthorizeSuccessAction => {
  return {
    type: UserActionTypes.AUTHORIZE_SUCCESS,
    payload: user,
  } as AuthorizeSuccessAction;
};

export const createAuthorizeErrorAction = (
  error: any
): AuthorizeErrorAction => {
  return {
    type: UserActionTypes.AUTHORIZE_ERROR,
    payload: error,
  } as AuthorizeErrorAction;
};

export const createLogoutAction = (): LogoutAction => {
  return {
    type: UserActionTypes.LOGOUT,
  } as LogoutAction;
};

export const createLogoutSuccessAction = (): LogoutSuccessAction => {
  return {
    type: UserActionTypes.LOGOUT_SUCCESS,
  } as LogoutSuccessAction;
};

export const createLogoutErrorAction = (error: any): LogoutErrorAction => {
  return {
    type: UserActionTypes.LOGOUT_ERROR,
    payload: error,
  } as LogoutErrorAction;
};
