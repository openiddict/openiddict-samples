"use strict";
require('rxjs/add/operator/map');
require('rxjs/add/operator/scan');
require('rxjs/add/operator/mergeMap');
require('rxjs/add/operator/concat');
require('rxjs/add/operator/concatMap');
require('rxjs/add/operator/every');
require('rxjs/add/operator/mergeAll');
require('rxjs/add/observable/from');
var core_1 = require('@angular/core');
var Observable_1 = require('rxjs/Observable');
var Subject_1 = require('rxjs/Subject');
var of_1 = require('rxjs/observable/of');
var apply_redirects_1 = require('./apply_redirects');
var config_1 = require('./config');
var create_router_state_1 = require('./create_router_state');
var create_url_tree_1 = require('./create_url_tree');
var recognize_1 = require('./recognize');
var resolve_1 = require('./resolve');
var router_outlet_map_1 = require('./router_outlet_map');
var router_state_1 = require('./router_state');
var shared_1 = require('./shared');
var url_tree_1 = require('./url_tree');
var collection_1 = require('./utils/collection');
var NavigationStart = (function () {
    function NavigationStart(id, url) {
        this.id = id;
        this.url = url;
    }
    NavigationStart.prototype.toString = function () { return "NavigationStart(id: " + this.id + ", url: '" + this.url + "')"; };
    return NavigationStart;
}());
exports.NavigationStart = NavigationStart;
var NavigationEnd = (function () {
    function NavigationEnd(id, url, urlAfterRedirects) {
        this.id = id;
        this.url = url;
        this.urlAfterRedirects = urlAfterRedirects;
    }
    NavigationEnd.prototype.toString = function () {
        return "NavigationEnd(id: " + this.id + ", url: '" + this.url + "', urlAfterRedirects: '" + this.urlAfterRedirects + "')";
    };
    return NavigationEnd;
}());
exports.NavigationEnd = NavigationEnd;
var NavigationCancel = (function () {
    function NavigationCancel(id, url) {
        this.id = id;
        this.url = url;
    }
    NavigationCancel.prototype.toString = function () { return "NavigationCancel(id: " + this.id + ", url: '" + this.url + "')"; };
    return NavigationCancel;
}());
exports.NavigationCancel = NavigationCancel;
var NavigationError = (function () {
    function NavigationError(id, url, error) {
        this.id = id;
        this.url = url;
        this.error = error;
    }
    NavigationError.prototype.toString = function () {
        return "NavigationError(id: " + this.id + ", url: '" + this.url + "', error: " + this.error + ")";
    };
    return NavigationError;
}());
exports.NavigationError = NavigationError;
var RoutesRecognized = (function () {
    function RoutesRecognized(id, url, urlAfterRedirects, state) {
        this.id = id;
        this.url = url;
        this.urlAfterRedirects = urlAfterRedirects;
        this.state = state;
    }
    RoutesRecognized.prototype.toString = function () {
        return "RoutesRecognized(id: " + this.id + ", url: '" + this.url + "', urlAfterRedirects: '" + this.urlAfterRedirects + "', state: " + this.state + ")";
    };
    return RoutesRecognized;
}());
exports.RoutesRecognized = RoutesRecognized;
var Router = (function () {
    function Router(rootComponentType, resolver, urlSerializer, outletMap, location, injector, config) {
        this.rootComponentType = rootComponentType;
        this.resolver = resolver;
        this.urlSerializer = urlSerializer;
        this.outletMap = outletMap;
        this.location = location;
        this.injector = injector;
        this.navigationId = 0;
        this.resetConfig(config);
        this.routerEvents = new Subject_1.Subject();
        this.currentUrlTree = url_tree_1.createEmptyUrlTree();
        this.currentRouterState = router_state_1.createEmptyState(this.currentUrlTree, this.rootComponentType);
    }
    Router.prototype.initialNavigation = function () {
        this.setUpLocationChangeListener();
        this.navigateByUrl(this.location.path());
    };
    Object.defineProperty(Router.prototype, "routerState", {
        get: function () { return this.currentRouterState; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Router.prototype, "url", {
        get: function () { return this.serializeUrl(this.currentUrlTree); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Router.prototype, "events", {
        get: function () { return this.routerEvents; },
        enumerable: true,
        configurable: true
    });
    Router.prototype.resetConfig = function (config) {
        config_1.validateConfig(config);
        this.config = config;
    };
    Router.prototype.dispose = function () { this.locationSubscription.unsubscribe(); };
    Router.prototype.createUrlTree = function (commands, _a) {
        var _b = _a === void 0 ? {} : _a, relativeTo = _b.relativeTo, queryParams = _b.queryParams, fragment = _b.fragment;
        var a = relativeTo ? relativeTo : this.routerState.root;
        return create_url_tree_1.createUrlTree(a, this.currentUrlTree, commands, queryParams, fragment);
    };
    Router.prototype.navigateByUrl = function (url) {
        if (url instanceof url_tree_1.UrlTree) {
            return this.scheduleNavigation(url, false);
        }
        else {
            var urlTree = this.urlSerializer.parse(url);
            return this.scheduleNavigation(urlTree, false);
        }
    };
    Router.prototype.navigate = function (commands, extras) {
        if (extras === void 0) { extras = {}; }
        return this.scheduleNavigation(this.createUrlTree(commands, extras), false);
    };
    Router.prototype.serializeUrl = function (url) { return this.urlSerializer.serialize(url); };
    Router.prototype.parseUrl = function (url) { return this.urlSerializer.parse(url); };
    Router.prototype.scheduleNavigation = function (url, preventPushState) {
        var _this = this;
        var id = ++this.navigationId;
        this.routerEvents.next(new NavigationStart(id, this.serializeUrl(url)));
        return Promise.resolve().then(function (_) { return _this.runNavigate(url, preventPushState, id); });
    };
    Router.prototype.setUpLocationChangeListener = function () {
        var _this = this;
        this.locationSubscription = this.location.subscribe(function (change) {
            return _this.scheduleNavigation(_this.urlSerializer.parse(change['url']), change['pop']);
        });
    };
    Router.prototype.runNavigate = function (url, preventPushState, id) {
        var _this = this;
        if (id !== this.navigationId) {
            this.location.go(this.urlSerializer.serialize(this.currentUrlTree));
            this.routerEvents.next(new NavigationCancel(id, this.serializeUrl(url)));
            return Promise.resolve(false);
        }
        return new Promise(function (resolvePromise, rejectPromise) {
            var updatedUrl;
            var state;
            apply_redirects_1.applyRedirects(url, _this.config)
                .mergeMap(function (u) {
                updatedUrl = u;
                return recognize_1.recognize(_this.rootComponentType, _this.config, updatedUrl, _this.serializeUrl(updatedUrl));
            })
                .mergeMap(function (newRouterStateSnapshot) {
                _this.routerEvents.next(new RoutesRecognized(id, _this.serializeUrl(url), _this.serializeUrl(updatedUrl), newRouterStateSnapshot));
                return resolve_1.resolve(_this.resolver, newRouterStateSnapshot);
            })
                .map(function (routerStateSnapshot) {
                return create_router_state_1.createRouterState(routerStateSnapshot, _this.currentRouterState);
            })
                .map(function (newState) {
                state = newState;
            })
                .mergeMap(function (_) {
                return new GuardChecks(state.snapshot, _this.currentRouterState.snapshot, _this.injector)
                    .check(_this.outletMap);
            })
                .forEach(function (shouldActivate) {
                if (!shouldActivate || id !== _this.navigationId) {
                    _this.routerEvents.next(new NavigationCancel(id, _this.serializeUrl(url)));
                    return Promise.resolve(false);
                }
                new ActivateRoutes(state, _this.currentRouterState).activate(_this.outletMap);
                _this.currentUrlTree = updatedUrl;
                _this.currentRouterState = state;
                if (!preventPushState) {
                    var path = _this.urlSerializer.serialize(updatedUrl);
                    if (_this.location.isCurrentPathEqualTo(path)) {
                        _this.location.replaceState(path);
                    }
                    else {
                        _this.location.go(path);
                    }
                }
                return Promise.resolve(true);
            })
                .then(function () {
                _this.routerEvents.next(new NavigationEnd(id, _this.serializeUrl(url), _this.serializeUrl(updatedUrl)));
                resolvePromise(true);
            }, function (e) {
                _this.routerEvents.next(new NavigationError(id, _this.serializeUrl(url), e));
                rejectPromise(e);
            });
        });
    };
    return Router;
}());
exports.Router = Router;
var CanActivate = (function () {
    function CanActivate(route) {
        this.route = route;
    }
    return CanActivate;
}());
var CanDeactivate = (function () {
    function CanDeactivate(component, route) {
        this.component = component;
        this.route = route;
    }
    return CanDeactivate;
}());
var GuardChecks = (function () {
    function GuardChecks(future, curr, injector) {
        this.future = future;
        this.curr = curr;
        this.injector = injector;
        this.checks = [];
    }
    GuardChecks.prototype.check = function (parentOutletMap) {
        var _this = this;
        var futureRoot = this.future._root;
        var currRoot = this.curr ? this.curr._root : null;
        this.traverseChildRoutes(futureRoot, currRoot, parentOutletMap);
        if (this.checks.length === 0)
            return of_1.of(true);
        return Observable_1.Observable.from(this.checks)
            .map(function (s) {
            if (s instanceof CanActivate) {
                return _this.runCanActivate(s.route);
            }
            else if (s instanceof CanDeactivate) {
                return _this.runCanDeactivate(s.component, s.route);
            }
            else {
                throw new Error('Cannot be reached');
            }
        })
            .mergeAll()
            .every(function (result) { return result === true; });
    };
    GuardChecks.prototype.traverseChildRoutes = function (futureNode, currNode, outletMap) {
        var _this = this;
        var prevChildren = nodeChildrenAsMap(currNode);
        futureNode.children.forEach(function (c) {
            _this.traverseRoutes(c, prevChildren[c.value.outlet], outletMap);
            delete prevChildren[c.value.outlet];
        });
        collection_1.forEach(prevChildren, function (v, k) { return _this.deactivateOutletAndItChildren(v, outletMap._outlets[k]); });
    };
    GuardChecks.prototype.traverseRoutes = function (futureNode, currNode, parentOutletMap) {
        var future = futureNode.value;
        var curr = currNode ? currNode.value : null;
        var outlet = parentOutletMap ? parentOutletMap._outlets[futureNode.value.outlet] : null;
        if (curr && future._routeConfig === curr._routeConfig) {
            if (!collection_1.shallowEqual(future.params, curr.params)) {
                this.checks.push(new CanDeactivate(outlet.component, curr), new CanActivate(future));
            }
            this.traverseChildRoutes(futureNode, currNode, outlet ? outlet.outletMap : null);
        }
        else {
            this.deactivateOutletAndItChildren(curr, outlet);
            this.checks.push(new CanActivate(future));
            this.traverseChildRoutes(futureNode, null, outlet ? outlet.outletMap : null);
        }
    };
    GuardChecks.prototype.deactivateOutletAndItChildren = function (route, outlet) {
        var _this = this;
        if (outlet && outlet.isActivated) {
            collection_1.forEach(outlet.outletMap._outlets, function (v) {
                if (v.isActivated) {
                    _this.deactivateOutletAndItChildren(v.activatedRoute.snapshot, v);
                }
            });
            this.checks.push(new CanDeactivate(outlet.component, route));
        }
    };
    GuardChecks.prototype.runCanActivate = function (future) {
        var _this = this;
        var canActivate = future._routeConfig ? future._routeConfig.canActivate : null;
        if (!canActivate || canActivate.length === 0)
            return of_1.of(true);
        return Observable_1.Observable.from(canActivate)
            .map(function (c) {
            var guard = _this.injector.get(c);
            if (guard.canActivate) {
                return wrapIntoObservable(guard.canActivate(future, _this.future));
            }
            else {
                return wrapIntoObservable(guard(future, _this.future));
            }
        })
            .mergeAll()
            .every(function (result) { return result === true; });
    };
    GuardChecks.prototype.runCanDeactivate = function (component, curr) {
        var _this = this;
        var canDeactivate = curr._routeConfig ? curr._routeConfig.canDeactivate : null;
        if (!canDeactivate || canDeactivate.length === 0)
            return of_1.of(true);
        return Observable_1.Observable.from(canDeactivate)
            .map(function (c) {
            var guard = _this.injector.get(c);
            if (guard.canDeactivate) {
                return wrapIntoObservable(guard.canDeactivate(component, curr, _this.curr));
            }
            else {
                return wrapIntoObservable(guard(component, curr, _this.curr));
            }
        })
            .mergeAll()
            .every(function (result) { return result === true; });
    };
    return GuardChecks;
}());
function wrapIntoObservable(value) {
    if (value instanceof Observable_1.Observable) {
        return value;
    }
    else {
        return of_1.of(value);
    }
}
var ActivateRoutes = (function () {
    function ActivateRoutes(futureState, currState) {
        this.futureState = futureState;
        this.currState = currState;
    }
    ActivateRoutes.prototype.activate = function (parentOutletMap) {
        var futureRoot = this.futureState._root;
        var currRoot = this.currState ? this.currState._root : null;
        pushQueryParamsAndFragment(this.futureState);
        this.activateChildRoutes(futureRoot, currRoot, parentOutletMap);
    };
    ActivateRoutes.prototype.activateChildRoutes = function (futureNode, currNode, outletMap) {
        var _this = this;
        var prevChildren = nodeChildrenAsMap(currNode);
        futureNode.children.forEach(function (c) {
            _this.activateRoutes(c, prevChildren[c.value.outlet], outletMap);
            delete prevChildren[c.value.outlet];
        });
        collection_1.forEach(prevChildren, function (v, k) { return _this.deactivateOutletAndItChildren(outletMap._outlets[k]); });
    };
    ActivateRoutes.prototype.activateRoutes = function (futureNode, currNode, parentOutletMap) {
        var future = futureNode.value;
        var curr = currNode ? currNode.value : null;
        var outlet = getOutlet(parentOutletMap, futureNode.value);
        if (future === curr) {
            router_state_1.advanceActivatedRoute(future);
            this.activateChildRoutes(futureNode, currNode, outlet.outletMap);
        }
        else {
            this.deactivateOutletAndItChildren(outlet);
            var outletMap = new router_outlet_map_1.RouterOutletMap();
            this.activateNewRoutes(outletMap, future, outlet);
            this.activateChildRoutes(futureNode, null, outletMap);
        }
    };
    ActivateRoutes.prototype.activateNewRoutes = function (outletMap, future, outlet) {
        var resolved = core_1.ReflectiveInjector.resolve([
            { provide: router_state_1.ActivatedRoute, useValue: future },
            { provide: router_outlet_map_1.RouterOutletMap, useValue: outletMap }
        ]);
        router_state_1.advanceActivatedRoute(future);
        outlet.activate(future._futureSnapshot._resolvedComponentFactory, future, resolved, outletMap);
    };
    ActivateRoutes.prototype.deactivateOutletAndItChildren = function (outlet) {
        var _this = this;
        if (outlet && outlet.isActivated) {
            collection_1.forEach(outlet.outletMap._outlets, function (v) { return _this.deactivateOutletAndItChildren(v); });
            outlet.deactivate();
        }
    };
    return ActivateRoutes;
}());
function pushQueryParamsAndFragment(state) {
    if (!collection_1.shallowEqual(state.snapshot.queryParams, state.queryParams.value)) {
        state.queryParams.next(state.snapshot.queryParams);
    }
    if (state.snapshot.fragment !== state.fragment.value) {
        state.fragment.next(state.snapshot.fragment);
    }
}
function nodeChildrenAsMap(node) {
    return node ? node.children.reduce(function (m, c) {
        m[c.value.outlet] = c;
        return m;
    }, {}) : {};
}
function getOutlet(outletMap, route) {
    var outlet = outletMap._outlets[route.outlet];
    if (!outlet) {
        var componentName = route.component.name;
        if (route.outlet === shared_1.PRIMARY_OUTLET) {
            throw new Error("Cannot find primary outlet to load '" + componentName + "'");
        }
        else {
            throw new Error("Cannot find the outlet " + route.outlet + " to load '" + componentName + "'");
        }
    }
    return outlet;
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicm91dGVyLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vc3JjL3JvdXRlci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUEsUUFBTyx1QkFBdUIsQ0FBQyxDQUFBO0FBQy9CLFFBQU8sd0JBQXdCLENBQUMsQ0FBQTtBQUNoQyxRQUFPLDRCQUE0QixDQUFDLENBQUE7QUFDcEMsUUFBTywwQkFBMEIsQ0FBQyxDQUFBO0FBQ2xDLFFBQU8sNkJBQTZCLENBQUMsQ0FBQTtBQUNyQyxRQUFPLHlCQUF5QixDQUFDLENBQUE7QUFDakMsUUFBTyw0QkFBNEIsQ0FBQyxDQUFBO0FBQ3BDLFFBQU8sMEJBQTBCLENBQUMsQ0FBQTtBQUdsQyxxQkFBb0UsZUFBZSxDQUFDLENBQUE7QUFDcEYsMkJBQXlCLGlCQUFpQixDQUFDLENBQUE7QUFDM0Msd0JBQXNCLGNBQWMsQ0FBQyxDQUFBO0FBRXJDLG1CQUFrQixvQkFBb0IsQ0FBQyxDQUFBO0FBRXZDLGdDQUE2QixtQkFBbUIsQ0FBQyxDQUFBO0FBQ2pELHVCQUEyQyxVQUFVLENBQUMsQ0FBQTtBQUN0RCxvQ0FBZ0MsdUJBQXVCLENBQUMsQ0FBQTtBQUN4RCxnQ0FBNEIsbUJBQW1CLENBQUMsQ0FBQTtBQUVoRCwwQkFBd0IsYUFBYSxDQUFDLENBQUE7QUFDdEMsd0JBQXNCLFdBQVcsQ0FBQyxDQUFBO0FBQ2xDLGtDQUE4QixxQkFBcUIsQ0FBQyxDQUFBO0FBQ3BELDZCQUFnSSxnQkFBZ0IsQ0FBQyxDQUFBO0FBQ2pKLHVCQUFxQyxVQUFVLENBQUMsQ0FBQTtBQUVoRCx5QkFBMEMsWUFBWSxDQUFDLENBQUE7QUFDdkQsMkJBQW9DLG9CQUFvQixDQUFDLENBQUE7QUFZekQ7SUFDRSx5QkFBbUIsRUFBVSxFQUFTLEdBQVc7UUFBOUIsT0FBRSxHQUFGLEVBQUUsQ0FBUTtRQUFTLFFBQUcsR0FBSCxHQUFHLENBQVE7SUFBRyxDQUFDO0lBRXJELGtDQUFRLEdBQVIsY0FBcUIsTUFBTSxDQUFDLHlCQUF1QixJQUFJ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LGFBQWEsR0FBUyxLQUFLLENBQUMsU0FBVSxDQUFDLElBQUksQ0FBQztRQUNsRCxFQUFFLENBQUMsQ0FBQyxLQUFLLENBQUMsTUFBTSxLQUFLLHVCQUFjLENBQUMsQ0FBQyxDQUFDO1lBQ3BDLE1BQU0sSUFBSSxLQUFLLENBQUMseUNBQXVDLGFBQWEsTUFBRyxDQUFDLENBQUM7UUFDM0UsQ0FBQztRQUFDLElBQUksQ0FBQyxDQUFDO1lBQ04sTUFBTSxJQUFJLEtBQUssQ0FBQyw0QkFBMEIsS0FBSyxDQUFDLE1BQU0sa0JBQWEsYUFBYSxNQUFHLENBQUMsQ0FBQztRQUN2RixDQUFDO0lBQ0gsQ0FBQztJQUNELE1BQU0sQ0FBQyxNQUFNLENBQUM7QUFDaEIsQ0FBQyIsInNvdXJjZXNDb250ZW50IjpbImltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3IvbWFwJztcbmltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3Ivc2Nhbic7XG5pbXBvcnQgJ3J4anMvYWRkL29wZXJhdG9yL21lcmdlTWFwJztcbmltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3IvY29uY2F0JztcbmltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3IvY29uY2F0TWFwJztcbmltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3IvZXZlcnknO1xuaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9tZXJnZUFsbCc7XG5pbXBvcnQgJ3J4anMvYWRkL29ic2VydmFibGUvZnJvbSc7XG5cbmltcG9ydCB7TG9jYXRpb259IGZyb20gJ0Bhbmd1bGFyL2NvbW1vbic7XG5pbXBvcnQge0NvbXBvbmVudFJlc29sdmVyLCBJbmplY3RvciwgUmVmbGVjdGl2ZUluamVjdG9yLCBUeXBlfSBmcm9tICdAYW5ndWxhci9jb3JlJztcbmltcG9ydCB7T2JzZXJ2YWJsZX0gZnJvbSAncnhqcy9PYnNlcnZhYmxlJztcbmltcG9ydCB7U3ViamVjdH0gZnJvbSAncnhqcy9TdWJqZWN0JztcbmltcG9ydCB7U3Vic2NyaXB0aW9ufSBmcm9tICdyeGpzL1N1YnNjcmlwdGlvbic7XG5pbXBvcnQge29mIH0gZnJvbSAncnhqcy9vYnNlcnZhYmxlL29mJztcblxuaW1wb3J0IHthcHBseVJlZGlyZWN0c30gZnJvbSAnLi9hcHBseV9yZWRpcmVjdHMnO1xuaW1wb3J0IHtSb3V0ZXJDb25maWcsIHZhbGlkYXRlQ29uZmlnfSBmcm9tICcuL2NvbmZpZyc7XG5pbXBvcnQge2NyZWF0ZVJvdXRlclN0YXRlfSBmcm9tICcuL2NyZWF0ZV9yb3V0ZXJfc3RhdGUnO1xuaW1wb3J0IHtjcmVhdGVVcmxUcmVlfSBmcm9tICcuL2NyZWF0ZV91cmxfdHJlZSc7XG5pbXBvcnQge1JvdXRlck91dGxldH0gZnJvbSAnLi9kaXJlY3RpdmVzL3JvdXRlcl9vdXRsZXQnO1xuaW1wb3J0IHtyZWNvZ25pemV9IGZyb20gJy4vcmVjb2duaXplJztcbmltcG9ydCB7cmVzb2x2ZX0gZnJvbSAnLi9yZXNvbHZlJztcbmltcG9ydCB7Um91dGVyT3V0bGV0TWFwfSBmcm9tICcuL3JvdXRlcl9vdXRsZXRfbWFwJztcbmltcG9ydCB7QWN0aXZhdGVkUm91dGUsIEFjdGl2YXRlZFJvdXRlU25hcHNob3QsIFJvdXRlclN0YXRlLCBSb3V0ZXJTdGF0ZVNuYXBzaG90LCBhZHZhbmNlQWN0aXZhdGVkUm91dGUsIGNyZWF0ZUVtcHR5U3RhdGV9IGZyb20gJy4vcm91dGVyX3N0YXRlJztcbmltcG9ydCB7UFJJTUFSWV9PVVRMRVQsIFBhcmFtc30gZnJvbSAnLi9zaGFyZWQnO1xuaW1wb3J0IHtVcmxTZXJpYWxpemVyfSBmcm9tICcuL3VybF9zZXJpYWxpemVyJztcbmltcG9ydCB7VXJsVHJlZSwgY3JlYXRlRW1wdHlVcmxUcmVlfSBmcm9tICcuL3VybF90cmVlJztcbmltcG9ydCB7Zm9yRWFjaCwgc2hhbGxvd0VxdWFsfSBmcm9tICcuL3V0aWxzL2NvbGxlY3Rpb24nO1xuaW1wb3J0IHtUcmVlTm9kZX0gZnJvbSAnLi91dGlscy90cmVlJztcblxuZXhwb3J0IGludGVyZmFjZSBOYXZpZ2F0aW9uRXh0cmFzIHtcbiAgcmVsYXRpdmVUbz86IEFjdGl2YXRlZFJvdXRlO1xuICBxdWVyeVBhcmFtcz86IFBhcmFtcztcbiAgZnJhZ21lbnQ/OiBzdHJpbmc7XG59XG5cbi8qKlxuICogQW4gZXZlbnQgdHJpZ2dlcmVkIHdoZW4gYSBuYXZpZ2F0aW9uIHN0YXJ0c1xuICovXG5leHBvcnQgY2xhc3MgTmF2aWdhdGlvblN0YXJ0IHtcbiAgY29uc3RydWN0b3IocHVibGljIGlkOiBudW1iZXIsIHB1YmxpYyB1cmw6IHN0cmluZykge31cblxuICB0b1N0cmluZygpOiBzdHJpbmcgeyByZXR1cm4gYE5hdmlnYXRpb25TdGFydChpZDogJHt0aGlzLmlkfSwgdXJsOiAnJHt0aGlzLnVybH0nKWA7IH1cbn1cblxuLyoqXG4gKiBBbiBldmVudCB0cmlnZ2VyZWQgd2hlbiBhIG5hdmlnYXRpb24gZW5kcyBzdWNjZXNzZnVsbHlcbiAqL1xuZXhwb3J0IGNsYXNzIE5hdmlnYXRpb25FbmQge1xuICBjb25zdHJ1Y3RvcihwdWJsaWMgaWQ6IG51bWJlciwgcHVibGljIHVybDogc3RyaW5nLCBwdWJsaWMgdXJsQWZ0ZXJSZWRpcmVjdHM6IHN0cmluZykge31cblxuICB0b1N0cmluZygpOiBzdHJpbmcge1xuICAgIHJldHVybiBgTmF2aWdhdGlvbkVuZChpZDogJHt0aGlzLmlkfSwgdXJsOiAnJHt0aGlzLnVybH0nLCB1cmxBZnRlclJlZGlyZWN0czogJyR7dGhpcy51cmxBZnRlclJlZGlyZWN0c30nKWA7XG4gIH1cbn1cblxuLyoqXG4gKiBBbiBldmVudCB0cmlnZ2VyZWQgd2hlbiBhIG5hdmlnYXRpb24gaXMgY2FuY2VsZWRcbiAqL1xuZXhwb3J0IGNsYXNzIE5hdmlnYXRpb25DYW5jZWwge1xuICBjb25zdHJ1Y3RvcihwdWJsaWMgaWQ6IG51bWJlciwgcHVibGljIHVybDogc3RyaW5nKSB7fVxuXG4gIHRvU3RyaW5nKCk6IHN0cmluZyB7IHJldHVybiBgTmF2aWdhdGlvbkNhbmNlbChpZDogJHt0aGlzLmlkfSwgdXJsOiAnJHt0aGlzLnVybH0nKWA7IH1cbn1cblxuLyoqXG4gKiBBbiBldmVudCB0cmlnZ2VyZWQgd2hlbiBhIG5hdmlnYXRpb24gZmFpbHMgZHVlIHRvIHVuZXhwZWN0ZWQgZXJyb3JcbiAqL1xuZXhwb3J0IGNsYXNzIE5hdmlnYXRpb25FcnJvciB7XG4gIGNvbnN0cnVjdG9yKHB1YmxpYyBpZDogbnVtYmVyLCBwdWJsaWMgdXJsOiBzdHJpbmcsIHB1YmxpYyBlcnJvcjogYW55KSB7fVxuXG4gIHRvU3RyaW5nKCk6IHN0cmluZyB7XG4gICAgcmV0dXJuIGBOYXZpZ2F0aW9uRXJyb3IoaWQ6ICR7dGhpcy5pZH0sIHVybDogJyR7dGhpcy51cmx9JywgZXJyb3I6ICR7dGhpcy5lcnJvcn0pYDtcbiAgfVxufVxuXG4vKipcbiAqIEFuIGV2ZW50IHRyaWdnZXJlZCB3aGVuIHJvdXRlcyBhcmUgcmVjb2duaXplZFxuICovXG5leHBvcnQgY2xhc3MgUm91dGVzUmVjb2duaXplZCB7XG4gIGNvbnN0cnVjdG9yKFxuICAgICAgcHVibGljIGlkOiBudW1iZXIsIHB1YmxpYyB1cmw6IHN0cmluZywgcHVibGljIHVybEFmdGVyUmVkaXJlY3RzOiBzdHJpbmcsXG4gICAgICBwdWJsaWMgc3RhdGU6IFJvdXRlclN0YXRlU25hcHNob3QpIHt9XG5cbiAgdG9TdHJpbmcoKTogc3RyaW5nIHtcbiAgICByZXR1cm4gYFJvdXRlc1JlY29nbml6ZWQoaWQ6ICR7dGhpcy5pZH0sIHVybDogJyR7dGhpcy51cmx9JywgdXJsQWZ0ZXJSZWRpcmVjdHM6ICcke3RoaXMudXJsQWZ0ZXJSZWRpcmVjdHN9Jywgc3RhdGU6ICR7dGhpcy5zdGF0ZX0pYDtcbiAgfVxufVxuXG5leHBvcnQgdHlwZSBFdmVudCA9IE5hdmlnYXRpb25TdGFydCB8IE5hdmlnYXRpb25FbmQgfCBOYXZpZ2F0aW9uQ2FuY2VsIHwgTmF2aWdhdGlvbkVycm9yO1xuXG4vKipcbiAqIFRoZSBgUm91dGVyYCBpcyByZXNwb25zaWJsZSBmb3IgbWFwcGluZyBVUkxzIHRvIGNvbXBvbmVudHMuXG4gKi9cbmV4cG9ydCBjbGFzcyBSb3V0ZXIge1xuICBwcml2YXRlIGN1cnJlbnRVcmxUcmVlOiBVcmxUcmVlO1xuICBwcml2YXRlIGN1cnJlbnRSb3V0ZXJTdGF0ZTogUm91dGVyU3RhdGU7XG4gIHByaXZhdGUgbG9jYXRpb25TdWJzY3JpcHRpb246IFN1YnNjcmlwdGlvbjtcbiAgcHJpdmF0ZSByb3V0ZXJFdmVudHM6IFN1YmplY3Q8RXZlbnQ+O1xuICBwcml2YXRlIG5hdmlnYXRpb25JZDogbnVtYmVyID0gMDtcbiAgcHJpdmF0ZSBjb25maWc6IFJvdXRlckNvbmZpZztcblxuICAvKipcbiAgICogQGludGVybmFsXG4gICAqL1xuICBjb25zdHJ1Y3RvcihcbiAgICAgIHByaXZhdGUgcm9vdENvbXBvbmVudFR5cGU6IFR5cGUsIHByaXZhdGUgcmVzb2x2ZXI6IENvbXBvbmVudFJlc29sdmVyLFxuICAgICAgcHJpdmF0ZSB1cmxTZXJpYWxpemVyOiBVcmxTZXJpYWxpemVyLCBwcml2YXRlIG91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwLFxuICAgICAgcHJpdmF0ZSBsb2NhdGlvbjogTG9jYXRpb24sIHByaXZhdGUgaW5qZWN0b3I6IEluamVjdG9yLCBjb25maWc6IFJvdXRlckNvbmZpZykge1xuICAgIHRoaXMucmVzZXRDb25maWcoY29uZmlnKTtcbiAgICB0aGlzLnJvdXRlckV2ZW50cyA9IG5ldyBTdWJqZWN0PEV2ZW50PigpO1xuICAgIHRoaXMuY3VycmVudFVybFRyZWUgPSBjcmVhdGVFbXB0eVVybFRyZWUoKTtcbiAgICB0aGlzLmN1cnJlbnRSb3V0ZXJTdGF0ZSA9IGNyZWF0ZUVtcHR5U3RhdGUodGhpcy5jdXJyZW50VXJsVHJlZSwgdGhpcy5yb290Q29tcG9uZW50VHlwZSk7XG4gIH1cblxuICAvKipcbiAgICogQGludGVybmFsXG4gICAqL1xuICBpbml0aWFsTmF2aWdhdGlvbigpOiB2b2lkIHtcbiAgICB0aGlzLnNldFVwTG9jYXRpb25DaGFuZ2VMaXN0ZW5lcigpO1xuICAgIHRoaXMubmF2aWdhdGVCeVVybCh0aGlzLmxvY2F0aW9uLnBhdGgoKSk7XG4gIH1cblxuICAvKipcbiAgICogUmV0dXJucyB0aGUgY3VycmVudCByb3V0ZSBzdGF0ZS5cbiAgICovXG4gIGdldCByb3V0ZXJTdGF0ZSgpOiBSb3V0ZXJTdGF0ZSB7IHJldHVybiB0aGlzLmN1cnJlbnRSb3V0ZXJTdGF0ZTsgfVxuXG4gIC8qKlxuICAgKiBSZXR1cm5zIHRoZSBjdXJyZW50IHVybC5cbiAgICovXG4gIGdldCB1cmwoKTogc3RyaW5nIHsgcmV0dXJuIHRoaXMuc2VyaWFsaXplVXJsKHRoaXMuY3VycmVudFVybFRyZWUpOyB9XG5cbiAgLyoqXG4gICAqIFJldHVybnMgYW4gb2JzZXJ2YWJsZSBvZiByb3V0ZSBldmVudHNcbiAgICovXG4gIGdldCBldmVudHMoKTogT2JzZXJ2YWJsZTxFdmVudD4geyByZXR1cm4gdGhpcy5yb3V0ZXJFdmVudHM7IH1cblxuICAvKipcbiAgICogUmVzZXRzIHRoZSBjb25maWd1cmF0aW9uIHVzZWQgZm9yIG5hdmlnYXRpb24gYW5kIGdlbmVyYXRpbmcgbGlua3MuXG4gICAqXG4gICAqICMjIyBVc2FnZVxuICAgKlxuICAgKiBgYGBcbiAgICogcm91dGVyLnJlc2V0Q29uZmlnKFtcbiAgICogIHsgcGF0aDogJ3RlYW0vOmlkJywgY29tcG9uZW50OiBUZWFtQ21wLCBjaGlsZHJlbjogW1xuICAgKiAgICB7IHBhdGg6ICdzaW1wbGUnLCBjb21wb25lbnQ6IFNpbXBsZUNtcCB9LFxuICAgKiAgICB7IHBhdGg6ICd1c2VyLzpuYW1lJywgY29tcG9uZW50OiBVc2VyQ21wIH1cbiAgICogIF0gfVxuICAgKiBdKTtcbiAgICogYGBgXG4gICAqL1xuICByZXNldENvbmZpZyhjb25maWc6IFJvdXRlckNvbmZpZyk6IHZvaWQge1xuICAgIHZhbGlkYXRlQ29uZmlnKGNvbmZpZyk7XG4gICAgdGhpcy5jb25maWcgPSBjb25maWc7XG4gIH1cblxuICAvKipcbiAgICogQGludGVybmFsXG4gICAqL1xuICBkaXNwb3NlKCk6IHZvaWQgeyB0aGlzLmxvY2F0aW9uU3Vic2NyaXB0aW9uLnVuc3Vic2NyaWJlKCk7IH1cblxuICAvKipcbiAgICogQXBwbGllcyBhbiBhcnJheSBvZiBjb21tYW5kcyB0byB0aGUgY3VycmVudCB1cmwgdHJlZSBhbmQgY3JlYXRlc1xuICAgKiBhIG5ldyB1cmwgdHJlZS5cbiAgICpcbiAgICogV2hlbiBnaXZlbiBhbiBhY3RpdmF0ZSByb3V0ZSwgYXBwbGllcyB0aGUgZ2l2ZW4gY29tbWFuZHMgc3RhcnRpbmcgZnJvbSB0aGUgcm91dGUuXG4gICAqIFdoZW4gbm90IGdpdmVuIGEgcm91dGUsIGFwcGxpZXMgdGhlIGdpdmVuIGNvbW1hbmQgc3RhcnRpbmcgZnJvbSB0aGUgcm9vdC5cbiAgICpcbiAgICogIyMjIFVzYWdlXG4gICAqXG4gICAqIGBgYFxuICAgKiAvLyBjcmVhdGUgL3RlYW0vMzMvdXNlci8xMVxuICAgKiByb3V0ZXIuY3JlYXRlVXJsVHJlZShbJy90ZWFtJywgMzMsICd1c2VyJywgMTFdKTtcbiAgICpcbiAgICogLy8gY3JlYXRlIC90ZWFtLzMzO2V4cGFuZD10cnVlL3VzZXIvMTFcbiAgICogcm91dGVyLmNyZWF0ZVVybFRyZWUoWycvdGVhbScsIDMzLCB7ZXhwYW5kOiB0cnVlfSwgJ3VzZXInLCAxMV0pO1xuICAgKlxuICAgKiAvLyB5b3UgY2FuIGNvbGxhcHNlIHN0YXRpYyBmcmFnbWVudHMgbGlrZSB0aGlzXG4gICAqIHJvdXRlci5jcmVhdGVVcmxUcmVlKFsnL3RlYW0vMzMvdXNlcicsIHVzZXJJZF0pO1xuICAgKlxuICAgKiAvLyBhc3N1bWluZyB0aGUgY3VycmVudCB1cmwgaXMgYC90ZWFtLzMzL3VzZXIvMTFgIGFuZCB0aGUgcm91dGUgcG9pbnRzIHRvIGB1c2VyLzExYFxuICAgKlxuICAgKiAvLyBuYXZpZ2F0ZSB0byAvdGVhbS8zMy91c2VyLzExL2RldGFpbHNcbiAgICogcm91dGVyLmNyZWF0ZVVybFRyZWUoWydkZXRhaWxzJ10sIHtyZWxhdGl2ZVRvOiByb3V0ZX0pO1xuICAgKlxuICAgKiAvLyBuYXZpZ2F0ZSB0byAvdGVhbS8zMy91c2VyLzIyXG4gICAqIHJvdXRlci5jcmVhdGVVcmxUcmVlKFsnLi4vMjInXSwge3JlbGF0aXZlVG86IHJvdXRlfSk7XG4gICAqXG4gICAqIC8vIG5hdmlnYXRlIHRvIC90ZWFtLzQ0L3VzZXIvMjJcbiAgICogcm91dGVyLmNyZWF0ZVVybFRyZWUoWycuLi8uLi90ZWFtLzQ0L3VzZXIvMjInXSwge3JlbGF0aXZlVG86IHJvdXRlfSk7XG4gICAqIGBgYFxuICAgKi9cbiAgY3JlYXRlVXJsVHJlZShjb21tYW5kczogYW55W10sIHtyZWxhdGl2ZVRvLCBxdWVyeVBhcmFtcywgZnJhZ21lbnR9OiBOYXZpZ2F0aW9uRXh0cmFzID0ge30pOlxuICAgICAgVXJsVHJlZSB7XG4gICAgY29uc3QgYSA9IHJlbGF0aXZlVG8gPyByZWxhdGl2ZVRvIDogdGhpcy5yb3V0ZXJTdGF0ZS5yb290O1xuICAgIHJldHVybiBjcmVhdGVVcmxUcmVlKGEsIHRoaXMuY3VycmVudFVybFRyZWUsIGNvbW1hbmRzLCBxdWVyeVBhcmFtcywgZnJhZ21lbnQpO1xuICB9XG5cbiAgLyoqXG4gICAqIE5hdmlnYXRlIGJhc2VkIG9uIHRoZSBwcm92aWRlZCB1cmwuIFRoaXMgbmF2aWdhdGlvbiBpcyBhbHdheXMgYWJzb2x1dGUuXG4gICAqXG4gICAqIFJldHVybnMgYSBwcm9taXNlIHRoYXQ6XG4gICAqIC0gaXMgcmVzb2x2ZWQgd2l0aCAndHJ1ZScgd2hlbiBuYXZpZ2F0aW9uIHN1Y2NlZWRzXG4gICAqIC0gaXMgcmVzb2x2ZWQgd2l0aCAnZmFsc2UnIHdoZW4gbmF2aWdhdGlvbiBmYWlsc1xuICAgKiAtIGlzIHJlamVjdGVkIHdoZW4gYW4gZXJyb3IgaGFwcGVuc1xuICAgKlxuICAgKiAjIyMgVXNhZ2VcbiAgICpcbiAgICogYGBgXG4gICAqIHJvdXRlci5uYXZpZ2F0ZUJ5VXJsKFwiL3RlYW0vMzMvdXNlci8xMVwiKTtcbiAgICogYGBgXG4gICAqL1xuICBuYXZpZ2F0ZUJ5VXJsKHVybDogc3RyaW5nfFVybFRyZWUpOiBQcm9taXNlPGJvb2xlYW4+IHtcbiAgICBpZiAodXJsIGluc3RhbmNlb2YgVXJsVHJlZSkge1xuICAgICAgcmV0dXJuIHRoaXMuc2NoZWR1bGVOYXZpZ2F0aW9uKHVybCwgZmFsc2UpO1xuICAgIH0gZWxzZSB7XG4gICAgICBjb25zdCB1cmxUcmVlID0gdGhpcy51cmxTZXJpYWxpemVyLnBhcnNlKHVybCk7XG4gICAgICByZXR1cm4gdGhpcy5zY2hlZHVsZU5hdmlnYXRpb24odXJsVHJlZSwgZmFsc2UpO1xuICAgIH1cbiAgfVxuXG4gIC8qKlxuICAgKiBOYXZpZ2F0ZSBiYXNlZCBvbiB0aGUgcHJvdmlkZWQgYXJyYXkgb2YgY29tbWFuZHMgYW5kIGEgc3RhcnRpbmcgcG9pbnQuXG4gICAqIElmIG5vIHN0YXJ0aW5nIHJvdXRlIGlzIHByb3ZpZGVkLCB0aGUgbmF2aWdhdGlvbiBpcyBhYnNvbHV0ZS5cbiAgICpcbiAgICogUmV0dXJucyBhIHByb21pc2UgdGhhdDpcbiAgICogLSBpcyByZXNvbHZlZCB3aXRoICd0cnVlJyB3aGVuIG5hdmlnYXRpb24gc3VjY2VlZHNcbiAgICogLSBpcyByZXNvbHZlZCB3aXRoICdmYWxzZScgd2hlbiBuYXZpZ2F0aW9uIGZhaWxzXG4gICAqIC0gaXMgcmVqZWN0ZWQgd2hlbiBhbiBlcnJvciBoYXBwZW5zXG4gICAqXG4gICAqICMjIyBVc2FnZVxuICAgKlxuICAgKiBgYGBcbiAgICogcm91dGVyLm5hdmlnYXRlKFsndGVhbScsIDMzLCAndGVhbScsICcxMV0sIHtyZWxhdGl2ZVRvOiByb3V0ZX0pO1xuICAgKiBgYGBcbiAgICovXG4gIG5hdmlnYXRlKGNvbW1hbmRzOiBhbnlbXSwgZXh0cmFzOiBOYXZpZ2F0aW9uRXh0cmFzID0ge30pOiBQcm9taXNlPGJvb2xlYW4+IHtcbiAgICByZXR1cm4gdGhpcy5zY2hlZHVsZU5hdmlnYXRpb24odGhpcy5jcmVhdGVVcmxUcmVlKGNvbW1hbmRzLCBleHRyYXMpLCBmYWxzZSk7XG4gIH1cblxuICAvKipcbiAgICogU2VyaWFsaXplcyBhIHtAbGluayBVcmxUcmVlfSBpbnRvIGEgc3RyaW5nLlxuICAgKi9cbiAgc2VyaWFsaXplVXJsKHVybDogVXJsVHJlZSk6IHN0cmluZyB7IHJldHVybiB0aGlzLnVybFNlcmlhbGl6ZXIuc2VyaWFsaXplKHVybCk7IH1cblxuICAvKipcbiAgICogUGFyc2UgYSBzdHJpbmcgaW50byBhIHtAbGluayBVcmxUcmVlfS5cbiAgICovXG4gIHBhcnNlVXJsKHVybDogc3RyaW5nKTogVXJsVHJlZSB7IHJldHVybiB0aGlzLnVybFNlcmlhbGl6ZXIucGFyc2UodXJsKTsgfVxuXG4gIHByaXZhdGUgc2NoZWR1bGVOYXZpZ2F0aW9uKHVybDogVXJsVHJlZSwgcHJldmVudFB1c2hTdGF0ZTogYm9vbGVhbik6IFByb21pc2U8Ym9vbGVhbj4ge1xuICAgIGNvbnN0IGlkID0gKyt0aGlzLm5hdmlnYXRpb25JZDtcbiAgICB0aGlzLnJvdXRlckV2ZW50cy5uZXh0KG5ldyBOYXZpZ2F0aW9uU3RhcnQoaWQsIHRoaXMuc2VyaWFsaXplVXJsKHVybCkpKTtcbiAgICByZXR1cm4gUHJvbWlzZS5yZXNvbHZlKCkudGhlbigoXykgPT4gdGhpcy5ydW5OYXZpZ2F0ZSh1cmwsIHByZXZlbnRQdXNoU3RhdGUsIGlkKSk7XG4gIH1cblxuICBwcml2YXRlIHNldFVwTG9jYXRpb25DaGFuZ2VMaXN0ZW5lcigpOiB2b2lkIHtcbiAgICB0aGlzLmxvY2F0aW9uU3Vic2NyaXB0aW9uID0gPGFueT50aGlzLmxvY2F0aW9uLnN1YnNjcmliZSgoY2hhbmdlKSA9PiB7XG4gICAgICByZXR1cm4gdGhpcy5zY2hlZHVsZU5hdmlnYXRpb24odGhpcy51cmxTZXJpYWxpemVyLnBhcnNlKGNoYW5nZVsndXJsJ10pLCBjaGFuZ2VbJ3BvcCddKTtcbiAgICB9KTtcbiAgfVxuXG4gIHByaXZhdGUgcnVuTmF2aWdhdGUodXJsOiBVcmxUcmVlLCBwcmV2ZW50UHVzaFN0YXRlOiBib29sZWFuLCBpZDogbnVtYmVyKTogUHJvbWlzZTxib29sZWFuPiB7XG4gICAgaWYgKGlkICE9PSB0aGlzLm5hdmlnYXRpb25JZCkge1xuICAgICAgdGhpcy5sb2NhdGlvbi5nbyh0aGlzLnVybFNlcmlhbGl6ZXIuc2VyaWFsaXplKHRoaXMuY3VycmVudFVybFRyZWUpKTtcbiAgICAgIHRoaXMucm91dGVyRXZlbnRzLm5leHQobmV3IE5hdmlnYXRpb25DYW5jZWwoaWQsIHRoaXMuc2VyaWFsaXplVXJsKHVybCkpKTtcbiAgICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUoZmFsc2UpO1xuICAgIH1cblxuICAgIHJldHVybiBuZXcgUHJvbWlzZSgocmVzb2x2ZVByb21pc2UsIHJlamVjdFByb21pc2UpID0+IHtcbiAgICAgIGxldCB1cGRhdGVkVXJsOiBVcmxUcmVlO1xuICAgICAgbGV0IHN0YXRlOiBSb3V0ZXJTdGF0ZTtcbiAgICAgIGFwcGx5UmVkaXJlY3RzKHVybCwgdGhpcy5jb25maWcpXG4gICAgICAgICAgLm1lcmdlTWFwKHUgPT4ge1xuICAgICAgICAgICAgdXBkYXRlZFVybCA9IHU7XG4gICAgICAgICAgICByZXR1cm4gcmVjb2duaXplKFxuICAgICAgICAgICAgICAgIHRoaXMucm9vdENvbXBvbmVudFR5cGUsIHRoaXMuY29uZmlnLCB1cGRhdGVkVXJsLCB0aGlzLnNlcmlhbGl6ZVVybCh1cGRhdGVkVXJsKSk7XG4gICAgICAgICAgfSlcblxuICAgICAgICAgIC5tZXJnZU1hcCgobmV3Um91dGVyU3RhdGVTbmFwc2hvdCkgPT4ge1xuICAgICAgICAgICAgdGhpcy5yb3V0ZXJFdmVudHMubmV4dChuZXcgUm91dGVzUmVjb2duaXplZChcbiAgICAgICAgICAgICAgICBpZCwgdGhpcy5zZXJpYWxpemVVcmwodXJsKSwgdGhpcy5zZXJpYWxpemVVcmwodXBkYXRlZFVybCksIG5ld1JvdXRlclN0YXRlU25hcHNob3QpKTtcbiAgICAgICAgICAgIHJldHVybiByZXNvbHZlKHRoaXMucmVzb2x2ZXIsIG5ld1JvdXRlclN0YXRlU25hcHNob3QpO1xuXG4gICAgICAgICAgfSlcbiAgICAgICAgICAubWFwKChyb3V0ZXJTdGF0ZVNuYXBzaG90KSA9PiB7XG4gICAgICAgICAgICByZXR1cm4gY3JlYXRlUm91dGVyU3RhdGUocm91dGVyU3RhdGVTbmFwc2hvdCwgdGhpcy5jdXJyZW50Um91dGVyU3RhdGUpO1xuXG4gICAgICAgICAgfSlcbiAgICAgICAgICAubWFwKChuZXdTdGF0ZTogUm91dGVyU3RhdGUpID0+IHtcbiAgICAgICAgICAgIHN0YXRlID0gbmV3U3RhdGU7XG5cbiAgICAgICAgICB9KVxuICAgICAgICAgIC5tZXJnZU1hcChfID0+IHtcbiAgICAgICAgICAgIHJldHVybiBuZXcgR3VhcmRDaGVja3Moc3RhdGUuc25hcHNob3QsIHRoaXMuY3VycmVudFJvdXRlclN0YXRlLnNuYXBzaG90LCB0aGlzLmluamVjdG9yKVxuICAgICAgICAgICAgICAgIC5jaGVjayh0aGlzLm91dGxldE1hcCk7XG5cbiAgICAgICAgICB9KVxuICAgICAgICAgIC5mb3JFYWNoKChzaG91bGRBY3RpdmF0ZTogYm9vbGVhbikgPT4ge1xuICAgICAgICAgICAgaWYgKCFzaG91bGRBY3RpdmF0ZSB8fCBpZCAhPT0gdGhpcy5uYXZpZ2F0aW9uSWQpIHtcbiAgICAgICAgICAgICAgdGhpcy5yb3V0ZXJFdmVudHMubmV4dChuZXcgTmF2aWdhdGlvbkNhbmNlbChpZCwgdGhpcy5zZXJpYWxpemVVcmwodXJsKSkpO1xuICAgICAgICAgICAgICByZXR1cm4gUHJvbWlzZS5yZXNvbHZlKGZhbHNlKTtcbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgbmV3IEFjdGl2YXRlUm91dGVzKHN0YXRlLCB0aGlzLmN1cnJlbnRSb3V0ZXJTdGF0ZSkuYWN0aXZhdGUodGhpcy5vdXRsZXRNYXApO1xuXG4gICAgICAgICAgICB0aGlzLmN1cnJlbnRVcmxUcmVlID0gdXBkYXRlZFVybDtcbiAgICAgICAgICAgIHRoaXMuY3VycmVudFJvdXRlclN0YXRlID0gc3RhdGU7XG4gICAgICAgICAgICBpZiAoIXByZXZlbnRQdXNoU3RhdGUpIHtcbiAgICAgICAgICAgICAgbGV0IHBhdGggPSB0aGlzLnVybFNlcmlhbGl6ZXIuc2VyaWFsaXplKHVwZGF0ZWRVcmwpO1xuICAgICAgICAgICAgICBpZiAodGhpcy5sb2NhdGlvbi5pc0N1cnJlbnRQYXRoRXF1YWxUbyhwYXRoKSkge1xuICAgICAgICAgICAgICAgIHRoaXMubG9jYXRpb24ucmVwbGFjZVN0YXRlKHBhdGgpO1xuICAgICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgIHRoaXMubG9jYXRpb24uZ28ocGF0aCk7XG4gICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUodHJ1ZSk7XG4gICAgICAgICAgfSlcbiAgICAgICAgICAudGhlbihcbiAgICAgICAgICAgICAgKCkgPT4ge1xuICAgICAgICAgICAgICAgIHRoaXMucm91dGVyRXZlbnRzLm5leHQoXG4gICAgICAgICAgICAgICAgICAgIG5ldyBOYXZpZ2F0aW9uRW5kKGlkLCB0aGlzLnNlcmlhbGl6ZVVybCh1cmwpLCB0aGlzLnNlcmlhbGl6ZVVybCh1cGRhdGVkVXJsKSkpO1xuICAgICAgICAgICAgICAgIHJlc29sdmVQcm9taXNlKHRydWUpO1xuXG4gICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgIGUgPT4ge1xuICAgICAgICAgICAgICAgIHRoaXMucm91dGVyRXZlbnRzLm5leHQobmV3IE5hdmlnYXRpb25FcnJvcihpZCwgdGhpcy5zZXJpYWxpemVVcmwodXJsKSwgZSkpO1xuICAgICAgICAgICAgICAgIHJlamVjdFByb21pc2UoZSk7XG4gICAgICAgICAgICAgIH0pO1xuICAgIH0pO1xuICB9XG59XG5cbmNsYXNzIENhbkFjdGl2YXRlIHtcbiAgY29uc3RydWN0b3IocHVibGljIHJvdXRlOiBBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90KSB7fVxufVxuY2xhc3MgQ2FuRGVhY3RpdmF0ZSB7XG4gIGNvbnN0cnVjdG9yKHB1YmxpYyBjb21wb25lbnQ6IE9iamVjdCwgcHVibGljIHJvdXRlOiBBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90KSB7fVxufVxuXG5jbGFzcyBHdWFyZENoZWNrcyB7XG4gIHByaXZhdGUgY2hlY2tzOiBBcnJheTxDYW5BY3RpdmF0ZXxDYW5EZWFjdGl2YXRlPiA9IFtdO1xuICBjb25zdHJ1Y3RvcihcbiAgICAgIHByaXZhdGUgZnV0dXJlOiBSb3V0ZXJTdGF0ZVNuYXBzaG90LCBwcml2YXRlIGN1cnI6IFJvdXRlclN0YXRlU25hcHNob3QsXG4gICAgICBwcml2YXRlIGluamVjdG9yOiBJbmplY3Rvcikge31cblxuICBjaGVjayhwYXJlbnRPdXRsZXRNYXA6IFJvdXRlck91dGxldE1hcCk6IE9ic2VydmFibGU8Ym9vbGVhbj4ge1xuICAgIGNvbnN0IGZ1dHVyZVJvb3QgPSB0aGlzLmZ1dHVyZS5fcm9vdDtcbiAgICBjb25zdCBjdXJyUm9vdCA9IHRoaXMuY3VyciA/IHRoaXMuY3Vyci5fcm9vdCA6IG51bGw7XG4gICAgdGhpcy50cmF2ZXJzZUNoaWxkUm91dGVzKGZ1dHVyZVJvb3QsIGN1cnJSb290LCBwYXJlbnRPdXRsZXRNYXApO1xuICAgIGlmICh0aGlzLmNoZWNrcy5sZW5ndGggPT09IDApIHJldHVybiBvZiAodHJ1ZSk7XG5cbiAgICByZXR1cm4gT2JzZXJ2YWJsZS5mcm9tKHRoaXMuY2hlY2tzKVxuICAgICAgICAubWFwKHMgPT4ge1xuICAgICAgICAgIGlmIChzIGluc3RhbmNlb2YgQ2FuQWN0aXZhdGUpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnJ1bkNhbkFjdGl2YXRlKHMucm91dGUpO1xuICAgICAgICAgIH0gZWxzZSBpZiAocyBpbnN0YW5jZW9mIENhbkRlYWN0aXZhdGUpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnJ1bkNhbkRlYWN0aXZhdGUocy5jb21wb25lbnQsIHMucm91dGUpO1xuICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoJ0Nhbm5vdCBiZSByZWFjaGVkJyk7XG4gICAgICAgICAgfVxuICAgICAgICB9KVxuICAgICAgICAubWVyZ2VBbGwoKVxuICAgICAgICAuZXZlcnkocmVzdWx0ID0+IHJlc3VsdCA9PT0gdHJ1ZSk7XG4gIH1cblxuICBwcml2YXRlIHRyYXZlcnNlQ2hpbGRSb3V0ZXMoXG4gICAgICBmdXR1cmVOb2RlOiBUcmVlTm9kZTxBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90PiwgY3Vyck5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlU25hcHNob3Q+LFxuICAgICAgb3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXApOiB2b2lkIHtcbiAgICBjb25zdCBwcmV2Q2hpbGRyZW46IHtba2V5OiBzdHJpbmddOiBhbnl9ID0gbm9kZUNoaWxkcmVuQXNNYXAoY3Vyck5vZGUpO1xuICAgIGZ1dHVyZU5vZGUuY2hpbGRyZW4uZm9yRWFjaChjID0+IHtcbiAgICAgIHRoaXMudHJhdmVyc2VSb3V0ZXMoYywgcHJldkNoaWxkcmVuW2MudmFsdWUub3V0bGV0XSwgb3V0bGV0TWFwKTtcbiAgICAgIGRlbGV0ZSBwcmV2Q2hpbGRyZW5bYy52YWx1ZS5vdXRsZXRdO1xuICAgIH0pO1xuICAgIGZvckVhY2goXG4gICAgICAgIHByZXZDaGlsZHJlbixcbiAgICAgICAgKHY6IGFueSwgazogc3RyaW5nKSA9PiB0aGlzLmRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKHYsIG91dGxldE1hcC5fb3V0bGV0c1trXSkpO1xuICB9XG5cbiAgdHJhdmVyc2VSb3V0ZXMoXG4gICAgICBmdXR1cmVOb2RlOiBUcmVlTm9kZTxBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90PiwgY3Vyck5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlU25hcHNob3Q+LFxuICAgICAgcGFyZW50T3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXApOiB2b2lkIHtcbiAgICBjb25zdCBmdXR1cmUgPSBmdXR1cmVOb2RlLnZhbHVlO1xuICAgIGNvbnN0IGN1cnIgPSBjdXJyTm9kZSA/IGN1cnJOb2RlLnZhbHVlIDogbnVsbDtcbiAgICBjb25zdCBvdXRsZXQgPSBwYXJlbnRPdXRsZXRNYXAgPyBwYXJlbnRPdXRsZXRNYXAuX291dGxldHNbZnV0dXJlTm9kZS52YWx1ZS5vdXRsZXRdIDogbnVsbDtcblxuICAgIGlmIChjdXJyICYmIGZ1dHVyZS5fcm91dGVDb25maWcgPT09IGN1cnIuX3JvdXRlQ29uZmlnKSB7XG4gICAgICBpZiAoIXNoYWxsb3dFcXVhbChmdXR1cmUucGFyYW1zLCBjdXJyLnBhcmFtcykpIHtcbiAgICAgICAgdGhpcy5jaGVja3MucHVzaChuZXcgQ2FuRGVhY3RpdmF0ZShvdXRsZXQuY29tcG9uZW50LCBjdXJyKSwgbmV3IENhbkFjdGl2YXRlKGZ1dHVyZSkpO1xuICAgICAgfVxuICAgICAgdGhpcy50cmF2ZXJzZUNoaWxkUm91dGVzKGZ1dHVyZU5vZGUsIGN1cnJOb2RlLCBvdXRsZXQgPyBvdXRsZXQub3V0bGV0TWFwIDogbnVsbCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMuZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4oY3Vyciwgb3V0bGV0KTtcbiAgICAgIHRoaXMuY2hlY2tzLnB1c2gobmV3IENhbkFjdGl2YXRlKGZ1dHVyZSkpO1xuICAgICAgdGhpcy50cmF2ZXJzZUNoaWxkUm91dGVzKGZ1dHVyZU5vZGUsIG51bGwsIG91dGxldCA/IG91dGxldC5vdXRsZXRNYXAgOiBudWxsKTtcbiAgICB9XG4gIH1cblxuICBwcml2YXRlIGRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKHJvdXRlOiBBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90LCBvdXRsZXQ6IFJvdXRlck91dGxldCk6IHZvaWQge1xuICAgIGlmIChvdXRsZXQgJiYgb3V0bGV0LmlzQWN0aXZhdGVkKSB7XG4gICAgICBmb3JFYWNoKG91dGxldC5vdXRsZXRNYXAuX291dGxldHMsICh2OiBSb3V0ZXJPdXRsZXQpID0+IHtcbiAgICAgICAgaWYgKHYuaXNBY3RpdmF0ZWQpIHtcbiAgICAgICAgICB0aGlzLmRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKHYuYWN0aXZhdGVkUm91dGUuc25hcHNob3QsIHYpO1xuICAgICAgICB9XG4gICAgICB9KTtcbiAgICAgIHRoaXMuY2hlY2tzLnB1c2gobmV3IENhbkRlYWN0aXZhdGUob3V0bGV0LmNvbXBvbmVudCwgcm91dGUpKTtcbiAgICB9XG4gIH1cblxuICBwcml2YXRlIHJ1bkNhbkFjdGl2YXRlKGZ1dHVyZTogQWN0aXZhdGVkUm91dGVTbmFwc2hvdCk6IE9ic2VydmFibGU8Ym9vbGVhbj4ge1xuICAgIGNvbnN0IGNhbkFjdGl2YXRlID0gZnV0dXJlLl9yb3V0ZUNvbmZpZyA/IGZ1dHVyZS5fcm91dGVDb25maWcuY2FuQWN0aXZhdGUgOiBudWxsO1xuICAgIGlmICghY2FuQWN0aXZhdGUgfHwgY2FuQWN0aXZhdGUubGVuZ3RoID09PSAwKSByZXR1cm4gb2YgKHRydWUpO1xuICAgIHJldHVybiBPYnNlcnZhYmxlLmZyb20oY2FuQWN0aXZhdGUpXG4gICAgICAgIC5tYXAoYyA9PiB7XG4gICAgICAgICAgY29uc3QgZ3VhcmQgPSB0aGlzLmluamVjdG9yLmdldChjKTtcbiAgICAgICAgICBpZiAoZ3VhcmQuY2FuQWN0aXZhdGUpIHtcbiAgICAgICAgICAgIHJldHVybiB3cmFwSW50b09ic2VydmFibGUoZ3VhcmQuY2FuQWN0aXZhdGUoZnV0dXJlLCB0aGlzLmZ1dHVyZSkpO1xuICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICByZXR1cm4gd3JhcEludG9PYnNlcnZhYmxlKGd1YXJkKGZ1dHVyZSwgdGhpcy5mdXR1cmUpKTtcbiAgICAgICAgICB9XG4gICAgICAgIH0pXG4gICAgICAgIC5tZXJnZUFsbCgpXG4gICAgICAgIC5ldmVyeShyZXN1bHQgPT4gcmVzdWx0ID09PSB0cnVlKTtcbiAgfVxuXG4gIHByaXZhdGUgcnVuQ2FuRGVhY3RpdmF0ZShjb21wb25lbnQ6IE9iamVjdCwgY3VycjogQWN0aXZhdGVkUm91dGVTbmFwc2hvdCk6IE9ic2VydmFibGU8Ym9vbGVhbj4ge1xuICAgIGNvbnN0IGNhbkRlYWN0aXZhdGUgPSBjdXJyLl9yb3V0ZUNvbmZpZyA/IGN1cnIuX3JvdXRlQ29uZmlnLmNhbkRlYWN0aXZhdGUgOiBudWxsO1xuICAgIGlmICghY2FuRGVhY3RpdmF0ZSB8fCBjYW5EZWFjdGl2YXRlLmxlbmd0aCA9PT0gMCkgcmV0dXJuIG9mICh0cnVlKTtcbiAgICByZXR1cm4gT2JzZXJ2YWJsZS5mcm9tKGNhbkRlYWN0aXZhdGUpXG4gICAgICAgIC5tYXAoYyA9PiB7XG4gICAgICAgICAgY29uc3QgZ3VhcmQgPSB0aGlzLmluamVjdG9yLmdldChjKTtcbiAgICAgICAgICBpZiAoZ3VhcmQuY2FuRGVhY3RpdmF0ZSkge1xuICAgICAgICAgICAgcmV0dXJuIHdyYXBJbnRvT2JzZXJ2YWJsZShndWFyZC5jYW5EZWFjdGl2YXRlKGNvbXBvbmVudCwgY3VyciwgdGhpcy5jdXJyKSk7XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHJldHVybiB3cmFwSW50b09ic2VydmFibGUoZ3VhcmQoY29tcG9uZW50LCBjdXJyLCB0aGlzLmN1cnIpKTtcbiAgICAgICAgICB9XG4gICAgICAgIH0pXG4gICAgICAgIC5tZXJnZUFsbCgpXG4gICAgICAgIC5ldmVyeShyZXN1bHQgPT4gcmVzdWx0ID09PSB0cnVlKTtcbiAgfVxufVxuXG5mdW5jdGlvbiB3cmFwSW50b09ic2VydmFibGU8VD4odmFsdWU6IFQgfCBPYnNlcnZhYmxlPFQ+KTogT2JzZXJ2YWJsZTxUPiB7XG4gIGlmICh2YWx1ZSBpbnN0YW5jZW9mIE9ic2VydmFibGUpIHtcbiAgICByZXR1cm4gdmFsdWU7XG4gIH0gZWxzZSB7XG4gICAgcmV0dXJuIG9mICh2YWx1ZSk7XG4gIH1cbn1cblxuY2xhc3MgQWN0aXZhdGVSb3V0ZXMge1xuICBjb25zdHJ1Y3Rvcihwcml2YXRlIGZ1dHVyZVN0YXRlOiBSb3V0ZXJTdGF0ZSwgcHJpdmF0ZSBjdXJyU3RhdGU6IFJvdXRlclN0YXRlKSB7fVxuXG4gIGFjdGl2YXRlKHBhcmVudE91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwKTogdm9pZCB7XG4gICAgY29uc3QgZnV0dXJlUm9vdCA9IHRoaXMuZnV0dXJlU3RhdGUuX3Jvb3Q7XG4gICAgY29uc3QgY3VyclJvb3QgPSB0aGlzLmN1cnJTdGF0ZSA/IHRoaXMuY3VyclN0YXRlLl9yb290IDogbnVsbDtcblxuICAgIHB1c2hRdWVyeVBhcmFtc0FuZEZyYWdtZW50KHRoaXMuZnV0dXJlU3RhdGUpO1xuICAgIHRoaXMuYWN0aXZhdGVDaGlsZFJvdXRlcyhmdXR1cmVSb290LCBjdXJyUm9vdCwgcGFyZW50T3V0bGV0TWFwKTtcbiAgfVxuXG4gIHByaXZhdGUgYWN0aXZhdGVDaGlsZFJvdXRlcyhcbiAgICAgIGZ1dHVyZU5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlPiwgY3Vyck5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlPixcbiAgICAgIG91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwKTogdm9pZCB7XG4gICAgY29uc3QgcHJldkNoaWxkcmVuOiB7W2tleTogc3RyaW5nXTogYW55fSA9IG5vZGVDaGlsZHJlbkFzTWFwKGN1cnJOb2RlKTtcbiAgICBmdXR1cmVOb2RlLmNoaWxkcmVuLmZvckVhY2goYyA9PiB7XG4gICAgICB0aGlzLmFjdGl2YXRlUm91dGVzKGMsIHByZXZDaGlsZHJlbltjLnZhbHVlLm91dGxldF0sIG91dGxldE1hcCk7XG4gICAgICBkZWxldGUgcHJldkNoaWxkcmVuW2MudmFsdWUub3V0bGV0XTtcbiAgICB9KTtcbiAgICBmb3JFYWNoKFxuICAgICAgICBwcmV2Q2hpbGRyZW4sXG4gICAgICAgICh2OiBhbnksIGs6IHN0cmluZykgPT4gdGhpcy5kZWFjdGl2YXRlT3V0bGV0QW5kSXRDaGlsZHJlbihvdXRsZXRNYXAuX291dGxldHNba10pKTtcbiAgfVxuXG4gIGFjdGl2YXRlUm91dGVzKFxuICAgICAgZnV0dXJlTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGU+LCBjdXJyTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGU+LFxuICAgICAgcGFyZW50T3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXApOiB2b2lkIHtcbiAgICBjb25zdCBmdXR1cmUgPSBmdXR1cmVOb2RlLnZhbHVlO1xuICAgIGNvbnN0IGN1cnIgPSBjdXJyTm9kZSA/IGN1cnJOb2RlLnZhbHVlIDogbnVsbDtcblxuICAgIGNvbnN0IG91dGxldCA9IGdldE91dGxldChwYXJlbnRPdXRsZXRNYXAsIGZ1dHVyZU5vZGUudmFsdWUpO1xuXG4gICAgaWYgKGZ1dHVyZSA9PT0gY3Vycikge1xuICAgICAgYWR2YW5jZUFjdGl2YXRlZFJvdXRlKGZ1dHVyZSk7XG4gICAgICB0aGlzLmFjdGl2YXRlQ2hpbGRSb3V0ZXMoZnV0dXJlTm9kZSwgY3Vyck5vZGUsIG91dGxldC5vdXRsZXRNYXApO1xuICAgIH0gZWxzZSB7XG4gICAgICB0aGlzLmRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKG91dGxldCk7XG4gICAgICBjb25zdCBvdXRsZXRNYXAgPSBuZXcgUm91dGVyT3V0bGV0TWFwKCk7XG4gICAgICB0aGlzLmFjdGl2YXRlTmV3Um91dGVzKG91dGxldE1hcCwgZnV0dXJlLCBvdXRsZXQpO1xuICAgICAgdGhpcy5hY3RpdmF0ZUNoaWxkUm91dGVzKGZ1dHVyZU5vZGUsIG51bGwsIG91dGxldE1hcCk7XG4gICAgfVxuICB9XG5cbiAgcHJpdmF0ZSBhY3RpdmF0ZU5ld1JvdXRlcyhcbiAgICAgIG91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwLCBmdXR1cmU6IEFjdGl2YXRlZFJvdXRlLCBvdXRsZXQ6IFJvdXRlck91dGxldCk6IHZvaWQge1xuICAgIGNvbnN0IHJlc29sdmVkID0gUmVmbGVjdGl2ZUluamVjdG9yLnJlc29sdmUoW1xuICAgICAge3Byb3ZpZGU6IEFjdGl2YXRlZFJvdXRlLCB1c2VWYWx1ZTogZnV0dXJlfSxcbiAgICAgIHtwcm92aWRlOiBSb3V0ZXJPdXRsZXRNYXAsIHVzZVZhbHVlOiBvdXRsZXRNYXB9XG4gICAgXSk7XG4gICAgYWR2YW5jZUFjdGl2YXRlZFJvdXRlKGZ1dHVyZSk7XG4gICAgb3V0bGV0LmFjdGl2YXRlKGZ1dHVyZS5fZnV0dXJlU25hcHNob3QuX3Jlc29sdmVkQ29tcG9uZW50RmFjdG9yeSwgZnV0dXJlLCByZXNvbHZlZCwgb3V0bGV0TWFwKTtcbiAgfVxuXG4gIHByaXZhdGUgZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4ob3V0bGV0OiBSb3V0ZXJPdXRsZXQpOiB2b2lkIHtcbiAgICBpZiAob3V0bGV0ICYmIG91dGxldC5pc0FjdGl2YXRlZCkge1xuICAgICAgZm9yRWFjaChcbiAgICAgICAgICBvdXRsZXQub3V0bGV0TWFwLl9vdXRsZXRzLCAodjogUm91dGVyT3V0bGV0KSA9PiB0aGlzLmRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKHYpKTtcbiAgICAgIG91dGxldC5kZWFjdGl2YXRlKCk7XG4gICAgfVxuICB9XG59XG5cbmZ1bmN0aW9uIHB1c2hRdWVyeVBhcmFtc0FuZEZyYWdtZW50KHN0YXRlOiBSb3V0ZXJTdGF0ZSk6IHZvaWQge1xuICBpZiAoIXNoYWxsb3dFcXVhbChzdGF0ZS5zbmFwc2hvdC5xdWVyeVBhcmFtcywgKDxhbnk+c3RhdGUucXVlcnlQYXJhbXMpLnZhbHVlKSkge1xuICAgICg8YW55PnN0YXRlLnF1ZXJ5UGFyYW1zKS5uZXh0KHN0YXRlLnNuYXBzaG90LnF1ZXJ5UGFyYW1zKTtcbiAgfVxuXG4gIGlmIChzdGF0ZS5zbmFwc2hvdC5mcmFnbWVudCAhPT0gKDxhbnk+c3RhdGUuZnJhZ21lbnQpLnZhbHVlKSB7XG4gICAgKDxhbnk+c3RhdGUuZnJhZ21lbnQpLm5leHQoc3RhdGUuc25hcHNob3QuZnJhZ21lbnQpO1xuICB9XG59XG5cbmZ1bmN0aW9uIG5vZGVDaGlsZHJlbkFzTWFwKG5vZGU6IFRyZWVOb2RlPGFueT4pIHtcbiAgcmV0dXJuIG5vZGUgPyBub2RlLmNoaWxkcmVuLnJlZHVjZSgobTogYW55LCBjOiBUcmVlTm9kZTxhbnk+KSA9PiB7XG4gICAgbVtjLnZhbHVlLm91dGxldF0gPSBjO1xuICAgIHJldHVybiBtO1xuICB9LCB7fSkgOiB7fTtcbn1cblxuZnVuY3Rpb24gZ2V0T3V0bGV0KG91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwLCByb3V0ZTogQWN0aXZhdGVkUm91dGUpOiBSb3V0ZXJPdXRsZXQge1xuICBsZXQgb3V0bGV0ID0gb3V0bGV0TWFwLl9vdXRsZXRzW3JvdXRlLm91dGxldF07XG4gIGlmICghb3V0bGV0KSB7XG4gICAgY29uc3QgY29tcG9uZW50TmFtZSA9ICg8YW55PnJvdXRlLmNvbXBvbmVudCkubmFtZTtcbiAgICBpZiAocm91dGUub3V0bGV0ID09PSBQUklNQVJZX09VVExFVCkge1xuICAgICAgdGhyb3cgbmV3IEVycm9yKGBDYW5ub3QgZmluZCBwcmltYXJ5IG91dGxldCB0byBsb2FkICcke2NvbXBvbmVudE5hbWV9J2ApO1xuICAgIH0gZWxzZSB7XG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYENhbm5vdCBmaW5kIHRoZSBvdXRsZXQgJHtyb3V0ZS5vdXRsZXR9IHRvIGxvYWQgJyR7Y29tcG9uZW50TmFtZX0nYCk7XG4gICAgfVxuICB9XG4gIHJldHVybiBvdXRsZXQ7XG59XG4iXX0=