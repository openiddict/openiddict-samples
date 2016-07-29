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
var authorize_component_1 = require('./authorize/authorize-component');
var router_1 = require('@angular/router');
var authorize_service_1 = require('./authorize/authorize-service');
var AppComponent = (function () {
    function AppComponent(jwtHelper, authservice, _parentRouter) {
        this.jwtHelper = jwtHelper;
        this.authservice = authservice;
        this._parentRouter = _parentRouter;
    }
    AppComponent.prototype.authcheck = function () {
        if (this.authservice.authenticated()) {
            this._parentRouter.navigate(['/dashboard']);
            this.authorizeComponentRefer.logstatus();
        }
    };
    __decorate([
        core_1.ViewChild(authorize_component_1.AuthorizeComponent), 
        __metadata('design:type', authorize_component_1.AuthorizeComponent)
    ], AppComponent.prototype, "authorizeComponentRefer", void 0);
    AppComponent = __decorate([
        core_1.Component({
            selector: 'body',
            templateUrl: 'app/app.component.html',
            directives: [router_1.ROUTER_DIRECTIVES, authorize_component_1.AuthorizeComponent],
        }), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, authorize_service_1.Authservice, router_1.Router])
    ], AppComponent);
    return AppComponent;
}());
exports.AppComponent = AppComponent;
//# sourceMappingURL=app.component.js.map