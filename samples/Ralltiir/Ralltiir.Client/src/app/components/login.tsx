import React, { useState } from 'react';
import { Alert, Button, Col, Form } from 'react-bootstrap';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect, withRouter } from 'react-router-dom';
import _ from 'underscore';

import { AppState } from '../redux/rootReducer';
import { createLoginAction } from '../redux/user/userActions';

function Login() {
  const dispatch = useDispatch()

  const usersState = useSelector((state: AppState) => state.user)
  const errors = usersState.get('loginError')
  const isLoading = usersState.get('isLoading')

  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')

  const handleChange = (event: any) => {
    let fieldName = event.target.name
    let fieldVal = event.target.value

    switch (fieldName) {
      case 'email':
        setEmail(fieldVal)
        break
      case 'password':
        setPassword(fieldVal)
        break
    }
  }

  const submit = () => {
    dispatch(createLoginAction(email, password))
  }
  return usersState.get('tokens') != undefined ? (
    <Redirect to="/" />
  ) : (
    <div className="login">
      <h2>Login</h2>
      <br />
      {errors?.length > 0 && (
        <Alert variant="danger">
          An error occured
          <ul>
            {_.map(errors, (e, i) => (
              <li key={i}>{e.error_description}</li>
            ))}
          </ul>
        </Alert>
      )}
      <Form>
        <Form.Row>
          <Form.Group as={Col} controlId="formGridEmail">
            <Form.Label>Email</Form.Label>
            <Form.Control
              type="email"
              name="email"
              placeholder="Enter email"
              onChange={handleChange}
            />
          </Form.Group>
        </Form.Row>
        <Form.Row>
          <Form.Group as={Col} controlId="formGridPass">
            <Form.Label>Password</Form.Label>
            <Form.Control
              type="password"
              name="password"
              onChange={handleChange}
            />
          </Form.Group>
        </Form.Row>
        <Form.Row>
          <Form.Group
            as={Col}
            controlId="formGridSubmit"
            className="text-center"
          >
            <br />
            <Button onClick={submit} disabled={isLoading}>
              Login
            </Button>
          </Form.Group>
        </Form.Row>
      </Form>
    </div>
  )
}

export default withRouter(Login)
