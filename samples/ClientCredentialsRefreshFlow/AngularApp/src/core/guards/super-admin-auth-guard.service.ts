import { Injectable } from '@angular/core';
import { CanLoad, Route } from '@angular/router';
import { CanActivate } from '@angular/router';
import { AuthGuard } from './auth-guard.service';
import { Observable } from 'rxjs';

@Injectable()
export class SuperAdminAuthGuard  implements CanActivate, CanLoad {
    private role: string = 'SuperAdmin';

    constructor(private authGuard: AuthGuard ) { }


    canActivate(): Observable<boolean> {
        return this.authGuard.isInRole(this.role);
    }
    canLoad(route: Route): Observable<boolean> {
        return this.canActivate();
    }
}
