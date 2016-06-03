import {Component, ViewChild,Input } from '@angular/core'
import {Http, Headers} from '@angular/http';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {Router} from '@angular/router-deprecated';
import {authervice} from './authoriza-service';
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
declare var System;
@Component({
    selector: 'authorize',
    templateUrl: './app/authorize/authorize-component.html',
    directives: [MODAL_DIRECTIVES],
    providers: []
})

export class authorizeComponent{
    constructor(public jwtHelper: JwtHelper, private _http: Http, private _parentRouter: Router,private authentication:authervice) {
       
    }

    //@Input('log') log: boolean;
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
    public token: any = "";
    public detoken: any = "";
    public isLoggedin: boolean;
    public logMsg: string;
    public model: logModel;
    public rmodel: registerModel;
    public pros: extprovider;
    public login: boolean;
    public register: boolean;
    public user: string;
    public loss: boolean;
    public externals: string;
    public authdata: any;
    public hodeModel: boolean = false;
    ngOnInit() {
       
        var instance = this;
         // check if auth key is present
        if (localStorage.getItem('auth_key')) {
            this.token = this.jwtHelper.decodeToken(localStorage.getItem("auth_key"));
           // this.authdata = localStorage.getItem('auth_key');
            if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) // check if its not expired
            {
                instance.getUserFromServer();
                this._parentRouter.navigate(['/Dashboard']);
                this.isLoggedin = true; // redirect from login page
            }
            else
            {
                if (localStorage.getItem('refresh_key')) { // check if refresh key is present it wont be present for external logged in users
                    this.refreshLogin(); // renew auth key and redirect
                }
            }

        } // logic to redirect user if already logged in
       // this.isLoggedin = false;
        this.model = new logModel();
        this.rmodel = new registerModel();
        this.pros = new extprovider();
        // below logic is for my login form snippet  to view login/register/loss password etc
        this.logMsg = "Type your credentials.";
        this.login = true;
        this.loss = false;
        this.register = false;
        //end of logic
    }
    // below logic is for my login form snippet to view login/register/loss password etc
    public callLogin() {
        this.login = true;
        this.register = false;
        this.loss = false;
    }
    public callLoss() {
        this.login = false;
        this.register = false;
        this.loss = true;
    }
    public callRegister() {
        this.login = false;
        this.register = true;
        this.loss = false;
    }
    // end


    public Login(creds: logModel) {
        var instance = this;
        this.authentication.Login(creds)
            .subscribe(
            Ttoken => {
                this.logMsg = "You are logged In Now , Please Wait ....";
                localStorage.setItem("auth_key", Ttoken.access_token);
                localStorage.setItem("refresh_key", Ttoken.refresh_token);
                this.isLoggedin = true;
                instance.getUserFromServer();
                this.mclose();
                this._parentRouter.navigate(['/Dashboard']);
            },
            Error => {
                this.logMsg = Error.error_description
            })
        
    }

    public refreshLogin() {
    var instance = this;
        this.authentication.refreshLogin()
            .subscribe(
            Ttoken => {
                this.logMsg = "You are logged In Now , Please Wait ....";
                localStorage.setItem("auth_key", Ttoken.access_token);
                localStorage.setItem("refresh_key", Ttoken.refresh_token);
                this.isLoggedin = true;
                instance.getUserFromServer();
                this.mclose();
                this._parentRouter.navigate(['/Dashboard']);
            },
            Error => {
                this.logMsg = Error.error_description
            })

    }
    public getUserFromServer() {
        this.authentication.getUserInfo().subscribe(data => {
            this.token = data;
        },
            error => {
                this.token = error;
            });
    }
    //open a popup for external login and set a interval function [API in Account Controller]
    public extLogin(provider: string) {
        var instance = this;
        var popup_window = window.open('http://localhost:58056/api/account/externalaccess?provider='+provider, '_blank', 'width=500, height=400');
        setInterval(function () {
            if (localStorage.getItem('auth_key')) {
                this.clearInterval();
                popup_window.close();//close external login popup
                instance.mclose();
                instance.getUserFromServer();
                this.isLoggedin = true;// close login box
                instance._parentRouter.navigate(['/Dashboard']);// navigate to dashboard, we can use returnurls too
            } }, 3000); //Check if the user has finished external login process after each 3 seconds.
    }


    public Logout() {
        this.authentication.logout().subscribe(data => {
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            this._parentRouter.navigate(['/Default']);
            this.isLoggedin = false;
        }, error => { this.logMsg = error });
    }

    public userRegister(creds:registerModel) {
        this.authentication.Register(creds)
            .subscribe(
            Ttoken => {
                if (Ttoken.Succeeded) {
                    this.model.username = creds.Email;
                    this.model.password = creds.Password;
                    this.Login(this.model);
                }
                else
                {
                    this.logMsg = Ttoken.Errors[0].Description
                }
            },
            Error => {
                this.logMsg = Error.Errors[0].Description
            });
    }
    }


export class logModel {
    public username: string;
    public password: string;
}

export class extprovider {
    public provider: string;
    public returnUrl: string;
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
    public Succeeded: string;
    public Errors: any[];
   
}