"use strict";
var platform_browser_dynamic_1 = require('@angular/platform-browser-dynamic');
var http_1 = require('@angular/http');
var app_route_1 = require('./app.route');
var app_component_1 = require('./app.component');
var app_constants_1 = require('./app.constants');
var angular2_jwt_1 = require('angular2-jwt');
var authoriza_service_1 = require('./authorize/authoriza-service');
platform_browser_dynamic_1.bootstrap(app_component_1.AppComponent, [
    angular2_jwt_1.JwtHelper,
    http_1.HTTP_PROVIDERS, authoriza_service_1.authervice, app_route_1.APP_ROUTER_PROVIDERS,
    app_constants_1.Configuration
]);
//# sourceMappingURL=boot.js.map