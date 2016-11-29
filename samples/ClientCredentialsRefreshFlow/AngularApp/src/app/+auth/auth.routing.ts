import { ModuleWithProviders }  from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { AuthComponent } from './auth.component';
import { VerifyComponent } from './verify/verify.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './forgot-password/reset-password.component';
import { AuthenticatedAuthGuard } from '../../core/guards/authenticated-auth-guard.service';

const authRoutes: Routes = [
    {
        path: '',
        component: AuthComponent,
        children: [
            {
                path: '',
                component: LoginComponent
            },
            {
                path: 'login',
                component: LoginComponent
            },
            {
                path: 'register',
                component: RegisterComponent
            },
            {
                path: 'forgot-password',
                component: ForgotPasswordComponent
            },
            {
                path: 'reset-password',
                component: ResetPasswordComponent
            },
            {
                path: 'verify',
                component: VerifyComponent,
                canActivate: [AuthenticatedAuthGuard]
            },
            {
                path: 'verify/:id/:code',
                component: VerifyComponent
            },
            {
                path: 'forgot-password',
                component: RegisterComponent
            }
        ]
    }
];



export const authRouting: ModuleWithProviders = RouterModule.forChild(authRoutes);

