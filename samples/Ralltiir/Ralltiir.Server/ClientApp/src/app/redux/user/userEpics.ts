import axios from 'axios';
import { push } from 'connected-react-router/immutable';
import { from, Observable, of } from 'rxjs';
import { catchError, filter, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import _ from 'underscore';

import * as config from '../../environments/config';
import { ProfileModel } from '../../models/profileModel';
import { TokenResponse } from '../../models/tokenResponse';
import { Action, AppState } from '../rootReducer';
import {
    AuthorizeAction, createAuthorizeAction, createAuthorizeErrorAction,
    createAuthorizeSuccessAction, createLoginAction, createLoginErrorAction,
    createLoginSuccessAction, createLogoutErrorAction, createLogoutSuccessAction,
    createRegisterUserErrorAction, createRegisterUserSuccessAction, LoginAction, RegisterUserAction,
    UserActionTypes
} from './userActions';

const registerUserEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.REGISTER_USER),
    map((action) => action as RegisterUserAction),
    switchMap((action) =>
      from(
        axios.post(`api/Account/register`, action.payload, {
          baseURL: config.authUrl,
        })
      ).pipe(
        switchMap(() => [
          createRegisterUserSuccessAction(),
          createLoginAction(action.payload.email, action.payload.password),
        ]),
        catchError((err) =>
          err.response != undefined
            ? of(
                createRegisterUserErrorAction(
                  _.map(err.response.data[""], (e) => ({
                    error_description: e,
                  }))
                )
              )
            : of(createRegisterUserErrorAction(err))
        )
      )
    )
  );

const loginEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.LOGIN),
    map((action) => action as LoginAction),
    switchMap((action) => {
      return from(
        axios.post(`connect/login`, action.payload, {
          baseURL: config.authUrl,
          withCredentials: true,
        })
      ).pipe(
        switchMap(() => [
          createLoginSuccessAction({} as TokenResponse, {} as ProfileModel),
          createAuthorizeAction(),
        ]),
        catchError((err) =>
          err.response != undefined
            ? of(createLoginErrorAction(err.response.data))
            : of(createLoginErrorAction(err))
        )
      );
    })
  );

const authorizeEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.AUTHORIZE),
    map((action) => action as AuthorizeAction),
    withLatestFrom(state$),
    switchMap(([action, state]) => {
      const userManager = state.user.get("userManager");
      const args = {
        extraQueryParams:
          action.payload === undefined ? undefined : { accept: action.payload },
      };

      return from(userManager.signinSilent(args)).pipe(
        tap((response) => {
          axios.defaults.headers.common[
            "Authorization"
          ] = `${response.token_type} ${response.access_token}`;
        }),
        switchMap((response) => [createAuthorizeSuccessAction(response)]),
        catchError((err) => {
          let result: Action[] = [createAuthorizeErrorAction(err)];

          if (err.error === "consent_required") {
            result.push(push("/authorize"));
          }

          return of(...result);
        })
      );
    })
  );

const logoutEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.LOGOUT),
    withLatestFrom(state$),
    switchMap(([action, state]) => {
      const userManager = state.user.get("userManager");

      return from(userManager.signoutRedirect()).pipe(
        tap(() => {
          axios.defaults.headers.common["Authorization"] = undefined;
        }),
        switchMap(() => [createLogoutSuccessAction()]),
        catchError((err) => of(createLogoutErrorAction(err)))
      );
    })
  );

export const userEpics = [
  registerUserEpic,
  loginEpic,
  authorizeEpic,
  logoutEpic,
];
