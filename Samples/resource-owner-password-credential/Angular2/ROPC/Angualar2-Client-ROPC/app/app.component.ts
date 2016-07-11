import {Component, provide, Input, Output,EventEmitter,ViewChild} from '@angular/core';
import {Http, Headers} from '@angular/http';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {authorizeComponent} from './authorize/authorize-component'
import {userComponent} from './User/user-component'
import {welcome} from './welcome-component'
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
import {extauthorizeComponent} from './authorize/externalauth'
import { ROUTER_DIRECTIVES, Router } from '@angular/router';
import {authervice} from './authorize/authoriza-service'
@Component({
    selector: 'body',
    templateUrl:'app/app.component.html',
    directives: [ROUTER_DIRECTIVES, authorizeComponent],
   
    providers: [
        
  
          ]
})



export class AppComponent {
    private log: boolean;
    private authodata;
    constructor(public jwtHelper: JwtHelper, private _http: Http, private _parentRouter: Router, private Authentication: authervice) {  
        
    }

    @ViewChild(authorizeComponent)
    private authorizeComponentRefer: authorizeComponent;


    public authcheck() {
        if (localStorage.getItem('auth_key') && !this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) { //validation for secure routes there are other ways too but i think its simplest
            this._parentRouter.navigate(['/dashboard']);
            this.authorizeComponentRefer.logstatus();
        }
        else {
            this.authorizeComponentRefer.mopen();
        }
    }
  
}