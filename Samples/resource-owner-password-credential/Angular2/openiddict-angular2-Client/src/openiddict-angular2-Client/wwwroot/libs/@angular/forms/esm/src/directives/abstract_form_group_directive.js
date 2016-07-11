import { ControlContainer } from './control_container';
import { composeAsyncValidators, composeValidators, controlPath } from './shared';
/**
  This is a base class for code shared between {@link NgModelGroup} and {@link FormGroupName}.
 */
export class AbstractFormGroupDirective extends ControlContainer {
    ngOnInit() { this.formDirective.addFormGroup(this); }
    ngOnDestroy() { this.formDirective.removeFormGroup(this); }
    /**
     * Get the {@link FormGroup} backing this binding.
     */
    get control() { return this.formDirective.getFormGroup(this); }
    /**
     * Get the path to this control group.
     */
    get path() { return controlPath(this.name, this._parent); }
    /**
     * Get the {@link Form} to which this group belongs.
     */
    get formDirective() { return this._parent.formDirective; }
    get validator() { return composeValidators(this._validators); }
    get asyncValidator() { return composeAsyncValidators(this._asyncValidators); }
}
//# sourceMappingURL=abstract_form_group_directive.js.map