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
var app_constants_1 = require('../app.constants');
var Rx_1 = require('rxjs/Rx');
var authorize_service_1 = require('../authorize/authorize-service');
require('rxjs/add/operator/map');
var ResourceService = (function () {
    function ResourceService(http, app, authservice) {
        this.http = http;
        this.app = app;
        this.authservice = authservice;
        this.authUrl = this.app.Server; // URL to web api
    }
    ResourceService.prototype.getUserInfo = function () {
        var authheaders = new http_1.Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
        var Authoptions = new http_1.RequestOptions({ headers: authheaders });
        return this.http.get(this.authUrl + "/api/Resource", Authoptions)
            .map(function (res) { return res.json(); })
            .catch(this.handleError);
    };
    ResourceService.prototype.handleError = function (error) {
        return Rx_1.Observable.throw(error || 'Server error');
    };
    ResourceService = __decorate([
        core_1.Injectable(), 
        __metadata('design:paramtypes', [http_1.Http, app_constants_1.Configuration, authorize_service_1.Authservice])
    ], ResourceService);
    return ResourceService;
}());
exports.ResourceService = ResourceService;
//# sourceMappingURL=resource-service.js.map