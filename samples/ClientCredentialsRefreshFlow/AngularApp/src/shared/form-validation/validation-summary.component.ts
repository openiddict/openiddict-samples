import { Component, Input } from '@angular/core';

@Component({
    selector: 'validation-summary',
    template: `
<div>
    <div *ngFor="let error of errorMessages">
        - {{error}}
    </div>
</div>`
})
export class ValidationSummaryComponent {
    @Input() errorMessages: string[];
}