import {Component, ViewChild } from '@angular/core'
import {JwtHelper} from 'angular2-jwt'
import {Router} from '@angular/router';
import {authservice} from './authorize-service';
import {ResourceService} from '../resource/resource-service';
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
declare var System;
@Component({
    selector: 'authorize',
    templateUrl: '/app/authorize/authorize-component.html',
    directives: [MODAL_DIRECTIVES],
})
export class authorizeComponent{

    constructor(public jwtHelper: JwtHelper,
                private _parentRouter: Router,
                private AuthService: authservice,
                private resourceService:ResourceService) {}
    public userDetails: any ;
    public isLoggedin: boolean;
    public isLoading : boolean; // to show the loading progress
    public logMsg: string;
    public model: logModel;
    public rmodel: registerModel;
    public login: boolean;
    public register: boolean;
    public user: string;
    public authdata: any;
    public hodeModel: boolean = false;
    ngOnInit() {
        var instance = this;
         // check if auth key is present
        if (localStorage.getItem('auth_key')) {
            this.userDetails = this.jwtHelper.decodeToken(localStorage.getItem("auth_key"));
            if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) // check if its not expired
            {
                instance.getUserFromServer();
                this._parentRouter.navigate(['/dashboard']);
                this.isLoggedin = true; // redirect from login page
            }
            else
            {
                if (localStorage.getItem('refresh_key')) { // check if refresh key is present it wont be present for external logged in users
                    this.refreshLogin(); // renew auth key and redirect
                }
            }
           } 
        this.model = new logModel();
        this.rmodel = new registerModel();
        // below logic is for my login form snippet  to view login/register 
        this.logMsg = "Type your credentials.";
        this.login = true;
        this.register = false;
        //end of logic
    }
    // below logic is for my login form snippet to view login/register 
    @ViewChild('myModal')
    modal: ModalComponent;

    public mclose() {
        this.modal.close();
    }

   public mopen() {
        this.modal.open();
    }

   public logstatus() {
       this.isLoggedin = true; 
   }

    public callLogin() { // method to open login form
        this.login = true;
        this.register = false;
    }

    public callRegister() {//method to open register form
        this.login = false;
        this.register = true;
    }
    // end

    public Login(creds: logModel) {
        this.isLoading=true;
        var instance = this;
        this.AuthService.Login(creds)
            .subscribe(
            Ttoken => {
                this.logMsg = "You are logged In Now , Please Wait ....";
                localStorage.setItem("auth_key", Ttoken.access_token);
                localStorage.setItem("refresh_key", Ttoken.refresh_token);
                instance.getUserFromServer();
                this.mclose();
                this._parentRouter.navigate(['/dashboard']);
                this.isLoading=false;
            },
            Error => {
                this.isLoading=false;
                var error, key;
                key = Error.json();
                for (error in key) 
                {
                    this.logMsg = "";
                    this.logMsg = this.logMsg + key[error] + "\n";
                }
            })
    }

    public refreshLogin() {
    this.isLoading=true;
    var instance = this;
        this.AuthService.refreshLogin()
            .subscribe(
            Ttoken => {
                this.isLoading=false;
                this.logMsg = "You are logged In Now , Please Wait ....";
                localStorage.setItem("auth_key", Ttoken.access_token);
                localStorage.setItem("refresh_key", Ttoken.refresh_token);
                instance.getUserFromServer();
                this.mclose();
                this._parentRouter.navigate(['/dashboard']);
            },
            Error => {
                this.isLoading=false;
                var error, key;
                key = Error.json();
                for (error in key) 
                {
                    this.logMsg = "";
                    this.logMsg = this.logMsg + key[error] + "\n";
                }
            })
    }

    public getUserFromServer() {
        this.isLoading=true;
        var instance = this;
        this.resourceService.getUserInfo().subscribe(data => {
            this.isLoading=false;
            instance.userDetails = data;
            this.isLoggedin = true;
            },
            error => 
            {
                this.isLoading=false;
                instance.userDetails = error;
            });
    }

    public Logout() {
        this.isLoading=true;
        this.AuthService.logout().subscribe(data => {
            this.isLoading=false;
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            this._parentRouter.navigate(['/']);
            this.isLoggedin = false;
        }, error => 
            {
             this.isLoading=false;
             this.logMsg = error 
            });
    }

    public userRegister(creds:registerModel) {
        this.isLoading=true;
        this.AuthService.Register(creds)
            .subscribe(
            Ttoken => {
                this.isLoading=false;
                if (Ttoken.succeeded) {
                    this.model.username = creds.Email;
                    this.model.password = creds.Password;
                    this.Login(this.model);
                }
                else
                {
                    this.logMsg = Ttoken.errors[0].description
                }
            },
            Error => {
                this.isLoading=false;
                var error, key;
                key = Error.json();
                for (error in key)
                {
                    this.logMsg = "";
                    this.logMsg = this.logMsg + key[error] + "\n";
                }
            });
     }
     //End of class
}


export class logModel {
    public username: string;
    public password: string;
}

export class registerModel {
    public Email: string;
    public Password: string;
    public ConfirmPassword: string;
}

export class token {
    public access_token: string;
    public expires_in: string;
    public refresh_token: string;
    public token_type: string;
}

export class Regresult {
    public succeeded: string;
    public errors: any[];
   
}