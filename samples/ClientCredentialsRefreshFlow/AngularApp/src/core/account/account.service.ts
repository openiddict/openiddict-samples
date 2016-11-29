import { Injectable } from '@angular/core';
import { RegisterModel } from '../models/register-model';
import { Observable } from 'rxjs';
import { Response, Http } from '@angular/http';
import { LoadingBarService } from '../loading-bar/loading-bar.service';
import { HttpExceptionService } from '../services/http-exceptions.service';
import { LoginModel } from '../models/login-model';
import { ChangePasswordModel } from '../models/change-password';
import { ResetPasswordModel } from '../models/reser-password.model';
import { ExternalRegistrationModel } from '../models/external-registration-model';
import { ExternalLoginModel } from '../models/external-login-model';
import { AuthTokenService } from '../auth-token/auth-token.service';
import { Store } from '@ngrx/store';
import { AppState } from '../../app/app-store';
import { LoggedInActions } from '../auth-store/logged-in.actions';
import { AuthTokenActions } from '../auth-token/auth-token.actions';
import { ProfileActions } from '../profile/profile.actions';
import { AuthHttp } from '../auth-http/auth-http.service';

@Injectable()
export class AccountService {

    constructor(private loadingBar: LoadingBarService,
                private http: Http,
                private httpExceptions: HttpExceptionService,
                private authHttp: AuthHttp,
                private authTokens: AuthTokenService,
                private store: Store<AppState>,
                private loggedInAction: LoggedInActions,
                private authTokenActions: AuthTokenActions,
                private profileActions: ProfileActions
    ) { }


    register(data: RegisterModel): Observable<Response> {
        return this.http.post('api/account/create', data)
            .catch( this.httpExceptions.handleError );
    }

    externalRegister(model: ExternalRegistrationModel) {
        return this.http.post('/api/account/registerexternal', model);
    }

    externalLogin(model: ExternalLoginModel) {
        return this.authTokens.getTokens(model, 'urn:ietf:params:oauth:grant-type:external_identity_token')
            .do(() => this.authTokens.scheduleRefresh());
    }

    login(user: LoginModel)  {
        return this.authTokens.getTokens(user, 'password')
            .do(res => this.authTokens.scheduleRefresh() );
    }

// TODO: give this a model
    sendForgotPassword(data: any) {
        return this.authHttp.post('/account/SendForgotPassword', data);
    }

    changePassword(data: ChangePasswordModel) {
        return this.authHttp.post('/account/changePassword', data);
    }

    resetPassword(data: ResetPasswordModel) {
        return this.authHttp.post('/account/resetPassword', data );

    }

    logout() {
        this.authTokens.deleteTokens();
        this.authTokens.unsubscribeRefresh();

        this.store.dispatch(this.loggedInAction.NotLoggedIn());
        this.store.dispatch(this.authTokenActions.Delete());
        this.store.dispatch(this.profileActions.Delete());
    }

}
