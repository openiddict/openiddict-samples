import {Component, provide, Input, Output,EventEmitter,ViewChild} from '@angular/core';
import {RouteConfig, ROUTER_DIRECTIVES, ROUTER_PROVIDERS, } from '@angular/router-deprecated';
import {Http, Headers} from '@angular/http';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {authorizeComponent} from './authorize/authorize-component'
import {userComponent} from './User/user-component'
import {welcome} from './welcome-component'
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
import {extauthorizeComponent} from './authorize/externalauth'
import {Router, RouterOutlet, ComponentInstruction} from '@angular/router-deprecated';
import {authervice} from './authorize/authoriza-service'
@Component({
    selector: 'my-app',
    templateUrl:'app/app.component.html',
    directives: [ROUTER_DIRECTIVES, authorizeComponent],
   
    providers: [
        
  
          ]
})
    @RouteConfig([
        { path: '/', name: 'Default', component: welcome, useAsDefault:true  },
        { path: '/login', name: 'Login', component: authorizeComponent},
        { path: '/dashboard', name: 'Dashboard', component: userComponent },
        { path: '/signin-oidc', name: 'Extauth', component: extauthorizeComponent }
        
])



export class AppComponent {
    private log: boolean;
    private authodata;
    constructor(public jwtHelper: JwtHelper, private _http: Http, private _parentRouter: Router, private Authentication: authervice) {  
        
    }

    @ViewChild(authorizeComponent)
    private authorizeComponentRefer: authorizeComponent;


    public authcheck() {
        if (localStorage.getItem('auth_key') && !this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) { //validation for secure routes there are other ways too but i think its simplest
            this._parentRouter.navigate(['/Dashboard']);
            this.authorizeComponentRefer.logstatus();
        }
        else {
            this.authorizeComponentRefer.mopen();
        }
    }
  
}