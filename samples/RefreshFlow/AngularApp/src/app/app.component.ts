import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { AuthService } from '../core/auth.service';
import { AuthStateModel } from '../core/models/auth-state-model';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
    authState$: Observable<AuthStateModel>;

    constructor(
        private authService: AuthService,
    ) { }

    refreshToken() {
        this.authService.refreshTokens()
            .subscribe();
    }

    ngOnInit(): void {
        this.authState$ = this.authService.state$;
        // This starts up the token refresh preocess for the app
        this.authService.init()
            .subscribe(
            () => { console.info('Startup success'); },
            error => console.warn(error)
            );
    }
}
