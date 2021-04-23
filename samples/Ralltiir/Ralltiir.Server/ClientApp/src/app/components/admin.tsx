import React, { useEffect } from 'react';
import { Button } from 'react-bootstrap';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect } from 'react-router-dom';
import _ from 'underscore';

import {
    createGetRolesAction, createGetUsersAction, createGrantRoleAction, createRevokeRoleAction
} from '../redux/admin/adminActions';
import { AppState } from '../redux/rootReducer';

function Admin() {
  const dispatch = useDispatch()

  useEffect((): void => {
    dispatch(createGetRolesAction())
    dispatch(createGetUsersAction())
  }, [])

  const userState = useSelector((state: AppState) => state.user)
  const roles = userState.get('roles')
  const isAdmin = _.contains(roles, 'admin')

  const adminState = useSelector((state: AppState) => state.admin)
  const users = adminState.get('users')
  const availableRoles = adminState.get('roles')

  const revokeRole = (userId: string, roleId: string) => {
    dispatch(createRevokeRoleAction(userId, roleId))
  }

  const grantRole = (userId: string, roleId: string) => {
    dispatch(createGrantRoleAction(userId, roleId))
  }

  var content = !isAdmin
    ? _.map(users, (user) => (
        <tr key={user.id}>
          <td>{user.id}</td>
          <td>{user.username}</td>
          <td>{user.roles.join(',')}</td>
        </tr>
      ))
    : _.map(users, (user) =>
        _.map(availableRoles, (role, i) => (
          <tr key={`${user.id}-${role.id}`}>
            <td>{i == 0 ? user.id : ''}</td>
            <td>{i == 0 ? user.username : ''}</td>
            <td>{role.name}</td>
            {_.contains(user.roles, role.name) ? (
              <td>
                <Button
                  className="w-100"
                  variant="danger"
                  onClick={() => revokeRole(user.id, role.name)}
                >
                  Revoke
                </Button>
              </td>
            ) : (
              <td>
                <Button
                  className="w-100"
                  variant="success"
                  onClick={() => grantRole(user.id, role.name)}
                >
                  Grant
                </Button>
              </td>
            )}
          </tr>
        ))
      )

  return userState.get('tokens') == undefined ? (
    <Redirect to="/" />
  ) : (
    <div className="admin">
      <table>
        <thead>
          <tr>
            <th>Id</th>
            <th>Username</th>
            <th>Roles</th>
            {isAdmin && <th></th>}
          </tr>
        </thead>
        <tbody>{content}</tbody>
      </table>
    </div>
  )
}

export default Admin
