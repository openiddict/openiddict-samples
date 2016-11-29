import { FormGroup, FormControl } from '@angular/forms';
import { Injectable } from '@angular/core';

Injectable();
export class FormValidationService {
    getValidatorErrorMessage(validatorName: string, validatorValue?: any) {
        let config: any = {
            'required'                : 'Required',
            'invalidCreditCard'       : 'Is invalid credit card number',
            'invalidEmailAddress'     : 'Invalid email address',
            'invalidPassword'         : 'Invalid password. Password must be at least 6 characters long, and contain a number.',
            'minlength'               : `Minimum length ${validatorValue.requiredLength}`,
            'invalidCompare'          : 'Passwords must match'
        };

        return config[validatorName];
    }

    passwordComparisonValidator(group: FormGroup) {
        let password = group.controls['password'] as FormControl;
        let confirmPassword = group.controls['confirmPassword'] as FormControl;

        if (password.dirty && confirmPassword.dirty) {
            if (password.value === confirmPassword.value) {
                return null;
            }else {
                return { invalidCompare: true };
            }
        }else {
            return { invalidCompare: true };
        }
    }

    creditCardValidator(control: FormControl) {
        // Visa, MasterCard, American Express, Diners Club, Discover, JCB
        if (control.value.match(/^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$/)) {
            return null;
        } else {
            return { invalidCreditCard: true };
        }
    }

    emailValidator(control: FormControl) {
        if (control.value.match(/\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*/)) {
            return null;
        } else {
            return { invalidEmailAddress: true };
        }
    }

    passwordValidator(control: FormControl) {
        // {6,100}           - Assert password is between 6 and 100 characters
        // (?=.*[0-9])       - Assert a string has at least one number
        if (control.value.match(/^(?=.*[0-9])[a-zA-Z0-9!@#$%^&*]{6,100}$/)) {
            return null;
        } else {
            return { invalidPassword: true };
        }
    }
}
