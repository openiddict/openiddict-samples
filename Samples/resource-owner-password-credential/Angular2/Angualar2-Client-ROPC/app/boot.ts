import { bootstrap } from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { APP_ROUTER_PROVIDERS } from './app.route';
import { AppComponent } from './app.component';
import { Configuration } from './app.constants';
import {JwtHelper} from 'angular2-jwt'
import {authservice} from './authorize/authorize-service';
import {ResourceService} from './resource/resource-service';
bootstrap(AppComponent, [JwtHelper,
                        HTTP_PROVIDERS, 
                        authservice,
                        ResourceService, 
                        APP_ROUTER_PROVIDERS,
                        Configuration]
        );
