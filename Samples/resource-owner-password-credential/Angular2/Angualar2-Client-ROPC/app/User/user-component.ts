import {Component} from '@angular/core'
import {Router} from '@angular/router';
import {ResourceService} from '../resource/resource-service';
import {Authservice} from '../authorize/authorize-service';
declare var System;

@Component({
    selector: 'authorize',
    template: `<h1>You are logged in</h1> <h3><img style="vertical-align:middle" src="../../images/Preloader.gif" width="30" [hidden]="!isLoading" />{{payload}}</h3><hr/>
               `,
})

export class UserComponent {
    private payload: string="";
   public isLoading : boolean; // to show the loading progress
    constructor( private _parentRouter: Router, 
                 private authservice:Authservice,
                 private resourceService:ResourceService) {}

    ngOnInit() {
        this.callResourceServer();
    }

    public callResourceServer() {
        this.isLoading=true;
         if (this.authservice.authenticated()) {
        this.resourceService.getUserInfo().subscribe(
            data =>
            { 
             this.isLoading=false;
             this.payload = JSON.stringify(data); 
            },
           error => {
               this.isLoading=false;
               this.payload=error 
            });  
        }else{ this._parentRouter.navigate(['/']);}
    }

}