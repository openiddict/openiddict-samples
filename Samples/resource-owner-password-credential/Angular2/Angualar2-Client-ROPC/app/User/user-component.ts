import {Component} from '@angular/core'
import {Router} from '@angular/router';
import {authervice} from '../authorize/authoriza-service'
declare var System;

@Component({
    selector: 'authorize',
    template: `<h1>You are logged in</h1> <h3>{{payload}}</h3><hr/>
               `,
})

export class userComponent {
    private payload: string="loading ...";

    constructor( private _parentRouter: Router, 
                private Authentication: authervice) {}

    ngOnInit() {
        this.getapi();
    }

    public getapi() {
        if( localStorage.getItem("auth_key")){
        this.Authentication.getUserInfo().subscribe(
            data =>
            { 
             this.payload = JSON.stringify(data); 
            },
           error => {
               this.payload=error 
            });  
        }else{ this._parentRouter.navigate(['/']);}
    }

    public Logout() {
        this.Authentication.logout().subscribe(data => {
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            this._parentRouter.navigate(['/']); }, error => { this.payload = error });  
    }
}