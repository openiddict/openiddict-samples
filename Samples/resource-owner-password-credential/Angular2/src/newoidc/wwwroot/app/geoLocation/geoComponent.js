System.register(['@angular/core', './geoservice', './geo.shared'], function(exports_1, context_1) {
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
    var core_1, geoservice_1, geo_shared_1;
    var geoComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (geoservice_1_1) {
                geoservice_1 = geoservice_1_1;
            },
            function (geo_shared_1_1) {
                geo_shared_1 = geo_shared_1_1;
            }],
        execute: function() {
            geoComponent = (function () {
                function geoComponent(_geoService, geosharedService, cdr) {
                    this._geoService = _geoService;
                    this.geosharedService = geosharedService;
                    this.cdr = cdr;
                    this.geoLocation = new geo_shared_1.geoSharedModel;
                }
                geoComponent.prototype.ngOnInit = function () {
                    var instance = this, autocomplete;
                    if (navigator.geolocation) {
                        navigator.geolocation.getCurrentPosition(function (res) {
                            instance._geoService.getPlaces(res.coords.latitude, res.coords.longitude).subscribe(function (data) {
                                instance.geoMapper = data;
                                console.log(instance.geoMapper);
                                var result = instance.geoMapper.status;
                                var result2 = instance.geoMapper.results;
                                if (result != "undefined" || "" || null) {
                                    instance.setValue(result2[0].address_components[6].long_name, result2[0].address_components[5].long_name, result2[0].address_components[4].long_name);
                                }
                            });
                        });
                    }
                    instance.input = document.getElementById('google_places_ac');
                    autocomplete = new google.maps.places.Autocomplete(instance.input, { types: ['(cities)'] });
                    google.maps.event.addListener(autocomplete, 'place_changed', function () {
                        var place = autocomplete.getPlace();
                        console.log(place);
                        if (place.address_components[3] != undefined) {
                            instance.setValue(place.address_components[3].long_name, place.address_components[2].long_name, place.address_components[1].long_name);
                        }
                        else {
                            instance.setValue(place.address_components[2].long_name, place.address_components[1].long_name, place.address_components[0].long_name);
                        }
                    });
                };
                geoComponent.prototype.setValue = function (a, b, c) {
                    this.geoLocation.country = a;
                    this.geoLocation.state = b;
                    this.geoLocation.city = c;
                    this.geosharedService.broadcastTextChange(this.geoLocation);
                    this.cdr.detectChanges();
                };
                geoComponent = __decorate([
                    core_1.Component({
                        selector: 'my-location',
                        template: " <li>\n                    <form>\n                            <div class=\"input-field\">\n                                <input id=\"google_places_ac\" [(attr.state)]=\"geoLocation.state\" [(attr.country)]=\"geoLocation.coutnry\" name=\"google_places_ac\" type=\"text\" value=\"{{geoLocation.city}}\" class=\"form-control\" />\n                                <label class=\"active\" for=\"google_places_ac\"><i class=\"small material-icons\" style=\"font-size: 20px; color:#000000\">my_location</i></label>\n                            </div>\n                    </form>\n                </li>",
                        directives: [],
                        providers: []
                    }), 
                    __metadata('design:paramtypes', [geoservice_1.geoService, geo_shared_1.GeoSharedService, core_1.ChangeDetectorRef])
                ], geoComponent);
                return geoComponent;
            }());
            exports_1("geoComponent", geoComponent);
        }
    }
});
//# sourceMappingURL=geoComponent.js.map