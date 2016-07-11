var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
import { ContentChildren, Directive, ElementRef, Input, QueryList, Renderer } from '@angular/core';
import { NavigationEnd, Router } from '../router';
import { containsTree } from '../url_tree';
import { RouterLink } from './router_link';
export let RouterLinkActive = class RouterLinkActive {
    constructor(router, element, renderer) {
        this.router = router;
        this.element = element;
        this.renderer = renderer;
        this.classes = [];
        this.routerLinkActiveOptions = { exact: true };
        this.subscription = router.events.subscribe(s => {
            if (s instanceof NavigationEnd) {
                this.update();
            }
        });
    }
    ngAfterContentInit() {
        this.links.changes.subscribe(s => this.update());
        this.update();
    }
    set routerLinkActive(data) {
        if (Array.isArray(data)) {
            this.classes = data;
        }
        else {
            this.classes = data.split(' ');
        }
    }
    ngOnChanges(changes) { this.update(); }
    ngOnDestroy() { this.subscription.unsubscribe(); }
    update() {
        if (!this.links || this.links.length === 0)
            return;
        const currentUrlTree = this.router.parseUrl(this.router.url);
        const isActive = this.links.reduce((res, link) => res || containsTree(currentUrlTree, link.urlTree, this.routerLinkActiveOptions.exact), false);
        this.classes.forEach(c => this.renderer.setElementClass(this.element.nativeElement, c, isActive));
    }
};
__decorate([
    ContentChildren(RouterLink), 
    __metadata('design:type', QueryList)
], RouterLinkActive.prototype, "links", void 0);
__decorate([
    Input(), 
    __metadata('design:type', Object)
], RouterLinkActive.prototype, "routerLinkActiveOptions", void 0);
__decorate([
    Input(), 
    __metadata('design:type', Object), 
    __metadata('design:paramtypes', [Object])
], RouterLinkActive.prototype, "routerLinkActive", null);
RouterLinkActive = __decorate([
    Directive({ selector: '[routerLinkActive]' }), 
    __metadata('design:paramtypes', [Router, ElementRef, Renderer])
], RouterLinkActive);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicm91dGVyX2xpbmtfYWN0aXZlLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vLi4vLi4vc3JjL2RpcmVjdGl2ZXMvcm91dGVyX2xpbmtfYWN0aXZlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7Ozs7OztPQUFPLEVBQW1CLGVBQWUsRUFBRSxTQUFTLEVBQUUsVUFBVSxFQUFFLEtBQUssRUFBd0IsU0FBUyxFQUFFLFFBQVEsRUFBQyxNQUFNLGVBQWU7T0FHakksRUFBQyxhQUFhLEVBQUUsTUFBTSxFQUFDLE1BQU0sV0FBVztPQUN4QyxFQUFDLFlBQVksRUFBQyxNQUFNLGFBQWE7T0FFakMsRUFBQyxVQUFVLEVBQUMsTUFBTSxlQUFlO0FBT3hDO0lBVUUsWUFBb0IsTUFBYyxFQUFVLE9BQW1CLEVBQVUsUUFBa0I7UUFBdkUsV0FBTSxHQUFOLE1BQU0sQ0FBUTtRQUFVLFlBQU8sR0FBUCxPQUFPLENBQVk7UUFBVSxhQUFRLEdBQVIsUUFBUSxDQUFVO1FBUm5GLFlBQU8sR0FBYSxFQUFFLENBQUM7UUFHZCw0QkFBdUIsR0FBNEIsRUFBQyxLQUFLLEVBQUUsSUFBSSxFQUFDLENBQUM7UUFNaEYsSUFBSSxDQUFDLFlBQVksR0FBRyxNQUFNLENBQUMsTUFBTSxDQUFDLFNBQVMsQ0FBQyxDQUFDO1lBQzNDLEVBQUUsQ0FBQyxDQUFDLENBQUMsWUFBWSxhQUFhLENBQUMsQ0FBQyxDQUFDO2dCQUMvQixJQUFJLENBQUMsTUFBTSxFQUFFLENBQUM7WUFDaEIsQ0FBQztRQUNILENBQUMsQ0FBQyxDQUFDO0lBQ0wsQ0FBQztJQUVELGtCQUFrQjtRQUNoQixJQUFJLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxTQUFTLENBQUMsQ0FBQyxJQUFJLElBQUksQ0FBQyxNQUFNLEVBQUUsQ0FBQyxDQUFDO1FBQ2pELElBQUksQ0FBQyxNQUFNLEVBQUUsQ0FBQztJQUNoQixDQUFDO0lBR0QsSUFBSSxnQkFBZ0IsQ0FBQyxJQUFxQjtRQUN4QyxFQUFFLENBQUMsQ0FBQyxLQUFLLENBQUMsT0FBTyxDQUFDLElBQUksQ0FBQyxDQUFDLENBQUMsQ0FBQztZQUN4QixJQUFJLENBQUMsT0FBTyxHQUFRLElBQUksQ0FBQztRQUMzQixDQUFDO1FBQUMsSUFBSSxDQUFDLENBQUM7WUFDTixJQUFJLENBQUMsT0FBTyxHQUFHLElBQUksQ0FBQyxLQUFLLENBQUMsR0FBRyxDQUFDLENBQUM7UUFDakMsQ0FBQztJQUNILENBQUM7SUFFRCxXQUFXLENBQUMsT0FBVyxJQUFTLElBQUksQ0FBQyxNQUFNLEVBQUUsQ0FBQyxDQUFDLENBQUM7SUFDaEQsV0FBVyxLQUFVLElBQUksQ0FBQyxZQUFZLENBQUMsV0FBVyxFQUFFLENBQUMsQ0FBQyxDQUFDO0lBRS9DLE1BQU07UUFDWixFQUFFLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxLQUFLLElBQUksSUFBSSxDQUFDLEtBQUssQ0FBQyxNQUFNLEtBQUssQ0FBQyxDQUFDO1lBQUMsTUFBTSxDQUFDO1FBRW5ELE1BQU0sY0FBYyxHQUFHLElBQUksQ0FBQyxNQUFNLENBQUMsUUFBUSxDQUFDLElBQUksQ0FBQyxNQUFNLENBQUMsR0FBRyxDQUFDLENBQUM7UUFDN0QsTUFBTSxRQUFRLEdBQUcsSUFBSSxDQUFDLEtBQUssQ0FBQyxNQUFNLENBQzlCLENBQUMsR0FBRyxFQUFFLElBQUksS0FDTixHQUFHLElBQUksWUFBWSxDQUFDLGNBQWMsRUFBRSxJQUFJLENBQUMsT0FBTyxFQUFFLElBQUksQ0FBQyx1QkFBdUIsQ0FBQyxLQUFLLENBQUMsRUFDekYsS0FBSyxDQUFDLENBQUM7UUFFWCxJQUFJLENBQUMsT0FBTyxDQUFDLE9BQU8sQ0FDaEIsQ0FBQyxJQUFJLElBQUksQ0FBQyxRQUFRLENBQUMsZUFBZSxDQUFDLElBQUksQ0FBQyxPQUFPLENBQUMsYUFBYSxFQUFFLENBQUMsRUFBRSxRQUFRLENBQUMsQ0FBQyxDQUFDO0lBQ25GLENBQUM7QUFDSCxDQUFDO0FBOUNDO0lBQUMsZUFBZSxDQUFDLFVBQVUsQ0FBQzs7K0NBQUE7QUFJNUI7SUFBQyxLQUFLLEVBQUU7O2lFQUFBO0FBa0JSO0lBQUMsS0FBSyxFQUFFOzs7d0RBQUE7QUF4QlY7SUFBQyxTQUFTLENBQUMsRUFBQyxRQUFRLEVBQUUsb0JBQW9CLEVBQUMsQ0FBQzs7b0JBQUE7QUFnRDNDIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHtBZnRlckNvbnRlbnRJbml0LCBDb250ZW50Q2hpbGRyZW4sIERpcmVjdGl2ZSwgRWxlbWVudFJlZiwgSW5wdXQsIE9uQ2hhbmdlcywgT25EZXN0cm95LCBRdWVyeUxpc3QsIFJlbmRlcmVyfSBmcm9tICdAYW5ndWxhci9jb3JlJztcbmltcG9ydCB7U3Vic2NyaXB0aW9ufSBmcm9tICdyeGpzL1N1YnNjcmlwdGlvbic7XG5cbmltcG9ydCB7TmF2aWdhdGlvbkVuZCwgUm91dGVyfSBmcm9tICcuLi9yb3V0ZXInO1xuaW1wb3J0IHtjb250YWluc1RyZWV9IGZyb20gJy4uL3VybF90cmVlJztcblxuaW1wb3J0IHtSb3V0ZXJMaW5rfSBmcm9tICcuL3JvdXRlcl9saW5rJztcblxuaW50ZXJmYWNlIFJvdXRlckxpbmtBY3RpdmVPcHRpb25zIHtcbiAgZXhhY3Q6IGJvb2xlYW47XG59XG5cbkBEaXJlY3RpdmUoe3NlbGVjdG9yOiAnW3JvdXRlckxpbmtBY3RpdmVdJ30pXG5leHBvcnQgY2xhc3MgUm91dGVyTGlua0FjdGl2ZSBpbXBsZW1lbnRzIE9uQ2hhbmdlcywgT25EZXN0cm95LCBBZnRlckNvbnRlbnRJbml0IHtcbiAgQENvbnRlbnRDaGlsZHJlbihSb3V0ZXJMaW5rKSBwcml2YXRlIGxpbmtzOiBRdWVyeUxpc3Q8Um91dGVyTGluaz47XG4gIHByaXZhdGUgY2xhc3Nlczogc3RyaW5nW10gPSBbXTtcbiAgcHJpdmF0ZSBzdWJzY3JpcHRpb246IFN1YnNjcmlwdGlvbjtcblxuICBASW5wdXQoKSBwcml2YXRlIHJvdXRlckxpbmtBY3RpdmVPcHRpb25zOiBSb3V0ZXJMaW5rQWN0aXZlT3B0aW9ucyA9IHtleGFjdDogdHJ1ZX07XG5cbiAgLyoqXG4gICAqIEBpbnRlcm5hbFxuICAgKi9cbiAgY29uc3RydWN0b3IocHJpdmF0ZSByb3V0ZXI6IFJvdXRlciwgcHJpdmF0ZSBlbGVtZW50OiBFbGVtZW50UmVmLCBwcml2YXRlIHJlbmRlcmVyOiBSZW5kZXJlcikge1xuICAgIHRoaXMuc3Vic2NyaXB0aW9uID0gcm91dGVyLmV2ZW50cy5zdWJzY3JpYmUocyA9PiB7XG4gICAgICBpZiAocyBpbnN0YW5jZW9mIE5hdmlnYXRpb25FbmQpIHtcbiAgICAgICAgdGhpcy51cGRhdGUoKTtcbiAgICAgIH1cbiAgICB9KTtcbiAgfVxuXG4gIG5nQWZ0ZXJDb250ZW50SW5pdCgpOiB2b2lkIHtcbiAgICB0aGlzLmxpbmtzLmNoYW5nZXMuc3Vic2NyaWJlKHMgPT4gdGhpcy51cGRhdGUoKSk7XG4gICAgdGhpcy51cGRhdGUoKTtcbiAgfVxuXG4gIEBJbnB1dCgpXG4gIHNldCByb3V0ZXJMaW5rQWN0aXZlKGRhdGE6IHN0cmluZ1tdfHN0cmluZykge1xuICAgIGlmIChBcnJheS5pc0FycmF5KGRhdGEpKSB7XG4gICAgICB0aGlzLmNsYXNzZXMgPSA8YW55PmRhdGE7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMuY2xhc3NlcyA9IGRhdGEuc3BsaXQoJyAnKTtcbiAgICB9XG4gIH1cblxuICBuZ09uQ2hhbmdlcyhjaGFuZ2VzOiB7fSk6IGFueSB7IHRoaXMudXBkYXRlKCk7IH1cbiAgbmdPbkRlc3Ryb3koKTogYW55IHsgdGhpcy5zdWJzY3JpcHRpb24udW5zdWJzY3JpYmUoKTsgfVxuXG4gIHByaXZhdGUgdXBkYXRlKCk6IHZvaWQge1xuICAgIGlmICghdGhpcy5saW5rcyB8fCB0aGlzLmxpbmtzLmxlbmd0aCA9PT0gMCkgcmV0dXJuO1xuXG4gICAgY29uc3QgY3VycmVudFVybFRyZWUgPSB0aGlzLnJvdXRlci5wYXJzZVVybCh0aGlzLnJvdXRlci51cmwpO1xuICAgIGNvbnN0IGlzQWN0aXZlID0gdGhpcy5saW5rcy5yZWR1Y2UoXG4gICAgICAgIChyZXMsIGxpbmspID0+XG4gICAgICAgICAgICByZXMgfHwgY29udGFpbnNUcmVlKGN1cnJlbnRVcmxUcmVlLCBsaW5rLnVybFRyZWUsIHRoaXMucm91dGVyTGlua0FjdGl2ZU9wdGlvbnMuZXhhY3QpLFxuICAgICAgICBmYWxzZSk7XG5cbiAgICB0aGlzLmNsYXNzZXMuZm9yRWFjaChcbiAgICAgICAgYyA9PiB0aGlzLnJlbmRlcmVyLnNldEVsZW1lbnRDbGFzcyh0aGlzLmVsZW1lbnQubmF0aXZlRWxlbWVudCwgYywgaXNBY3RpdmUpKTtcbiAgfVxufVxuIl19