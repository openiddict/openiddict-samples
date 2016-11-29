import { Action } from '@ngrx/store';

import { LoadingBarActionTypes } from './loading-bar.actions';

const initalState = false;

export function loadingBarReducer(state = initalState, action: Action): boolean {
    switch (action.type) {
        case LoadingBarActionTypes.START:
            return true;

        case LoadingBarActionTypes.DONE:
            return false;

        default:
            return state;
    }
}
