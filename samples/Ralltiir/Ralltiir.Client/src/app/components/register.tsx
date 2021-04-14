import React, { useState } from 'react';
import { Alert, Button, Col, Form } from 'react-bootstrap';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect, withRouter } from 'react-router-dom';
import _ from 'underscore';

import { AppState } from '../redux/rootReducer';
import { createRegisterUserAction } from '../redux/user/userActions';

function Register() {
  const dispatch = useDispatch()

  const usersState = useSelector((state: AppState) => state.user)
  const errors = usersState.get('registerError')

  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')
  const [passwordConfirm, setPasswordConfirm] = useState<string>('')

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
      case 'password_confirm':
        setPasswordConfirm(fieldVal)
        break
    }
  }

  const submit = () => {
    dispatch(createRegisterUserAction(email, password, passwordConfirm))
  }
  return usersState.get('tokens') != undefined ? (
    <Redirect to="/" />
  ) : (
    <div className="register">
      <h2>Register</h2>
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
          <Form.Group as={Col} controlId="formGridConfirmPass">
            <Form.Label>Confirm Password</Form.Label>
            <Form.Control
              type="password"
              name="password_confirm"
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
            <Button onClick={submit}>Submit</Button>
          </Form.Group>
        </Form.Row>
      </Form>
    </div>
  )
}

export default withRouter(Register)
