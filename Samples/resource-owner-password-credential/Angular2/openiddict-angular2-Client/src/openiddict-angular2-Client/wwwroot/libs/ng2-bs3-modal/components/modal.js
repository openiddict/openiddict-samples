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
var modal_instance_1 = require('./modal-instance');
var ModalComponent = (function () {
    function ModalComponent(element) {
        var _this = this;
        this.element = element;
        this.overrideSize = null;
        this.visible = false;
        this.animation = true;
        this.backdrop = true;
        this.keyboard = true;
        this.onClose = new core_1.EventEmitter(false);
        this.onDismiss = new core_1.EventEmitter(false);
        this.onOpen = new core_1.EventEmitter(false);
        this.instance = new modal_instance_1.ModalInstance(this.element);
        this.instance.hidden.subscribe(function (result) {
            _this.visible = _this.instance.visible;
            if (result === modal_instance_1.ModalResult.Dismiss)
                _this.onDismiss.emit(undefined);
        });
        this.instance.shown.subscribe(function () {
            _this.onOpen.emit(undefined);
        });
    }
    Object.defineProperty(ModalComponent.prototype, "fadeClass", {
        get: function () { return this.animation; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ModalComponent.prototype, "dataKeyboardAttr", {
        get: function () { return this.keyboard; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ModalComponent.prototype, "dataBackdropAttr", {
        get: function () { return this.backdrop; },
        enumerable: true,
        configurable: true
    });
    ModalComponent.prototype.ngOnDestroy = function () {
        return this.instance && this.instance.destroy();
    };
    ModalComponent.prototype.routerCanDeactivate = function () {
        return this.ngOnDestroy();
    };
    ModalComponent.prototype.open = function (size) {
        var _this = this;
        if (ModalSize.validSize(size))
            this.overrideSize = size;
        return this.instance.open().then(function () {
            _this.visible = _this.instance.visible;
        });
    };
    ModalComponent.prototype.close = function () {
        var _this = this;
        return this.instance.close().then(function () {
            _this.onClose.emit(undefined);
        });
    };
    ModalComponent.prototype.dismiss = function () {
        return this.instance.dismiss();
    };
    ModalComponent.prototype.isSmall = function () {
        return this.overrideSize !== ModalSize.Large
            && this.size === ModalSize.Small
            || this.overrideSize === ModalSize.Small;
    };
    ModalComponent.prototype.isLarge = function () {
        return this.overrideSize !== ModalSize.Small
            && this.size === ModalSize.Large
            || this.overrideSize === ModalSize.Large;
    };
    __decorate([
        core_1.Input(), 
        __metadata('design:type', Boolean)
    ], ModalComponent.prototype, "animation", void 0);
    __decorate([
        core_1.Input(), 
        __metadata('design:type', Object)
    ], ModalComponent.prototype, "backdrop", void 0);
    __decorate([
        core_1.Input(), 
        __metadata('design:type', Boolean)
    ], ModalComponent.prototype, "keyboard", void 0);
    __decorate([
        core_1.Input(), 
        __metadata('design:type', String)
    ], ModalComponent.prototype, "size", void 0);
    __decorate([
        core_1.Output(), 
        __metadata('design:type', core_1.EventEmitter)
    ], ModalComponent.prototype, "onClose", void 0);
    __decorate([
        core_1.Output(), 
        __metadata('design:type', core_1.EventEmitter)
    ], ModalComponent.prototype, "onDismiss", void 0);
    __decorate([
        core_1.Output(), 
        __metadata('design:type', core_1.EventEmitter)
    ], ModalComponent.prototype, "onOpen", void 0);
    __decorate([
        core_1.HostBinding('class.fade'), 
        __metadata('design:type', Boolean)
    ], ModalComponent.prototype, "fadeClass", null);
    __decorate([
        core_1.HostBinding('attr.data-keyboard'), 
        __metadata('design:type', Boolean)
    ], ModalComponent.prototype, "dataKeyboardAttr", null);
    __decorate([
        core_1.HostBinding('attr.data-backdrop'), 
        __metadata('design:type', Object)
    ], ModalComponent.prototype, "dataBackdropAttr", null);
    ModalComponent = __decorate([
        core_1.Component({
            selector: 'modal',
            host: {
                'class': 'modal',
                'role': 'dialog',
                'tabindex': '-1'
            },
            template: "\n        <div class=\"modal-dialog\" [ngClass]=\"{ 'modal-sm': isSmall(), 'modal-lg': isLarge() }\">\n            <div class=\"modal-content\">\n                <ng-content></ng-content>\n            </div>\n        </div>\n    "
        }), 
        __metadata('design:paramtypes', [core_1.ElementRef])
    ], ModalComponent);
    return ModalComponent;
}());
exports.ModalComponent = ModalComponent;
var ModalSize = (function () {
    function ModalSize() {
    }
    ModalSize.validSize = function (size) {
        return size && (size === ModalSize.Small || size === ModalSize.Large);
    };
    ModalSize.Small = 'sm';
    ModalSize.Large = 'lg';
    return ModalSize;
}());
exports.ModalSize = ModalSize;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibW9kYWwuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi9zcmMvbmcyLWJzMy1tb2RhbC9jb21wb25lbnRzL21vZGFsLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7Ozs7Ozs7QUFBQSxxQkFBaUcsZUFBZSxDQUFDLENBQUE7QUFDakgsK0JBQTJDLGtCQUFrQixDQUFDLENBQUE7QUFpQjlEO0lBb0JJLHdCQUFvQixPQUFtQjtRQXBCM0MsaUJBc0VDO1FBbER1QixZQUFPLEdBQVAsT0FBTyxDQUFZO1FBbEIvQixpQkFBWSxHQUFXLElBQUksQ0FBQztRQUdwQyxZQUFPLEdBQVksS0FBSyxDQUFDO1FBRWhCLGNBQVMsR0FBWSxJQUFJLENBQUM7UUFDMUIsYUFBUSxHQUFxQixJQUFJLENBQUM7UUFDbEMsYUFBUSxHQUFZLElBQUksQ0FBQztRQUd4QixZQUFPLEdBQXNCLElBQUksbUJBQVksQ0FBQyxLQUFLLENBQUMsQ0FBQztRQUNyRCxjQUFTLEdBQXNCLElBQUksbUJBQVksQ0FBQyxLQUFLLENBQUMsQ0FBQztRQUN2RCxXQUFNLEdBQXNCLElBQUksbUJBQVksQ0FBQyxLQUFLLENBQUMsQ0FBQztRQU8xRCxJQUFJLENBQUMsUUFBUSxHQUFHLElBQUksOEJBQWEsQ0FBQyxJQUFJLENBQUMsT0FBTyxDQUFDLENBQUM7UUFFaEQsSUFBSSxDQUFDLFFBQVEsQ0FBQyxNQUFNLENBQUMsU0FBUyxDQUFDLFVBQUMsTUFBTTtZQUNsQyxLQUFJLENBQUMsT0FBTyxHQUFHLEtBQUksQ0FBQyxRQUFRLENBQUMsT0FBTyxDQUFDO1lBQ3JDLEVBQUUsQ0FBQyxDQUFDLE1BQU0sS0FBSyw0QkFBVyxDQUFDLE9BQU8sQ0FBQztnQkFDL0IsS0FBSSxDQUFDLFNBQVMsQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUM7UUFDdkMsQ0FBQyxDQUFDLENBQUM7UUFFSCxJQUFJLENBQUMsUUFBUSxDQUFDLEtBQUssQ0FBQyxTQUFTLENBQUM7WUFDMUIsS0FBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUM7UUFDaEMsQ0FBQyxDQUFDLENBQUM7SUFDUCxDQUFDO0lBaEIwQixzQkFBSSxxQ0FBUzthQUFiLGNBQTJCLE1BQU0sQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUMsQ0FBQzs7O09BQUE7SUFDM0Msc0JBQUksNENBQWdCO2FBQXBCLGNBQWtDLE1BQU0sQ0FBQyxJQUFJLENBQUMsUUFBUSxDQUFDLENBQUMsQ0FBQzs7O09BQUE7SUFDekQsc0JBQUksNENBQWdCO2FBQXBCLGNBQTJDLE1BQU0sQ0FBQyxJQUFJLENBQUMsUUFBUSxDQUFDLENBQUMsQ0FBQzs7O09BQUE7SUFnQnJHLG9DQUFXLEdBQVg7UUFDSSxNQUFNLENBQUMsSUFBSSxDQUFDLFFBQVEsSUFBSSxJQUFJLENBQUMsUUFBUSxDQUFDLE9BQU8sRUFBRSxDQUFDO0lBQ3BELENBQUM7SUFFRCw0Q0FBbUIsR0FBbkI7UUFDSSxNQUFNLENBQUMsSUFBSSxDQUFDLFdBQVcsRUFBRSxDQUFDO0lBQzlCLENBQUM7SUFFRCw2QkFBSSxHQUFKLFVBQUssSUFBYTtRQUFsQixpQkFLQztRQUpHLEVBQUUsQ0FBQyxDQUFDLFNBQVMsQ0FBQyxTQUFTLENBQUMsSUFBSSxDQUFDLENBQUM7WUFBQyxJQUFJLENBQUMsWUFBWSxHQUFHLElBQUksQ0FBQztRQUN4RCxNQUFNLENBQUMsSUFBSSxDQUFDLFFBQVEsQ0FBQyxJQUFJLEVBQUUsQ0FBQyxJQUFJLENBQUM7WUFDN0IsS0FBSSxDQUFDLE9BQU8sR0FBRyxLQUFJLENBQUMsUUFBUSxDQUFDLE9BQU8sQ0FBQztRQUN6QyxDQUFDLENBQUMsQ0FBQztJQUNQLENBQUM7SUFFRCw4QkFBSyxHQUFMO1FBQUEsaUJBSUM7UUFIRyxNQUFNLENBQUMsSUFBSSxDQUFDLFFBQVEsQ0FBQyxLQUFLLEVBQUUsQ0FBQyxJQUFJLENBQUM7WUFDOUIsS0FBSSxDQUFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUM7UUFDakMsQ0FBQyxDQUFDLENBQUM7SUFDUCxDQUFDO0lBRUQsZ0NBQU8sR0FBUDtRQUNJLE1BQU0sQ0FBQyxJQUFJLENBQUMsUUFBUSxDQUFDLE9BQU8sRUFBRSxDQUFDO0lBQ25DLENBQUM7SUFFTyxnQ0FBTyxHQUFmO1FBQ0ksTUFBTSxDQUFDLElBQUksQ0FBQyxZQUFZLEtBQUssU0FBUyxDQUFDLEtBQUs7ZUFDckMsSUFBSSxDQUFDLElBQUksS0FBSyxTQUFTLENBQUMsS0FBSztlQUM3QixJQUFJLENBQUMsWUFBWSxLQUFLLFNBQVMsQ0FBQyxLQUFLLENBQUM7SUFDakQsQ0FBQztJQUVPLGdDQUFPLEdBQWY7UUFDSSxNQUFNLENBQUMsSUFBSSxDQUFDLFlBQVksS0FBSyxTQUFTLENBQUMsS0FBSztlQUNyQyxJQUFJLENBQUMsSUFBSSxLQUFLLFNBQVMsQ0FBQyxLQUFLO2VBQzdCLElBQUksQ0FBQyxZQUFZLEtBQUssU0FBUyxDQUFDLEtBQUssQ0FBQztJQUNqRCxDQUFDO0lBOUREO1FBQUMsWUFBSyxFQUFFOztxREFBQTtJQUNSO1FBQUMsWUFBSyxFQUFFOztvREFBQTtJQUNSO1FBQUMsWUFBSyxFQUFFOztvREFBQTtJQUNSO1FBQUMsWUFBSyxFQUFFOztnREFBQTtJQUVSO1FBQUMsYUFBTSxFQUFFOzttREFBQTtJQUNUO1FBQUMsYUFBTSxFQUFFOztxREFBQTtJQUNUO1FBQUMsYUFBTSxFQUFFOztrREFBQTtJQUVUO1FBQUMsa0JBQVcsQ0FBQyxZQUFZLENBQUM7O21EQUFBO0lBQzFCO1FBQUMsa0JBQVcsQ0FBQyxvQkFBb0IsQ0FBQzs7MERBQUE7SUFDbEM7UUFBQyxrQkFBVyxDQUFDLG9CQUFvQixDQUFDOzswREFBQTtJQWpDdEM7UUFBQyxnQkFBUyxDQUFDO1lBQ1AsUUFBUSxFQUFFLE9BQU87WUFDakIsSUFBSSxFQUFFO2dCQUNGLE9BQU8sRUFBRSxPQUFPO2dCQUNoQixNQUFNLEVBQUUsUUFBUTtnQkFDaEIsVUFBVSxFQUFFLElBQUk7YUFDbkI7WUFDRCxRQUFRLEVBQUUsdU9BTVQ7U0FDSixDQUFDOztzQkFBQTtJQXVFRixxQkFBQztBQUFELENBQUMsQUF0RUQsSUFzRUM7QUF0RVksc0JBQWMsaUJBc0UxQixDQUFBO0FBRUQ7SUFBQTtJQU9BLENBQUM7SUFIVSxtQkFBUyxHQUFoQixVQUFpQixJQUFZO1FBQ3pCLE1BQU0sQ0FBQyxJQUFJLElBQUksQ0FBQyxJQUFJLEtBQUssU0FBUyxDQUFDLEtBQUssSUFBSSxJQUFJLEtBQUssU0FBUyxDQUFDLEtBQUssQ0FBQyxDQUFDO0lBQzFFLENBQUM7SUFMTSxlQUFLLEdBQUcsSUFBSSxDQUFDO0lBQ2IsZUFBSyxHQUFHLElBQUksQ0FBQztJQUt4QixnQkFBQztBQUFELENBQUMsQUFQRCxJQU9DO0FBUFksaUJBQVMsWUFPckIsQ0FBQSJ9