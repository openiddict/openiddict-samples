import { Component, OnInit } from '@angular/core';
import {LoadingBarService} from '../../../core/loading-bar/loading-bar.service';
import {ActivatedRoute, Router} from '@angular/router';
import {AlertService} from '../../../core/alert/alert.service';
import {FormGroup, FormBuilder, Validators} from '@angular/forms';
import {FormValidationService} from '../../../core/services/form-validation.service';
import { AccountService } from '../../../core/account/account.service';

@Component({
    selector: 'app-reset-password',
    templateUrl: 'reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
    private id: string;
    private code: string;
    resetPasswordForm: FormGroup;

    constructor(private alert: AlertService,
                private route: ActivatedRoute,
                private loadingBar: LoadingBarService,
                private formBuilder: FormBuilder,
                private formValidator: FormValidationService,
                private account: AccountService,
                private router: Router
    ) {}


    ngOnInit() {

        let code = this.route.snapshot.queryParams['code'];
        let id = this.route.snapshot.queryParams['userId'];
        if (code && id) {
            this.id = id;
            this.code = code;
        }else {
            this.alert.sendError('Missing UserID and reset code');
        }

        this.resetPasswordForm = this.formBuilder.group({
                password: ['', [Validators.required, this.formValidator.passwordValidator]],
                confirmPassword: ['', [Validators.required, this.formValidator.passwordValidator]]
            }, {validator: this.formValidator.passwordComparisonValidator}
        );
    }


    onSubmit() {
        let data = Object.assign({}, this.resetPasswordForm.value, {userId: this.id, code: this.code});
        this.account.resetPassword(data).subscribe(
            () => {
                this.alert.sendSuccess('Successfully reset password');
                this.router.navigate(['auth/login']);
            },
            error => this.alert.sendError(error),
        );
    }
}
