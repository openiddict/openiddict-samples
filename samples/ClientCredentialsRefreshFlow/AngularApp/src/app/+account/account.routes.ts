import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthenticatedAuthGuard } from '../../core/guards/authenticated-auth-guard.service';
import { AccountComponent } from './account.component';

const accountRoutes: Routes = [
    {
        path: '',
        component: AccountComponent,
        canActivate: [AuthenticatedAuthGuard],
        children: [
            // {
            //     path: '',
            //     component: RolesComponent
            // },

        ]
    }
];

export const accountRouting: ModuleWithProviders = RouterModule.forChild(accountRoutes);
