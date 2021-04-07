import React from 'react';
import { Nav, Navbar } from 'react-bootstrap';
import { useDispatch, useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import _ from 'underscore';

import { AppState } from '../redux/rootReducer';
import { createLogoutAction } from '../redux/user/userActions';

export function Header() {
  const dispatch = useDispatch()

  const usersState = useSelector((state: AppState) => state.user)
  const tokens = usersState.get('tokens')
  const roles = usersState.get('roles')

  var logout = () => {
    dispatch(createLogoutAction())
  }

  return (
    <div className="header">
      <Navbar className="bg-dark" variant="dark" expand="sm" fixed="top">
        <Navbar.Brand as={Link} to="/">
          Your application (Ralltiir.Client)
        </Navbar.Brand>
        <Nav className="mr-auto"></Nav>
        <Nav>
          {_.intersection(roles, ['admin', 'moderator']).length > 0 && (
            <Nav.Link as={Link} to="/admin">
              Admin
            </Nav.Link>
          )}
          {tokens != null && <Nav.Link onClick={logout}>Logout</Nav.Link>}
          {tokens == null && (
            <>
              <Nav.Link as={Link} to="/register">
                Register
              </Nav.Link>
              <Nav.Link as={Link} to="/login">
                Login
              </Nav.Link>
            </>
          )}
        </Nav>
      </Navbar>
    </div>
  )
}

export default Header
