import axios, { AxiosResponse } from 'axios';
import jwtDecode from 'jwt-decode';
import { from, Observable, of } from 'rxjs';
import { catchError, filter, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import _ from 'underscore';

import * as config from '../../environments/config';
import { ProfileModel } from '../../models/profileModel';
import { TokenResponse } from '../../models/tokenResponse';
import { AppState } from '../rootReducer';
import {
    createLoginAction, createLoginErrorAction, createLoginSuccessAction, createLogoutErrorAction,
    createLogoutSuccessAction, createRegisterUserErrorAction, createRegisterUserSuccessAction,
    LoginAction, RegisterUserAction, UserActionTypes
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

          // const cookies = new Cookies()
          // console.log('cookies', cookies.get('.AspNetCore.Identity.Application'));

          axios.defaults.headers.common[
            'Authorization'
          ] = `Bearer ${tokens.access_token}`

          const profile: ProfileModel = jwtDecode(tokens.id_token)

          return { tokens, profile }
        }),
        switchMap((response) => [
          createLoginSuccessAction(response.tokens, response.profile),
          // createTestAuthAction(),
          // createParentDispatchAction(push(response.redirectUrl.substring(response.redirectUrl.indexOf('/'))))
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

export const userEpics = [registerUserEpic, loginEpic, logoutEpic]
