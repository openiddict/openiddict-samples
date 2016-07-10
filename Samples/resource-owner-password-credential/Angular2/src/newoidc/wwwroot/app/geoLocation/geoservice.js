System.register(['@angular/core', '@angular/http', 'rxjs/add/operator/map', 'rxjs/add/operator/catch'], function(exports_1, context_1) {
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
    var core_1, http_1;
    var geoService;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (_1) {},
            function (_2) {}],
        execute: function() {
            geoService = (function () {
                function geoService(http) {
                    this.http = http;
                }
                geoService.prototype.getPlaces = function (xml_Latitude, xml_Lang) {
                    return this.http.get("http://maps.googleapis.com/maps/api/geocode/json?latlng=" + xml_Latitude + "," + xml_Lang + "&sensor=true")
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                geoService.prototype.handleError = function (error) {
                    console.error(error);
                    return error.json().error || 'Server error';
                };
                geoService = __decorate([
                    core_1.Injectable(), 
                    __metadata('design:paramtypes', [http_1.Http])
                ], geoService);
                return geoService;
            }());
            exports_1("geoService", geoService);
        }
    }
});
//# sourceMappingURL=geoservice.js.map