"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require('@angular/core');
var http_1 = require('@angular/http');
var angular2_jwt_1 = require('angular2-jwt');
var router_1 = require('@angular/router');
var authoriza_service_1 = require('./authoriza-service');
var ng2_bs3_modal_1 = require('ng2-bs3-modal/ng2-bs3-modal');
var authorizeComponent = (function () {
    function authorizeComponent(jwtHelper, _http, _parentRouter, authentication) {
        this.jwtHelper = jwtHelper;
        this._http = _http;
        this._parentRouter = _parentRouter;
        this.authentication = authentication;
        this.token = "";
        this.detoken = "";
        this.hodeModel = false;
    }
    authorizeComponent.prototype.mclose = function () {
        this.modal.close();
    };
    authorizeComponent.prototype.mopen = function () {
        this.modal.open();
    };
    authorizeComponent.prototype.logstatus = function () {
        this.isLoggedin = true;
    };
    authorizeComponent.prototype.ngOnInit = function () {
        var instance = this;
        // check if auth key is present
        if (localStorage.getItem('auth_key')) {
            this.token = this.jwtHelper.decodeToken(localStorage.getItem("auth_key"));
            // this.authdata = localStorage.getItem('auth_key');
            if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) {
                instance.getUserFromServer();
                this._parentRouter.navigate(['/dashboard']);
                this.isLoggedin = true; // redirect from login page
            }
            else {
                if (localStorage.getItem('refresh_key')) {
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
    };
    // below logic is for my login form snippet to view login/register/loss password etc
    authorizeComponent.prototype.callLogin = function () {
        this.login = true;
        this.register = false;
        this.loss = false;
    };
    authorizeComponent.prototype.callLoss = function () {
        this.login = false;
        this.register = false;
        this.loss = true;
    };
    authorizeComponent.prototype.callRegister = function () {
        this.login = false;
        this.register = true;
        this.loss = false;
    };
    // end
    authorizeComponent.prototype.Login = function (creds) {
        var _this = this;
        var instance = this;
        this.authentication.Login(creds)
            .subscribe(function (Ttoken) {
            _this.logMsg = "You are logged In Now , Please Wait ....";
            localStorage.setItem("auth_key", Ttoken.access_token);
            localStorage.setItem("refresh_key", Ttoken.refresh_token);
            _this.isLoggedin = true;
            instance.getUserFromServer();
            _this.mclose();
            _this._parentRouter.navigate(['/dashboard']);
        }, function (Error) {
            var error, key;
            key = Error.json();
            for (error in key) {
                _this.logMsg = "";
                _this.logMsg = _this.logMsg + key[error] + "\n";
            }
        });
    };
    authorizeComponent.prototype.refreshLogin = function () {
        var _this = this;
        var instance = this;
        this.authentication.refreshLogin()
            .subscribe(function (Ttoken) {
            _this.logMsg = "You are logged In Now , Please Wait ....";
            localStorage.setItem("auth_key", Ttoken.access_token);
            localStorage.setItem("refresh_key", Ttoken.refresh_token);
            _this.isLoggedin = true;
            instance.getUserFromServer();
            _this.mclose();
            _this._parentRouter.navigate(['/dashboard']);
        }, function (Error) {
            var error, key;
            key = Error.json();
            for (error in key) {
                _this.logMsg = "";
                _this.logMsg = _this.logMsg + key[error] + "\n";
            }
        });
    };
    authorizeComponent.prototype.getUserFromServer = function () {
        var instance = this;
        this.authentication.getUserInfo().subscribe(function (data) {
            instance.token = data;
        }, function (error) {
            instance.token = error;
        });
    };
    //open a popup for external login and set a interval function [API in Account Controller]
    authorizeComponent.prototype.extLogin = function (provider) {
        var instance = this;
        var popup_window = window.open('http://localhost:52606/api/account/externalaccess?provider=' + provider + '&returnUrl=http://localhost:52606/connect/authorize?client_id=localApp%26redirect_uri=http://localhost:52992/signin-oidc%26response_type=token%26scope=offline_access%20profile%20email%20roles%26nonce=123456%26client_id=firebaseApp%26resource=http://localhost:58056', '_blank', 'width=500, height=400');
        var intervalId = setInterval(function () {
            if (localStorage.getItem('auth_key')) {
                popup_window.close(); //close external login popup
                instance.mclose();
                instance.getUserFromServer();
                instance.isLoggedin = true; // close login box
                instance._parentRouter.navigate(['/dashboard']);
                clearInterval(intervalId); // navigate to dashboard, we can use returnurls too
            }
        }, 3000); //Check if the user has finished external login process after each 3 seconds.
    };
    authorizeComponent.prototype.Logout = function () {
        var _this = this;
        this.authentication.logout().subscribe(function (data) {
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            _this._parentRouter.navigate(['/']);
            _this.isLoggedin = false;
        }, function (error) { _this.logMsg = error; });
    };
    authorizeComponent.prototype.userRegister = function (creds) {
        var _this = this;
        this.authentication.Register(creds)
            .subscribe(function (Ttoken) {
            if (Ttoken.succeeded) {
                _this.model.username = creds.Email;
                _this.model.password = creds.Password;
                _this.Login(_this.model);
            }
            else {
                _this.logMsg = Ttoken.errors[0].description;
            }
        }, function (Error) {
            var error, key;
            key = Error.json();
            for (error in key) {
                _this.logMsg = "";
                _this.logMsg = _this.logMsg + key[error] + "\n";
            }
        });
    };
    __decorate([
        core_1.ViewChild('myModal'), 
        __metadata('design:type', ng2_bs3_modal_1.ModalComponent)
    ], authorizeComponent.prototype, "modal", void 0);
    authorizeComponent = __decorate([
        core_1.Component({
            selector: 'authorize',
            templateUrl: '/app/authorize/authorize-component.html',
            directives: [ng2_bs3_modal_1.MODAL_DIRECTIVES],
            providers: []
        }), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, http_1.Http, router_1.Router, authoriza_service_1.authervice])
    ], authorizeComponent);
    return authorizeComponent;
}());
exports.authorizeComponent = authorizeComponent;
var logModel = (function () {
    function logModel() {
    }
    return logModel;
}());
exports.logModel = logModel;
var extprovider = (function () {
    function extprovider() {
    }
    return extprovider;
}());
exports.extprovider = extprovider;
var registerModel = (function () {
    function registerModel() {
    }
    return registerModel;
}());
exports.registerModel = registerModel;
var token = (function () {
    function token() {
    }
    return token;
}());
exports.token = token;
var Regresult = (function () {
    function Regresult() {
    }
    return Regresult;
}());
exports.Regresult = Regresult;
//# sourceMappingURL=authorize-component.js.map