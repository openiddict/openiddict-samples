System.register(['@angular/core', '@angular/http', '../app.constants', 'rxjs/Rx', 'rxjs/add/operator/map'], function(exports_1, context_1) {
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
    var core_1, http_1, app_constants_1, Rx_1;
    var authervice;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (app_constants_1_1) {
                app_constants_1 = app_constants_1_1;
            },
            function (Rx_1_1) {
                Rx_1 = Rx_1_1;
            },
            function (_1) {}],
        execute: function() {
            authervice = (function () {
                function authervice(http, app) {
                    this.http = http;
                    this.app = app;
                    this._authUrl = this.app.Server;
                    this.headers = new http_1.Headers({ 'Content-Type': 'application/X-www-form-urlencoded' });
                    this.jheaders = new http_1.Headers({ 'Content-Type': 'application/json' });
                    this.options = new http_1.RequestOptions({ headers: this.headers });
                    this.joptions = new http_1.RequestOptions({ headers: this.jheaders });
                    this.tokenParams = "grant_type=password" +
                        "&resource=" + this.app.Server + "/" +
                        "&responseType=token" +
                        "&scope=offline_access profile email roles";
                }
                authervice.prototype.getUserInfo = function () {
                    if (localStorage.getItem("auth_key")) {
                        this.authheaders = new http_1.Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
                        this.Authoptions = new http_1.RequestOptions({ headers: this.authheaders });
                        return this.http.get(this._authUrl + "/api/Resource", this.Authoptions)
                            .map(function (res) { return res.json(); })
                            .catch(this.handleError);
                    }
                };
                authervice.prototype.logout = function () {
                    if (localStorage.getItem("auth_key")) {
                        this.authheaders = new http_1.Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
                        this.Authoptions = new http_1.RequestOptions({ headers: this.authheaders });
                        return this.http.get(this._authUrl + "/connect/logout", this.Authoptions)
                            .map(function (res) { return res; })
                            .catch(this.handleError);
                    }
                };
                authervice.prototype.Login = function (inputType) {
                    return this.http.post(this._authUrl + "/connect/token", this.tokenParams + "&username=" + inputType.username + "&password=" + inputType.password, this.options)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                authervice.prototype.refreshLogin = function () {
                    var body = "grant_type=refresh_token" +
                        "&resource=" + this.app.Server + "/" +
                        "&refresh_token=" + localStorage.getItem("refresh_key");
                    return this.http.post(this._authUrl + "/connect/token", body, this.options)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                authervice.prototype.Register = function (inputType) {
                    var body = JSON.stringify(inputType);
                    return this.http.post(this._authUrl + "/api/account/register", body, this.joptions)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                authervice.prototype.forgotPass = function (inputType) {
                    return this.http.get(this._authUrl + "/api/account/forgotPassword?email=" + inputType.Email, this.joptions)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                authervice.prototype.handleError = function (error) {
                    return Rx_1.Observable.throw(error || 'Server error');
                };
                authervice = __decorate([
                    core_1.Injectable(), 
                    __metadata('design:paramtypes', [http_1.Http, app_constants_1.Configuration])
                ], authervice);
                return authervice;
            }());
            exports_1("authervice", authervice);
        }
    }
});
//# sourceMappingURL=authoriza-service.js.map