import { provide } from '@angular/core';
import { bootstrap } from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { ROUTER_PROVIDERS } from '@angular/router-deprecated';
import { AppComponent } from './app.component';
import { Configuration } from './app.constants';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {authervice} from './authorize/authoriza-service';
import {GeoSharedService } from './geoLocation/geo.shared';
import {geoService} from './geoLocation/geoservice';
import {categoryService}       from './category/categoryService';
import {AuthLoginService} from './sharedservice'
import "angular2-materialize";

bootstrap(AppComponent, [
JwtHelper,
    HTTP_PROVIDERS, authervice, ROUTER_PROVIDERS, AuthLoginService,
    GeoSharedService, geoService, categoryService,
    Configuration
]);
