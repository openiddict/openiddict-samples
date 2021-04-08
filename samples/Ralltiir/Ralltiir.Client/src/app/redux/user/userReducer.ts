import { Record } from 'immutable';
import { Reducer } from 'redux';
import _ from 'underscore';

import { ProfileModel } from '../../models/profileModel';
import { TokenResponse } from '../../models/tokenResponse';
import { Action } from '../rootReducer';
import { UserActionTypes } from './userActions';

export interface BaseUserState {
  isLoading: boolean
  loginError: any
  registerError: any
  tokens: TokenResponse | null
  profile: ProfileModel | null
  name: string
  roles: string[]
}
export type UserState = Record<BaseUserState>

export function getInitialUserState(): UserState {
  const result = {
    isLoading: false,
    loginError: null,
    registerError: null,
    tokens: null,
    profile: null,
    name: '',
    roles: [],
  } as BaseUserState

  return Record(result)()
}

export const userReducer: Reducer<UserState, Action> = (
  state: UserState | undefined,
  action: Action
) => {
  if (state === undefined) {
    state = getInitialUserState()
  }

  switch (action.type) {
    case UserActionTypes.REGISTER_USER:
      return state.merge({
        isLoading: true,
        registerError: null,
      })
    case UserActionTypes.REGISTER_USER_SUCCESS:
      return state.merge({
        isLoading: false
      })
    case UserActionTypes.REGISTER_USER_ERROR:
      return state.merge({
        isLoading: false,
        registerError: action.payload,
      })
    case UserActionTypes.LOGIN:
      return state.merge({
        isLoading: true,
        loginError: null,
      })
    case UserActionTypes.LOGIN_SUCCESS:
      return state.merge({
        isLoading: false,
        tokens: action.payload.tokens,
        profile: action.payload.profile,
        name: action.payload.profile.name,
        roles: _.isArray(action.payload.profile.role)
          ? action.payload.profile.role
          : [action.payload.profile.role],
      })
    case UserActionTypes.LOGIN_ERROR:
      return state.merge({
        isLoading: false,
        loginError: [action.payload],
      })
    case UserActionTypes.LOGOUT_SUCCESS:
      return state.merge({
        isLoading: false,
        tokens: null,
        profile: null,
        name: '',
        roles: [],
      })
      case UserActionTypes.REFRESH_TOKENS:
      return state.merge({
        isLoading: true,
        loginError: null,
      })
    case UserActionTypes.REFRESH_TOKENS_SUCCESS:
      return state.merge({
        isLoading: false,
        tokens: action.payload.tokens
      })
    case UserActionTypes.REFRESH_TOKENS_ERROR:
      return state.merge({
        isLoading: false,
        loginError: [action.payload],
      })

    default:
      return state
  }
}
