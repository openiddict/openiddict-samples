import {Component} from '@angular/core'
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {Http, Headers} from '@angular/http';
import {Router, RouterOutlet, ComponentInstruction} from '@angular/router-deprecated';
import {authervice} from '../authorize/authoriza-service'

declare var System;
@Component({
    selector: 'authorize',
    template: `<h1>You are logged in</h1> <h3>{{payload}}</h3><hr/>

`,

    providers: []
})
export class userComponent {
    private payload: string="loading ...";
    constructor(public jwtHelper: JwtHelper,  private _parentRouter: Router, private Authentication: authervice) {
     
    }
    ngOnInit() {

        this.getapi();
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