import { NgModule, Optional, SkipSelf } from '@angular/core';
import { SuperAdminAuthGuard } from './guards/super-admin-auth-guard.service';
import { AuthenticatedAuthGuard } from './guards/authenticated-auth-guard.service';
import { Title } from '@angular/platform-browser';
import { LocalStorageBackend, Storage, StorageBackend } from './storage';
import { AlertService } from './alert/alert.service';
import { ProfileService } from './profile/profile.service';
import { HttpExceptionService } from './services/http-exceptions.service';
import { FormValidationService } from './services/form-validation.service';
import { AuthGuard } from './guards/auth-guard.service';
import { ExternalAuthService } from './auth-token/external-auth.service';
import { AuthTokenService } from './auth-token/auth-token.service';
import { AccountService } from './account/account.service';
import { LoadingBarService } from './loading-bar/loading-bar.service';
import { AuthHttp } from './auth-http/auth-http.service';
import { LoggedInActions } from './auth-store/logged-in.actions';
import { ProfileActions } from './profile/profile.actions';
import { AuthTokenActions } from './auth-token/auth-token.actions';
import { AlertActions } from './alert/alert.actions';
import { LoadingBarActions } from './loading-bar/loading-bar.actions';
import { AuthReadyActions } from './auth-store/auth-ready.actions';


@NgModule({
    providers: [
        LoadingBarService,
        AlertService,
        ProfileService,
        SuperAdminAuthGuard,
        AuthenticatedAuthGuard,
        Title,
        HttpExceptionService,
        FormValidationService,
        AuthGuard,
        AuthTokenService,
        AccountService,
        ExternalAuthService,
        AuthHttp,

        LoggedInActions,
        ProfileActions,
        AuthTokenActions,
        AlertActions,
        LoadingBarActions,
        AuthReadyActions,

        { provide: StorageBackend, useClass: LocalStorageBackend },
        Storage,
    ]

})
export class CoreModule {
    constructor (@Optional() @SkipSelf() parentModule: CoreModule) {
        if (parentModule) {
            throw new Error(
                'CoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
