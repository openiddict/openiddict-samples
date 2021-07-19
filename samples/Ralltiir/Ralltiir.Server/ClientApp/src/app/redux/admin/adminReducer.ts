import { Record } from "immutable";
import { Reducer } from "redux";
import _ from "underscore";

import { GetRolesResponse } from "../../models/getRolesResponse";
import { GetUsersResponse } from "../../models/getUsersResponse";
import { Action } from "../rootReducer";
import { AdminActionTypes, GrantRoleSuccessAction } from "./adminActions";

export interface BaseAdminState {
  loading: number;
  error: any;
  users: GetUsersResponse[];
  roles: GetRolesResponse[];
}
export type AdminState = Record<BaseAdminState>;

export function getInitialAdminState(): AdminState {
  const result = {
    loading: 0,
    error: {},
    users: [],
    roles: [],
  } as BaseAdminState;

  return Record(result)();
}

export const adminReducer: Reducer<AdminState, Action> = (
  state: AdminState | undefined,
  action: Action
) => {
  if (state === undefined) {
    state = getInitialAdminState();
  }

  switch (action.type) {
    case AdminActionTypes.GET_USERS:
    case AdminActionTypes.GET_ROLES:
    case AdminActionTypes.GRANT_ROLE:
    case AdminActionTypes.REVOKE_ROLE:
      return state.merge({
        loading: state.get("loading") + 1,
        error: {},
      });
    case AdminActionTypes.GET_USERS_SUCCESS:
      return state.merge({
        loading: state.get("loading") - 1,
        users: action.payload,
      });
    case AdminActionTypes.GET_ROLES_SUCCESS:
      return state.merge({
        loading: state.get("loading") - 1,
        roles: action.payload,
      });
    case AdminActionTypes.GRANT_ROLE_SUCCESS: {
      const grantAction = action as GrantRoleSuccessAction;

      const users = state.get("users");
      const affectedUser = _.find(
        users,
        (u) => u.id === grantAction.payload.userId
      );

      if (affectedUser !== undefined) {
        affectedUser.roles = _.unique([
          ...affectedUser.roles,
          grantAction.payload.roleName,
        ]);
      }

      return state.merge({
        loading: state.get("loading") - 1,
        users: users,
      });
    }
    case AdminActionTypes.REVOKE_ROLE_SUCCESS: {
      const revokeAction = action as GrantRoleSuccessAction;

      const users = state.get("users");
      const affectedUser = _.find(
        users,
        (u) => u.id === revokeAction.payload.userId
      );

      if (affectedUser !== undefined) {
        affectedUser.roles = _.without(
          affectedUser.roles,
          revokeAction.payload.roleName
        );
      }

      return state.merge({
        loading: state.get("loading") - 1,
        users: users,
      });
    }
    case AdminActionTypes.GET_USERS_ERROR:
    case AdminActionTypes.GET_ROLES_ERROR:
    case AdminActionTypes.GRANT_ROLE_ERROR:
    case AdminActionTypes.REVOKE_ROLE_ERROR:
      return state.merge({
        loading: state.get("loading") - 1,
        error: action.payload,
      });

    default:
      return state;
  }
};
