import { Injectable } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { Response, Headers, RequestOptions, Http } from '@angular/http';
import { AppState } from '../../app/app-store';
import { Store } from '@ngrx/store';
import { ProfileModel } from '../models/profile-model';
import { LoginModel } from '../models/login-model';
import { Storage } from '../storage';
import { JwtHelper } from 'angular2-jwt';
import { AuthTokenModel } from '../models/auth-tokens.model';
import { LoggedInActions } from '../auth-store/logged-in.actions';
import { AuthTokenActions } from './auth-token.actions';
import { AuthReadyActions } from '../auth-store/auth-ready.actions';
import { ProfileActions } from '../profile/profile.actions';

@Injectable()
export class AuthTokenService {
    refreshSubscription$: Subscription;
    jwtHelper: JwtHelper = new JwtHelper();

    constructor(private storage: Storage,
                private http: Http,
                private store: Store<AppState>,
                private loggedInActions: LoggedInActions,
                private authTokenActions: AuthTokenActions,
                private authReadActions: AuthReadyActions,
                private profileActions: ProfileActions
    ) { }


    getTokens(data: LoginModel | RefreshGrant, grantType: string): Observable<void> {
        // data can be any since it can either be a refresh tokens or login details
        // The request for tokens must be x-www-form-urlencoded IE: parameter string, it cant be json
        let headers = new Headers({ 'Content-Type': 'application/x-www-form-urlencoded'});
        let options = new RequestOptions({ headers: headers });

        Object.assign(data, {
            grant_type: grantType,
            scope: ['openid offline_access']
        });

        return this.http.post('http://localhost:5000/connect/token', this.encodeObjectToParams(data) , options)
            .map( res => res.json())
            .map( (tokens: AuthTokenModel) => {
                this.store.dispatch(this.authTokenActions.load(tokens));
                this.store.dispatch(this.loggedInActions.loggedIn());

                let profile = this.jwtHelper.decodeToken(tokens.id_token) as ProfileModel;
                this.store.dispatch(this.profileActions.Load(profile));

                this.storage.setItem('auth-tokens', tokens);
            })
            .do( _ => this.store.dispatch(this.authReadActions.ready()))
          //  .catch( error => this.httpExceptions.handleTokenBadRequest(error));

    }

    deleteTokens() {
        this.storage.removeItem('auth-tokens');
        this.store.dispatch(this.authTokenActions.delete());
    }

    unsubscribeRefresh() {
        if (this.refreshSubscription$) {
            this.refreshSubscription$.unsubscribe();
        }
    }

    refreshTokens(): Observable<Response> {
        return this.store.map( state => state.auth.authTokens.refresh_token)
            .first()
            .flatMap( refreshToken => {
                return this.getTokens(
                    { refresh_token: refreshToken } as RefreshGrant, 'refresh_token')
                    .catch( error => Observable.throw('Session Expired'));
            });
    }

    startupTokenRefresh() {
        return this.storage.getItem('auth-tokens')
            .flatMap( (tokens: AuthTokenModel) => {
                // check if the token is even if localStorage, if it isn't tell them it's not and return
                if (!tokens) {
                    this.store.dispatch(this.authReadActions.ready());
                    return Observable.throw('No token in Storage');
                }
                // parse the token into a model and throw it into the store
                this.store.dispatch(this.authTokenActions.load(tokens));

                if (!this.jwtHelper.isTokenExpired(tokens.id_token)) {
                    // grab the profile out so we can store it
                    let profile = this.jwtHelper.decodeToken(tokens.id_token) as ProfileModel;
                    this.store.dispatch(this.profileActions.Load(profile));

                    // we can let the app know that we're good to go ahead of time
                    this.store.dispatch(this.loggedInActions.loggedIn());
                    this.store.dispatch(this.authReadActions.ready());
                }

                // it if is able to refresh then the getTokens method will let the app know that we're auth ready
                return this.refreshTokens();
            })
            .catch(error => {
                this.store.dispatch(this.loggedInActions.notLoggedIn());
                this.store.dispatch(this.authReadActions.ready());
                return Observable.throw(error);
            });
    }

    scheduleRefresh(): void {
        let source = this.store.select( state => state.auth.authTokens)
            .take(1)
            .flatMap((tokens: AuthTokenModel) => {
                let delay = tokens.expires_in / 2 * 1000;
                console.log(delay);
                return Observable.interval(delay);
            });

        this.refreshSubscription$ = source.subscribe(() => {
            console.log('refresh fired');
            this.refreshTokens()
                .subscribe( );
        });
    }

    private encodeObjectToParams(obj: any): string {
        return Object.keys(obj)
            .map(key => encodeURIComponent(key) + '=' + encodeURIComponent(obj[key]))
            .join('&');
    }

}
export interface RefreshGrant {
    refresh_token: string;
}
