import axios, { AxiosResponse } from 'axios';
import { from, Observable, of } from 'rxjs';
import { catchError, filter, map, switchMap } from 'rxjs/operators';
import _ from 'underscore';

import * as config from '../../environments/config';
import { AppState } from '../rootReducer';
import {
    createTestAdminErrorAction, createTestAdminSuccessAction, createTestAuthErrorAction,
    createTestAuthSuccessAction, createTestModeratorErrorAction, createTestModeratorSuccessAction,
    TestActionTypes
} from './testActions';

const testAuthEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === TestActionTypes.TEST_AUTH),
    switchMap(() => {
      return from(
        axios.get<string>(`Resource/message`, {
          baseURL: config.authUrl,
        })
      ).pipe(
        map((response: AxiosResponse<string>) => {
          return response.data
        }),
        map((response) => createTestAuthSuccessAction(response)),
        catchError((err) =>
          of(
            createTestAuthErrorAction(
              err?.response == undefined
                ? 'An error occured'
                : `${err.response.status} ${err.response.statusText}`
            )
          )
        )
      )
    })
  )

const testModeratorEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === TestActionTypes.TEST_MODERATOR),
    switchMap(() => {
      return from(
        axios.get<string>(`Resource/message/moderator`, {
          baseURL: config.authUrl,
        })
      ).pipe(
        map((response: AxiosResponse<string>) => {
          return response.data
        }),
        map((response) => createTestModeratorSuccessAction(response)),
        catchError((err) =>
          of(
            createTestModeratorErrorAction(
              err?.response == undefined
                ? 'An error occured'
                : `${err.response.status} ${err.response.statusText}`
            )
          )
        )
      )
    })
  )

const testAdminEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === TestActionTypes.TEST_ADMIN),
    switchMap(() => {
      return from(
        axios.get<string>(`Resource/message/admin`, {
          baseURL: config.authUrl,
        })
      ).pipe(
        map((response: AxiosResponse<string>) => {
          return response.data
        }),
        map((response) => createTestAdminSuccessAction(response)),
        catchError((err) =>
          of(
            createTestAdminErrorAction(
              err?.response == undefined
                ? 'An error occured'
                : `${err.response.status} ${err.response.statusText}`
            )
          )
        )
      )
    })
  )

export const testEpics = [testAuthEpic, testModeratorEpic, testAdminEpic]
