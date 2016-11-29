import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppState } from '../../app/app-store';
import { Store } from '@ngrx/store';
import { Storage } from '../storage';

@Injectable()
export class ProfileService {
    constructor(private storage: Storage,
                private store: Store<AppState>
    ) { }

    isEmailConfirmed(): Observable<boolean> {
        // TODO: fix this sill serilization bug
        return this.store.select( state => state.auth.profile.email_confirmed)
            .map(emailConfirmed => emailConfirmed.toString() === 'True');

    }


    isInRole(pageRole: string): Observable<boolean> {
        return this.store.map( state => state.auth.profile.role)
            .map( (role: string[]) => {
                return role.indexOf(pageRole, 0) > -1;
            });
    }

}
