System.register(['@angular/core', '@angular/router-deprecated', '@angular/http', 'angular2-jwt', './authorize/authorize-component', './authorize/authorize_nav', './User/user-component', './welcome-component', './authorize/externalauth', './authorize/authoriza-service', './sharedservice', './virtual-proxy', './geoLocation/geoComponent', './geoLocation/geo.shared', './geoLocation/geoservice', "angular2-materialize"], function(exports_1, context_1) {
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
    var core_1, router_deprecated_1, http_1, angular2_jwt_1, authorize_component_1, authorize_nav_1, user_component_1, welcome_component_1, externalauth_1, router_deprecated_2, authoriza_service_1, sharedservice_1, virtual_proxy_1, geoComponent_1, geo_shared_1, geoservice_1, angular2_materialize_1;
    var AppComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (router_deprecated_1_1) {
                router_deprecated_1 = router_deprecated_1_1;
                router_deprecated_2 = router_deprecated_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (angular2_jwt_1_1) {
                angular2_jwt_1 = angular2_jwt_1_1;
            },
            function (authorize_component_1_1) {
                authorize_component_1 = authorize_component_1_1;
            },
            function (authorize_nav_1_1) {
                authorize_nav_1 = authorize_nav_1_1;
            },
            function (user_component_1_1) {
                user_component_1 = user_component_1_1;
            },
            function (welcome_component_1_1) {
                welcome_component_1 = welcome_component_1_1;
            },
            function (externalauth_1_1) {
                externalauth_1 = externalauth_1_1;
            },
            function (authoriza_service_1_1) {
                authoriza_service_1 = authoriza_service_1_1;
            },
            function (sharedservice_1_1) {
                sharedservice_1 = sharedservice_1_1;
            },
            function (virtual_proxy_1_1) {
                virtual_proxy_1 = virtual_proxy_1_1;
            },
            function (geoComponent_1_1) {
                geoComponent_1 = geoComponent_1_1;
            },
            function (geo_shared_1_1) {
                geo_shared_1 = geo_shared_1_1;
            },
            function (geoservice_1_1) {
                geoservice_1 = geoservice_1_1;
            },
            function (angular2_materialize_1_1) {
                angular2_materialize_1 = angular2_materialize_1_1;
            }],
        execute: function() {
            AppComponent = (function () {
                function AppComponent(jwtHelper, _http, _parentRouter, Authentication, od, _geoService, geosharedService) {
                    var _this = this;
                    this.jwtHelper = jwtHelper;
                    this._http = _http;
                    this._parentRouter = _parentRouter;
                    this.Authentication = Authentication;
                    this.od = od;
                    this._geoService = _geoService;
                    this.geosharedService = geosharedService;
                    this.item = 0;
                    this.subscription = this.od.getLoggedInEmitter()
                        .subscribe(function (item) { _this.authcheck(); });
                    this.subscription = this.od.getLoggedOutEmitter()
                        .subscribe(function (item) { _this.logOut(); });
                }
                AppComponent.prototype.logOut = function () {
                    this.authorizeComponentRefer.Logout();
                };
                AppComponent.prototype.authcheck = function () {
                    if (localStorage.getItem('auth_key') && !this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) {
                        this._parentRouter.navigate(['/Dashboard']);
                        this.authorizeComponentRefer.logstatus();
                    }
                    else {
                        this.authorizeComponentRefer.mopen();
                    }
                };
                __decorate([
                    core_1.ViewChild(authorize_component_1.authorizeComponent), 
                    __metadata('design:type', authorize_component_1.authorizeComponent)
                ], AppComponent.prototype, "authorizeComponentRefer", void 0);
                AppComponent = __decorate([
                    router_deprecated_1.RouteConfig([
                        { path: '/', name: 'Default', component: welcome_component_1.welcome, useAsDefault: true },
                        { path: '/login', name: 'Login', component: authorize_component_1.authorizeComponent },
                        { path: '/dashboard', name: 'Dashboard', component: user_component_1.userComponent },
                        { path: '/signin-oidc', name: 'Extauth', component: externalauth_1.extauthorizeComponent },
                        {
                            path: '/Category',
                            component: virtual_proxy_1.componentProxyFactory({
                                path: '/app/category/category-component',
                                provide: function (m) { return m.categoryComponent; }
                            }), useAsDefault: false,
                            name: 'Category'
                        },
                        {
                            path: '/Product',
                            component: virtual_proxy_1.componentProxyFactory({
                                path: '/app/Product/productComponent',
                                provide: function (m) { return m.ProductComponent; }
                            }),
                            name: 'Product'
                        },
                        {
                            path: '/searchProduct',
                            component: virtual_proxy_1.componentProxyFactory({
                                path: '/app/Product/proListComponent',
                                provide: function (m) { return m.ProductListComponent; }
                            }),
                            name: 'SearchProduct'
                        }
                    ]),
                    core_1.Component({
                        selector: 'body',
                        templateUrl: 'app/app.component.html',
                        directives: [angular2_materialize_1.MaterializeDirective, router_deprecated_1.ROUTER_DIRECTIVES, authorize_component_1.authorizeComponent, authorize_nav_1.auth_nav, geoComponent_1.geoComponent],
                        providers: []
                    }), 
                    __metadata('design:paramtypes', [angular2_jwt_1.JwtHelper, http_1.Http, router_deprecated_2.Router, authoriza_service_1.authervice, sharedservice_1.AuthLoginService, geoservice_1.geoService, geo_shared_1.GeoSharedService])
                ], AppComponent);
                return AppComponent;
            }());
            exports_1("AppComponent", AppComponent);
        }
    }
});
//# sourceMappingURL=app.component.js.map