import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthTokenService } from '../core/auth-token/auth-token.service';
import { Store } from '@ngrx/store';
import { AppState } from './app-store';
import { Observable } from 'rxjs';
import { AuthState } from '../core/auth-store/auth.store';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
    authState$: Observable<AuthState>;

    constructor(private tokens: AuthTokenService,
                private store: Store<AppState>
    ) { }

    refreshToken() {
        this.tokens.refreshTokens()
            .subscribe();
    }

    ngOnInit(): void {
        this.authState$ = this.store.select(state => state.auth);

        this.tokens.startupTokenRefresh()
            .subscribe(
            () => {
                // we manage to refresh the tokens so we can carry with the scheduleRefresh
                this.tokens.scheduleRefresh();
            }, error => {
                console.error(error);
            });
    }

    ngOnDestroy(): void {
        this.tokens.unsubscribeRefresh();
    }
}
