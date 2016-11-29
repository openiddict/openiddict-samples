import { AuthHttp } from '../../../core/auth-http/auth-http.service';
import { OnInit, Component } from '@angular/core';
import { AlertService } from '../../../core/alert/alert.service';
import { ProfileService } from '../../../core/profile/profile.service';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { LoadingBarService } from '../../../core/loading-bar/loading-bar.service';
import { Store } from '@ngrx/store';
import { AuthTokenService } from '../../../core/auth-token/auth-token.service';
import { AppState } from '../../app-store';

@Component({
    selector: 'app-verify',
    templateUrl: './verify.component.html'
})
export class VerifyComponent implements OnInit {
    constructor(private profile: ProfileService,
                private authHttp: AuthHttp,
                private alert: AlertService,
                private route: ActivatedRoute,
                private http: Http,
                private loadingBar: LoadingBarService,
                private tokens: AuthTokenService,
                private store: Store<AppState>
    ) {}
    ngOnInit(): void {

        if (!this.profile.isEmailConfirmed()) {
            let code = this.route.snapshot.queryParams['code'];
            let id = this.route.snapshot.queryParams['userId'];
            if (code && id) {
                this.confirmEmail(code, id);
            }else {
                this.sendConfirmationEmail();
            }
        }
    }

    confirmEmail(code: string, id: string): void {
        code = encodeURIComponent(code);

        // TODO: put this in a service
        this.http.get('api/account/ConfirmEmail?userId=' + id + '&code=' + code)
            .subscribe(
                (res) => {
                    this.tokens.refreshTokens()
                        .subscribe(
                        () => this.alert.sendSuccess('Your email has been confirmed'),
                        _ => this.alert.sendError('an error occured soz')
                    );
                },
                (res) => this.alert.sendError('an error occured soz')
        );
    }

    sendConfirmationEmail(): void {
        this.authHttp.get('api/account/SendConfirmEmail')
            .subscribe(
                () => this.alert.sendSuccess('A confirmation email has been send'),
                (error) => this.alert.sendError(error),
        );
    }

}
