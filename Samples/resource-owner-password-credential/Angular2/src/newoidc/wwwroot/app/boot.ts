import { provide } from '@angular/core';
import { bootstrap } from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { ROUTER_PROVIDERS } from '@angular/router-deprecated';
import { AppComponent } from './app.component';
import { Configuration } from './app.constants';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {authervice} from './authorize/authoriza-service';



bootstrap(AppComponent, [
        
JwtHelper,
    HTTP_PROVIDERS, authervice, ROUTER_PROVIDERS,
    Configuration
]);
