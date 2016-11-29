import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { AuthGuard } from './auth-guard.service';
import { Observable } from 'rxjs';

@Injectable()
export class AdminAuthGuard implements CanActivate {
    private role: string = 'SuperAdmin';

    constructor(private authGuard: AuthGuard ) { }

    canActivate(): Observable<boolean> {
        return this.authGuard.isInRole(this.role);
    }
}
