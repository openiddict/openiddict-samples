import {Component, OnInit} from '@angular/core';
import { FormGroup, Validators,    FormBuilder }    from '@angular/forms';
import {Router} from '@angular/router';
import { AccountService } from '../../core/account/account.service';

@Component({
    selector: 'app-register',
    templateUrl: './register.template.html'
})
export class RegisterComponent  implements OnInit {
    registerForm: FormGroup;
    response: any;

    constructor(private formBuilder: FormBuilder,
                private account: AccountService,
                private router: Router,
    ) {   }

    ngOnInit() {
        this.registerForm = this.formBuilder.group({
            userName: ['', [Validators.required]],
            password: ['', [Validators.required]],
            confirmPassword: ['', [Validators.required]]
        });
    }

    onSubmit() {
        this.account.register(this.registerForm.value)
            .subscribe( () => {
                this.ngOnInit();
                this.response = 'Successfully registered';
            },
            error => this.response = error );
    };
}
