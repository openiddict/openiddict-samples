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
    var GeoSharedService, geoSharedModel, geoResult;
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
            GeoSharedService = (function () {
                function GeoSharedService() {
                    this.LocationModel = new geoSharedModel();
                    this.country = new Subject_1.Subject();
                    this.address = new BehaviorSubject_1.BehaviorSubject(this.LocationModel);
                    this.searchTextStream$ = this.country.asObservable();
                }
                GeoSharedService.prototype.broadcastTextChange = function (text) {
                    this.address.next(text);
                    this.country.next(text);
                };
                GeoSharedService = __decorate([
                    core_1.Injectable(), 
                    __metadata('design:paramtypes', [])
                ], GeoSharedService);
                return GeoSharedService;
            }());
            exports_1("GeoSharedService", GeoSharedService);
            geoSharedModel = (function () {
                function geoSharedModel() {
                }
                return geoSharedModel;
            }());
            exports_1("geoSharedModel", geoSharedModel);
            geoResult = (function () {
                function geoResult() {
                }
                return geoResult;
            }());
            exports_1("geoResult", geoResult);
        }
    }
});
//# sourceMappingURL=geo.shared.js.map