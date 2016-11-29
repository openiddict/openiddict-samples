import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {FormValidationService} from '../../../core/services/form-validation.service';
import {AlertService} from '../../../core/alert/alert.service';
import { AccountService } from '../../../core/account/account.service';

@Component({
    selector: 'app-forgot-password',
    templateUrl: 'forgot-password.component.html'
})
export class ForgotPasswordComponent implements OnInit {
    forgotPasswordForm: FormGroup;

    constructor(private formBuilder: FormBuilder,
                private alert: AlertService,
                private account: AccountService,
                private formValidator: FormValidationService
    ) { }


    ngOnInit(): void {
        this.forgotPasswordForm = this.formBuilder.group({
            userName: ['', [Validators.required, this.formValidator.emailValidator]],
        });
    }

    onSubmit() {
        this.account.sendForgotPassword(this.forgotPasswordForm.value)
            .subscribe(
                () => this.alert.sendSuccess('Please check your email')

            );
    }
}
