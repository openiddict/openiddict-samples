System.register(['@angular/core', '@angular/http', 'angular2-jwt', '@angular/router-deprecated', './authoriza-service', "angular2-materialize", '../sharedservice'], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
        var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
        if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
        else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
        return c > 3 && r && Object.defineProperty(target, key, r), r;
    };
    var __metadata = (this && this.__metadata) || function (k, v) {
        if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
    };
    var core_1, http_1, angular2_jwt_1, router_deprecated_1, authoriza_service_1, angular2_materialize_1, sharedservice_1;
    var authorizeComponent, logModel, extprovider, registerModel, token, Regresult, forgotPassword;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (angular2_jwt_1_1) {
                angular2_jwt_1 = angular2_jwt_1_1;
            },
            function (router_deprecated_1_1) {
                router_deprecated_1 = router_deprecated_1_1;
            },
            function (authoriza_service_1_1) {
                authoriza_service_1 = authoriza_service_1_1;
            },
            function (angular2_materialize_1_1) {
                angular2_materialize_1 = angular2_materialize_1_1;
            },
            function (sharedservice_1_1) {
                sharedservice_1 = sharedservice_1_1;
            }],
        execute: function() {
            authorizeComponent = (function () {
                function authorizeComponent(jwtHelper, _http, _parentRouter, authentication, _AuthLoginService) {
                    this.jwtHelper = jwtHelper;
                    this._http = _http;
                    this._parentRouter = _parentRouter;
                    this.authentication = authentication;
                    this._AuthLoginService = _AuthLoginService;
                    this.token = "";
                    this.detoken = "";
                    this.hodeModel = false;
                    this._SharedUserDetailsModel = new sharedservice_1.SharedUserDetailsModel();
                }
                authorizeComponent.prototype.mclose = function () {
                    $('#login-modal').closeModal();
                };
                authorizeComponent.prototype.mopen = function () {
                    $('#login-modal').openModal();
                };
                authorizeComponent.prototype.logstatus = function () {
                    this.isLoggedin = true;
                };
                authorizeComponent.prototype.brodacstUserStatus = function () {
                    this._AuthLoginService.broadcastTextChange(this._SharedUserDetailsModel);
                };
                authorizeComponent.prototype.ngOnInit = function () {
                    this.isLoggedin = false;
                    this._SharedUserDetailsModel.isLoggedIn = false;
                    this._SharedUserDetailsModel.username = null;
                    this.brodacstUserStatus();
                    var instance = this;
                    if (localStorage.getItem('auth_key')) {
                        this.token = this.jwtHelper.decodeToken(localStorage.getItem("auth_key"));
                        if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) {
                            instance.getUserFromServer();
                            this._parentRouter.navigate(['/Dashboard']);
                            this.isLoggedin = true;
                            this._SharedUserDetailsModel.isLoggedIn = true;
                            this._SharedUserDetailsModel.username = this.token.UserName;
                            this.brodacstUserStatus();
                        }
                        else {
                            if (localStorage.getItem('refresh_key')) {
                                this.refreshLogin();
                            }
                        }
                    }
                    this.model = new logModel();
                    this.rmodel = new registerModel();
                    this.fmodel = new forgotPassword();
                    this.pros = new extprovider();
                    this.logMsg = "Type your credentials.";
                    this.login = true;
                    this.loss = false;
                    this.register = false;
                };
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
                        _this._parentRouter.navigate(['/Dashboard']);
                    }, function (Error) {
                        _this.logMsg = Error.json().error_description;
                    });
                };
                authorizeComponent.prototype.ForgotPass = function (creds) {
                    var instance = this;
                    this.authentication.forgotPass(creds)
                        .subscribe(function (Ttoken) {
                        alert(JSON.stringify(Ttoken));
                        if (Ttoken.Succeeded) {
                            instance.logMsg = "Instructions to reset your password with Verification code has been sent to your Email Address";
                        }
                        else {
                            instance.logMsg = Ttoken.Errors[0].Description;
                        }
                    }, function (Error) {
                        instance.logMsg = Error.json().error_description;
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
                        _this._parentRouter.navigate(['/Dashboard']);
                    }, function (Error) {
                        _this.logMsg = Error.json().error_description;
                    });
                };
                authorizeComponent.prototype.getUserFromServer = function () {
                    var _this = this;
                    var instance = this;
                    this.authentication.getUserInfo().subscribe(function (data) {
                        instance.token = data;
                        _this._SharedUserDetailsModel.isLoggedIn = true;
                        _this._SharedUserDetailsModel.username = instance.token.UserName;
                        _this.brodacstUserStatus();
                    }, function (error) {
                        instance.token = error;
                    });
                };
                authorizeComponent.prototype.extLogin = function (provider) {
                    var instance = this;
                    var popup_window = window.open('http://localhost:58056/api/account/externalaccess?provider=' + provider, '_blank', 'width=500, height=400');
                    var intervalId = setInterval(function () {
                        if (localStorage.getItem('auth_key')) {
                            popup_window.close();
                            instance.mclose();
                            instance.getUserFromServer();
                            instance.isLoggedin = true;
                            instance._parentRouter.navigate(['/Dashboard']);
                            clearInterval(intervalId);
                        }
                    }, 3000);
                };
                authorizeComponent.prototype.Logout = function () {
                    var _this = this;
                    this.authentication.logout().subscribe(function (data) {
                        localStorage.removeItem("auth_key");
                        localStorage.removeItem("refresh_key");
                        _this._parentRouter.navigate(['/Default']);
                        _this.isLoggedin = false;
                        _this._SharedUserDetailsModel.isLoggedIn = false;
                        _this._SharedUserDetailsModel.username = null;
                        _this.brodacstUserStatus();
                    }, function (error) { _this.logMsg = error; });
                };
                authorizeComponent.prototype.userRegister = function (creds) {
                    var _this = this;
                    this.authentication.Register(creds)
                        .subscribe(function (Ttoken) {
                        if (Ttoken.Succeeded) {
                            _this.model.username = creds.Email;
                            _this.model.password = creds.Password;
                            _this.Login(_this.model);
                        }
                        else {
                            _this.logMsg = Ttoken.Errors[0].Description;
                        }
                    }, function (Error) {
                        _this.logMsg = Error.Errors[0].Description;
                    });
                };
                authorizeComponent = __decorate([
                    core_1.Component({
                        selector: 'authorize',
                        templateUrl: './app/authorize/authorize-component.html',
                        directives: [angular2_materialize_1.MaterializeDirective],
                        providers: []
                    }), 
                    __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, http_1.Http, router_deprecated_1.Router, authoriza_service_1.authervice, sharedservice_1.AuthLoginService])
                ], authorizeComponent);
                return authorizeComponent;
            }());
            exports_1("authorizeComponent", authorizeComponent);
            logModel = (function () {
                function logModel() {
                }
                return logModel;
            }());
            exports_1("logModel", logModel);
            extprovider = (function () {
                function extprovider() {
                }
                return extprovider;
            }());
            exports_1("extprovider", extprovider);
            registerModel = (function () {
                function registerModel() {
                }
                return registerModel;
            }());
            exports_1("registerModel", registerModel);
            token = (function () {
                function token() {
                }
                return token;
            }());
            exports_1("token", token);
            Regresult = (function () {
                function Regresult() {
                }
                return Regresult;
            }());
            exports_1("Regresult", Regresult);
            forgotPassword = (function () {
                function forgotPassword() {
                }
                return forgotPassword;
            }());
            exports_1("forgotPassword", forgotPassword);
        }
    }
});
//# sourceMappingURL=authorize-component.js.map