"use strict";
var platform_browser_dynamic_1 = require('@angular/platform-browser-dynamic');
var http_1 = require('@angular/http');
var app_route_1 = require('./app.route');
var app_component_1 = require('./app.component');
var app_constants_1 = require('./app.constants');
var core_1 = require('@angular/core');
var angular2_jwt_1 = require('angular2-jwt');
var authorize_service_1 = require('./authorize/authorize-service');
var resource_service_1 = require('./resource/resource-service');
var forms_1 = require('@angular/forms');
core_1.enableProdMode();
platform_browser_dynamic_1.bootstrap(app_component_1.AppComponent, [forms_1.disableDeprecatedForms(),
    forms_1.provideForms(),
    angular2_jwt_1.JwtHelper,
    http_1.HTTP_PROVIDERS,
    authorize_service_1.Authservice,
    resource_service_1.ResourceService,
    app_route_1.APP_ROUTER_PROVIDERS,
    app_constants_1.Configuration]);
//# sourceMappingURL=boot.js.map