import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ProfileService } from '../../core/profile/profile.service';
import { FormValidationService } from '../../core/services/form-validation.service';
import { AccountService } from '../../core/account/account.service';
import { AlertService } from '../../core/alert/alert.service';

@Component({
    selector: 'app-account',
    templateUrl: 'account.component.html'
})
export class AccountComponent implements OnInit {

    resetPasswordForm: FormGroup;
    errors: string[];

    constructor(private profile: ProfileService,
                private formBuilder: FormBuilder,
                private formValidator: FormValidationService,
                private account: AccountService,
                private alert: AlertService
    ) { }

    ngOnInit() {
        this.resetPasswordForm = this.formBuilder.group({
            oldPassword: ['', [Validators.required, this.formValidator.passwordValidator]],
            passwords: this.formBuilder.group({
                password: ['', [Validators.required, this.formValidator.passwordValidator]],
                confirmPassword: ['', [Validators.required, this.formValidator.passwordValidator]]
            }, {validator: this.formValidator.passwordComparisonValidator})
        });
    }

    submitChangePassword() {
        let password = Object.assign({}, this.resetPasswordForm.value, this.resetPasswordForm.controls['passwords'].value);
        delete password['passwords'];

        this.account.changePassword(password)
            .subscribe(
                () => this.alert.sendSuccess('Password successfully sent'),
                errors => {
                    this.alert.sendWarning(errors[0]);
                    console.log(errors);
                }
            );
    }
}
