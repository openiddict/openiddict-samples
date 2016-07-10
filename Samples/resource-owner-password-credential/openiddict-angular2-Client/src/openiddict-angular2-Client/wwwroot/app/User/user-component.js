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
var authoriza_service_1 = require('../authorize/authoriza-service');
var userComponent = (function () {
    function userComponent(jwtHelper, _parentRouter, Authentication) {
        this.jwtHelper = jwtHelper;
        this._parentRouter = _parentRouter;
        this.Authentication = Authentication;
        this.payload = "loading ...";
    }
    userComponent.prototype.ngOnInit = function () {
        this.getapi();
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
            _this._parentRouter.navigate(['/default']);
        }, function (error) { _this.payload = error; });
    };
    userComponent = __decorate([
        core_1.Component({
            selector: 'authorize',
            template: "<h1>You are logged in</h1> <h3>{{payload}}</h3><hr/>\n\n",
            providers: []
        }), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, router_1.Router, authoriza_service_1.authervice])
    ], userComponent);
    return userComponent;
}());
exports.userComponent = userComponent;
//# sourceMappingURL=user-component.js.map