import { provideRouter, RouterConfig } from '@angular/router';
import {AuthorizeComponent} from './authorize/authorize-component';
import {UserComponent} from './User/user-component';
import {welcome} from './welcome-component';

export const routes: RouterConfig = [
    { path: '',  component: welcome},
    { path: 'login',  component: AuthorizeComponent },
    { path: 'dashboard', component: UserComponent },
];
export const APP_ROUTER_PROVIDERS = [
    provideRouter(routes)
];
