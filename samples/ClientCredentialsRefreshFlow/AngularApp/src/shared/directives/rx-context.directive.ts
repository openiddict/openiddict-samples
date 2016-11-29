import {Directive, TemplateRef, Input, ViewContainerRef, EmbeddedViewRef, ChangeDetectorRef} from '@angular/core';
import {Observable} from 'rxjs';
@Directive({
    selector: '[rxContext][rxContextOn]'
})
export class RxContextDirective{
    @Input() rxContextOn: Observable<any>;
    _viewRef: EmbeddedViewRef<any>;
    constructor(private templateRef: TemplateRef<any>,
                private vcr: ViewContainerRef,
                private _cdr: ChangeDetectorRef
    ){
        this._cdr.detach();
    }

    ngOnInit() {
        this.rxContextOn.subscribe( state => {
            if(!this._viewRef) {
                this._viewRef = this.vcr.createEmbeddedView(this.templateRef, {'$implicit': state})
            }
            this._viewRef.context.$implicit = state;
            this._cdr.detectChanges();
        })
    }
}

//*rxContext="let user on userStream"