import { bootstrap } from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { APP_ROUTER_PROVIDERS } from './app.route';
import { AppComponent } from './app.component';
import { Configuration } from './app.constants';
import { enableProdMode } from '@angular/core';
import {JwtHelper} from 'angular2-jwt'
import {Authservice} from './authorize/authorize-service';
import {ResourceService} from './resource/resource-service';
import {disableDeprecatedForms, provideForms} from '@angular/forms';
enableProdMode();
bootstrap(AppComponent, [disableDeprecatedForms(),
    provideForms(),
    JwtHelper,
    HTTP_PROVIDERS,
    Authservice,
    ResourceService,
    APP_ROUTER_PROVIDERS,
    Configuration]);