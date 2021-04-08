import axios, { AxiosResponse } from 'axios';
import jwtDecode from 'jwt-decode';
import { from, interval, Observable, of } from 'rxjs';
import { catchError, filter, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import _ from 'underscore';

import * as config from '../../environments/config';
import { ProfileModel } from '../../models/profileModel';
import { TokenResponse } from '../../models/tokenResponse';
import { AppState } from '../rootReducer';
import {
    createLoginAction, createLoginErrorAction, createLoginSuccessAction, createLogoutErrorAction,
    createLogoutSuccessAction, createRefreshTokensAction, createRefreshTokensErrorAction,
    createRefreshTokensSuccessAction, createRegisterUserErrorAction,
    createRegisterUserSuccessAction, LoginAction, RegisterUserAction, UserActionTypes
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
        axios.post(`Account/register`, action.payload, {
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
                  _.map(err.response.data[''], (e) => ({
                    error_description: e,
                  }))
                )
              )
            : of(createRegisterUserErrorAction(err))
        )
      )
    )
  )

const loginEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.LOGIN),
    map((action) => action as LoginAction),
    withLatestFrom(state$),
    switchMap(([action, state]) => {
      const bodyData: any = Object.assign(
        {
          grant_type: 'password',
          scope: 'openid offline_access profile roles',
          client_id: 'ralltiir-client',
        },
        action.payload
      )

      const body = _.map(
        _.keys(bodyData),
        (key) => `${key}=${bodyData[key]}`
      ).join('&')

      return from(
        axios.post<TokenResponse>(`connect/token`, body, {
          baseURL: config.authUrl,
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
        })
      ).pipe(
        map((response: AxiosResponse<TokenResponse>) => {
          const tokens = response.data

          const now = new Date()
          tokens.expiration_date = new Date(
            now.getTime() + tokens.expires_in * 1000
          )
            .getTime()
            .toString()

          axios.defaults.headers.common[
            'Authorization'
          ] = `Bearer ${tokens.access_token}`

          const profile: ProfileModel = jwtDecode(tokens.id_token)

          return { tokens, profile }
        }),
        switchMap((response) => [
          createLoginSuccessAction(response.tokens, response.profile),
        ]),
        catchError((err) =>
          err.response != undefined
            ? of(createLoginErrorAction(err.response.data))
            : of(createLoginErrorAction(err))
        )
      )
    })
  )

const logoutEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.LOGOUT),
    tap(() => {
      axios.defaults.headers.common['Authorization'] = undefined
    }),
    switchMap(() => [createLogoutSuccessAction()]),
    catchError((err) => of(createLogoutErrorAction(err)))
  )

const scheduleRefreshTokensEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === UserActionTypes.LOGIN_SUCCESS),
    withLatestFrom(state$),
    switchMap(([action, state]) => interval(((state.user.get('tokens')?.expires_in || 0) / 2) * 1000)),
    withLatestFrom(state$),
    filter(([n, state]) => state.user.get('tokens') !==  null),
    switchMap(() => [createRefreshTokensAction()])
  )


const refreshTokensEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
action$.pipe(
  filter((action) => action.type === UserActionTypes.REFRESH_TOKENS),
  map((action) => action as LoginAction),
  withLatestFrom(state$),
  switchMap(([action, state]) => {
    const bodyData: any = Object.assign(
      {
        grant_type: 'refresh_token',
        scope: 'openid offline_access',
        client_id: 'ralltiir-client',
        refresh_token: state.user.get('tokens')?.refresh_token
      },
      action.payload
    )

    const body = _.map(
      _.keys(bodyData),
      (key) => `${key}=${bodyData[key]}`
    ).join('&')

    return from(
      axios.post<TokenResponse>(`connect/token`, body, {
        baseURL: config.authUrl,
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      })
    ).pipe(
      map((response: AxiosResponse<TokenResponse>) => {
        const tokens = response.data

        const now = new Date()
        tokens.expiration_date = new Date(
          now.getTime() + tokens.expires_in * 1000
        )
          .getTime()
          .toString()

        axios.defaults.headers.common[
          'Authorization'
        ] = `Bearer ${tokens.access_token}`

        return tokens
      }),
      switchMap((tokens) => [
        createRefreshTokensSuccessAction(tokens),
      ]),
      catchError((err) =>
        err.response != undefined
          ? of(createRefreshTokensErrorAction(err.response.data))
          : of(createRefreshTokensErrorAction(err))
      )
    )
  })
)

export const userEpics = [
  registerUserEpic,
  loginEpic,
  logoutEpic,
  refreshTokensEpic,
  scheduleRefreshTokensEpic
]
