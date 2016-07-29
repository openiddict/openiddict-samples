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
var angular2_jwt_1 = require('angular2-jwt');
var router_1 = require('@angular/router');
var authorize_service_1 = require('./authorize-service');
var resource_service_1 = require('../resource/resource-service');
var ng2_bs3_modal_1 = require('ng2-bs3-modal/ng2-bs3-modal');
var AuthorizeComponent = (function () {
    function AuthorizeComponent(jwtHelper, parentRouter, authService, resourceService) {
        this.jwtHelper = jwtHelper;
        this.parentRouter = parentRouter;
        this.authService = authService;
        this.resourceService = resourceService;
    }
    AuthorizeComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.subscription = this.authService.getLoggedInEmitter()
            .subscribe(function (item) { _this.mopen(); });
        var instance = this;
        // check if auth key is present.
        if (this.authService.authenticated()) {
            instance.getUserFromServer();
            this.parentRouter.navigate(['/dashboard']);
        }
        else {
            if (localStorage.getItem('refresh_key')) {
                this.refreshLogin(); // renew auth key 
            }
        }
        this.model = new LogModel();
        this.rmodel = new RegisterModel();
        // below logic is for my login form snippet  to view login/register 
        this.logMsg = "Type your credentials.";
        this.payloader = "Authorizing Server Access";
        this.loginBool = true;
        this.registerBool = false;
        this.refreshBool = false;
        //end of logic
    };
    AuthorizeComponent.prototype.mclose = function () {
        this.modal.close();
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
    };
    AuthorizeComponent.prototype.mopen = function () {
        if (localStorage.getItem('refresh_key')) {
            this.refreshBool = true;
            this.loginBool = false;
            this.registerBool = false;
            this.modal.open(); // check if refresh key is present 
            this.refreshLogin(); // renew auth key
        }
        else {
            this.refreshBool = false;
            this.loginBool = true;
            this.registerBool = false;
            this.modal.open();
        }
    };
    AuthorizeComponent.prototype.logstatus = function () {
        this.isLoggedin = true;
    };
    AuthorizeComponent.prototype.callLogin = function () {
        this.loginBool = true;
        this.refreshBool = false;
        this.registerBool = false;
    };
    AuthorizeComponent.prototype.callRegister = function () {
        this.loginBool = false;
        this.refreshBool = false;
        this.registerBool = true;
    };
    // end
    AuthorizeComponent.prototype.login = function (creds) {
        var _this = this;
        this.isLoading = true;
        var instance = this;
        this.authService.login(creds)
            .subscribe(function (TokenResult) {
            _this.logMsg = "You are logged In Now , Please Wait ....";
            localStorage.setItem("auth_key", TokenResult.access_token);
            localStorage.setItem("refresh_key", TokenResult.refresh_token);
            instance.getUserFromServer();
            _this.mclose();
            _this.parentRouter.navigate(['/dashboard']);
            _this.isLoading = false;
        }, function (Error) {
            _this.isLoading = false;
            var error, key;
            key = Error.json();
            for (error in key) {
                _this.logMsg = "";
                _this.logMsg = _this.logMsg + key[error] + "\n";
            }
        });
    };
    AuthorizeComponent.prototype.refreshLogin = function () {
        var _this = this;
        this.isLoading = true;
        var instance = this;
        this.authService.refreshLogin()
            .subscribe(function (TokenResult) {
            _this.isLoading = false;
            _this.logMsg = "You are logged In Now , Please Wait ....";
            localStorage.setItem("auth_key", TokenResult.access_token);
            localStorage.setItem("refresh_key", TokenResult.refresh_token);
            instance.getUserFromServer();
            _this.payloader = "Authorized";
            _this.mclose();
            _this.parentRouter.navigate(['/dashboard']);
        }, function (Error) {
            _this.isLoading = false;
            var error, key;
            key = Error.json();
            for (error in key) {
                _this.logMsg = "";
                _this.logMsg = _this.logMsg + key[error] + "\n";
            }
        });
    };
    AuthorizeComponent.prototype.getUserFromServer = function () {
        var _this = this;
        this.isLoading = true;
        var instance = this;
        this.resourceService.getUserInfo().subscribe(function (Data) {
            _this.isLoading = false;
            instance.userDetails = Data;
            _this.isLoggedin = true;
            _this.mclose();
        }, function (Error) {
            _this.isLoading = false;
            instance.userDetails = Error;
        });
    };
    AuthorizeComponent.prototype.logout = function () {
        var _this = this;
        this.isLoading = true;
        this.authService.logout().subscribe(function (Data) {
            _this.isLoading = false;
            localStorage.removeItem("auth_key");
            localStorage.removeItem("refresh_key");
            _this.parentRouter.navigate(['/']);
            _this.isLoggedin = false;
        }, function (Error) {
            _this.isLoading = false;
            _this.logMsg = Error;
        });
    };
    AuthorizeComponent.prototype.userRegister = function (creds) {
        var _this = this;
        this.isLoading = true;
        this.authService.register(creds)
            .subscribe(function (TokenResult) {
            _this.isLoading = false;
            if (TokenResult.succeeded) {
                _this.model.username = creds.Email;
                _this.model.password = creds.Password;
                _this.login(_this.model);
            }
            else {
                _this.logMsg = TokenResult.errors[0].description;
            }
        }, function (Error) {
            _this.isLoading = false;
            var error, key;
            key = Error.json();
            if (key.succeeded == false) {
                _this.logMsg = key.errors[0].description;
            }
            else {
                for (error in key) {
                    _this.logMsg = "";
                    _this.logMsg = _this.logMsg + key[error] + "\n";
                }
            }
        });
    };
    __decorate([
        core_1.ViewChild('myModal'), 
        __metadata('design:type', ng2_bs3_modal_1.ModalComponent)
    ], AuthorizeComponent.prototype, "modal", void 0);
    AuthorizeComponent = __decorate([
        core_1.Component({
            selector: 'authorize',
            templateUrl: '/app/authorize/authorize-component.html',
            directives: [ng2_bs3_modal_1.MODAL_DIRECTIVES]
        }), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, router_1.Router, authorize_service_1.Authservice, resource_service_1.ResourceService])
    ], AuthorizeComponent);
    return AuthorizeComponent;
}());
exports.AuthorizeComponent = AuthorizeComponent;
var LogModel = (function () {
    function LogModel() {
    }
    return LogModel;
}());
exports.LogModel = LogModel;
var RegisterModel = (function () {
    function RegisterModel() {
    }
    return RegisterModel;
}());
exports.RegisterModel = RegisterModel;
var TokenResult = (function () {
    function TokenResult() {
    }
    return TokenResult;
}());
exports.TokenResult = TokenResult;
var Regresult = (function () {
    function Regresult() {
    }
    return Regresult;
}());
exports.Regresult = Regresult;
//# sourceMappingURL=authorize-component.js.map