import { NgModule, Optional, SkipSelf } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthService } from './auth.service';

@NgModule({
    providers: [
        AuthService
    ]
})
export class CoreModule {
    constructor( @Optional() @SkipSelf() parentModule: CoreModule) {
        if (parentModule) {
            throw new Error(
                'CoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
