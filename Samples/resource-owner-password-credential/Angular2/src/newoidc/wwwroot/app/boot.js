System.register(['@angular/platform-browser-dynamic', '@angular/http', '@angular/router-deprecated', './app.component', './app.constants', 'angular2-jwt', './authorize/authoriza-service', './geoLocation/geo.shared', './geoLocation/geoservice', './category/categoryService', './sharedservice', "angular2-materialize"], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var platform_browser_dynamic_1, http_1, router_deprecated_1, app_component_1, app_constants_1, angular2_jwt_1, authoriza_service_1, geo_shared_1, geoservice_1, categoryService_1, sharedservice_1;
    return {
        setters:[
            function (platform_browser_dynamic_1_1) {
                platform_browser_dynamic_1 = platform_browser_dynamic_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (router_deprecated_1_1) {
                router_deprecated_1 = router_deprecated_1_1;
            },
            function (app_component_1_1) {
                app_component_1 = app_component_1_1;
            },
            function (app_constants_1_1) {
                app_constants_1 = app_constants_1_1;
            },
            function (angular2_jwt_1_1) {
                angular2_jwt_1 = angular2_jwt_1_1;
            },
            function (authoriza_service_1_1) {
                authoriza_service_1 = authoriza_service_1_1;
            },
            function (geo_shared_1_1) {
                geo_shared_1 = geo_shared_1_1;
            },
            function (geoservice_1_1) {
                geoservice_1 = geoservice_1_1;
            },
            function (categoryService_1_1) {
                categoryService_1 = categoryService_1_1;
            },
            function (sharedservice_1_1) {
                sharedservice_1 = sharedservice_1_1;
            },
            function (_1) {}],
        execute: function() {
            platform_browser_dynamic_1.bootstrap(app_component_1.AppComponent, [
                angular2_jwt_1.JwtHelper,
                http_1.HTTP_PROVIDERS, authoriza_service_1.authervice, router_deprecated_1.ROUTER_PROVIDERS, sharedservice_1.AuthLoginService,
                geo_shared_1.GeoSharedService, geoservice_1.geoService, categoryService_1.categoryService,
                app_constants_1.Configuration
            ]);
        }
    }
});
//# sourceMappingURL=boot.js.map