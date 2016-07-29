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
var angular2_jwt_1 = require('angular2-jwt');
require('rxjs/add/operator/map');
var Authservice = (function () {
    function Authservice(jwtHelper, http, app) {
        this.jwtHelper = jwtHelper;
        this.http = http;
        this.app = app;
        //event emitter to open login box
        this.loginEmitter = new core_1.EventEmitter();
        this.authUrl = this.app.Server; // URL to web api
        this.headers = new http_1.Headers({ 'Content-Type': 'application/X-www-form-urlencoded' });
        this.jheaders = new http_1.Headers({ 'Content-Type': 'application/json' });
        this.options = new http_1.RequestOptions({ headers: this.headers });
        this.joptions = new http_1.RequestOptions({ headers: this.jheaders });
        this.tokenParams = "grant_type=password" +
            "&client_id=localApp" +
            "&resource=" + this.app.FileServer +
            "&responseType=token" +
            "&scope=offline_access profile email roles"; // offline_access for refresh_token read more on docs / blog
    }
    Authservice.prototype.emitNotLoggedIn = function () {
        this.loginEmitter.emit(null);
    };
    Authservice.prototype.getLoggedInEmitter = function () {
        return this.loginEmitter;
    };
    //common method to check auth status
    Authservice.prototype.authenticated = function () {
        if (angular2_jwt_1.tokenNotExpired("auth_key")) {
            return true;
        }
        else {
            this.emitNotLoggedIn();
            return false;
        }
    };
    Authservice.prototype.logout = function () {
        if (localStorage.getItem("auth_key")) {
            this.authheaders = new http_1.Headers({ 'Content-Type': 'application/x-www-form-urlencoded' });
            this.authoptions = new http_1.RequestOptions({ headers: this.authheaders });
            return this.http.post(this.authUrl + "/connect/revoke", "token=" + localStorage.getItem("refresh_key") + "&token_type=refresh_token", this.authoptions)
                .map(function (res) { return res; })
                .catch(this.handleError);
        }
    };
    Authservice.prototype.login = function (inputType) {
        return this.http.post(this.authUrl + "/connect/token", this.tokenParams + "&username=" + inputType.username + "&password=" + inputType.password, this.options)
            .map(function (res) { return res.json(); })
            .catch(this.handleError);
    };
    Authservice.prototype.refreshLogin = function () {
        var refreshParams = "grant_type=refresh_token" +
            "&client_id=localApp" +
            "&resource=" + this.app.FileServer +
            "&refresh_token=" + localStorage.getItem("refresh_key"); // get refresh token stored when logged in 
        return this.http.post(this.authUrl + "/connect/token", refreshParams, this.options)
            .map(function (res) { return res.json(); })
            .catch(this.handleError);
    };
    Authservice.prototype.register = function (inputType) {
        var body = JSON.stringify(inputType);
        return this.http.post(this.authUrl + "/api/account/register", body, this.joptions)
            .map(function (res) { return res.json(); })
            .catch(this.handleError);
    };
    Authservice.prototype.handleError = function (error) {
        return Rx_1.Observable.throw(error || 'Server error');
    };
    Authservice = __decorate([
        core_1.Injectable(), 
        __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, http_1.Http, app_constants_1.Configuration])
    ], Authservice);
    return Authservice;
}());
exports.Authservice = Authservice;
//# sourceMappingURL=authorize-service.js.map