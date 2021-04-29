import React from 'react';
import { Button } from 'react-bootstrap';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect } from 'react-router-dom';
import _ from 'underscore';

import { AppState } from '../redux/rootReducer';
import { createAuthorizeAction, createLogoutAction } from '../redux/user/userActions';

function Authorize() {
  const dispatch = useDispatch();

  const usersState = useSelector((state: AppState) => state.user);
  const isLoading = usersState.get("isLoading");
  const userManager = usersState.get("userManager");
  const scopes = userManager.settings.scope;
  const profile = usersState.get("profile");

  const accept = () => {
    dispatch(createAuthorizeAction(true));
  };
  const deny = () => {
    dispatch(createLogoutAction());
  };
  return _.every(usersState.get("loginError"), e => e.error !== "consent_required") ? (
    <Redirect to="/" />
  ) : (
    <div className="authorize">
      <h2>Authorization</h2>
      <br />
      <p className="lead text-left">
        Do you want to grant <strong>{profile?.azp}</strong> access to your data?
        (scopes requested: {scopes})
      </p>

      <div className="text-center">
        <Button
          className="m-1"
          variant="success"
          onClick={accept}
          disabled={isLoading}
        >
          Yes
        </Button>
        <Button
          className="m-1"
          variant="danger"
          onClick={deny}
          disabled={isLoading}
        >
          No
        </Button>
      </div>
    </div>
  );
}

export default Authorize;
