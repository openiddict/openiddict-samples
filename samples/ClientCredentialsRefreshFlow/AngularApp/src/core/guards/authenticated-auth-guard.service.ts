import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import {AuthGuard} from './auth-guard.service';
import {Observable} from 'rxjs';

@Injectable()
export class AuthenticatedAuthGuard implements CanActivate {

    constructor(private authGuard: AuthGuard ) { }


    canActivate(): Observable<boolean> {
        return this.authGuard.isLoggedIn();
    }
}
