import { Injectable } from '@angular/core';
import { Headers, Response, Http, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { AppState } from '../../app/app-store';
import { Store } from '@ngrx/store';
import { AuthState } from '../auth-store/auth.store';
import { LoadingBarService } from '../loading-bar/loading-bar.service';
import { HttpExceptionService } from '../services/http-exceptions.service';

@Injectable()
export class AuthHttp {
    private baseUrl: string = '/api';
    private globalHeaders = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };

    constructor (private http: Http,
            private store: Store<AppState>,
            private httpExceptions: HttpExceptionService,
            private loadingBar: LoadingBarService
    ) {}

    private getHeaders(): Observable<Headers> {
        return this.store.map(state => state.auth)
            .first((auth: AuthState) => auth.authReady)
            .map((auth: AuthState) => auth.authTokens.access_token)
            .map((accessToken: string) => new Headers(Object.assign({},
                this.globalHeaders,
                {
                    Authorization: 'Bearer ' + accessToken
                }
            )));
    }

    get(url: string): Observable<any> {
        return this.loadingBar.doWithLoader(
            this.getHeaders()
                .flatMap((headers: Headers) => {
                    let options = new RequestOptions({headers});
                    return this.http.get(this.baseUrl + url, options)
                        .map( this.checkForError)
                        .catch( error => Observable.throw(error))
                        .map(this.getJson);
                })
        );
    }

    post(url: string, data: any): Observable<any> {
        return this.loadingBar.doWithLoader(
            this.getHeaders()
                .flatMap((headers: Headers) => {
                    let options = new RequestOptions({headers});
                    return this.http.post(this.baseUrl + url, data, options)
                        .map( this.checkForError)
                        .catch( error => this.httpExceptions.handleError(error))
                        .map(this.getJson);
                })
        );
    }

    private getJson(res: Response) {
        // check to see if it's an empty response
        if (res.text() !== '') {
            return res.json();
        }

    }

    private checkForError(res: Response) {
        if (res.ok) {
            return res;
        }

        throw res;
    }
}
