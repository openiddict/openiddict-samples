import { Action } from '@ngrx/store';
import { type } from '../../util/action-name-helper';
import { Alert } from '../models/alert.model';
import { Injectable } from '@angular/core';

export const AlertActionTypes = {
    ADD: type('[Alert] Add'),
    DELETE: type('[Alert] Delete')
};

@Injectable()
export class AlertActions {
    Add(payload: Alert): Action {
        return {
            type: AlertActionTypes.ADD,
            payload
        };
    }
    Delete(payload: Alert): Action {
        return {
            type: AlertActionTypes.DELETE,
            payload
        };
    }
}
