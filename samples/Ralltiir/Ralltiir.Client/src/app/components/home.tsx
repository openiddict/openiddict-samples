import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import _ from 'underscore';

import { AppState } from '../redux/rootReducer';
import {
    createTestAdminAction, createTestAuthAction, createTestModeratorAction
} from '../redux/test/testActions';

export function Home() {
  const dispatch = useDispatch()

  const testState = useSelector((state: AppState) => state.test)

  const usersState = useSelector((state: AppState) => state.user)
  const userName = usersState.get('name')
  const profile = usersState.get('profile')

  const profileKeys = _.without(_.keys(profile), 'name', 'exp', 'iat')
  var profileValues = _.map(profileKeys, (k) => {
    const val = (profile as any)[k]

    return _.isArray(val) ? val.join(',') : val
  })

  return (
    <div className="home">
      <div className="jumbotron text-center">
        <h1>Welcome, {userName != '' ? userName : 'anonymous'}</h1>

        <br />
        {_.map(profileKeys, (k, i) => (
          <div key={k}>
            {k}:&nbsp;<strong>{profileValues[i]}</strong>
          </div>
        ))}

        {userName == '' && (
          <Link to="/login">
            <button className="btn btn-primary">Sign in</button>
          </Link>
        )}
      </div>
      <div>
        <button
          className="btn btn-primary w-25 m-2"
          onClick={() => dispatch(createTestAuthAction())}
        >
          Test Authenticated
        </button>
        <span>{testState.get('testAuthMessage')}</span>
      </div>
      <div>
        <button
          className="btn btn-primary w-25 m-2"
          onClick={() => dispatch(createTestAdminAction())}
        >
          Test Admin Role
        </button>
        <span>{testState.get('testAdminMessage')}</span>
      </div>
      <div>
        <button
          className="btn btn-primary w-25 m-2"
          onClick={() => dispatch(createTestModeratorAction())}
        >
          Test Moderator Role
        </button>
        <span>{testState.get('testModeratorMessage')}</span>
      </div>
    </div>
  )
}

export default Home
