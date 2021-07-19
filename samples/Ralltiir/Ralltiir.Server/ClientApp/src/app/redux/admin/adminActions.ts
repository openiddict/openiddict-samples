import { GetRolesResponse } from "../../models/getRolesResponse";
import { GetUsersResponse } from "../../models/getUsersResponse";
import { Action } from "../rootReducer";

export enum AdminActionTypes {
  GET_USERS = "[ADMIN] GET_USERS",
  GET_USERS_SUCCESS = "[ADMIN] GET_USERS_SUCCESS",
  GET_USERS_ERROR = "[ADMIN] GET_USERS_ERROR",
  GET_ROLES = "[ADMIN] GET_ROLES",
  GET_ROLES_SUCCESS = "[ADMIN] GET_ROLES_SUCCESS",
  GET_ROLES_ERROR = "[ADMIN] GET_ROLES_ERROR",
  GRANT_ROLE = "[ADMIN] GRANT_ROLE",
  GRANT_ROLE_SUCCESS = "[ADMIN] GRANT_ROLE_SUCCESS",
  GRANT_ROLE_ERROR = "[ADMIN] GRANT_ROLE_ERROR",
  REVOKE_ROLE = "[ADMIN] REVOKE_ROLE",
  REVOKE_ROLE_SUCCESS = "[ADMIN] REVOKE_ROLE_SUCCESS",
  REVOKE_ROLE_ERROR = "[ADMIN] REVOKE_ROLE_ERROR",
}

export interface GetUsersAction extends Action<AdminActionTypes.GET_USERS> {}
export interface GetUsersSuccessAction
  extends Action<AdminActionTypes.GET_USERS_SUCCESS, GetUsersResponse[]> {}
export interface GetUsersErrorAction
  extends Action<AdminActionTypes.GET_USERS_ERROR, any> {}

export interface GetRolesAction extends Action<AdminActionTypes.GET_ROLES> {}
export interface GetRolesSuccessAction
  extends Action<AdminActionTypes.GET_ROLES_SUCCESS, GetRolesResponse[]> {}
export interface GetRolesErrorAction
  extends Action<AdminActionTypes.GET_ROLES_ERROR, any> {}

export interface GrantRoleAction
  extends Action<
    AdminActionTypes.GRANT_ROLE,
    { userId: string; roleName: string }
  > {}
export interface GrantRoleSuccessAction
  extends Action<
    AdminActionTypes.GRANT_ROLE_SUCCESS,
    { userId: string; roleName: string }
  > {}
export interface GrantRoleErrorAction
  extends Action<AdminActionTypes.GRANT_ROLE_ERROR, any> {}

export interface RevokeRoleAction
  extends Action<
    AdminActionTypes.REVOKE_ROLE,
    { userId: string; roleName: string }
  > {}
export interface RevokeRoleSuccessAction
  extends Action<
    AdminActionTypes.REVOKE_ROLE_SUCCESS,
    { userId: string; roleName: string }
  > {}
export interface RevokeRoleErrorAction
  extends Action<AdminActionTypes.REVOKE_ROLE_ERROR, any> {}

export const createGetUsersAction = (): GetUsersAction => {
  return {
    type: AdminActionTypes.GET_USERS,
  } as GetUsersAction;
};

export const createGetUsersSuccessAction = (
  users: GetUsersResponse[]
): GetUsersSuccessAction => {
  return {
    type: AdminActionTypes.GET_USERS_SUCCESS,
    payload: users,
  } as GetUsersSuccessAction;
};

export const createGetUsersErrorAction = (error: any): GetUsersErrorAction => {
  return {
    type: AdminActionTypes.GET_USERS_ERROR,
    payload: error,
  } as GetUsersErrorAction;
};

export const createGetRolesAction = (): GetRolesAction => {
  return {
    type: AdminActionTypes.GET_ROLES,
  } as GetRolesAction;
};

export const createGetRolesSuccessAction = (
  roles: GetRolesResponse[]
): GetRolesSuccessAction => {
  return {
    type: AdminActionTypes.GET_ROLES_SUCCESS,
    payload: roles,
  } as GetRolesSuccessAction;
};

export const createGetRolesErrorAction = (error: any): GetRolesErrorAction => {
  return {
    type: AdminActionTypes.GET_ROLES_ERROR,
    payload: error,
  } as GetRolesErrorAction;
};

export const createGrantRoleAction = (
  userId: string,
  roleName: string
): GrantRoleAction => {
  return {
    type: AdminActionTypes.GRANT_ROLE,
    payload: { userId, roleName },
  } as GrantRoleAction;
};

export const createGrantRoleSuccessAction = (
  userId: string,
  roleName: string
): GrantRoleSuccessAction => {
  return {
    type: AdminActionTypes.GRANT_ROLE_SUCCESS,
    payload: { userId, roleName },
  } as GrantRoleSuccessAction;
};

export const createGrantRoleErrorAction = (
  error: any
): GrantRoleErrorAction => {
  return {
    type: AdminActionTypes.GRANT_ROLE_ERROR,
    payload: error,
  } as GrantRoleErrorAction;
};

export const createRevokeRoleAction = (
  userId: string,
  roleName: string
): RevokeRoleAction => {
  return {
    type: AdminActionTypes.REVOKE_ROLE,
    payload: { userId, roleName },
  } as RevokeRoleAction;
};

export const createRevokeRoleSuccessAction = (
  userId: string,
  roleName: string
): RevokeRoleSuccessAction => {
  return {
    type: AdminActionTypes.REVOKE_ROLE_SUCCESS,
    payload: { userId, roleName },
  } as RevokeRoleSuccessAction;
};

export const createRevokeRoleErrorAction = (
  error: any
): RevokeRoleErrorAction => {
  return {
    type: AdminActionTypes.REVOKE_ROLE_ERROR,
    payload: error,
  } as RevokeRoleErrorAction;
};
