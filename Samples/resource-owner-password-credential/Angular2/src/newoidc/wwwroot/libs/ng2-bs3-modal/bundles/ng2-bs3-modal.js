var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
System.register("ng2-bs3-modal/components/modal-instance", ['rxjs/Observable', 'rxjs/add/operator/map', 'rxjs/add/observable/fromEvent'], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var Observable_1;
    var ModalInstance, ModalResult;
    function booleanOrValue(value) {
        if (value === 'true')
            return true;
        else if (value === 'false')
            return false;
        return value;
    }
    function toPromise(observable) {
        return new Promise(function (resolve, reject) {
            observable.subscribe(function (next) {
                resolve(next);
            });
        });
    }
    return {
        setters:[
            function (Observable_1_1) {
                Observable_1 = Observable_1_1;
            },
            function (_1) {},
            function (_2) {}],
        execute: function() {
            ModalInstance = (function () {
                function ModalInstance(element) {
                    this.element = element;
                    this.suffix = '.ng2-bs3-modal';
                    this.shownEventName = 'shown.bs.modal' + this.suffix;
                    this.hiddenEventName = 'hidden.bs.modal' + this.suffix;
                    this.visible = false;
                    this.init();
                }
                ModalInstance.prototype.open = function () {
                    return this.show();
                };
                ModalInstance.prototype.close = function () {
                    this.result = ModalResult.Close;
                    return this.hide();
                };
                ModalInstance.prototype.dismiss = function () {
                    this.result = ModalResult.Dismiss;
                    return this.hide();
                };
                ModalInstance.prototype.destroy = function () {
                    var _this = this;
                    return this.hide().then(function () {
                        if (_this.$modal) {
                            _this.$modal.data('bs.modal', null);
                            _this.$modal.remove();
                        }
                    });
                };
                ModalInstance.prototype.show = function () {
                    var promise = toPromise(this.shown);
                    this.resetData();
                    this.$modal.modal();
                    return promise;
                };
                ModalInstance.prototype.hide = function () {
                    if (this.$modal && this.visible) {
                        var promise = toPromise(this.hidden);
                        this.$modal.modal('hide');
                        return promise;
                    }
                    return Promise.resolve(this.result);
                };
                ModalInstance.prototype.init = function () {
                    var _this = this;
                    this.$modal = jQuery(this.element.nativeElement);
                    this.$modal.appendTo('body');
                    this.shown = Observable_1.Observable.fromEvent(this.$modal, this.shownEventName)
                        .map(function () {
                        _this.visible = true;
                    });
                    this.hidden = Observable_1.Observable.fromEvent(this.$modal, this.hiddenEventName)
                        .map(function () {
                        var result = (!_this.result || _this.result === ModalResult.None)
                            ? ModalResult.Dismiss : _this.result;
                        _this.result = ModalResult.None;
                        _this.visible = false;
                        return result;
                    });
                };
                ModalInstance.prototype.resetData = function () {
                    this.$modal.removeData();
                    this.$modal.data('backdrop', booleanOrValue(this.$modal.attr('data-backdrop')));
                    this.$modal.data('keyboard', booleanOrValue(this.$modal.attr('data-keyboard')));
                };
                return ModalInstance;
            }());
            exports_1("ModalInstance", ModalInstance);
            (function (ModalResult) {
                ModalResult[ModalResult["None"] = 0] = "None";
                ModalResult[ModalResult["Close"] = 1] = "Close";
                ModalResult[ModalResult["Dismiss"] = 2] = "Dismiss";
            })(ModalResult || (ModalResult = {}));
            exports_1("ModalResult", ModalResult);
        }
    }
});
System.register("ng2-bs3-modal/components/modal", ['@angular/core', "ng2-bs3-modal/components/modal-instance"], function(exports_2, context_2) {
    "use strict";
    var __moduleName = context_2 && context_2.id;
    var core_1, modal_instance_1;
    var ModalComponent, ModalSize;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (modal_instance_1_1) {
                modal_instance_1 = modal_instance_1_1;
            }],
        execute: function() {
            ModalComponent = (function () {
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
            exports_2("ModalComponent", ModalComponent);
            ModalSize = (function () {
                function ModalSize() {
                }
                ModalSize.validSize = function (size) {
                    return size && (size === ModalSize.Small || size === ModalSize.Large);
                };
                ModalSize.Small = 'sm';
                ModalSize.Large = 'lg';
                return ModalSize;
            }());
            exports_2("ModalSize", ModalSize);
        }
    }
});
System.register("ng2-bs3-modal/components/modal-header", ['@angular/core', "ng2-bs3-modal/components/modal"], function(exports_3, context_3) {
    "use strict";
    var __moduleName = context_3 && context_3.id;
    var core_2, modal_1;
    var ModalHeaderComponent;
    return {
        setters:[
            function (core_2_1) {
                core_2 = core_2_1;
            },
            function (modal_1_1) {
                modal_1 = modal_1_1;
            }],
        execute: function() {
            ModalHeaderComponent = (function () {
                function ModalHeaderComponent(modal) {
                    this.modal = modal;
                    this.showClose = false;
                }
                __decorate([
                    core_2.Input('show-close'), 
                    __metadata('design:type', Boolean)
                ], ModalHeaderComponent.prototype, "showClose", void 0);
                ModalHeaderComponent = __decorate([
                    core_2.Component({
                        selector: 'modal-header',
                        template: "\n        <div class=\"modal-header\">\n            <button *ngIf=\"showClose\" type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\" (click)=\"modal.dismiss()\">\n                <span aria-hidden=\"true\">&times;</span>\n            </button>\n            <ng-content></ng-content>\n        </div>\n    "
                    }), 
                    __metadata('design:paramtypes', [modal_1.ModalComponent])
                ], ModalHeaderComponent);
                return ModalHeaderComponent;
            }());
            exports_3("ModalHeaderComponent", ModalHeaderComponent);
        }
    }
});
System.register("ng2-bs3-modal/components/modal-body", ['@angular/core', "ng2-bs3-modal/components/modal"], function(exports_4, context_4) {
    "use strict";
    var __moduleName = context_4 && context_4.id;
    var core_3, modal_2;
    var ModalBodyComponent;
    return {
        setters:[
            function (core_3_1) {
                core_3 = core_3_1;
            },
            function (modal_2_1) {
                modal_2 = modal_2_1;
            }],
        execute: function() {
            ModalBodyComponent = (function () {
                function ModalBodyComponent(modal) {
                    this.modal = modal;
                }
                ModalBodyComponent = __decorate([
                    core_3.Component({
                        selector: 'modal-body',
                        template: "\n        <div class=\"modal-body\">\n            <ng-content></ng-content>\n        </div>\n    "
                    }), 
                    __metadata('design:paramtypes', [modal_2.ModalComponent])
                ], ModalBodyComponent);
                return ModalBodyComponent;
            }());
            exports_4("ModalBodyComponent", ModalBodyComponent);
        }
    }
});
System.register("ng2-bs3-modal/components/modal-footer", ['@angular/core', "ng2-bs3-modal/components/modal"], function(exports_5, context_5) {
    "use strict";
    var __moduleName = context_5 && context_5.id;
    var core_4, modal_3;
    var ModalFooterComponent;
    return {
        setters:[
            function (core_4_1) {
                core_4 = core_4_1;
            },
            function (modal_3_1) {
                modal_3 = modal_3_1;
            }],
        execute: function() {
            ModalFooterComponent = (function () {
                function ModalFooterComponent(modal) {
                    this.modal = modal;
                    this.showDefaultButtons = false;
                }
                __decorate([
                    core_4.Input('show-default-buttons'), 
                    __metadata('design:type', Boolean)
                ], ModalFooterComponent.prototype, "showDefaultButtons", void 0);
                ModalFooterComponent = __decorate([
                    core_4.Component({
                        selector: 'modal-footer',
                        template: "\n        <div class=\"modal-footer\">\n            <ng-content></ng-content>\n            <button *ngIf=\"showDefaultButtons\" type=\"button\" class=\"btn btn-default\" data-dismiss=\"modal\" (click)=\"modal.dismiss()\">Close</button>\n            <button *ngIf=\"showDefaultButtons\" type=\"button\" class=\"btn btn-primary\" (click)=\"modal.close()\">Save</button>\n        </div>\n    "
                    }), 
                    __metadata('design:paramtypes', [modal_3.ModalComponent])
                ], ModalFooterComponent);
                return ModalFooterComponent;
            }());
            exports_5("ModalFooterComponent", ModalFooterComponent);
        }
    }
});
System.register("ng2-bs3-modal/directives/autofocus", ['@angular/core', "ng2-bs3-modal/components/modal"], function(exports_6, context_6) {
    "use strict";
    var __moduleName = context_6 && context_6.id;
    var core_5, modal_4;
    var AutofocusDirective;
    return {
        setters:[
            function (core_5_1) {
                core_5 = core_5_1;
            },
            function (modal_4_1) {
                modal_4 = modal_4_1;
            }],
        execute: function() {
            AutofocusDirective = (function () {
                function AutofocusDirective(el, modal) {
                    var _this = this;
                    this.el = el;
                    this.modal = modal;
                    if (modal != null) {
                        this.modal.onOpen.subscribe(function () {
                            _this.el.nativeElement.focus();
                        });
                    }
                }
                AutofocusDirective = __decorate([
                    core_5.Directive({
                        selector: '[autofocus]'
                    }), 
                    __metadata('design:paramtypes', [core_5.ElementRef, modal_4.ModalComponent])
                ], AutofocusDirective);
                return AutofocusDirective;
            }());
            exports_6("AutofocusDirective", AutofocusDirective);
        }
    }
});
System.register("ng2-bs3-modal/ng2-bs3-modal", ["ng2-bs3-modal/components/modal", "ng2-bs3-modal/components/modal-header", "ng2-bs3-modal/components/modal-body", "ng2-bs3-modal/components/modal-footer", "ng2-bs3-modal/directives/autofocus", "ng2-bs3-modal/components/modal-instance"], function(exports_7, context_7) {
    "use strict";
    var __moduleName = context_7 && context_7.id;
    var modal_5, modal_header_1, modal_body_1, modal_footer_1, autofocus_1;
    var MODAL_DIRECTIVES;
    var exportedNames_1 = {
        'MODAL_DIRECTIVES': true
    };
    function exportStar_1(m) {
        var exports = {};
        for(var n in m) {
            if (n !== "default"&& !exportedNames_1.hasOwnProperty(n)) exports[n] = m[n];
        }
        exports_7(exports);
    }
    return {
        setters:[
            function (modal_5_1) {
                modal_5 = modal_5_1;
                exportStar_1(modal_5_1);
            },
            function (modal_header_1_1) {
                modal_header_1 = modal_header_1_1;
                exportStar_1(modal_header_1_1);
            },
            function (modal_body_1_1) {
                modal_body_1 = modal_body_1_1;
                exportStar_1(modal_body_1_1);
            },
            function (modal_footer_1_1) {
                modal_footer_1 = modal_footer_1_1;
                exportStar_1(modal_footer_1_1);
            },
            function (autofocus_1_1) {
                autofocus_1 = autofocus_1_1;
            },
            function (modal_instance_2_1) {
                exportStar_1(modal_instance_2_1);
            }],
        execute: function() {
            exports_7("MODAL_DIRECTIVES", MODAL_DIRECTIVES = [
                modal_5.ModalComponent,
                modal_header_1.ModalHeaderComponent,
                modal_body_1.ModalBodyComponent,
                modal_footer_1.ModalFooterComponent,
                autofocus_1.AutofocusDirective
            ]);
        }
    }
});
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibmcyLWJzMy1tb2RhbC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uL3NyYy9uZzItYnMzLW1vZGFsL2NvbXBvbmVudHMvbW9kYWwtaW5zdGFuY2UudHMiLCIuLi9zcmMvbmcyLWJzMy1tb2RhbC9jb21wb25lbnRzL21vZGFsLnRzIiwiLi4vc3JjL25nMi1iczMtbW9kYWwvY29tcG9uZW50cy9tb2RhbC1oZWFkZXIudHMiLCIuLi9zcmMvbmcyLWJzMy1tb2RhbC9jb21wb25lbnRzL21vZGFsLWJvZHkudHMiLCIuLi9zcmMvbmcyLWJzMy1tb2RhbC9jb21wb25lbnRzL21vZGFsLWZvb3Rlci50cyIsIi4uL3NyYy9uZzItYnMzLW1vZGFsL2RpcmVjdGl2ZXMvYXV0b2ZvY3VzLnRzIiwiLi4vc3JjL25nMi1iczMtbW9kYWwvbmcyLWJzMy1tb2RhbC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOzs7Ozs7Ozs7Ozs7OztJQTBGQSx3QkFBd0IsS0FBSztRQUN6QixFQUFFLENBQUMsQ0FBQyxLQUFLLEtBQUssTUFBTSxDQUFDO1lBQ2pCLE1BQU0sQ0FBQyxJQUFJLENBQUM7UUFDaEIsSUFBSSxDQUFDLEVBQUUsQ0FBQyxDQUFDLEtBQUssS0FBSyxPQUFPLENBQUM7WUFDdkIsTUFBTSxDQUFDLEtBQUssQ0FBQztRQUNqQixNQUFNLENBQUMsS0FBSyxDQUFDO0lBQ2pCLENBQUM7SUFFRCxtQkFBc0IsVUFBeUI7UUFDM0MsTUFBTSxDQUFDLElBQUksT0FBTyxDQUFDLFVBQUMsT0FBTyxFQUFFLE1BQU07WUFDL0IsVUFBVSxDQUFDLFNBQVMsQ0FBQyxVQUFBLElBQUk7Z0JBQ3JCLE9BQU8sQ0FBQyxJQUFJLENBQUMsQ0FBQztZQUNsQixDQUFDLENBQUMsQ0FBQztRQUNQLENBQUMsQ0FBQyxDQUFDO0lBQ1AsQ0FBQzs7Ozs7Ozs7O1lBakdEO2dCQVlJLHVCQUFvQixPQUFtQjtvQkFBbkIsWUFBTyxHQUFQLE9BQU8sQ0FBWTtvQkFWL0IsV0FBTSxHQUFXLGdC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