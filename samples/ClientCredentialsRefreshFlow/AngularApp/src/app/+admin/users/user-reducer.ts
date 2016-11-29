import { Action } from '@ngrx/store';
import { User } from '../models/user';

export function usersReducer(state: User[] = [], action: Action): User[] {
    switch (action.type) {
        case 'GET_USERS':
            return action.payload;

        default:
        return state;
    }
}
