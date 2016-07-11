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
var core_1 = require('@angular/core');
var router_1 = require('../router');
var url_tree_1 = require('../url_tree');
var router_link_1 = require('./router_link');
var RouterLinkActive = (function () {
    function RouterLinkActive(router, element, renderer) {
        var _this = this;
        this.router = router;
        this.element = element;
        this.renderer = renderer;
        this.classes = [];
        this.routerLinkActiveOptions = { exact: true };
        this.subscription = router.events.subscribe(function (s) {
            if (s instanceof router_1.NavigationEnd) {
                _this.update();
            }
        });
    }
    RouterLinkActive.prototype.ngAfterContentInit = function () {
        var _this = this;
        this.links.changes.subscribe(function (s) { return _this.update(); });
        this.update();
    };
    Object.defineProperty(RouterLinkActive.prototype, "routerLinkActive", {
        set: function (data) {
            if (Array.isArray(data)) {
                this.classes = data;
            }
            else {
                this.classes = data.split(' ');
            }
        },
        enumerable: true,
        configurable: true
    });
    RouterLinkActive.prototype.ngOnChanges = function (changes) { this.update(); };
    RouterLinkActive.prototype.ngOnDestroy = function () { this.subscription.unsubscribe(); };
    RouterLinkActive.prototype.update = function () {
        var _this = this;
        if (!this.links || this.links.length === 0)
            return;
        var currentUrlTree = this.router.parseUrl(this.router.url);
        var isActive = this.links.reduce(function (res, link) {
            return res || url_tree_1.containsTree(currentUrlTree, link.urlTree, _this.routerLinkActiveOptions.exact);
        }, false);
        this.classes.forEach(function (c) { return _this.renderer.setElementClass(_this.element.nativeElement, c, isActive); });
    };
    __decorate([
        core_1.ContentChildren(router_link_1.RouterLink), 
        __metadata('design:type', core_1.QueryList)
    ], RouterLinkActive.prototype, "links", void 0);
    __decorate([
        core_1.Input(), 
        __metadata('design:type', Object)
    ], RouterLinkActive.prototype, "routerLinkActiveOptions", void 0);
    __decorate([
        core_1.Input(), 
        __metadata('design:type', Object), 
        __metadata('design:paramtypes', [Object])
    ], RouterLinkActive.prototype, "routerLinkActive", null);
    RouterLinkActive = __decorate([
        core_1.Directive({ selector: '[routerLinkActive]' }), 
        __metadata('design:paramtypes', [router_1.Router, core_1.ElementRef, core_1.Renderer])
    ], RouterLinkActive);
    return RouterLinkActive;
}());
exports.RouterLinkActive = RouterLinkActive;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicm91dGVyX2xpbmtfYWN0aXZlLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vLi4vc3JjL2RpcmVjdGl2ZXMvcm91dGVyX2xpbmtfYWN0aXZlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7Ozs7Ozs7QUFBQSxxQkFBeUgsZUFBZSxDQUFDLENBQUE7QUFHekksdUJBQW9DLFdBQVcsQ0FBQyxDQUFBO0FBQ2hELHlCQUEyQixhQUFhLENBQUMsQ0FBQTtBQUV6Qyw0QkFBeUIsZUFBZSxDQUFDLENBQUE7QUFPekM7SUFVRSwwQkFBb0IsTUFBYyxFQUFVLE9BQW1CLEVBQVUsUUFBa0I7UUFWN0YsaUJBK0NDO1FBckNxQixXQUFNLEdBQU4sTUFBTSxDQUFRO1FBQVUsWUFBTyxHQUFQLE9BQU8sQ0FBWTtRQUFVLGFBQVEsR0FBUixRQUFRLENBQVU7UUFSbkYsWUFBTyxHQUFhLEVBQUUsQ0FBQztRQUdkLDRCQUF1QixHQUE0QixFQUFDLEtBQUssRUFBRSxJQUFJLEVBQUMsQ0FBQztRQU1oRixJQUFJLENBQUMsWUFBWSxHQUFHLE1BQU0sQ0FBQyxNQUFNLENBQUMsU0FBUyxDQUFDLFVBQUEsQ0FBQztZQUMzQyxFQUFFLENBQUMsQ0FBQyxDQUFDLFlBQVksc0JBQWEsQ0FBQyxDQUFDLENBQUM7Z0JBQy9CLEtBQUksQ0FBQyxNQUFNLEVBQUUsQ0FBQztZQUNoQixDQUFDO1FBQ0gsQ0FBQyxDQUFDLENBQUM7SUFDTCxDQUFDO0lBRUQsNkNBQWtCLEdBQWxCO1FBQUEsaUJBR0M7UUFGQyxJQUFJLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxTQUFTLENBQUMsVUFBQSxDQUFDLElBQUksT0FBQSxLQUFJLENBQUMsTUFBTSxFQUFFLEVBQWIsQ0FBYSxDQUFDLENBQUM7UUFDakQsSUFBSSxDQUFDLE1BQU0sRUFBRSxDQUFDO0lBQ2hCLENBQUM7SUFHRCxzQkFBSSw4Q0FBZ0I7YUFBcEIsVUFBcUIsSUFBcUI7WUFDeEMsRUFBRSxDQUFDLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUM7Z0JBQ3hCLElBQUksQ0FBQyxPQUFPLEdBQVEsSUFBSSxDQUFDO1lBQzNCLENBQUM7WUFBQyxJQUFJLENBQUMsQ0FBQztnQkFDTixJQUFJLENBQUMsT0FBTyxHQUFHLElBQUksQ0FBQyxLQUFLLENBQUMsR0FBRyxDQUFDLENBQUM7WUFDakMsQ0FBQztRQUNILENBQUM7OztPQUFBO0lBRUQsc0NBQVcsR0FBWCxVQUFZLE9BQVcsSUFBUyxJQUFJLENBQUMsTUFBTSxFQUFFLENBQUMsQ0FBQyxDQUFDO0lBQ2hELHNDQUFXLEdBQVgsY0FBcUIsSUFBSSxDQUFDLFlBQVksQ0FBQyxXQUFXLEVBQUUsQ0FBQyxDQUFDLENBQUM7SUFFL0MsaUNBQU0sR0FBZDtRQUFBLGlCQVdDO1FBVkMsRUFBRSxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUMsS0FBSyxJQUFJLElBQUksQ0FBQyxLQUFLLENBQUMsTUFBTSxLQUFLLENBQUMsQ0FBQztZQUFDLE1BQU0sQ0FBQztRQUVuRCxJQUFNLGNBQWMsR0FBRyxJQUFJLENBQUMsTUFBTSxDQUFDLFFBQVEsQ0FBQyxJQUFJLENBQUMsTUFBTSxDQUFDLEdBQUcsQ0FBQyxDQUFDO1FBQzdELElBQU0sUUFBUSxHQUFHLElBQUksQ0FBQyxLQUFLLENBQUMsTUFBTSxDQUM5QixVQUFDLEdBQUcsRUFBRSxJQUFJO1lBQ04sT0FBQSxHQUFHLElBQUksdUJBQVksQ0FBQyxjQUFjLEVBQUUsSUFBSSxDQUFDLE9BQU8sRUFBRSxLQUFJLENBQUMsdUJBQXVCLENBQUMsS0FBSyxDQUFDO1FBQXJGLENBQXFGLEVBQ3pGLEtBQUssQ0FBQyxDQUFDO1FBRVgsSUFBSSxDQUFDLE9BQU8sQ0FBQyxPQUFPLENBQ2hCLFVBQUEsQ0FBQyxJQUFJLE9BQUEsS0FBSSxDQUFDLFFBQVEsQ0FBQyxlQUFlLENBQUMsS0FBSSxDQUFDLE9BQU8sQ0FBQyxhQUFhLEVBQUUsQ0FBQyxFQUFFLFFBQVEsQ0FBQyxFQUF0RSxDQUFzRSxDQUFDLENBQUM7SUFDbkYsQ0FBQztJQTdDRDtRQUFDLHNCQUFlLENBQUMsd0JBQVUsQ0FBQzs7bURBQUE7SUFJNUI7UUFBQyxZQUFLLEVBQUU7O3FFQUFBO0lBa0JSO1FBQUMsWUFBSyxFQUFFOzs7NERBQUE7SUF4QlY7UUFBQyxnQkFBUyxDQUFDLEVBQUMsUUFBUSxFQUFFLG9CQUFvQixFQUFDLENBQUM7O3dCQUFBO0lBZ0Q1Qyx1QkFBQztBQUFELENBQUMsQUEvQ0QsSUErQ0M7QUEvQ1ksd0JBQWdCLG1CQStDNUIsQ0FBQSIsInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB7QWZ0ZXJDb250ZW50SW5pdCwgQ29udGVudENoaWxkcmVuLCBEaXJlY3RpdmUsIEVsZW1lbnRSZWYsIElucHV0LCBPbkNoYW5nZXMsIE9uRGVzdHJveSwgUXVlcnlMaXN0LCBSZW5kZXJlcn0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQge1N1YnNjcmlwdGlvbn0gZnJvbSAncnhqcy9TdWJzY3JpcHRpb24nO1xuXG5pbXBvcnQge05hdmlnYXRpb25FbmQsIFJvdXRlcn0gZnJvbSAnLi4vcm91dGVyJztcbmltcG9ydCB7Y29udGFpbnNUcmVlfSBmcm9tICcuLi91cmxfdHJlZSc7XG5cbmltcG9ydCB7Um91dGVyTGlua30gZnJvbSAnLi9yb3V0ZXJfbGluayc7XG5cbmludGVyZmFjZSBSb3V0ZXJMaW5rQWN0aXZlT3B0aW9ucyB7XG4gIGV4YWN0OiBib29sZWFuO1xufVxuXG5ARGlyZWN0aXZlKHtzZWxlY3RvcjogJ1tyb3V0ZXJMaW5rQWN0aXZlXSd9KVxuZXhwb3J0IGNsYXNzIFJvdXRlckxpbmtBY3RpdmUgaW1wbGVtZW50cyBPbkNoYW5nZXMsIE9uRGVzdHJveSwgQWZ0ZXJDb250ZW50SW5pdCB7XG4gIEBDb250ZW50Q2hpbGRyZW4oUm91dGVyTGluaykgcHJpdmF0ZSBsaW5rczogUXVlcnlMaXN0PFJvdXRlckxpbms+O1xuICBwcml2YXRlIGNsYXNzZXM6IHN0cmluZ1tdID0gW107XG4gIHByaXZhdGUgc3Vic2NyaXB0aW9uOiBTdWJzY3JpcHRpb247XG5cbiAgQElucHV0KCkgcHJpdmF0ZSByb3V0ZXJMaW5rQWN0aXZlT3B0aW9uczogUm91dGVyTGlua0FjdGl2ZU9wdGlvbnMgPSB7ZXhhY3Q6IHRydWV9O1xuXG4gIC8qKlxuICAgKiBAaW50ZXJuYWxcbiAgICovXG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgcm91dGVyOiBSb3V0ZXIsIHByaXZhdGUgZWxlbWVudDogRWxlbWVudFJlZiwgcHJpdmF0ZSByZW5kZXJlcjogUmVuZGVyZXIpIHtcbiAgICB0aGlzLnN1YnNjcmlwdGlvbiA9IHJvdXRlci5ldmVudHMuc3Vic2NyaWJlKHMgPT4ge1xuICAgICAgaWYgKHMgaW5zdGFuY2VvZiBOYXZpZ2F0aW9uRW5kKSB7XG4gICAgICAgIHRoaXMudXBkYXRlKCk7XG4gICAgICB9XG4gICAgfSk7XG4gIH1cblxuICBuZ0FmdGVyQ29udGVudEluaXQoKTogdm9pZCB7XG4gICAgdGhpcy5saW5rcy5jaGFuZ2VzLnN1YnNjcmliZShzID0+IHRoaXMudXBkYXRlKCkpO1xuICAgIHRoaXMudXBkYXRlKCk7XG4gIH1cblxuICBASW5wdXQoKVxuICBzZXQgcm91dGVyTGlua0FjdGl2ZShkYXRhOiBzdHJpbmdbXXxzdHJpbmcpIHtcbiAgICBpZiAoQXJyYXkuaXNBcnJheShkYXRhKSkge1xuICAgICAgdGhpcy5jbGFzc2VzID0gPGFueT5kYXRhO1xuICAgIH0gZWxzZSB7XG4gICAgICB0aGlzLmNsYXNzZXMgPSBkYXRhLnNwbGl0KCcgJyk7XG4gICAgfVxuICB9XG5cbiAgbmdPbkNoYW5nZXMoY2hhbmdlczoge30pOiBhbnkgeyB0aGlzLnVwZGF0ZSgpOyB9XG4gIG5nT25EZXN0cm95KCk6IGFueSB7IHRoaXMuc3Vic2NyaXB0aW9uLnVuc3Vic2NyaWJlKCk7IH1cblxuICBwcml2YXRlIHVwZGF0ZSgpOiB2b2lkIHtcbiAgICBpZiAoIXRoaXMubGlua3MgfHwgdGhpcy5saW5rcy5sZW5ndGggPT09IDApIHJldHVybjtcblxuICAgIGNvbnN0IGN1cnJlbnRVcmxUcmVlID0gdGhpcy5yb3V0ZXIucGFyc2VVcmwodGhpcy5yb3V0ZXIudXJsKTtcbiAgICBjb25zdCBpc0FjdGl2ZSA9IHRoaXMubGlua3MucmVkdWNlKFxuICAgICAgICAocmVzLCBsaW5rKSA9PlxuICAgICAgICAgICAgcmVzIHx8IGNvbnRhaW5zVHJlZShjdXJyZW50VXJsVHJlZSwgbGluay51cmxUcmVlLCB0aGlzLnJvdXRlckxpbmtBY3RpdmVPcHRpb25zLmV4YWN0KSxcbiAgICAgICAgZmFsc2UpO1xuXG4gICAgdGhpcy5jbGFzc2VzLmZvckVhY2goXG4gICAgICAgIGMgPT4gdGhpcy5yZW5kZXJlci5zZXRFbGVtZW50Q2xhc3ModGhpcy5lbGVtZW50Lm5hdGl2ZUVsZW1lbnQsIGMsIGlzQWN0aXZlKSk7XG4gIH1cbn1cbiJdfQ==