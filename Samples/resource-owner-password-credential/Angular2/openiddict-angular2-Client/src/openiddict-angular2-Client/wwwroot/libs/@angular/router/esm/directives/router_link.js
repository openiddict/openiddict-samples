var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
import { LocationStrategy } from '@angular/common';
import { Directive, HostBinding, HostListener, Input } from '@angular/core';
import { Router } from '../router';
import { ActivatedRoute } from '../router_state';
export let RouterLink = class RouterLink {
    constructor(router, route, locationStrategy) {
        this.router = router;
        this.route = route;
        this.locationStrategy = locationStrategy;
        this.commands = [];
    }
    set routerLink(data) {
        if (Array.isArray(data)) {
            this.commands = data;
        }
        else {
            this.commands = [data];
        }
    }
    ngOnChanges(changes) { this.updateTargetUrlAndHref(); }
    onClick(button, ctrlKey, metaKey) {
        if (button !== 0 || ctrlKey || metaKey) {
            return true;
        }
        if (typeof this.target === 'string' && this.target != '_self') {
            return true;
        }
        this.router.navigateByUrl(this.urlTree);
        return false;
    }
    updateTargetUrlAndHref() {
        this.urlTree = this.router.createUrlTree(this.commands, { relativeTo: this.route, queryParams: this.queryParams, fragment: this.fragment });
        if (this.urlTree) {
            this.href = this.locationStrategy.prepareExternalUrl(this.router.serializeUrl(this.urlTree));
        }
    }
};
__decorate([
    Input(), 
    __metadata('design:type', String)
], RouterLink.prototype, "target", void 0);
__decorate([
    Input(), 
    __metadata('design:type', Object)
], RouterLink.prototype, "queryParams", void 0);
__decorate([
    Input(), 
    __metadata('design:type', String)
], RouterLink.prototype, "fragment", void 0);
__decorate([
    HostBinding(), 
    __metadata('design:type', String)
], RouterLink.prototype, "href", void 0);
__decorate([
    Input(), 
    __metadata('design:type', Object), 
    __metadata('design:paramtypes', [Object])
], RouterLink.prototype, "routerLink", null);
__decorate([
    HostListener('click', ['$event.button', '$event.ctrlKey', '$event.metaKey']), 
    __metadata('design:type', Function), 
    __metadata('design:paramtypes', [Number, Boolean, Boolean]), 
    __metadata('design:returntype', Boolean)
], RouterLink.prototype, "onClick", null);
RouterLink = __decorate([
    Directive({ selector: '[routerLink]' }), 
    __metadata('design:paramtypes', [Router, ActivatedRoute, LocationStrategy])
], RouterLink);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicm91dGVyX2xpbmsuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi8uLi8uLi9zcmMvZGlyZWN0aXZlcy9yb3V0ZXJfbGluay50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOzs7Ozs7Ozs7T0FBTyxFQUFDLGdCQUFnQixFQUFDLE1BQU0saUJBQWlCO09BQ3pDLEVBQUMsU0FBUyxFQUFFLFdBQVcsRUFBRSxZQUFZLEVBQUUsS0FBSyxFQUFZLE1BQU0sZUFBZTtPQUU3RSxFQUFDLE1BQU0sRUFBQyxNQUFNLFdBQVc7T0FDekIsRUFBQyxjQUFjLEVBQUMsTUFBTSxpQkFBaUI7QUErQjlDO0lBY0UsWUFDWSxNQUFjLEVBQVUsS0FBcUIsRUFDN0MsZ0JBQWtDO1FBRGxDLFdBQU0sR0FBTixNQUFNLENBQVE7UUFBVSxVQUFLLEdBQUwsS0FBSyxDQUFnQjtRQUM3QyxxQkFBZ0IsR0FBaEIsZ0JBQWdCLENBQWtCO1FBZHRDLGFBQVEsR0FBVSxFQUFFLENBQUM7SUFjb0IsQ0FBQztJQUdsRCxJQUFJLFVBQVUsQ0FBQyxJQUFrQjtRQUMvQixFQUFFLENBQUMsQ0FBQyxLQUFLLENBQUMsT0FBTyxDQUFDLElBQUksQ0FBQyxDQUFDLENBQUMsQ0FBQztZQUN4QixJQUFJLENBQUMsUUFBUSxHQUFRLElBQUksQ0FBQztRQUM1QixDQUFDO1FBQUMsSUFBSSxDQUFDLENBQUM7WUFDTixJQUFJLENBQUMsUUFBUSxHQUFHLENBQUMsSUFBSSxDQUFDLENBQUM7UUFDekIsQ0FBQztJQUNILENBQUM7SUFFRCxXQUFXLENBQUMsT0FBVyxJQUFTLElBQUksQ0FBQyxzQkFBc0IsRUFBRSxDQUFDLENBQUMsQ0FBQztJQUdoRSxPQUFPLENBQUMsTUFBYyxFQUFFLE9BQWdCLEVBQUUsT0FBZ0I7UUFDeEQsRUFBRSxDQUFDLENBQUMsTUFBTSxLQUFLLENBQUMsSUFBSSxPQUFPLElBQUksT0FBTyxDQUFDLENBQUMsQ0FBQztZQUN2QyxNQUFNLENBQUMsSUFBSSxDQUFDO1FBQ2QsQ0FBQztRQUVELEVBQUUsQ0FBQyxDQUFDLE9BQU8sSUFBSSxDQUFDLE1BQU0sS0FBSyxRQUFRLElBQUksSUFBSSxDQUFDLE1BQU0sSUFBSSxPQUFPLENBQUMsQ0FBQyxDQUFDO1lBQzlELE1BQU0sQ0FBQyxJQUFJLENBQUM7UUFDZCxDQUFDO1FBRUQsSUFBSSxDQUFDLE1BQU0sQ0FBQyxhQUFhLENBQUMsSUFBSSxDQUFDLE9BQU8sQ0FBQyxDQUFDO1FBQ3hDLE1BQU0sQ0FBQyxLQUFLLENBQUM7SUFDZixDQUFDO0lBRU8sc0JBQXNCO1FBQzVCLElBQUksQ0FBQyxPQUFPLEdBQUcsSUFBSSxDQUFDLE1BQU0sQ0FBQyxhQUFhLENBQ3BDLElBQUksQ0FBQyxRQUFRLEVBQ2IsRUFBQyxVQUFVLEVBQUUsSUFBSSxDQUFDLEtBQUssRUFBRSxXQUFXLEVBQUUsSUFBSSxDQUFDLFdBQVcsRUFBRSxRQUFRLEVBQUUsSUFBSSxDQUFDLFFBQVEsRUFBQyxDQUFDLENBQUM7UUFDdEYsRUFBRSxDQUFDLENBQUMsSUFBSSxDQUFDLE9BQU8sQ0FBQyxDQUFDLENBQUM7WUFDakIsSUFBSSxDQUFDLElBQUksR0FBRyxJQUFJLENBQUMsZ0JBQWdCLENBQUMsa0JBQWtCLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxZQUFZLENBQUMsSUFBSSxDQUFDLE9BQU8sQ0FBQyxDQUFDLENBQUM7UUFDL0YsQ0FBQztJQUNILENBQUM7QUFDSCxDQUFDO0FBbERDO0lBQUMsS0FBSyxFQUFFOzswQ0FBQTtBQUVSO0lBQUMsS0FBSyxFQUFFOzsrQ0FBQTtBQUNSO0lBQUMsS0FBSyxFQUFFOzs0Q0FBQTtBQUdSO0lBQUMsV0FBVyxFQUFFOzt3Q0FBQTtBQVdkO0lBQUMsS0FBSyxFQUFFOzs7NENBQUE7QUFXUjtJQUFDLFlBQVksQ0FBQyxPQUFPLEVBQUUsQ0FBQyxlQUFlLEVBQUUsZ0JBQWdCLEVBQUUsZ0JBQWdCLENBQUMsQ0FBQzs7Ozt5Q0FBQTtBQTlCL0U7SUFBQyxTQUFTLENBQUMsRUFBQyxRQUFRLEVBQUUsY0FBYyxFQUFDLENBQUM7O2NBQUE7QUFvRHJDIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHtMb2NhdGlvblN0cmF0ZWd5fSBmcm9tICdAYW5ndWxhci9jb21tb24nO1xuaW1wb3J0IHtEaXJlY3RpdmUsIEhvc3RCaW5kaW5nLCBIb3N0TGlzdGVuZXIsIElucHV0LCBPbkNoYW5nZXN9IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuXG5pbXBvcnQge1JvdXRlcn0gZnJvbSAnLi4vcm91dGVyJztcbmltcG9ydCB7QWN0aXZhdGVkUm91dGV9IGZyb20gJy4uL3JvdXRlcl9zdGF0ZSc7XG5pbXBvcnQge1VybFRyZWV9IGZyb20gJy4uL3VybF90cmVlJztcblxuXG5cbi8qKlxuICogVGhlIFJvdXRlckxpbmsgZGlyZWN0aXZlIGxldHMgeW91IGxpbmsgdG8gc3BlY2lmaWMgcGFydHMgb2YgeW91ciBhcHAuXG4gKlxuICogQ29uc2lkZXIgdGhlIGZvbGxvd2luZyByb3V0ZSBjb25maWd1cmF0aW9uOlxuXG4gKiBgYGBcbiAqIFt7IHBhdGg6ICcvdXNlcicsIGNvbXBvbmVudDogVXNlckNtcCB9XVxuICogYGBgXG4gKlxuICogV2hlbiBsaW5raW5nIHRvIHRoaXMgYFVzZXJgIHJvdXRlLCB5b3UgY2FuIHdyaXRlOlxuICpcbiAqIGBgYFxuICogPGEgW3JvdXRlckxpbmtdPVwiWycvdXNlciddXCI+bGluayB0byB1c2VyIGNvbXBvbmVudDwvYT5cbiAqIGBgYFxuICpcbiAqIFJvdXRlckxpbmsgZXhwZWN0cyB0aGUgdmFsdWUgdG8gYmUgYW4gYXJyYXkgb2YgcGF0aCBzZWdtZW50cywgZm9sbG93ZWQgYnkgdGhlIHBhcmFtc1xuICogZm9yIHRoYXQgbGV2ZWwgb2Ygcm91dGluZy4gRm9yIGluc3RhbmNlIGBbJy90ZWFtJywge3RlYW1JZDogMX0sICd1c2VyJywge3VzZXJJZDogMn1dYFxuICogbWVhbnMgdGhhdCB3ZSB3YW50IHRvIGdlbmVyYXRlIGEgbGluayB0byBgL3RlYW07dGVhbUlkPTEvdXNlcjt1c2VySWQ9MmAuXG4gKlxuICogVGhlIGZpcnN0IHNlZ21lbnQgbmFtZSBjYW4gYmUgcHJlcGVuZGVkIHdpdGggYC9gLCBgLi9gLCBvciBgLi4vYC5cbiAqIElmIHRoZSBzZWdtZW50IGJlZ2lucyB3aXRoIGAvYCwgdGhlIHJvdXRlciB3aWxsIGxvb2sgdXAgdGhlIHJvdXRlIGZyb20gdGhlIHJvb3Qgb2YgdGhlIGFwcC5cbiAqIElmIHRoZSBzZWdtZW50IGJlZ2lucyB3aXRoIGAuL2AsIG9yIGRvZXNuJ3QgYmVnaW4gd2l0aCBhIHNsYXNoLCB0aGUgcm91dGVyIHdpbGxcbiAqIGluc3RlYWQgbG9vayBpbiB0aGUgY3VycmVudCBjb21wb25lbnQncyBjaGlsZHJlbiBmb3IgdGhlIHJvdXRlLlxuICogQW5kIGlmIHRoZSBzZWdtZW50IGJlZ2lucyB3aXRoIGAuLi9gLCB0aGUgcm91dGVyIHdpbGwgZ28gdXAgb25lIGxldmVsLlxuICovXG5ARGlyZWN0aXZlKHtzZWxlY3RvcjogJ1tyb3V0ZXJMaW5rXSd9KVxuZXhwb3J0IGNsYXNzIFJvdXRlckxpbmsgaW1wbGVtZW50cyBPbkNoYW5nZXMge1xuICBASW5wdXQoKSB0YXJnZXQ6IHN0cmluZztcbiAgcHJpdmF0ZSBjb21tYW5kczogYW55W10gPSBbXTtcbiAgQElucHV0KCkgcXVlcnlQYXJhbXM6IHtbazogc3RyaW5nXTogYW55fTtcbiAgQElucHV0KCkgZnJhZ21lbnQ6IHN0cmluZztcblxuICAvLyB0aGUgdXJsIGRpc3BsYXllZCBvbiB0aGUgYW5jaG9yIGVsZW1lbnQuXG4gIEBIb3N0QmluZGluZygpIGhyZWY6IHN0cmluZztcblxuICB1cmxUcmVlOiBVcmxUcmVlO1xuXG4gIC8qKlxuICAgKiBAaW50ZXJuYWxcbiAgICovXG4gIGNvbnN0cnVjdG9yKFxuICAgICAgcHJpdmF0ZSByb3V0ZXI6IFJvdXRlciwgcHJpdmF0ZSByb3V0ZTogQWN0aXZhdGVkUm91dGUsXG4gICAgICBwcml2YXRlIGxvY2F0aW9uU3RyYXRlZ3k6IExvY2F0aW9uU3RyYXRlZ3kpIHt9XG5cbiAgQElucHV0KClcbiAgc2V0IHJvdXRlckxpbmsoZGF0YTogYW55W118c3RyaW5nKSB7XG4gICAgaWYgKEFycmF5LmlzQXJyYXkoZGF0YSkpIHtcbiAgICAgIHRoaXMuY29tbWFuZHMgPSA8YW55PmRhdGE7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMuY29tbWFuZHMgPSBbZGF0YV07XG4gICAgfVxuICB9XG5cbiAgbmdPbkNoYW5nZXMoY2hhbmdlczoge30pOiBhbnkgeyB0aGlzLnVwZGF0ZVRhcmdldFVybEFuZEhyZWYoKTsgfVxuXG4gIEBIb3N0TGlzdGVuZXIoJ2NsaWNrJywgWyckZXZlbnQuYnV0dG9uJywgJyRldmVudC5jdHJsS2V5JywgJyRldmVudC5tZXRhS2V5J10pXG4gIG9uQ2xpY2soYnV0dG9uOiBudW1iZXIsIGN0cmxLZXk6IGJvb2xlYW4sIG1ldGFLZXk6IGJvb2xlYW4pOiBib29sZWFuIHtcbiAgICBpZiAoYnV0dG9uICE9PSAwIHx8IGN0cmxLZXkgfHwgbWV0YUtleSkge1xuICAgICAgcmV0dXJuIHRydWU7XG4gICAgfVxuXG4gICAgaWYgKHR5cGVvZiB0aGlzLnRhcmdldCA9PT0gJ3N0cmluZycgJiYgdGhpcy50YXJnZXQgIT0gJ19zZWxmJykge1xuICAgICAgcmV0dXJuIHRydWU7XG4gICAgfVxuXG4gICAgdGhpcy5yb3V0ZXIubmF2aWdhdGVCeVVybCh0aGlzLnVybFRyZWUpO1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIHByaXZhdGUgdXBkYXRlVGFyZ2V0VXJsQW5kSHJlZigpOiB2b2lkIHtcbiAgICB0aGlzLnVybFRyZWUgPSB0aGlzLnJvdXRlci5jcmVhdGVVcmxUcmVlKFxuICAgICAgICB0aGlzLmNvbW1hbmRzLFxuICAgICAgICB7cmVsYXRpdmVUbzogdGhpcy5yb3V0ZSwgcXVlcnlQYXJhbXM6IHRoaXMucXVlcnlQYXJhbXMsIGZyYWdtZW50OiB0aGlzLmZyYWdtZW50fSk7XG4gICAgaWYgKHRoaXMudXJsVHJlZSkge1xuICAgICAgdGhpcy5ocmVmID0gdGhpcy5sb2NhdGlvblN0cmF0ZWd5LnByZXBhcmVFeHRlcm5hbFVybCh0aGlzLnJvdXRlci5zZXJpYWxpemVVcmwodGhpcy51cmxUcmVlKSk7XG4gICAgfVxuICB9XG59XG4iXX0=