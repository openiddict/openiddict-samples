import { Action } from '@ngrx/store';

import { AuthTokenModel } from '../models/auth-tokens.model';
import { AuthTokenActionTypes } from './auth-token.actions';

const initalState: AuthTokenModel = {
    id_token: null,
    access_token: null,
    refresh_token: null,
    expires_in: 0,
    token_type: null
};

export function authTokenReducer(state = initalState, action: Action): AuthTokenModel {
    switch (action.type) {
        case AuthTokenActionTypes.LOAD:
            return action.payload;

        case AuthTokenActionTypes.DELETE:
            return initalState;

        default:
            return state;
    }
}
