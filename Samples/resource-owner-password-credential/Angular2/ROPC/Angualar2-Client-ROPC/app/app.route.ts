import { provideRouter, RouterConfig } from '@angular/router';
import {authorizeComponent} from './authorize/authorize-component';
import {userComponent} from './User/user-component';
import {welcome} from './welcome-component';

export const routes: RouterConfig = [
    { path: '',  component: welcome},
    { path: 'login',  component: authorizeComponent },
    { path: 'dashboard', component: userComponent },
];
export const APP_ROUTER_PROVIDERS = [
    provideRouter(routes)
];
