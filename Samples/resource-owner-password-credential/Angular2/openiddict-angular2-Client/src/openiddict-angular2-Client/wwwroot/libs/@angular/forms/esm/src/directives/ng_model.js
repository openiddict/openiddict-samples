import { Directive, Host, Inject, Input, Optional, Output, Self, forwardRef } from '@angular/core';
import { EventEmitter, ObservableWrapper } from '../facade/async';
import { BaseException } from '../facade/exceptions';
import { FormControl } from '../model';
import { NG_ASYNC_VALIDATORS, NG_VALIDATORS } from '../validators';
import { ControlContainer } from './control_container';
import { NG_VALUE_ACCESSOR } from './control_value_accessor';
import { NgControl } from './ng_control';
import { composeAsyncValidators, composeValidators, controlPath, isPropertyUpdated, selectValueAccessor, setUpControl } from './shared';
export const formControlBinding = 
/*@ts2dart_const*/ /* @ts2dart_Provider */ {
    provide: NgControl,
    useExisting: forwardRef(() => NgModel)
};
export class NgModel extends NgControl {
    constructor(_parent, _validators, _asyncValidators, valueAccessors) {
        super();
        this._parent = _parent;
        this._validators = _validators;
        this._asyncValidators = _asyncValidators;
        /** @internal */
        this._added = false;
        this.update = new EventEmitter();
        this.valueAccessor = selectValueAccessor(this, valueAccessors);
        if (!this._parent)
            this._control = new FormControl();
    }
    ngOnChanges(changes) {
        this._checkName();
        if (!this._added)
            this._addControl();
        if (isPropertyUpdated(changes, this.viewModel)) {
            this._control.updateValue(this.model);
            this.viewModel = this.model;
        }
    }
    ngOnDestroy() { this.formDirective && this.formDirective.removeControl(this); }
    get control() { return this._control; }
    get path() {
        return this._parent ? controlPath(this.name, this._parent) : [];
    }
    get formDirective() { return this._parent ? this._parent.formDirective : null; }
    get validator() { return composeValidators(this._validators); }
    get asyncValidator() {
        return composeAsyncValidators(this._asyncValidators);
    }
    viewToModelUpdate(newValue) {
        this.viewModel = newValue;
        ObservableWrapper.callEmit(this.update, newValue);
    }
    _addControl() {
        this._control = this.formDirective ? this.formDirective.addControl(this) :
            this._addStandaloneControl();
        this._added = true;
    }
    _addStandaloneControl() {
        setUpControl(this._control, this);
        this._control.updateValueAndValidity({ emitEvent: false });
        return this._control;
    }
    _checkName() {
        if (this.options && this.options.name)
            this.name = this.options.name;
        if (this._parent && !this.name) {
            throw new BaseException(`Name attribute must be set if ngModel is used within a form.
                      Example: <input [(ngModel)]="person.firstName" name="first">`);
        }
    }
}
/** @nocollapse */
NgModel.decorators = [
    { type: Directive, args: [{
                selector: '[ngModel]:not([formControlName]):not([formControl])',
                providers: [formControlBinding],
                exportAs: 'ngModel'
            },] },
];
/** @nocollapse */
NgModel.ctorParameters = [
    { type: ControlContainer, decorators: [{ type: Optional }, { type: Host },] },
    { type: Array, decorators: [{ type: Optional }, { type: Self }, { type: Inject, args: [NG_VALIDATORS,] },] },
    { type: Array, decorators: [{ type: Optional }, { type: Self }, { type: Inject, args: [NG_ASYNC_VALIDATORS,] },] },
    { type: Array, decorators: [{ type: Optional }, { type: Self }, { type: Inject, args: [NG_VALUE_ACCESSOR,] },] },
];
/** @nocollapse */
NgModel.propDecorators = {
    'model': [{ type: Input, args: ['ngModel',] },],
    'name': [{ type: Input },],
    'options': [{ type: Input, args: ['ngModelOptions',] },],
    'update': [{ type: Output, args: ['ngModelChange',] },],
};
//# sourceMappingURL=ng_model.js.map