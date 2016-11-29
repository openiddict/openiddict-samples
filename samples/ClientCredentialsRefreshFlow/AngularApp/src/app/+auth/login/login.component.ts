import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {FormValidationService} from '../../../core/services/form-validation.service';
import {AlertService} from '../../../core/alert/alert.service';
import { AccountService } from '../../../core/account/account.service';
import { ExternalAuthService } from '../../../core/auth-token/external-auth.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.template.html'
})
export class LoginComponent implements OnInit {
    loginForm: FormGroup;
    errors: string[];

    constructor(private formBuilder: FormBuilder,
                private account: AccountService,
                private formValidator: FormValidationService,
                private alert: AlertService,
                private externalAuth: ExternalAuthService
    ) { }


    ngOnInit(): void {
        this.loginForm = this.formBuilder.group({
            username: ['', [Validators.required, this.formValidator.emailValidator]],
            password: ['', [Validators.required, this.formValidator.passwordValidator]],
        });
        this.externalAuth.init();
    }

    onSubmit() {
        this.account.login(this.loginForm.value)
            .subscribe(
                () => this.alert.sendSuccess('Successfully logged in'),
                (error: string[]) => {
                    this.alert.sendWarning('Failed to login');
                    this.alert.sendInfo(error[0], 7000);
                    console.log(error);
                }
            );
    }

    facebookAuthorize() {
        this.externalAuth.login('Facebook')
        .subscribe( x => {
                this.alert.sendSuccess('Successfully registered');
            });
    }

    googleAuthorize() {
        this.externalAuth.login('Google')
            .subscribe( x => {
                this.alert.sendSuccess('Successfully registered');
            });
    }

}
