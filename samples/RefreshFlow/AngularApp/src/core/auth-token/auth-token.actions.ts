import { Action } from '@ngrx/store';
import { type } from '../../util/action-name-helper';
import { AuthTokenModel } from '../models/auth-tokens.model';
import { Injectable } from '@angular/core';

export const AuthTokenActionTypes = {
    LOAD: type('[AuthToken] Load'),
    DELETE: type('[AuthToken] Delete')
};

@Injectable()
export class AuthTokenActions {
    delete(): Action {
        return {
            type: AuthTokenActionTypes.LOAD
        };
    }
    load(payload: AuthTokenModel): Action {
        return {
            type: AuthTokenActionTypes.LOAD,
            payload
        };
    }
}
