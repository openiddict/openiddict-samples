import {NgModule} from '@angular/core';
import {accountRouting} from './account.routes';
import {AccountComponent} from './account.component';
import { SharedModule } from '../../shared/shared.module';
import { AuthenticatedAuthGuard } from '../../core/guards/authenticated-auth-guard.service';

    @NgModule({
        imports: [
            accountRouting,
            SharedModule
        ],
        declarations: [
            AccountComponent
        ],
        providers: [
            AuthenticatedAuthGuard
        ],
    })
export class AccountModule {}
