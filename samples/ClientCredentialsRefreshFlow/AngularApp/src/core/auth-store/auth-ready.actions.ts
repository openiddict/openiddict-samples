import { type } from '../../util/action-name-helper';
import { Action } from '@ngrx/store';
import { Injectable } from '@angular/core';

export const AuthReadyActionTypes = {
    READY: type('[AuthReady] True')
};

@Injectable()
export class AuthReadyActions {
    Ready(): Action {
        return {
            type: AuthReadyActionTypes.READY
        };
    }
}
