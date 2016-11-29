import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoadingBarService } from '../../../core/loading-bar/loading-bar.service';
import { AppState } from '../../app-store';
import { AuthHttp } from '../../../core/auth-http/auth-http.service';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';

@Injectable()
export class UserService {
    path: string = '/users';

    constructor(private authHttp: AuthHttp,
                private loadingBar: LoadingBarService,
                private store: Store<AppState>
    ) {}

    getUsers(): Observable<Response> {
        return this.authHttp.get(this.path + '/getUsers')
            .do( users => this.store.dispatch({ type: 'GET_USERS', payload: users}) );
    }
}
