import {NgModule} from '@angular/core';
import { CommonModule }        from '@angular/common';
import {AlertComponent} from './alert/alert.component';
import {ValidationSummaryComponent} from './form-validation/validation-summary.component';
import {HttpModule} from '@angular/http';
import {ReactiveFormsModule} from '@angular/forms';
import {ControlMessagesComponent} from './form-validation/control-messages.component';
import {LoadingBarComponent} from './loading-bar/loading-bar.component';
import {FileUploadComponent} from './file-upload/file-upload.component';
import {DefaultValuePipe} from './pipes/default-value/default-value.pipe';
import {MaterialModule} from '@angular/material';
import {RxContextDirective} from './directives/rx-context.directive';


@NgModule({
    imports: [
        ReactiveFormsModule,
        CommonModule,
        HttpModule,
        MaterialModule.forRoot()
    ],
    declarations: [
        DefaultValuePipe,
        LoadingBarComponent,
        AlertComponent,
        ControlMessagesComponent,
        ValidationSummaryComponent,
        FileUploadComponent,
        RxContextDirective
    ],
    exports: [
        MaterialModule,
        DefaultValuePipe,
        ReactiveFormsModule,
        HttpModule,
        LoadingBarComponent,
        AlertComponent,
        ControlMessagesComponent,
        CommonModule,
        ValidationSummaryComponent,
        FileUploadComponent
    ]
})
export class SharedModule {}