System.register(['@angular/core', '../sharedservice'], function(exports_1, context_1) {
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
    var core_1, sharedservice_1;
    var auth_nav;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (sharedservice_1_1) {
                sharedservice_1 = sharedservice_1_1;
            }],
        execute: function() {
            auth_nav = (function () {
                function auth_nav(_AuthLoginService, cdr) {
                    this._AuthLoginService = _AuthLoginService;
                    this.cdr = cdr;
                }
                auth_nav.prototype.ngOnInit = function () {
                    var _this = this;
                    this._AuthLoginService.space.subscribe(function (val) {
                        _this.token = val;
                        console.log(_this.token);
                    });
                };
                auth_nav.prototype.mopen = function () {
                    this._AuthLoginService.emitNotLoggedIn();
                    this.cdr.detectChanges();
                };
                auth_nav.prototype.mclose = function () {
                    this._AuthLoginService.emitLogOut();
                    this.cdr.detectChanges();
                };
                auth_nav = __decorate([
                    core_1.Component({
                        selector: 'authorize-nav',
                        template: "<li [hidden]=\"token.isLoggedIn\"><button class=\"waves-effect waves-teal btn-flat\" (click)=\"mopen()\">Login</button></li>\n\n\n<li [hidden]=\"!token.isLoggedIn\">Welcome {{token.username}}</li>\n<li [hidden]=\"!token.isLoggedIn\"><button class=\"waves-effect waves-teal btn-flat\" (click)=\"mclose()\">Logout</button></li>",
                    }), 
                    __metadata('design:paramtypes', [sharedservice_1.AuthLoginService, core_1.ChangeDetectorRef])
                ], auth_nav);
                return auth_nav;
            }());
            exports_1("auth_nav", auth_nav);
        }
    }
});
//# sourceMappingURL=authorize_nav.js.map