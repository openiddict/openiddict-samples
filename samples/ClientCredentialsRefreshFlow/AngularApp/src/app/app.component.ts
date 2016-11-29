import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthTokenService } from '../core/auth-token/auth-token.service';
import { Store } from '@ngrx/store';
import { AlertService } from '../core/alert/alert.service';
import { AppState } from './app-store';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {

    constructor(
      private tokens: AuthTokenService,
               private store: Store<AppState>,
                private alert: AlertService
               ) { }

  ngOnInit(): void {
        this.tokens.startupTokenRefresh()
            .subscribe(
            () => {
                console.info('startup done');
                // we manage to refresh the tokens so we can carry with the scheduleRefresh
                this.tokens.scheduleRefresh();
            }, error => {
                console.error(error);
                // keep it silent if there's nothing in storage
                if (error !== 'No token in Storage') {
                    this.alert.sendWarning('error');
                }
            });
    }

    ngOnDestroy(): void {
        this.tokens.unsubscribeRefresh();
    }
}
