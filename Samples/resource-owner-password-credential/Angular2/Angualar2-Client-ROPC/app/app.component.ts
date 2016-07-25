import {Component,ViewChild} from '@angular/core';
import {JwtHelper} from 'angular2-jwt'
import {AuthorizeComponent} from './authorize/authorize-component'
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
import { ROUTER_DIRECTIVES, Router } from '@angular/router';
@Component({
    selector: 'body',
    templateUrl:'app/app.component.html',
    directives: [ROUTER_DIRECTIVES, AuthorizeComponent],
})

export class AppComponent {
    private log: boolean;
    private authodata;
    constructor(public jwtHelper: JwtHelper,    
                private _parentRouter: Router) { }

    @ViewChild(AuthorizeComponent)
    private authorizeComponentRefer: AuthorizeComponent;

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