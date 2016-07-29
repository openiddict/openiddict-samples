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
var router_1 = require('@angular/router');
var resource_service_1 = require('../resource/resource-service');
var authorize_service_1 = require('../authorize/authorize-service');
var UserComponent = (function () {
    function UserComponent(_parentRouter, authservice, resourceService) {
        this._parentRouter = _parentRouter;
        this.authservice = authservice;
        this.resourceService = resourceService;
        this.payload = "";
    }
    UserComponent.prototype.ngOnInit = function () {
        this.callResourceServer();
    };
    UserComponent.prototype.callResourceServer = function () {
        var _this = this;
        this.isLoading = true;
        if (this.authservice.authenticated()) {
            this.resourceService.getUserInfo().subscribe(function (Data) {
                _this.isLoading = false;
                _this.payload = JSON.stringify(Data);
            }, function (Error) {
                _this.isLoading = false;
                _this.payload = Error;
            });
        }
        else {
            this._parentRouter.navigate(['/']);
        }
    };
    UserComponent = __decorate([
        core_1.Component({
            selector: 'authorize',
            template: "<h1>You are logged in</h1> <h3>\n    <img style=\"vertical-align:middle\" src=\"../../images/Preloader.gif\" width=\"30\" [hidden]=\"!isLoading\" />\n    {{payload}}</h3><hr/>\n               ",
        }), 
        __metadata('design:paramtypes', [router_1.Router, authorize_service_1.Authservice, resource_service_1.ResourceService])
    ], UserComponent);
    return UserComponent;
}());
exports.UserComponent = UserComponent;
//# sourceMappingURL=user-component.js.map