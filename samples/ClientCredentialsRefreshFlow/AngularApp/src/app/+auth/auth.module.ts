import { NgModule }            from '@angular/core';
import { AuthComponent }       from './auth.component';
import { authRouting }         from './auth.routing';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './forgot-password/reset-password.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { VerifyComponent } from './verify/verify.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
    imports: [
        authRouting,
        SharedModule
    ],
    declarations: [
        AuthComponent,
        LoginComponent,
        RegisterComponent,
        VerifyComponent,
        ForgotPasswordComponent,
        ResetPasswordComponent
    ]
})
export class AuthModule { }
