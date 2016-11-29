import { Action } from '@ngrx/store';
import { Alert } from '../../core/models/alert.model';
import { AlertActionTypes } from './alert.actions';

const initalState: Alert[] = [];

export function alertReducer(state = initalState, action: Action): Alert[] {
    switch (action.type) {
        case AlertActionTypes.ADD:
            return [
                ...state,
                action.payload
            ];

        case AlertActionTypes.DELETE:
            return state.filter( alert =>
                alert.message !== action.payload.message
            );

        default:
            return state;
    }
}
