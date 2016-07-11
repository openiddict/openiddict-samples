import 'rxjs/add/operator/map';
import 'rxjs/add/operator/scan';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/concat';
import 'rxjs/add/operator/concatMap';
import 'rxjs/add/operator/every';
import 'rxjs/add/operator/mergeAll';
import 'rxjs/add/observable/from';
import { ReflectiveInjector } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { of } from 'rxjs/observable/of';
import { applyRedirects } from './apply_redirects';
import { validateConfig } from './config';
import { createRouterState } from './create_router_state';
import { createUrlTree } from './create_url_tree';
import { recognize } from './recognize';
import { resolve } from './resolve';
import { RouterOutletMap } from './router_outlet_map';
import { ActivatedRoute, advanceActivatedRoute, createEmptyState } from './router_state';
import { PRIMARY_OUTLET } from './shared';
import { UrlTree, createEmptyUrlTree } from './url_tree';
import { forEach, shallowEqual } from './utils/collection';
export class NavigationStart {
    constructor(id, url) {
        this.id = id;
        this.url = url;
    }
    toString() { return `NavigationStart(id: ${this.id}, url: '${this.url}')`; }
}
export class NavigationEnd {
    constructor(id, url, urlAfterRedirects) {
        this.id = id;
        this.url = url;
        this.urlAfterRedirects = urlAfterRedirects;
    }
    toString() {
        return `NavigationEnd(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}')`;
    }
}
export class NavigationCancel {
    constructor(id, url) {
        this.id = id;
        this.url = url;
    }
    toString() { return `NavigationCancel(id: ${this.id}, url: '${this.url}')`; }
}
export class NavigationError {
    constructor(id, url, error) {
        this.id = id;
        this.url = url;
        this.error = error;
    }
    toString() {
        return `NavigationError(id: ${this.id}, url: '${this.url}', error: ${this.error})`;
    }
}
export class RoutesRecognized {
    constructor(id, url, urlAfterRedirects, state) {
        this.id = id;
        this.url = url;
        this.urlAfterRedirects = urlAfterRedirects;
        this.state = state;
    }
    toString() {
        return `RoutesRecognized(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state})`;
    }
}
export class Router {
    constructor(rootComponentType, resolver, urlSerializer, outletMap, location, injector, config) {
        this.rootComponentType = rootComponentType;
        this.resolver = resolver;
        this.urlSerializer = urlSerializer;
        this.outletMap = outletMap;
        this.location = location;
        this.injector = injector;
        this.navigationId = 0;
        this.resetConfig(config);
        this.routerEvents = new Subject();
        this.currentUrlTree = createEmptyUrlTree();
        this.currentRouterState = createEmptyState(this.currentUrlTree, this.rootComponentType);
    }
    initialNavigation() {
        this.setUpLocationChangeListener();
        this.navigateByUrl(this.location.path());
    }
    get routerState() { return this.currentRouterState; }
    get url() { return this.serializeUrl(this.currentUrlTree); }
    get events() { return this.routerEvents; }
    resetConfig(config) {
        validateConfig(config);
        this.config = config;
    }
    dispose() { this.locationSubscription.unsubscribe(); }
    createUrlTree(commands, { relativeTo, queryParams, fragment } = {}) {
        const a = relativeTo ? relativeTo : this.routerState.root;
        return createUrlTree(a, this.currentUrlTree, commands, queryParams, fragment);
    }
    navigateByUrl(url) {
        if (url instanceof UrlTree) {
            return this.scheduleNavigation(url, false);
        }
        else {
            const urlTree = this.urlSerializer.parse(url);
            return this.scheduleNavigation(urlTree, false);
        }
    }
    navigate(commands, extras = {}) {
        return this.scheduleNavigation(this.createUrlTree(commands, extras), false);
    }
    serializeUrl(url) { return this.urlSerializer.serialize(url); }
    parseUrl(url) { return this.urlSerializer.parse(url); }
    scheduleNavigation(url, preventPushState) {
        const id = ++this.navigationId;
        this.routerEvents.next(new NavigationStart(id, this.serializeUrl(url)));
        return Promise.resolve().then((_) => this.runNavigate(url, preventPushState, id));
    }
    setUpLocationChangeListener() {
        this.locationSubscription = this.location.subscribe((change) => {
            return this.scheduleNavigation(this.urlSerializer.parse(change['url']), change['pop']);
        });
    }
    runNavigate(url, preventPushState, id) {
        if (id !== this.navigationId) {
            this.location.go(this.urlSerializer.serialize(this.currentUrlTree));
            this.routerEvents.next(new NavigationCancel(id, this.serializeUrl(url)));
            return Promise.resolve(false);
        }
        return new Promise((resolvePromise, rejectPromise) => {
            let updatedUrl;
            let state;
            applyRedirects(url, this.config)
                .mergeMap(u => {
                updatedUrl = u;
                return recognize(this.rootComponentType, this.config, updatedUrl, this.serializeUrl(updatedUrl));
            })
                .mergeMap((newRouterStateSnapshot) => {
                this.routerEvents.next(new RoutesRecognized(id, this.serializeUrl(url), this.serializeUrl(updatedUrl), newRouterStateSnapshot));
                return resolve(this.resolver, newRouterStateSnapshot);
            })
                .map((routerStateSnapshot) => {
                return createRouterState(routerStateSnapshot, this.currentRouterState);
            })
                .map((newState) => {
                state = newState;
            })
                .mergeMap(_ => {
                return new GuardChecks(state.snapshot, this.currentRouterState.snapshot, this.injector)
                    .check(this.outletMap);
            })
                .forEach((shouldActivate) => {
                if (!shouldActivate || id !== this.navigationId) {
                    this.routerEvents.next(new NavigationCancel(id, this.serializeUrl(url)));
                    return Promise.resolve(false);
                }
                new ActivateRoutes(state, this.currentRouterState).activate(this.outletMap);
                this.currentUrlTree = updatedUrl;
                this.currentRouterState = state;
                if (!preventPushState) {
                    let path = this.urlSerializer.serialize(updatedUrl);
                    if (this.location.isCurrentPathEqualTo(path)) {
                        this.location.replaceState(path);
                    }
                    else {
                        this.location.go(path);
                    }
                }
                return Promise.resolve(true);
            })
                .then(() => {
                this.routerEvents.next(new NavigationEnd(id, this.serializeUrl(url), this.serializeUrl(updatedUrl)));
                resolvePromise(true);
            }, e => {
                this.routerEvents.next(new NavigationError(id, this.serializeUrl(url), e));
                rejectPromise(e);
            });
        });
    }
}
class CanActivate {
    constructor(route) {
        this.route = route;
    }
}
class CanDeactivate {
    constructor(component, route) {
        this.component = component;
        this.route = route;
    }
}
class GuardChecks {
    constructor(future, curr, injector) {
        this.future = future;
        this.curr = curr;
        this.injector = injector;
        this.checks = [];
    }
    check(parentOutletMap) {
        const futureRoot = this.future._root;
        const currRoot = this.curr ? this.curr._root : null;
        this.traverseChildRoutes(futureRoot, currRoot, parentOutletMap);
        if (this.checks.length === 0)
            return of(true);
        return Observable.from(this.checks)
            .map(s => {
            if (s instanceof CanActivate) {
                return this.runCanActivate(s.route);
            }
            else if (s instanceof CanDeactivate) {
                return this.runCanDeactivate(s.component, s.route);
            }
            else {
                throw new Error('Cannot be reached');
            }
        })
            .mergeAll()
            .every(result => result === true);
    }
    traverseChildRoutes(futureNode, currNode, outletMap) {
        const prevChildren = nodeChildrenAsMap(currNode);
        futureNode.children.forEach(c => {
            this.traverseRoutes(c, prevChildren[c.value.outlet], outletMap);
            delete prevChildren[c.value.outlet];
        });
        forEach(prevChildren, (v, k) => this.deactivateOutletAndItChildren(v, outletMap._outlets[k]));
    }
    traverseRoutes(futureNode, currNode, parentOutletMap) {
        const future = futureNode.value;
        const curr = currNode ? currNode.value : null;
        const outlet = parentOutletMap ? parentOutletMap._outlets[futureNode.value.outlet] : null;
        if (curr && future._routeConfig === curr._routeConfig) {
            if (!shallowEqual(future.params, curr.params)) {
                this.checks.push(new CanDeactivate(outlet.component, curr), new CanActivate(future));
            }
            this.traverseChildRoutes(futureNode, currNode, outlet ? outlet.outletMap : null);
        }
        else {
            this.deactivateOutletAndItChildren(curr, outlet);
            this.checks.push(new CanActivate(future));
            this.traverseChildRoutes(futureNode, null, outlet ? outlet.outletMap : null);
        }
    }
    deactivateOutletAndItChildren(route, outlet) {
        if (outlet && outlet.isActivated) {
            forEach(outlet.outletMap._outlets, (v) => {
                if (v.isActivated) {
                    this.deactivateOutletAndItChildren(v.activatedRoute.snapshot, v);
                }
            });
            this.checks.push(new CanDeactivate(outlet.component, route));
        }
    }
    runCanActivate(future) {
        const canActivate = future._routeConfig ? future._routeConfig.canActivate : null;
        if (!canActivate || canActivate.length === 0)
            return of(true);
        return Observable.from(canActivate)
            .map(c => {
            const guard = this.injector.get(c);
            if (guard.canActivate) {
                return wrapIntoObservable(guard.canActivate(future, this.future));
            }
            else {
                return wrapIntoObservable(guard(future, this.future));
            }
        })
            .mergeAll()
            .every(result => result === true);
    }
    runCanDeactivate(component, curr) {
        const canDeactivate = curr._routeConfig ? curr._routeConfig.canDeactivate : null;
        if (!canDeactivate || canDeactivate.length === 0)
            return of(true);
        return Observable.from(canDeactivate)
            .map(c => {
            const guard = this.injector.get(c);
            if (guard.canDeactivate) {
                return wrapIntoObservable(guard.canDeactivate(component, curr, this.curr));
            }
            else {
                return wrapIntoObservable(guard(component, curr, this.curr));
            }
        })
            .mergeAll()
            .every(result => result === true);
    }
}
function wrapIntoObservable(value) {
    if (value instanceof Observable) {
        return value;
    }
    else {
        return of(value);
    }
}
class ActivateRoutes {
    constructor(futureState, currState) {
        this.futureState = futureState;
        this.currState = currState;
    }
    activate(parentOutletMap) {
        const futureRoot = this.futureState._root;
        const currRoot = this.currState ? this.currState._root : null;
        pushQueryParamsAndFragment(this.futureState);
        this.activateChildRoutes(futureRoot, currRoot, parentOutletMap);
    }
    activateChildRoutes(futureNode, currNode, outletMap) {
        const prevChildren = nodeChildrenAsMap(currNode);
        futureNode.children.forEach(c => {
            this.activateRoutes(c, prevChildren[c.value.outlet], outletMap);
            delete prevChildren[c.value.outlet];
        });
        forEach(prevChildren, (v, k) => this.deactivateOutletAndItChildren(outletMap._outlets[k]));
    }
    activateRoutes(futureNode, currNode, parentOutletMap) {
        const future = futureNode.value;
        const curr = currNode ? currNode.value : null;
        const outlet = getOutlet(parentOutletMap, futureNode.value);
        if (future === curr) {
            advanceActivatedRoute(future);
            this.activateChildRoutes(futureNode, currNode, outlet.outletMap);
        }
        else {
            this.deactivateOutletAndItChildren(outlet);
            const outletMap = new RouterOutletMap();
            this.activateNewRoutes(outletMap, future, outlet);
            this.activateChildRoutes(futureNode, null, outletMap);
        }
    }
    activateNewRoutes(outletMap, future, outlet) {
        const resolved = ReflectiveInjector.resolve([
            { provide: ActivatedRoute, useValue: future },
            { provide: RouterOutletMap, useValue: outletMap }
        ]);
        advanceActivatedRoute(future);
        outlet.activate(future._futureSnapshot._resolvedComponentFactory, future, resolved, outletMap);
    }
    deactivateOutletAndItChildren(outlet) {
        if (outlet && outlet.isActivated) {
            forEach(outlet.outletMap._outlets, (v) => this.deactivateOutletAndItChildren(v));
            outlet.deactivate();
        }
    }
}
function pushQueryParamsAndFragment(state) {
    if (!shallowEqual(state.snapshot.queryParams, state.queryParams.value)) {
        state.queryParams.next(state.snapshot.queryParams);
    }
    if (state.snapshot.fragment !== state.fragment.value) {
        state.fragment.next(state.snapshot.fragment);
    }
}
function nodeChildrenAsMap(node) {
    return node ? node.children.reduce((m, c) => {
        m[c.value.outlet] = c;
        return m;
    }, {}) : {};
}
function getOutlet(outletMap, route) {
    let outlet = outletMap._outlets[route.outlet];
    if (!outlet) {
        const componentName = route.component.name;
        if (route.outlet === PRIMARY_OUTLET) {
            throw new Error(`Cannot find primary outlet to load '${componentName}'`);
        }
        else {
            throw new Error(`Cannot find the outlet ${route.outlet} to load '${componentName}'`);
        }
    }
    return outlet;
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicm91dGVyLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vLi4vc3JjL3JvdXRlci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiT0FBTyx1QkFBdUI7T0FDdkIsd0JBQXdCO09BQ3hCLDRCQUE0QjtPQUM1QiwwQkFBMEI7T0FDMUIsNkJBQTZCO09BQzdCLHlCQUF5QjtPQUN6Qiw0QkFBNEI7T0FDNUIsMEJBQTBCO09BRzFCLEVBQThCLGtCQUFrQixFQUFPLE1BQU0sZUFBZTtPQUM1RSxFQUFDLFVBQVUsRUFBQyxNQUFNLGlCQUFpQjtPQUNuQyxFQUFDLE9BQU8sRUFBQyxNQUFNLGNBQWM7T0FFN0IsRUFBQyxFQUFFLEVBQUUsTUFBTSxvQkFBb0I7T0FFL0IsRUFBQyxjQUFjLEVBQUMsTUFBTSxtQkFBbUI7T0FDekMsRUFBZSxjQUFjLEVBQUMsTUFBTSxVQUFVO09BQzlDLEVBQUMsaUJBQWlCLEVBQUMsTUFBTSx1QkFBdUI7T0FDaEQsRUFBQyxhQUFhLEVBQUMsTUFBTSxtQkFBbUI7T0FFeEMsRUFBQyxTQUFTLEVBQUMsTUFBTSxhQUFhO09BQzlCLEVBQUMsT0FBTyxFQUFDLE1BQU0sV0FBVztPQUMxQixFQUFDLGVBQWUsRUFBQyxNQUFNLHFCQUFxQjtPQUM1QyxFQUFDLGNBQWMsRUFBNEQscUJBQXFCLEVBQUUsZ0JBQWdCLEVBQUMsTUFBTSxnQkFBZ0I7T0FDekksRUFBQyxjQUFjLEVBQVMsTUFBTSxVQUFVO09BRXhDLEVBQUMsT0FBTyxFQUFFLGtCQUFrQixFQUFDLE1BQU0sWUFBWTtPQUMvQyxFQUFDLE9BQU8sRUFBRSxZQUFZLEVBQUMsTUFBTSxvQkFBb0I7QUFZeEQ7SUFDRSxZQUFtQixFQUFVLEVBQVMsR0FBVztRQUE5QixPQUFFLEdBQUYsRUFBRSxDQUFRO1FBQVMsUUFBRyxHQUFILEdBQUcsQ0FBUTtJQUFHLENBQUM7SUFFckQsUUFBUSxLQUFhLE1BQU0sQ0FBQyx1QkFBdUIsSUFBSSxDQUFDLEVBQUUsV0FBVyxJQUFJLENBQUMsR0FB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QyxJQUFJLENBQUM7UUFDbEQsRUFBRSxDQUFDLENBQUMsS0FBSyxDQUFDLE1BQU0sS0FBSyxjQUFjLENBQUMsQ0FBQyxDQUFDO1lBQ3BDLE1BQU0sSUFBSSxLQUFLLENBQUMsdUNBQXVDLGFBQWEsR0FBRyxDQUFDLENBQUM7UUFDM0UsQ0FBQztRQUFDLElBQUksQ0FBQyxDQUFDO1lBQ04sTUFBTSxJQUFJLEtBQUssQ0FBQywwQkFBMEIsS0FBSyxDQUFDLE1BQU0sYUFBYSxhQUFhLEdBQUcsQ0FBQyxDQUFDO1FBQ3ZGLENBQUM7SUFDSCxDQUFDO0lBQ0QsTUFBTSxDQUFDLE1BQU0sQ0FBQztBQUNoQixDQUFDIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9tYXAnO1xuaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9zY2FuJztcbmltcG9ydCAncnhqcy9hZGQvb3BlcmF0b3IvbWVyZ2VNYXAnO1xuaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9jb25jYXQnO1xuaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9jb25jYXRNYXAnO1xuaW1wb3J0ICdyeGpzL2FkZC9vcGVyYXRvci9ldmVyeSc7XG5pbXBvcnQgJ3J4anMvYWRkL29wZXJhdG9yL21lcmdlQWxsJztcbmltcG9ydCAncnhqcy9hZGQvb2JzZXJ2YWJsZS9mcm9tJztcblxuaW1wb3J0IHtMb2NhdGlvbn0gZnJvbSAnQGFuZ3VsYXIvY29tbW9uJztcbmltcG9ydCB7Q29tcG9uZW50UmVzb2x2ZXIsIEluamVjdG9yLCBSZWZsZWN0aXZlSW5qZWN0b3IsIFR5cGV9IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHtPYnNlcnZhYmxlfSBmcm9tICdyeGpzL09ic2VydmFibGUnO1xuaW1wb3J0IHtTdWJqZWN0fSBmcm9tICdyeGpzL1N1YmplY3QnO1xuaW1wb3J0IHtTdWJzY3JpcHRpb259IGZyb20gJ3J4anMvU3Vic2NyaXB0aW9uJztcbmltcG9ydCB7b2YgfSBmcm9tICdyeGpzL29ic2VydmFibGUvb2YnO1xuXG5pbXBvcnQge2FwcGx5UmVkaXJlY3RzfSBmcm9tICcuL2FwcGx5X3JlZGlyZWN0cyc7XG5pbXBvcnQge1JvdXRlckNvbmZpZywgdmFsaWRhdGVDb25maWd9IGZyb20gJy4vY29uZmlnJztcbmltcG9ydCB7Y3JlYXRlUm91dGVyU3RhdGV9IGZyb20gJy4vY3JlYXRlX3JvdXRlcl9zdGF0ZSc7XG5pbXBvcnQge2NyZWF0ZVVybFRyZWV9IGZyb20gJy4vY3JlYXRlX3VybF90cmVlJztcbmltcG9ydCB7Um91dGVyT3V0bGV0fSBmcm9tICcuL2RpcmVjdGl2ZXMvcm91dGVyX291dGxldCc7XG5pbXBvcnQge3JlY29nbml6ZX0gZnJvbSAnLi9yZWNvZ25pemUnO1xuaW1wb3J0IHtyZXNvbHZlfSBmcm9tICcuL3Jlc29sdmUnO1xuaW1wb3J0IHtSb3V0ZXJPdXRsZXRNYXB9IGZyb20gJy4vcm91dGVyX291dGxldF9tYXAnO1xuaW1wb3J0IHtBY3RpdmF0ZWRSb3V0ZSwgQWN0aXZhdGVkUm91dGVTbmFwc2hvdCwgUm91dGVyU3RhdGUsIFJvdXRlclN0YXRlU25hcHNob3QsIGFkdmFuY2VBY3RpdmF0ZWRSb3V0ZSwgY3JlYXRlRW1wdHlTdGF0ZX0gZnJvbSAnLi9yb3V0ZXJfc3RhdGUnO1xuaW1wb3J0IHtQUklNQVJZX09VVExFVCwgUGFyYW1zfSBmcm9tICcuL3NoYXJlZCc7XG5pbXBvcnQge1VybFNlcmlhbGl6ZXJ9IGZyb20gJy4vdXJsX3NlcmlhbGl6ZXInO1xuaW1wb3J0IHtVcmxUcmVlLCBjcmVhdGVFbXB0eVVybFRyZWV9IGZyb20gJy4vdXJsX3RyZWUnO1xuaW1wb3J0IHtmb3JFYWNoLCBzaGFsbG93RXF1YWx9IGZyb20gJy4vdXRpbHMvY29sbGVjdGlvbic7XG5pbXBvcnQge1RyZWVOb2RlfSBmcm9tICcuL3V0aWxzL3RyZWUnO1xuXG5leHBvcnQgaW50ZXJmYWNlIE5hdmlnYXRpb25FeHRyYXMge1xuICByZWxhdGl2ZVRvPzogQWN0aXZhdGVkUm91dGU7XG4gIHF1ZXJ5UGFyYW1zPzogUGFyYW1zO1xuICBmcmFnbWVudD86IHN0cmluZztcbn1cblxuLyoqXG4gKiBBbiBldmVudCB0cmlnZ2VyZWQgd2hlbiBhIG5hdmlnYXRpb24gc3RhcnRzXG4gKi9cbmV4cG9ydCBjbGFzcyBOYXZpZ2F0aW9uU3RhcnQge1xuICBjb25zdHJ1Y3RvcihwdWJsaWMgaWQ6IG51bWJlciwgcHVibGljIHVybDogc3RyaW5nKSB7fVxuXG4gIHRvU3RyaW5nKCk6IHN0cmluZyB7IHJldHVybiBgTmF2aWdhdGlvblN0YXJ0KGlkOiAke3RoaXMuaWR9LCB1cmw6ICcke3RoaXMudXJsfScpYDsgfVxufVxuXG4vKipcbiAqIEFuIGV2ZW50IHRyaWdnZXJlZCB3aGVuIGEgbmF2aWdhdGlvbiBlbmRzIHN1Y2Nlc3NmdWxseVxuICovXG5leHBvcnQgY2xhc3MgTmF2aWdhdGlvbkVuZCB7XG4gIGNvbnN0cnVjdG9yKHB1YmxpYyBpZDogbnVtYmVyLCBwdWJsaWMgdXJsOiBzdHJpbmcsIHB1YmxpYyB1cmxBZnRlclJlZGlyZWN0czogc3RyaW5nKSB7fVxuXG4gIHRvU3RyaW5nKCk6IHN0cmluZyB7XG4gICAgcmV0dXJuIGBOYXZpZ2F0aW9uRW5kKGlkOiAke3RoaXMuaWR9LCB1cmw6ICcke3RoaXMudXJsfScsIHVybEFmdGVyUmVkaXJlY3RzOiAnJHt0aGlzLnVybEFmdGVyUmVkaXJlY3RzfScpYDtcbiAgfVxufVxuXG4vKipcbiAqIEFuIGV2ZW50IHRyaWdnZXJlZCB3aGVuIGEgbmF2aWdhdGlvbiBpcyBjYW5jZWxlZFxuICovXG5leHBvcnQgY2xhc3MgTmF2aWdhdGlvbkNhbmNlbCB7XG4gIGNvbnN0cnVjdG9yKHB1YmxpYyBpZDogbnVtYmVyLCBwdWJsaWMgdXJsOiBzdHJpbmcpIHt9XG5cbiAgdG9TdHJpbmcoKTogc3RyaW5nIHsgcmV0dXJuIGBOYXZpZ2F0aW9uQ2FuY2VsKGlkOiAke3RoaXMuaWR9LCB1cmw6ICcke3RoaXMudXJsfScpYDsgfVxufVxuXG4vKipcbiAqIEFuIGV2ZW50IHRyaWdnZXJlZCB3aGVuIGEgbmF2aWdhdGlvbiBmYWlscyBkdWUgdG8gdW5leHBlY3RlZCBlcnJvclxuICovXG5leHBvcnQgY2xhc3MgTmF2aWdhdGlvbkVycm9yIHtcbiAgY29uc3RydWN0b3IocHVibGljIGlkOiBudW1iZXIsIHB1YmxpYyB1cmw6IHN0cmluZywgcHVibGljIGVycm9yOiBhbnkpIHt9XG5cbiAgdG9TdHJpbmcoKTogc3RyaW5nIHtcbiAgICByZXR1cm4gYE5hdmlnYXRpb25FcnJvcihpZDogJHt0aGlzLmlkfSwgdXJsOiAnJHt0aGlzLnVybH0nLCBlcnJvcjogJHt0aGlzLmVycm9yfSlgO1xuICB9XG59XG5cbi8qKlxuICogQW4gZXZlbnQgdHJpZ2dlcmVkIHdoZW4gcm91dGVzIGFyZSByZWNvZ25pemVkXG4gKi9cbmV4cG9ydCBjbGFzcyBSb3V0ZXNSZWNvZ25pemVkIHtcbiAgY29uc3RydWN0b3IoXG4gICAgICBwdWJsaWMgaWQ6IG51bWJlciwgcHVibGljIHVybDogc3RyaW5nLCBwdWJsaWMgdXJsQWZ0ZXJSZWRpcmVjdHM6IHN0cmluZyxcbiAgICAgIHB1YmxpYyBzdGF0ZTogUm91dGVyU3RhdGVTbmFwc2hvdCkge31cblxuICB0b1N0cmluZygpOiBzdHJpbmcge1xuICAgIHJldHVybiBgUm91dGVzUmVjb2duaXplZChpZDogJHt0aGlzLmlkfSwgdXJsOiAnJHt0aGlzLnVybH0nLCB1cmxBZnRlclJlZGlyZWN0czogJyR7dGhpcy51cmxBZnRlclJlZGlyZWN0c30nLCBzdGF0ZTogJHt0aGlzLnN0YXRlfSlgO1xuICB9XG59XG5cbmV4cG9ydCB0eXBlIEV2ZW50ID0gTmF2aWdhdGlvblN0YXJ0IHwgTmF2aWdhdGlvbkVuZCB8IE5hdmlnYXRpb25DYW5jZWwgfCBOYXZpZ2F0aW9uRXJyb3I7XG5cbi8qKlxuICogVGhlIGBSb3V0ZXJgIGlzIHJlc3BvbnNpYmxlIGZvciBtYXBwaW5nIFVSTHMgdG8gY29tcG9uZW50cy5cbiAqL1xuZXhwb3J0IGNsYXNzIFJvdXRlciB7XG4gIHByaXZhdGUgY3VycmVudFVybFRyZWU6IFVybFRyZWU7XG4gIHByaXZhdGUgY3VycmVudFJvdXRlclN0YXRlOiBSb3V0ZXJTdGF0ZTtcbiAgcHJpdmF0ZSBsb2NhdGlvblN1YnNjcmlwdGlvbjogU3Vic2NyaXB0aW9uO1xuICBwcml2YXRlIHJvdXRlckV2ZW50czogU3ViamVjdDxFdmVudD47XG4gIHByaXZhdGUgbmF2aWdhdGlvbklkOiBudW1iZXIgPSAwO1xuICBwcml2YXRlIGNvbmZpZzogUm91dGVyQ29uZmlnO1xuXG4gIC8qKlxuICAgKiBAaW50ZXJuYWxcbiAgICovXG4gIGNvbnN0cnVjdG9yKFxuICAgICAgcHJpdmF0ZSByb290Q29tcG9uZW50VHlwZTogVHlwZSwgcHJpdmF0ZSByZXNvbHZlcjogQ29tcG9uZW50UmVzb2x2ZXIsXG4gICAgICBwcml2YXRlIHVybFNlcmlhbGl6ZXI6IFVybFNlcmlhbGl6ZXIsIHByaXZhdGUgb3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXAsXG4gICAgICBwcml2YXRlIGxvY2F0aW9uOiBMb2NhdGlvbiwgcHJpdmF0ZSBpbmplY3RvcjogSW5qZWN0b3IsIGNvbmZpZzogUm91dGVyQ29uZmlnKSB7XG4gICAgdGhpcy5yZXNldENvbmZpZyhjb25maWcpO1xuICAgIHRoaXMucm91dGVyRXZlbnRzID0gbmV3IFN1YmplY3Q8RXZlbnQ+KCk7XG4gICAgdGhpcy5jdXJyZW50VXJsVHJlZSA9IGNyZWF0ZUVtcHR5VXJsVHJlZSgpO1xuICAgIHRoaXMuY3VycmVudFJvdXRlclN0YXRlID0gY3JlYXRlRW1wdHlTdGF0ZSh0aGlzLmN1cnJlbnRVcmxUcmVlLCB0aGlzLnJvb3RDb21wb25lbnRUeXBlKTtcbiAgfVxuXG4gIC8qKlxuICAgKiBAaW50ZXJuYWxcbiAgICovXG4gIGluaXRpYWxOYXZpZ2F0aW9uKCk6IHZvaWQge1xuICAgIHRoaXMuc2V0VXBMb2NhdGlvbkNoYW5nZUxpc3RlbmVyKCk7XG4gICAgdGhpcy5uYXZpZ2F0ZUJ5VXJsKHRoaXMubG9jYXRpb24ucGF0aCgpKTtcbiAgfVxuXG4gIC8qKlxuICAgKiBSZXR1cm5zIHRoZSBjdXJyZW50IHJvdXRlIHN0YXRlLlxuICAgKi9cbiAgZ2V0IHJvdXRlclN0YXRlKCk6IFJvdXRlclN0YXRlIHsgcmV0dXJuIHRoaXMuY3VycmVudFJvdXRlclN0YXRlOyB9XG5cbiAgLyoqXG4gICAqIFJldHVybnMgdGhlIGN1cnJlbnQgdXJsLlxuICAgKi9cbiAgZ2V0IHVybCgpOiBzdHJpbmcgeyByZXR1cm4gdGhpcy5zZXJpYWxpemVVcmwodGhpcy5jdXJyZW50VXJsVHJlZSk7IH1cblxuICAvKipcbiAgICogUmV0dXJucyBhbiBvYnNlcnZhYmxlIG9mIHJvdXRlIGV2ZW50c1xuICAgKi9cbiAgZ2V0IGV2ZW50cygpOiBPYnNlcnZhYmxlPEV2ZW50PiB7IHJldHVybiB0aGlzLnJvdXRlckV2ZW50czsgfVxuXG4gIC8qKlxuICAgKiBSZXNldHMgdGhlIGNvbmZpZ3VyYXRpb24gdXNlZCBmb3IgbmF2aWdhdGlvbiBhbmQgZ2VuZXJhdGluZyBsaW5rcy5cbiAgICpcbiAgICogIyMjIFVzYWdlXG4gICAqXG4gICAqIGBgYFxuICAgKiByb3V0ZXIucmVzZXRDb25maWcoW1xuICAgKiAgeyBwYXRoOiAndGVhbS86aWQnLCBjb21wb25lbnQ6IFRlYW1DbXAsIGNoaWxkcmVuOiBbXG4gICAqICAgIHsgcGF0aDogJ3NpbXBsZScsIGNvbXBvbmVudDogU2ltcGxlQ21wIH0sXG4gICAqICAgIHsgcGF0aDogJ3VzZXIvOm5hbWUnLCBjb21wb25lbnQ6IFVzZXJDbXAgfVxuICAgKiAgXSB9XG4gICAqIF0pO1xuICAgKiBgYGBcbiAgICovXG4gIHJlc2V0Q29uZmlnKGNvbmZpZzogUm91dGVyQ29uZmlnKTogdm9pZCB7XG4gICAgdmFsaWRhdGVDb25maWcoY29uZmlnKTtcbiAgICB0aGlzLmNvbmZpZyA9IGNvbmZpZztcbiAgfVxuXG4gIC8qKlxuICAgKiBAaW50ZXJuYWxcbiAgICovXG4gIGRpc3Bvc2UoKTogdm9pZCB7IHRoaXMubG9jYXRpb25TdWJzY3JpcHRpb24udW5zdWJzY3JpYmUoKTsgfVxuXG4gIC8qKlxuICAgKiBBcHBsaWVzIGFuIGFycmF5IG9mIGNvbW1hbmRzIHRvIHRoZSBjdXJyZW50IHVybCB0cmVlIGFuZCBjcmVhdGVzXG4gICAqIGEgbmV3IHVybCB0cmVlLlxuICAgKlxuICAgKiBXaGVuIGdpdmVuIGFuIGFjdGl2YXRlIHJvdXRlLCBhcHBsaWVzIHRoZSBnaXZlbiBjb21tYW5kcyBzdGFydGluZyBmcm9tIHRoZSByb3V0ZS5cbiAgICogV2hlbiBub3QgZ2l2ZW4gYSByb3V0ZSwgYXBwbGllcyB0aGUgZ2l2ZW4gY29tbWFuZCBzdGFydGluZyBmcm9tIHRoZSByb290LlxuICAgKlxuICAgKiAjIyMgVXNhZ2VcbiAgICpcbiAgICogYGBgXG4gICAqIC8vIGNyZWF0ZSAvdGVhbS8zMy91c2VyLzExXG4gICAqIHJvdXRlci5jcmVhdGVVcmxUcmVlKFsnL3RlYW0nLCAzMywgJ3VzZXInLCAxMV0pO1xuICAgKlxuICAgKiAvLyBjcmVhdGUgL3RlYW0vMzM7ZXhwYW5kPXRydWUvdXNlci8xMVxuICAgKiByb3V0ZXIuY3JlYXRlVXJsVHJlZShbJy90ZWFtJywgMzMsIHtleHBhbmQ6IHRydWV9LCAndXNlcicsIDExXSk7XG4gICAqXG4gICAqIC8vIHlvdSBjYW4gY29sbGFwc2Ugc3RhdGljIGZyYWdtZW50cyBsaWtlIHRoaXNcbiAgICogcm91dGVyLmNyZWF0ZVVybFRyZWUoWycvdGVhbS8zMy91c2VyJywgdXNlcklkXSk7XG4gICAqXG4gICAqIC8vIGFzc3VtaW5nIHRoZSBjdXJyZW50IHVybCBpcyBgL3RlYW0vMzMvdXNlci8xMWAgYW5kIHRoZSByb3V0ZSBwb2ludHMgdG8gYHVzZXIvMTFgXG4gICAqXG4gICAqIC8vIG5hdmlnYXRlIHRvIC90ZWFtLzMzL3VzZXIvMTEvZGV0YWlsc1xuICAgKiByb3V0ZXIuY3JlYXRlVXJsVHJlZShbJ2RldGFpbHMnXSwge3JlbGF0aXZlVG86IHJvdXRlfSk7XG4gICAqXG4gICAqIC8vIG5hdmlnYXRlIHRvIC90ZWFtLzMzL3VzZXIvMjJcbiAgICogcm91dGVyLmNyZWF0ZVVybFRyZWUoWycuLi8yMiddLCB7cmVsYXRpdmVUbzogcm91dGV9KTtcbiAgICpcbiAgICogLy8gbmF2aWdhdGUgdG8gL3RlYW0vNDQvdXNlci8yMlxuICAgKiByb3V0ZXIuY3JlYXRlVXJsVHJlZShbJy4uLy4uL3RlYW0vNDQvdXNlci8yMiddLCB7cmVsYXRpdmVUbzogcm91dGV9KTtcbiAgICogYGBgXG4gICAqL1xuICBjcmVhdGVVcmxUcmVlKGNvbW1hbmRzOiBhbnlbXSwge3JlbGF0aXZlVG8sIHF1ZXJ5UGFyYW1zLCBmcmFnbWVudH06IE5hdmlnYXRpb25FeHRyYXMgPSB7fSk6XG4gICAgICBVcmxUcmVlIHtcbiAgICBjb25zdCBhID0gcmVsYXRpdmVUbyA/IHJlbGF0aXZlVG8gOiB0aGlzLnJvdXRlclN0YXRlLnJvb3Q7XG4gICAgcmV0dXJuIGNyZWF0ZVVybFRyZWUoYSwgdGhpcy5jdXJyZW50VXJsVHJlZSwgY29tbWFuZHMsIHF1ZXJ5UGFyYW1zLCBmcmFnbWVudCk7XG4gIH1cblxuICAvKipcbiAgICogTmF2aWdhdGUgYmFzZWQgb24gdGhlIHByb3ZpZGVkIHVybC4gVGhpcyBuYXZpZ2F0aW9uIGlzIGFsd2F5cyBhYnNvbHV0ZS5cbiAgICpcbiAgICogUmV0dXJucyBhIHByb21pc2UgdGhhdDpcbiAgICogLSBpcyByZXNvbHZlZCB3aXRoICd0cnVlJyB3aGVuIG5hdmlnYXRpb24gc3VjY2VlZHNcbiAgICogLSBpcyByZXNvbHZlZCB3aXRoICdmYWxzZScgd2hlbiBuYXZpZ2F0aW9uIGZhaWxzXG4gICAqIC0gaXMgcmVqZWN0ZWQgd2hlbiBhbiBlcnJvciBoYXBwZW5zXG4gICAqXG4gICAqICMjIyBVc2FnZVxuICAgKlxuICAgKiBgYGBcbiAgICogcm91dGVyLm5hdmlnYXRlQnlVcmwoXCIvdGVhbS8zMy91c2VyLzExXCIpO1xuICAgKiBgYGBcbiAgICovXG4gIG5hdmlnYXRlQnlVcmwodXJsOiBzdHJpbmd8VXJsVHJlZSk6IFByb21pc2U8Ym9vbGVhbj4ge1xuICAgIGlmICh1cmwgaW5zdGFuY2VvZiBVcmxUcmVlKSB7XG4gICAgICByZXR1cm4gdGhpcy5zY2hlZHVsZU5hdmlnYXRpb24odXJsLCBmYWxzZSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIGNvbnN0IHVybFRyZWUgPSB0aGlzLnVybFNlcmlhbGl6ZXIucGFyc2UodXJsKTtcbiAgICAgIHJldHVybiB0aGlzLnNjaGVkdWxlTmF2aWdhdGlvbih1cmxUcmVlLCBmYWxzZSk7XG4gICAgfVxuICB9XG5cbiAgLyoqXG4gICAqIE5hdmlnYXRlIGJhc2VkIG9uIHRoZSBwcm92aWRlZCBhcnJheSBvZiBjb21tYW5kcyBhbmQgYSBzdGFydGluZyBwb2ludC5cbiAgICogSWYgbm8gc3RhcnRpbmcgcm91dGUgaXMgcHJvdmlkZWQsIHRoZSBuYXZpZ2F0aW9uIGlzIGFic29sdXRlLlxuICAgKlxuICAgKiBSZXR1cm5zIGEgcHJvbWlzZSB0aGF0OlxuICAgKiAtIGlzIHJlc29sdmVkIHdpdGggJ3RydWUnIHdoZW4gbmF2aWdhdGlvbiBzdWNjZWVkc1xuICAgKiAtIGlzIHJlc29sdmVkIHdpdGggJ2ZhbHNlJyB3aGVuIG5hdmlnYXRpb24gZmFpbHNcbiAgICogLSBpcyByZWplY3RlZCB3aGVuIGFuIGVycm9yIGhhcHBlbnNcbiAgICpcbiAgICogIyMjIFVzYWdlXG4gICAqXG4gICAqIGBgYFxuICAgKiByb3V0ZXIubmF2aWdhdGUoWyd0ZWFtJywgMzMsICd0ZWFtJywgJzExXSwge3JlbGF0aXZlVG86IHJvdXRlfSk7XG4gICAqIGBgYFxuICAgKi9cbiAgbmF2aWdhdGUoY29tbWFuZHM6IGFueVtdLCBleHRyYXM6IE5hdmlnYXRpb25FeHRyYXMgPSB7fSk6IFByb21pc2U8Ym9vbGVhbj4ge1xuICAgIHJldHVybiB0aGlzLnNjaGVkdWxlTmF2aWdhdGlvbih0aGlzLmNyZWF0ZVVybFRyZWUoY29tbWFuZHMsIGV4dHJhcyksIGZhbHNlKTtcbiAgfVxuXG4gIC8qKlxuICAgKiBTZXJpYWxpemVzIGEge0BsaW5rIFVybFRyZWV9IGludG8gYSBzdHJpbmcuXG4gICAqL1xuICBzZXJpYWxpemVVcmwodXJsOiBVcmxUcmVlKTogc3RyaW5nIHsgcmV0dXJuIHRoaXMudXJsU2VyaWFsaXplci5zZXJpYWxpemUodXJsKTsgfVxuXG4gIC8qKlxuICAgKiBQYXJzZSBhIHN0cmluZyBpbnRvIGEge0BsaW5rIFVybFRyZWV9LlxuICAgKi9cbiAgcGFyc2VVcmwodXJsOiBzdHJpbmcpOiBVcmxUcmVlIHsgcmV0dXJuIHRoaXMudXJsU2VyaWFsaXplci5wYXJzZSh1cmwpOyB9XG5cbiAgcHJpdmF0ZSBzY2hlZHVsZU5hdmlnYXRpb24odXJsOiBVcmxUcmVlLCBwcmV2ZW50UHVzaFN0YXRlOiBib29sZWFuKTogUHJvbWlzZTxib29sZWFuPiB7XG4gICAgY29uc3QgaWQgPSArK3RoaXMubmF2aWdhdGlvbklkO1xuICAgIHRoaXMucm91dGVyRXZlbnRzLm5leHQobmV3IE5hdmlnYXRpb25TdGFydChpZCwgdGhpcy5zZXJpYWxpemVVcmwodXJsKSkpO1xuICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUoKS50aGVuKChfKSA9PiB0aGlzLnJ1bk5hdmlnYXRlKHVybCwgcHJldmVudFB1c2hTdGF0ZSwgaWQpKTtcbiAgfVxuXG4gIHByaXZhdGUgc2V0VXBMb2NhdGlvbkNoYW5nZUxpc3RlbmVyKCk6IHZvaWQge1xuICAgIHRoaXMubG9jYXRpb25TdWJzY3JpcHRpb24gPSA8YW55PnRoaXMubG9jYXRpb24uc3Vic2NyaWJlKChjaGFuZ2UpID0+IHtcbiAgICAgIHJldHVybiB0aGlzLnNjaGVkdWxlTmF2aWdhdGlvbih0aGlzLnVybFNlcmlhbGl6ZXIucGFyc2UoY2hhbmdlWyd1cmwnXSksIGNoYW5nZVsncG9wJ10pO1xuICAgIH0pO1xuICB9XG5cbiAgcHJpdmF0ZSBydW5OYXZpZ2F0ZSh1cmw6IFVybFRyZWUsIHByZXZlbnRQdXNoU3RhdGU6IGJvb2xlYW4sIGlkOiBudW1iZXIpOiBQcm9taXNlPGJvb2xlYW4+IHtcbiAgICBpZiAoaWQgIT09IHRoaXMubmF2aWdhdGlvbklkKSB7XG4gICAgICB0aGlzLmxvY2F0aW9uLmdvKHRoaXMudXJsU2VyaWFsaXplci5zZXJpYWxpemUodGhpcy5jdXJyZW50VXJsVHJlZSkpO1xuICAgICAgdGhpcy5yb3V0ZXJFdmVudHMubmV4dChuZXcgTmF2aWdhdGlvbkNhbmNlbChpZCwgdGhpcy5zZXJpYWxpemVVcmwodXJsKSkpO1xuICAgICAgcmV0dXJuIFByb21pc2UucmVzb2x2ZShmYWxzZSk7XG4gICAgfVxuXG4gICAgcmV0dXJuIG5ldyBQcm9taXNlKChyZXNvbHZlUHJvbWlzZSwgcmVqZWN0UHJvbWlzZSkgPT4ge1xuICAgICAgbGV0IHVwZGF0ZWRVcmw6IFVybFRyZWU7XG4gICAgICBsZXQgc3RhdGU6IFJvdXRlclN0YXRlO1xuICAgICAgYXBwbHlSZWRpcmVjdHModXJsLCB0aGlzLmNvbmZpZylcbiAgICAgICAgICAubWVyZ2VNYXAodSA9PiB7XG4gICAgICAgICAgICB1cGRhdGVkVXJsID0gdTtcbiAgICAgICAgICAgIHJldHVybiByZWNvZ25pemUoXG4gICAgICAgICAgICAgICAgdGhpcy5yb290Q29tcG9uZW50VHlwZSwgdGhpcy5jb25maWcsIHVwZGF0ZWRVcmwsIHRoaXMuc2VyaWFsaXplVXJsKHVwZGF0ZWRVcmwpKTtcbiAgICAgICAgICB9KVxuXG4gICAgICAgICAgLm1lcmdlTWFwKChuZXdSb3V0ZXJTdGF0ZVNuYXBzaG90KSA9PiB7XG4gICAgICAgICAgICB0aGlzLnJvdXRlckV2ZW50cy5uZXh0KG5ldyBSb3V0ZXNSZWNvZ25pemVkKFxuICAgICAgICAgICAgICAgIGlkLCB0aGlzLnNlcmlhbGl6ZVVybCh1cmwpLCB0aGlzLnNlcmlhbGl6ZVVybCh1cGRhdGVkVXJsKSwgbmV3Um91dGVyU3RhdGVTbmFwc2hvdCkpO1xuICAgICAgICAgICAgcmV0dXJuIHJlc29sdmUodGhpcy5yZXNvbHZlciwgbmV3Um91dGVyU3RhdGVTbmFwc2hvdCk7XG5cbiAgICAgICAgICB9KVxuICAgICAgICAgIC5tYXAoKHJvdXRlclN0YXRlU25hcHNob3QpID0+IHtcbiAgICAgICAgICAgIHJldHVybiBjcmVhdGVSb3V0ZXJTdGF0ZShyb3V0ZXJTdGF0ZVNuYXBzaG90LCB0aGlzLmN1cnJlbnRSb3V0ZXJTdGF0ZSk7XG5cbiAgICAgICAgICB9KVxuICAgICAgICAgIC5tYXAoKG5ld1N0YXRlOiBSb3V0ZXJTdGF0ZSkgPT4ge1xuICAgICAgICAgICAgc3RhdGUgPSBuZXdTdGF0ZTtcblxuICAgICAgICAgIH0pXG4gICAgICAgICAgLm1lcmdlTWFwKF8gPT4ge1xuICAgICAgICAgICAgcmV0dXJuIG5ldyBHdWFyZENoZWNrcyhzdGF0ZS5zbmFwc2hvdCwgdGhpcy5jdXJyZW50Um91dGVyU3RhdGUuc25hcHNob3QsIHRoaXMuaW5qZWN0b3IpXG4gICAgICAgICAgICAgICAgLmNoZWNrKHRoaXMub3V0bGV0TWFwKTtcblxuICAgICAgICAgIH0pXG4gICAgICAgICAgLmZvckVhY2goKHNob3VsZEFjdGl2YXRlOiBib29sZWFuKSA9PiB7XG4gICAgICAgICAgICBpZiAoIXNob3VsZEFjdGl2YXRlIHx8IGlkICE9PSB0aGlzLm5hdmlnYXRpb25JZCkge1xuICAgICAgICAgICAgICB0aGlzLnJvdXRlckV2ZW50cy5uZXh0KG5ldyBOYXZpZ2F0aW9uQ2FuY2VsKGlkLCB0aGlzLnNlcmlhbGl6ZVVybCh1cmwpKSk7XG4gICAgICAgICAgICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUoZmFsc2UpO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICBuZXcgQWN0aXZhdGVSb3V0ZXMoc3RhdGUsIHRoaXMuY3VycmVudFJvdXRlclN0YXRlKS5hY3RpdmF0ZSh0aGlzLm91dGxldE1hcCk7XG5cbiAgICAgICAgICAgIHRoaXMuY3VycmVudFVybFRyZWUgPSB1cGRhdGVkVXJsO1xuICAgICAgICAgICAgdGhpcy5jdXJyZW50Um91dGVyU3RhdGUgPSBzdGF0ZTtcbiAgICAgICAgICAgIGlmICghcHJldmVudFB1c2hTdGF0ZSkge1xuICAgICAgICAgICAgICBsZXQgcGF0aCA9IHRoaXMudXJsU2VyaWFsaXplci5zZXJpYWxpemUodXBkYXRlZFVybCk7XG4gICAgICAgICAgICAgIGlmICh0aGlzLmxvY2F0aW9uLmlzQ3VycmVudFBhdGhFcXVhbFRvKHBhdGgpKSB7XG4gICAgICAgICAgICAgICAgdGhpcy5sb2NhdGlvbi5yZXBsYWNlU3RhdGUocGF0aCk7XG4gICAgICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAgICAgdGhpcy5sb2NhdGlvbi5nbyhwYXRoKTtcbiAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIFByb21pc2UucmVzb2x2ZSh0cnVlKTtcbiAgICAgICAgICB9KVxuICAgICAgICAgIC50aGVuKFxuICAgICAgICAgICAgICAoKSA9PiB7XG4gICAgICAgICAgICAgICAgdGhpcy5yb3V0ZXJFdmVudHMubmV4dChcbiAgICAgICAgICAgICAgICAgICAgbmV3IE5hdmlnYXRpb25FbmQoaWQsIHRoaXMuc2VyaWFsaXplVXJsKHVybCksIHRoaXMuc2VyaWFsaXplVXJsKHVwZGF0ZWRVcmwpKSk7XG4gICAgICAgICAgICAgICAgcmVzb2x2ZVByb21pc2UodHJ1ZSk7XG5cbiAgICAgICAgICAgICAgfSxcbiAgICAgICAgICAgICAgZSA9PiB7XG4gICAgICAgICAgICAgICAgdGhpcy5yb3V0ZXJFdmVudHMubmV4dChuZXcgTmF2aWdhdGlvbkVycm9yKGlkLCB0aGlzLnNlcmlhbGl6ZVVybCh1cmwpLCBlKSk7XG4gICAgICAgICAgICAgICAgcmVqZWN0UHJvbWlzZShlKTtcbiAgICAgICAgICAgICAgfSk7XG4gICAgfSk7XG4gIH1cbn1cblxuY2xhc3MgQ2FuQWN0aXZhdGUge1xuICBjb25zdHJ1Y3RvcihwdWJsaWMgcm91dGU6IEFjdGl2YXRlZFJvdXRlU25hcHNob3QpIHt9XG59XG5jbGFzcyBDYW5EZWFjdGl2YXRlIHtcbiAgY29uc3RydWN0b3IocHVibGljIGNvbXBvbmVudDogT2JqZWN0LCBwdWJsaWMgcm91dGU6IEFjdGl2YXRlZFJvdXRlU25hcHNob3QpIHt9XG59XG5cbmNsYXNzIEd1YXJkQ2hlY2tzIHtcbiAgcHJpdmF0ZSBjaGVja3M6IEFycmF5PENhbkFjdGl2YXRlfENhbkRlYWN0aXZhdGU+ID0gW107XG4gIGNvbnN0cnVjdG9yKFxuICAgICAgcHJpdmF0ZSBmdXR1cmU6IFJvdXRlclN0YXRlU25hcHNob3QsIHByaXZhdGUgY3VycjogUm91dGVyU3RhdGVTbmFwc2hvdCxcbiAgICAgIHByaXZhdGUgaW5qZWN0b3I6IEluamVjdG9yKSB7fVxuXG4gIGNoZWNrKHBhcmVudE91dGxldE1hcDogUm91dGVyT3V0bGV0TWFwKTogT2JzZXJ2YWJsZTxib29sZWFuPiB7XG4gICAgY29uc3QgZnV0dXJlUm9vdCA9IHRoaXMuZnV0dXJlLl9yb290O1xuICAgIGNvbnN0IGN1cnJSb290ID0gdGhpcy5jdXJyID8gdGhpcy5jdXJyLl9yb290IDogbnVsbDtcbiAgICB0aGlzLnRyYXZlcnNlQ2hpbGRSb3V0ZXMoZnV0dXJlUm9vdCwgY3VyclJvb3QsIHBhcmVudE91dGxldE1hcCk7XG4gICAgaWYgKHRoaXMuY2hlY2tzLmxlbmd0aCA9PT0gMCkgcmV0dXJuIG9mICh0cnVlKTtcblxuICAgIHJldHVybiBPYnNlcnZhYmxlLmZyb20odGhpcy5jaGVja3MpXG4gICAgICAgIC5tYXAocyA9PiB7XG4gICAgICAgICAgaWYgKHMgaW5zdGFuY2VvZiBDYW5BY3RpdmF0ZSkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucnVuQ2FuQWN0aXZhdGUocy5yb3V0ZSk7XG4gICAgICAgICAgfSBlbHNlIGlmIChzIGluc3RhbmNlb2YgQ2FuRGVhY3RpdmF0ZSkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucnVuQ2FuRGVhY3RpdmF0ZShzLmNvbXBvbmVudCwgcy5yb3V0ZSk7XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcignQ2Fubm90IGJlIHJlYWNoZWQnKTtcbiAgICAgICAgICB9XG4gICAgICAgIH0pXG4gICAgICAgIC5tZXJnZUFsbCgpXG4gICAgICAgIC5ldmVyeShyZXN1bHQgPT4gcmVzdWx0ID09PSB0cnVlKTtcbiAgfVxuXG4gIHByaXZhdGUgdHJhdmVyc2VDaGlsZFJvdXRlcyhcbiAgICAgIGZ1dHVyZU5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlU25hcHNob3Q+LCBjdXJyTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGVTbmFwc2hvdD4sXG4gICAgICBvdXRsZXRNYXA6IFJvdXRlck91dGxldE1hcCk6IHZvaWQge1xuICAgIGNvbnN0IHByZXZDaGlsZHJlbjoge1trZXk6IHN0cmluZ106IGFueX0gPSBub2RlQ2hpbGRyZW5Bc01hcChjdXJyTm9kZSk7XG4gICAgZnV0dXJlTm9kZS5jaGlsZHJlbi5mb3JFYWNoKGMgPT4ge1xuICAgICAgdGhpcy50cmF2ZXJzZVJvdXRlcyhjLCBwcmV2Q2hpbGRyZW5bYy52YWx1ZS5vdXRsZXRdLCBvdXRsZXRNYXApO1xuICAgICAgZGVsZXRlIHByZXZDaGlsZHJlbltjLnZhbHVlLm91dGxldF07XG4gICAgfSk7XG4gICAgZm9yRWFjaChcbiAgICAgICAgcHJldkNoaWxkcmVuLFxuICAgICAgICAodjogYW55LCBrOiBzdHJpbmcpID0+IHRoaXMuZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4odiwgb3V0bGV0TWFwLl9vdXRsZXRzW2tdKSk7XG4gIH1cblxuICB0cmF2ZXJzZVJvdXRlcyhcbiAgICAgIGZ1dHVyZU5vZGU6IFRyZWVOb2RlPEFjdGl2YXRlZFJvdXRlU25hcHNob3Q+LCBjdXJyTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGVTbmFwc2hvdD4sXG4gICAgICBwYXJlbnRPdXRsZXRNYXA6IFJvdXRlck91dGxldE1hcCk6IHZvaWQge1xuICAgIGNvbnN0IGZ1dHVyZSA9IGZ1dHVyZU5vZGUudmFsdWU7XG4gICAgY29uc3QgY3VyciA9IGN1cnJOb2RlID8gY3Vyck5vZGUudmFsdWUgOiBudWxsO1xuICAgIGNvbnN0IG91dGxldCA9IHBhcmVudE91dGxldE1hcCA/IHBhcmVudE91dGxldE1hcC5fb3V0bGV0c1tmdXR1cmVOb2RlLnZhbHVlLm91dGxldF0gOiBudWxsO1xuXG4gICAgaWYgKGN1cnIgJiYgZnV0dXJlLl9yb3V0ZUNvbmZpZyA9PT0gY3Vyci5fcm91dGVDb25maWcpIHtcbiAgICAgIGlmICghc2hhbGxvd0VxdWFsKGZ1dHVyZS5wYXJhbXMsIGN1cnIucGFyYW1zKSkge1xuICAgICAgICB0aGlzLmNoZWNrcy5wdXNoKG5ldyBDYW5EZWFjdGl2YXRlKG91dGxldC5jb21wb25lbnQsIGN1cnIpLCBuZXcgQ2FuQWN0aXZhdGUoZnV0dXJlKSk7XG4gICAgICB9XG4gICAgICB0aGlzLnRyYXZlcnNlQ2hpbGRSb3V0ZXMoZnV0dXJlTm9kZSwgY3Vyck5vZGUsIG91dGxldCA/IG91dGxldC5vdXRsZXRNYXAgOiBudWxsKTtcbiAgICB9IGVsc2Uge1xuICAgICAgdGhpcy5kZWFjdGl2YXRlT3V0bGV0QW5kSXRDaGlsZHJlbihjdXJyLCBvdXRsZXQpO1xuICAgICAgdGhpcy5jaGVja3MucHVzaChuZXcgQ2FuQWN0aXZhdGUoZnV0dXJlKSk7XG4gICAgICB0aGlzLnRyYXZlcnNlQ2hpbGRSb3V0ZXMoZnV0dXJlTm9kZSwgbnVsbCwgb3V0bGV0ID8gb3V0bGV0Lm91dGxldE1hcCA6IG51bGwpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4ocm91dGU6IEFjdGl2YXRlZFJvdXRlU25hcHNob3QsIG91dGxldDogUm91dGVyT3V0bGV0KTogdm9pZCB7XG4gICAgaWYgKG91dGxldCAmJiBvdXRsZXQuaXNBY3RpdmF0ZWQpIHtcbiAgICAgIGZvckVhY2gob3V0bGV0Lm91dGxldE1hcC5fb3V0bGV0cywgKHY6IFJvdXRlck91dGxldCkgPT4ge1xuICAgICAgICBpZiAodi5pc0FjdGl2YXRlZCkge1xuICAgICAgICAgIHRoaXMuZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4odi5hY3RpdmF0ZWRSb3V0ZS5zbmFwc2hvdCwgdik7XG4gICAgICAgIH1cbiAgICAgIH0pO1xuICAgICAgdGhpcy5jaGVja3MucHVzaChuZXcgQ2FuRGVhY3RpdmF0ZShvdXRsZXQuY29tcG9uZW50LCByb3V0ZSkpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgcnVuQ2FuQWN0aXZhdGUoZnV0dXJlOiBBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90KTogT2JzZXJ2YWJsZTxib29sZWFuPiB7XG4gICAgY29uc3QgY2FuQWN0aXZhdGUgPSBmdXR1cmUuX3JvdXRlQ29uZmlnID8gZnV0dXJlLl9yb3V0ZUNvbmZpZy5jYW5BY3RpdmF0ZSA6IG51bGw7XG4gICAgaWYgKCFjYW5BY3RpdmF0ZSB8fCBjYW5BY3RpdmF0ZS5sZW5ndGggPT09IDApIHJldHVybiBvZiAodHJ1ZSk7XG4gICAgcmV0dXJuIE9ic2VydmFibGUuZnJvbShjYW5BY3RpdmF0ZSlcbiAgICAgICAgLm1hcChjID0+IHtcbiAgICAgICAgICBjb25zdCBndWFyZCA9IHRoaXMuaW5qZWN0b3IuZ2V0KGMpO1xuICAgICAgICAgIGlmIChndWFyZC5jYW5BY3RpdmF0ZSkge1xuICAgICAgICAgICAgcmV0dXJuIHdyYXBJbnRvT2JzZXJ2YWJsZShndWFyZC5jYW5BY3RpdmF0ZShmdXR1cmUsIHRoaXMuZnV0dXJlKSk7XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHJldHVybiB3cmFwSW50b09ic2VydmFibGUoZ3VhcmQoZnV0dXJlLCB0aGlzLmZ1dHVyZSkpO1xuICAgICAgICAgIH1cbiAgICAgICAgfSlcbiAgICAgICAgLm1lcmdlQWxsKClcbiAgICAgICAgLmV2ZXJ5KHJlc3VsdCA9PiByZXN1bHQgPT09IHRydWUpO1xuICB9XG5cbiAgcHJpdmF0ZSBydW5DYW5EZWFjdGl2YXRlKGNvbXBvbmVudDogT2JqZWN0LCBjdXJyOiBBY3RpdmF0ZWRSb3V0ZVNuYXBzaG90KTogT2JzZXJ2YWJsZTxib29sZWFuPiB7XG4gICAgY29uc3QgY2FuRGVhY3RpdmF0ZSA9IGN1cnIuX3JvdXRlQ29uZmlnID8gY3Vyci5fcm91dGVDb25maWcuY2FuRGVhY3RpdmF0ZSA6IG51bGw7XG4gICAgaWYgKCFjYW5EZWFjdGl2YXRlIHx8IGNhbkRlYWN0aXZhdGUubGVuZ3RoID09PSAwKSByZXR1cm4gb2YgKHRydWUpO1xuICAgIHJldHVybiBPYnNlcnZhYmxlLmZyb20oY2FuRGVhY3RpdmF0ZSlcbiAgICAgICAgLm1hcChjID0+IHtcbiAgICAgICAgICBjb25zdCBndWFyZCA9IHRoaXMuaW5qZWN0b3IuZ2V0KGMpO1xuICAgICAgICAgIGlmIChndWFyZC5jYW5EZWFjdGl2YXRlKSB7XG4gICAgICAgICAgICByZXR1cm4gd3JhcEludG9PYnNlcnZhYmxlKGd1YXJkLmNhbkRlYWN0aXZhdGUoY29tcG9uZW50LCBjdXJyLCB0aGlzLmN1cnIpKTtcbiAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgcmV0dXJuIHdyYXBJbnRvT2JzZXJ2YWJsZShndWFyZChjb21wb25lbnQsIGN1cnIsIHRoaXMuY3VycikpO1xuICAgICAgICAgIH1cbiAgICAgICAgfSlcbiAgICAgICAgLm1lcmdlQWxsKClcbiAgICAgICAgLmV2ZXJ5KHJlc3VsdCA9PiByZXN1bHQgPT09IHRydWUpO1xuICB9XG59XG5cbmZ1bmN0aW9uIHdyYXBJbnRvT2JzZXJ2YWJsZTxUPih2YWx1ZTogVCB8IE9ic2VydmFibGU8VD4pOiBPYnNlcnZhYmxlPFQ+IHtcbiAgaWYgKHZhbHVlIGluc3RhbmNlb2YgT2JzZXJ2YWJsZSkge1xuICAgIHJldHVybiB2YWx1ZTtcbiAgfSBlbHNlIHtcbiAgICByZXR1cm4gb2YgKHZhbHVlKTtcbiAgfVxufVxuXG5jbGFzcyBBY3RpdmF0ZVJvdXRlcyB7XG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgZnV0dXJlU3RhdGU6IFJvdXRlclN0YXRlLCBwcml2YXRlIGN1cnJTdGF0ZTogUm91dGVyU3RhdGUpIHt9XG5cbiAgYWN0aXZhdGUocGFyZW50T3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXApOiB2b2lkIHtcbiAgICBjb25zdCBmdXR1cmVSb290ID0gdGhpcy5mdXR1cmVTdGF0ZS5fcm9vdDtcbiAgICBjb25zdCBjdXJyUm9vdCA9IHRoaXMuY3VyclN0YXRlID8gdGhpcy5jdXJyU3RhdGUuX3Jvb3QgOiBudWxsO1xuXG4gICAgcHVzaFF1ZXJ5UGFyYW1zQW5kRnJhZ21lbnQodGhpcy5mdXR1cmVTdGF0ZSk7XG4gICAgdGhpcy5hY3RpdmF0ZUNoaWxkUm91dGVzKGZ1dHVyZVJvb3QsIGN1cnJSb290LCBwYXJlbnRPdXRsZXRNYXApO1xuICB9XG5cbiAgcHJpdmF0ZSBhY3RpdmF0ZUNoaWxkUm91dGVzKFxuICAgICAgZnV0dXJlTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGU+LCBjdXJyTm9kZTogVHJlZU5vZGU8QWN0aXZhdGVkUm91dGU+LFxuICAgICAgb3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXApOiB2b2lkIHtcbiAgICBjb25zdCBwcmV2Q2hpbGRyZW46IHtba2V5OiBzdHJpbmddOiBhbnl9ID0gbm9kZUNoaWxkcmVuQXNNYXAoY3Vyck5vZGUpO1xuICAgIGZ1dHVyZU5vZGUuY2hpbGRyZW4uZm9yRWFjaChjID0+IHtcbiAgICAgIHRoaXMuYWN0aXZhdGVSb3V0ZXMoYywgcHJldkNoaWxkcmVuW2MudmFsdWUub3V0bGV0XSwgb3V0bGV0TWFwKTtcbiAgICAgIGRlbGV0ZSBwcmV2Q2hpbGRyZW5bYy52YWx1ZS5vdXRsZXRdO1xuICAgIH0pO1xuICAgIGZvckVhY2goXG4gICAgICAgIHByZXZDaGlsZHJlbixcbiAgICAgICAgKHY6IGFueSwgazogc3RyaW5nKSA9PiB0aGlzLmRlYWN0aXZhdGVPdXRsZXRBbmRJdENoaWxkcmVuKG91dGxldE1hcC5fb3V0bGV0c1trXSkpO1xuICB9XG5cbiAgYWN0aXZhdGVSb3V0ZXMoXG4gICAgICBmdXR1cmVOb2RlOiBUcmVlTm9kZTxBY3RpdmF0ZWRSb3V0ZT4sIGN1cnJOb2RlOiBUcmVlTm9kZTxBY3RpdmF0ZWRSb3V0ZT4sXG4gICAgICBwYXJlbnRPdXRsZXRNYXA6IFJvdXRlck91dGxldE1hcCk6IHZvaWQge1xuICAgIGNvbnN0IGZ1dHVyZSA9IGZ1dHVyZU5vZGUudmFsdWU7XG4gICAgY29uc3QgY3VyciA9IGN1cnJOb2RlID8gY3Vyck5vZGUudmFsdWUgOiBudWxsO1xuXG4gICAgY29uc3Qgb3V0bGV0ID0gZ2V0T3V0bGV0KHBhcmVudE91dGxldE1hcCwgZnV0dXJlTm9kZS52YWx1ZSk7XG5cbiAgICBpZiAoZnV0dXJlID09PSBjdXJyKSB7XG4gICAgICBhZHZhbmNlQWN0aXZhdGVkUm91dGUoZnV0dXJlKTtcbiAgICAgIHRoaXMuYWN0aXZhdGVDaGlsZFJvdXRlcyhmdXR1cmVOb2RlLCBjdXJyTm9kZSwgb3V0bGV0Lm91dGxldE1hcCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMuZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4ob3V0bGV0KTtcbiAgICAgIGNvbnN0IG91dGxldE1hcCA9IG5ldyBSb3V0ZXJPdXRsZXRNYXAoKTtcbiAgICAgIHRoaXMuYWN0aXZhdGVOZXdSb3V0ZXMob3V0bGV0TWFwLCBmdXR1cmUsIG91dGxldCk7XG4gICAgICB0aGlzLmFjdGl2YXRlQ2hpbGRSb3V0ZXMoZnV0dXJlTm9kZSwgbnVsbCwgb3V0bGV0TWFwKTtcbiAgICB9XG4gIH1cblxuICBwcml2YXRlIGFjdGl2YXRlTmV3Um91dGVzKFxuICAgICAgb3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXAsIGZ1dHVyZTogQWN0aXZhdGVkUm91dGUsIG91dGxldDogUm91dGVyT3V0bGV0KTogdm9pZCB7XG4gICAgY29uc3QgcmVzb2x2ZWQgPSBSZWZsZWN0aXZlSW5qZWN0b3IucmVzb2x2ZShbXG4gICAgICB7cHJvdmlkZTogQWN0aXZhdGVkUm91dGUsIHVzZVZhbHVlOiBmdXR1cmV9LFxuICAgICAge3Byb3ZpZGU6IFJvdXRlck91dGxldE1hcCwgdXNlVmFsdWU6IG91dGxldE1hcH1cbiAgICBdKTtcbiAgICBhZHZhbmNlQWN0aXZhdGVkUm91dGUoZnV0dXJlKTtcbiAgICBvdXRsZXQuYWN0aXZhdGUoZnV0dXJlLl9mdXR1cmVTbmFwc2hvdC5fcmVzb2x2ZWRDb21wb25lbnRGYWN0b3J5LCBmdXR1cmUsIHJlc29sdmVkLCBvdXRsZXRNYXApO1xuICB9XG5cbiAgcHJpdmF0ZSBkZWFjdGl2YXRlT3V0bGV0QW5kSXRDaGlsZHJlbihvdXRsZXQ6IFJvdXRlck91dGxldCk6IHZvaWQge1xuICAgIGlmIChvdXRsZXQgJiYgb3V0bGV0LmlzQWN0aXZhdGVkKSB7XG4gICAgICBmb3JFYWNoKFxuICAgICAgICAgIG91dGxldC5vdXRsZXRNYXAuX291dGxldHMsICh2OiBSb3V0ZXJPdXRsZXQpID0+IHRoaXMuZGVhY3RpdmF0ZU91dGxldEFuZEl0Q2hpbGRyZW4odikpO1xuICAgICAgb3V0bGV0LmRlYWN0aXZhdGUoKTtcbiAgICB9XG4gIH1cbn1cblxuZnVuY3Rpb24gcHVzaFF1ZXJ5UGFyYW1zQW5kRnJhZ21lbnQoc3RhdGU6IFJvdXRlclN0YXRlKTogdm9pZCB7XG4gIGlmICghc2hhbGxvd0VxdWFsKHN0YXRlLnNuYXBzaG90LnF1ZXJ5UGFyYW1zLCAoPGFueT5zdGF0ZS5xdWVyeVBhcmFtcykudmFsdWUpKSB7XG4gICAgKDxhbnk+c3RhdGUucXVlcnlQYXJhbXMpLm5leHQoc3RhdGUuc25hcHNob3QucXVlcnlQYXJhbXMpO1xuICB9XG5cbiAgaWYgKHN0YXRlLnNuYXBzaG90LmZyYWdtZW50ICE9PSAoPGFueT5zdGF0ZS5mcmFnbWVudCkudmFsdWUpIHtcbiAgICAoPGFueT5zdGF0ZS5mcmFnbWVudCkubmV4dChzdGF0ZS5zbmFwc2hvdC5mcmFnbWVudCk7XG4gIH1cbn1cblxuZnVuY3Rpb24gbm9kZUNoaWxkcmVuQXNNYXAobm9kZTogVHJlZU5vZGU8YW55Pikge1xuICByZXR1cm4gbm9kZSA/IG5vZGUuY2hpbGRyZW4ucmVkdWNlKChtOiBhbnksIGM6IFRyZWVOb2RlPGFueT4pID0+IHtcbiAgICBtW2MudmFsdWUub3V0bGV0XSA9IGM7XG4gICAgcmV0dXJuIG07XG4gIH0sIHt9KSA6IHt9O1xufVxuXG5mdW5jdGlvbiBnZXRPdXRsZXQob3V0bGV0TWFwOiBSb3V0ZXJPdXRsZXRNYXAsIHJvdXRlOiBBY3RpdmF0ZWRSb3V0ZSk6IFJvdXRlck91dGxldCB7XG4gIGxldCBvdXRsZXQgPSBvdXRsZXRNYXAuX291dGxldHNbcm91dGUub3V0bGV0XTtcbiAgaWYgKCFvdXRsZXQpIHtcbiAgICBjb25zdCBjb21wb25lbnROYW1lID0gKDxhbnk+cm91dGUuY29tcG9uZW50KS5uYW1lO1xuICAgIGlmIChyb3V0ZS5vdXRsZXQgPT09IFBSSU1BUllfT1VUTEVUKSB7XG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYENhbm5vdCBmaW5kIHByaW1hcnkgb3V0bGV0IHRvIGxvYWQgJyR7Y29tcG9uZW50TmFtZX0nYCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRocm93IG5ldyBFcnJvcihgQ2Fubm90IGZpbmQgdGhlIG91dGxldCAke3JvdXRlLm91dGxldH0gdG8gbG9hZCAnJHtjb21wb25lbnROYW1lfSdgKTtcbiAgICB9XG4gIH1cbiAgcmV0dXJuIG91dGxldDtcbn1cbiJdfQ==