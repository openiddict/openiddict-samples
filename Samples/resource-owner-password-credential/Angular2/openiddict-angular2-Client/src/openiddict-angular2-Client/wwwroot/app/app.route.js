"use strict";
var router_1 = require('@angular/router');
var authorize_component_1 = require('./authorize/authorize-component');
var user_component_1 = require('./User/user-component');
var welcome_component_1 = require('./welcome-component');
var externalauth_1 = require('./authorize/externalauth');
exports.routes = [
    { path: '', component: welcome_component_1.welcome },
    { path: 'login', component: authorize_component_1.authorizeComponent },
    { path: 'dashboard', component: user_component_1.userComponent },
    { path: 'signin-oidc', component: externalauth_1.extauthorizeComponent }
];
exports.APP_ROUTER_PROVIDERS = [
    router_1.provideRouter(exports.routes)
];
//# sourceMappingURL=app.route.js.map