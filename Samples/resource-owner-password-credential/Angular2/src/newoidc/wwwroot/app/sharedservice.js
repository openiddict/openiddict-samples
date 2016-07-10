System.register(['@angular/core', 'rxjs/Subject', 'rxjs/BehaviorSubject'], function(exports_1, context_1) {
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
    var core_1, Subject_1, BehaviorSubject_1;
    var AuthLoginService, SharedUserDetailsModel;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (Subject_1_1) {
                Subject_1 = Subject_1_1;
            },
            function (BehaviorSubject_1_1) {
                BehaviorSubject_1 = BehaviorSubject_1_1;
            }],
        execute: function() {
            AuthLoginService = (function () {
                function AuthLoginService() {
                    this.cehckStatus = new core_1.EventEmitter();
                    this.logout = new core_1.EventEmitter();
                    this.defaultshare = new SharedUserDetailsModel();
                    this.UserStatus = new Subject_1.Subject();
                    this.space = new BehaviorSubject_1.BehaviorSubject(this.defaultshare);
                    this.searchTextStream$ = this.UserStatus.asObservable();
                }
                AuthLoginService.prototype.emitNotLoggedIn = function () {
                    this.cehckStatus.emit(null);
                };
                AuthLoginService.prototype.emitLogOut = function () {
                    this.logout.emit(null);
                };
                AuthLoginService.prototype.getLoggedInEmitter = function () {
                    return this.cehckStatus;
                };
                AuthLoginService.prototype.getLoggedOutEmitter = function () {
                    return this.logout;
                };
                AuthLoginService.prototype.broadcastTextChange = function (text) {
                    this.space.next(text);
                    this.UserStatus.next(text);
                };
                AuthLoginService = __decorate([
                    core_1.Injectable(), 
                    __metadata('design:paramtypes', [])
                ], AuthLoginService);
                return AuthLoginService;
            }());
            exports_1("AuthLoginService", AuthLoginService);
            SharedUserDetailsModel = (function () {
                function SharedUserDetailsModel() {
                }
                return SharedUserDetailsModel;
            }());
            exports_1("SharedUserDetailsModel", SharedUserDetailsModel);
        }
    }
});
//# sourceMappingURL=sharedservice.js.map