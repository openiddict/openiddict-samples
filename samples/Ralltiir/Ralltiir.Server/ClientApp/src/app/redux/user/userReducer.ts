import { Record } from "immutable";
import { User, UserManager } from "oidc-client";
import { Reducer } from "redux";
import _ from "underscore";

import { ProfileModel } from "../../models/profileModel";
import { Action } from "../rootReducer";
import { UserActionTypes } from "./userActions";

export interface BaseUserState {
  userManager: UserManager;
  isLoading: boolean;
  loginError: any;
  registerError: any;
  tokens: User | null;
  profile: ProfileModel | null;
  name: string;
  roles: string[];
}
export type UserState = Record<BaseUserState>;

function getInitialUserState(oidcUserManager: UserManager): UserState {
  const result = {
    userManager: oidcUserManager,
    isLoading: false,
    loginError: null,
    registerError: null,
    tokens: null,
    profile: null,
    name: "",
    roles: [],
  } as BaseUserState;

  return Record(result)();
}

export function createUserReducer(
  oidcUserManager: UserManager
): Reducer<UserState, Action> {
  return (state: UserState | undefined, action: Action) => {
    if (state === undefined) {
      state = getInitialUserState(oidcUserManager);
    }

    switch (action.type) {
      case UserActionTypes.REGISTER_USER:
        return state.merge({
          isLoading: true,
          registerError: null,
        });
      case UserActionTypes.REGISTER_USER_SUCCESS:
        return state.merge({
          isLoading: false,
        });
      case UserActionTypes.REGISTER_USER_ERROR:
        return state.merge({
          isLoading: false,
          registerError: action.payload,
        });
      case UserActionTypes.LOGIN:
        return state.merge({
          isLoading: true,
          loginError: null,
        });
      case UserActionTypes.LOGIN_SUCCESS:
        return state.merge({
          isLoading: false,
        });
      case UserActionTypes.LOGIN_ERROR:
        return state.merge({
          isLoading: false,
          loginError: [action.payload],
        });
      case UserActionTypes.AUTHORIZE:
        return state.merge({
          isLoading: true,
          loginError: null,
        });
      case UserActionTypes.AUTHORIZE_SUCCESS:
        return state.merge({
          isLoading: false,
          loginError: null,
          tokens: action.payload,
          profile: action.payload.profile,
          name: action.payload.profile.name,
          roles: _.isArray(action.payload.profile.role)
            ? action.payload.profile.role
            : [action.payload.profile.role],
        });
      case UserActionTypes.AUTHORIZE_ERROR:
        return state.merge({
          isLoading: false,
          loginError: [action.payload],
        });
      case UserActionTypes.LOGOUT_SUCCESS:
        return state.merge({
          isLoading: false,
          loginError: null,
          tokens: null,
          profile: null,
          name: "",
          roles: [],
        });

      default:
        return state;
    }
  };
}
