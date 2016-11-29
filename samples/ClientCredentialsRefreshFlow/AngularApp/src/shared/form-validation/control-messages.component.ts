import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { FormValidationService } from '../../core/services/form-validation.service';

@Component({
    selector: 'app-control-messages',
    template: `<div *ngIf="errorMessage !== null">{{errorMessage}}</div>`
})
export class ControlMessagesComponent {
    @Input() control: FormControl;
    constructor( private validator: FormValidationService) { }

    get errorMessage() {
        for (let propertyName in this.control.errors) {
            if (this.control.errors.hasOwnProperty(propertyName) && this.control.touched) {
                return this.validator.getValidatorErrorMessage(propertyName, this.control.errors[propertyName]);
            }
        }

        return null;
    }
}
