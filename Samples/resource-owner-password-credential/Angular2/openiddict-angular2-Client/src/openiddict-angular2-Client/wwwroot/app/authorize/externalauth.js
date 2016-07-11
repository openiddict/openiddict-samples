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
/*
This class simply takes new token from the server and saves it in localstorage for further usage
*/
var core_1 = require('@angular/core');
var extauthorizeComponent = (function () {
    function extauthorizeComponent() {
        //get id token from urls part 
        var x = location.hash;
        var extToken = x.replace("#id_token=", "");
        //save into localstorage
        localStorage.setItem("auth_key", extToken);
    }
    extauthorizeComponent = __decorate([
        core_1.Component({
            selector: 'authorize',
            template: '<h3>Successfully authorized </h3><h4>Loading ...</h4>',
            directives: []
        }), 
        __metadata('design:paramtypes', [])
    ], extauthorizeComponent);
    return extauthorizeComponent;
}());
exports.extauthorizeComponent = extauthorizeComponent;
;
//# sourceMappingURL=externalauth.js.map