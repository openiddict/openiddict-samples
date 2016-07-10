System.register(['@angular/core', 'angular2-jwt', '@angular/router-deprecated', '../authorize/authoriza-service', '../sharedservice'], function(exports_1, context_1) {
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
    var core_1, angular2_jwt_1, router_deprecated_1, authoriza_service_1, sharedservice_1;
    var userComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
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
            function (sharedservice_1_1) {
                sharedservice_1 = sharedservice_1_1;
            }],
        execute: function() {
            userComponent = (function () {
                function userComponent(jwtHelper, _parentRouter, _AuthLoginService, Authentication) {
                    this.jwtHelper = jwtHelper;
                    this._parentRouter = _parentRouter;
                    this._AuthLoginService = _AuthLoginService;
                    this.Authentication = Authentication;
                }
                userComponent.prototype.ngOnInit = function () {
                    var instance = this;
                    if (localStorage.getItem('auth_key')) {
                        if (!this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) {
                            this.payload = "Loading your profile data...";
                            instance.getapi();
                        }
                    }
                    else {
                        this.payload = "You are not logged In Please login";
                        this._AuthLoginService.emitNotLoggedIn();
                    }
                };
                userComponent.prototype.getapi = function () {
                    var _this = this;
                    this.Authentication.getUserInfo().subscribe(function (data) { _this.payload = JSON.stringify(data); }, function (error) { _this.payload = error; });
                };
                userComponent.prototype.Logout = function () {
                    var _this = this;
                    this.Authentication.logout().subscribe(function (data) {
                        localStorage.removeItem("auth_key");
                        localStorage.removeItem("refresh_key");
                        _this._parentRouter.parent.navigate(['/Default']);
                    }, function (error) { _this.payload = error; });
                };
                userComponent = __decorate([
                    core_1.Component({
                        selector: 'router-outlet',
                        template: "<blockquote><h5>You are logged in</h5> <h6>{{payload}}</h6>\n</blockquote>\n",
                        providers: []
                    }), 
                    __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, router_deprecated_1.Router, sharedservice_1.AuthLoginService, authoriza_service_1.authervice])
                ], userComponent);
                return userComponent;
            }());
            exports_1("userComponent", userComponent);
        }
    }
});
//# sourceMappingURL=user-component.js.map