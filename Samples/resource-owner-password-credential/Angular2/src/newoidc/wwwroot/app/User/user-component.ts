import {Component, Input, Output, EventEmitter, ViewChild} from '@angular/core'
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {Http, Headers} from '@angular/http';
import {Router, RouterOutlet, ComponentInstruction} from '@angular/router-deprecated';
import {authervice} from '../authorize/authoriza-service'
import {authorizeComponent} from '../authorize/authorize-component'
import {AuthLoginService} from '../sharedservice'

declare var System;
@Component({
    selector: 'router-outlet',
    template: `<blockquote><h5>You are logged in</h5> <h6>{{payload}}</h6>
</blockquote>
`,

    providers: []
})
export class userComponent {
   
    public payload;
    constructor(public jwtHelper: JwtHelper, private _parentRouter: Router, private _AuthLoginService: AuthLoginService, private Authentication: authervice) {
     
    }
    ngOnInit() {
            var instance = this;
            // check if auth key is present
            if (localStorage.getItem('auth_key')) {
                if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) // check if its not expired
                {
                    this.payload = "Loading your profile data..."
                    instance.getapi();
                }
            }
            else {
                this.payload = "You are not logged In Please login"
                this._AuthLoginService.emitNotLoggedIn();
            }
     
    }


    public getapi() {
        this.Authentication.getUserInfo().subscribe(data => { this.payload = JSON.stringify(data); }, error => { this.payload=error });  
    }

    public Logout() {
        this.Authentication.logout().subscribe(data => {
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            this._parentRouter.parent.navigate(['/Default']); }, error => { this.payload = error });  
    }
}