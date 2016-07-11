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
var authorize_component_1 = require('./authorize/authorize-component');
var router_1 = require('@angular/router');
var authoriza_service_1 = require('./authorize/authoriza-service');
var AppComponent = (function () {
    function AppComponent(jwtHelper, _http, _parentRouter, Authentication) {
        this.jwtHelper = jwtHelper;
        this._http = _http;
        this._parentRouter = _parentRouter;
        this.Authentication = Authentication;
    }
    AppComponent.prototype.authcheck = function () {
        if (localStorage.getItem('auth_key') && !this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) {
            this._parentRouter.navigate(['/dashboard']);
            this.authorizeComponentRefer.logstatus();
        }
        else {
            this.authorizeComponentRefer.mopen();
        }
    };
    __decorate([
        core_1.ViewChild(authorize_component_1.authorizeComponent), 
        __metadata('design:type', authorize_component_1.authorizeComponent)
    ], AppComponent.prototype, "authorizeComponentRefer", void 0);
    AppComponent = __decorate([
        core_1.Component({
            selector: 'body',
            templateUrl: 'app/app.component.html',
            directives: [router_1.ROUTER_DIRECTIVES, authorize_component_1.authorizeComponent],
            providers: []
        }), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, http_1.Http, router_1.Router, authoriza_service_1.authervice])
    ], AppComponent);
    return AppComponent;
}());
exports.AppComponent = AppComponent;
//# sourceMappingURL=app.component.js.map