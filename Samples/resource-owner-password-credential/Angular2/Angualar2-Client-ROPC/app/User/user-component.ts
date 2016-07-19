import {Component} from '@angular/core'
import {Router} from '@angular/router';
import {ResourceService} from '../resource/resource-service';
declare var System;

@Component({
    selector: 'authorize',
    template: `<h1>You are logged in</h1> <h3>{{payload}}</h3><hr/>
               `,
})

export class userComponent {
    private payload: string="loading ...";

    constructor( private _parentRouter: Router, 
                 private resourceService:ResourceService) {}

    ngOnInit() {
        this.callResourceServer();
    }

    public callResourceServer() {
        if( localStorage.getItem("auth_key")){
        this.resourceService.getUserInfo().subscribe(
            data =>
            { 
             this.payload = JSON.stringify(data); 
            },
           error => {
               this.payload=error 
            });  
        }else{ this._parentRouter.navigate(['/']);}
    }

}