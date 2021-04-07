import axios, { AxiosResponse } from 'axios';
import { from, Observable, of } from 'rxjs';
import { catchError, filter, map, switchMap } from 'rxjs/operators';
import _ from 'underscore';

import * as config from '../../environments/config';
import { GetRolesResponse } from '../../models/getRolesResponse';
import { GetUsersResponse } from '../../models/getUsersResponse';
import { AppState } from '../rootReducer';
import {
    AdminActionTypes, createGetRolesErrorAction, createGetRolesSuccessAction,
    createGetUsersErrorAction, createGetUsersSuccessAction, createGrantRoleErrorAction,
    createGrantRoleSuccessAction, createRevokeRoleErrorAction, createRevokeRoleSuccessAction,
    GrantRoleAction, RevokeRoleAction
} from './adminActions';

const getUsersEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === AdminActionTypes.GET_USERS),
    // map(action => action as GetUsersAction),
    // withLatestFrom(state$),
    switchMap(() => {
      return from(
        axios.get<GetUsersResponse[]>(`api/Admin/users`, {
          baseURL: config.authUrl,
        })
      ).pipe(
        map((response: AxiosResponse<GetUsersResponse[]>) => {
          return response.data as GetUsersResponse[]
        }),
        map((response) => createGetUsersSuccessAction(response)),
        catchError((err) => of(createGetUsersErrorAction(err.response.data)))
      )
    })
  )

const getRolesEpic = (action$: Observable<any>, state$: Observable<AppState>) =>
  action$.pipe(
    filter((action) => action.type === AdminActionTypes.GET_ROLES),
    switchMap(() => {
      return from(
        axios.get<GetRolesResponse[]>(`api/Admin/roles`, {
          baseURL: config.authUrl,
        })
      ).pipe(
        map((response: AxiosResponse<GetRolesResponse[]>) => {
          return response.data as GetRolesResponse[]
        }),
        map((response) => createGetRolesSuccessAction(response)),
        catchError((err) => of(createGetRolesErrorAction(err.response.data)))
      )
    })
  )

const grantRoleEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === AdminActionTypes.GRANT_ROLE),
    map((action) => action as GrantRoleAction),
    switchMap((action) => {
      return from(
        axios.post(
          `api/Admin/users/${action.payload.userId}/roles/${action.payload.roleName}`,
          null,
          {
            baseURL: config.authUrl,
          }
        )
      ).pipe(
        map((response) =>
          createGrantRoleSuccessAction(
            action.payload.userId,
            action.payload.roleName
          )
        ),
        catchError((err) => of(createGrantRoleErrorAction(err.response.data)))
      )
    })
  )

const revokeRoleEpic = (
  action$: Observable<any>,
  state$: Observable<AppState>
) =>
  action$.pipe(
    filter((action) => action.type === AdminActionTypes.REVOKE_ROLE),
    map((action) => action as RevokeRoleAction),
    switchMap((action) => {
      return from(
        axios.post(
          `api/Admin/users/${action.payload.userId}/roles/${action.payload.roleName}`,
          null,
          {
            baseURL: config.authUrl,
          }
        )
      ).pipe(
        map((response) =>
          createRevokeRoleSuccessAction(
            action.payload.userId,
            action.payload.roleName
          )
        ),
        catchError((err) => of(createRevokeRoleErrorAction(err.response.data)))
      )
    })
  )

export const adminEpics = [
  getUsersEpic,
  getRolesEpic,
  grantRoleEpic,
  revokeRoleEpic,
]
