import { provideRouter, RouterConfig } from '@angular/router';
import {authorizeComponent} from './authorize/authorize-component';
import {userComponent} from './User/user-component';
import {welcome} from './welcome-component';
import {extauthorizeComponent} from './authorize/externalauth';
export const routes: RouterConfig = [
    { path: '',  component: welcome},
    { path: 'login',  component: authorizeComponent },
    { path: 'dashboard', component: userComponent },
    { path: 'signin-oidc',  component: extauthorizeComponent }
];
export const APP_ROUTER_PROVIDERS = [
    provideRouter(routes)
];
