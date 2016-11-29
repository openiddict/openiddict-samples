import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../core/account/account.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.template.html'
})
export class LoginComponent implements OnInit {
    loginForm: FormGroup;
    response: any;

    constructor(private formBuilder: FormBuilder,
                private account: AccountService
    ) { }


    ngOnInit(): void {
        this.loginForm = this.formBuilder.group({
            username: ['', [Validators.required]],
            password: ['', [Validators.required]],
        });
    }

    onSubmit() {
        this.account.login(this.loginForm.value)
            .subscribe( () => {
                this.ngOnInit();
                this.response = 'Successfully loggedin';
            },
            error => this.response = error );
    }

}
