import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Alert } from '../models/alert.model';
import { AlertType } from '../models/alert-types';
import { Store}  from '@ngrx/store';
import { AppState } from '../../app/app-store';
import { AlertActions } from './alert.actions';

@Injectable()
export class AlertService {
    constructor(private store: Store<AppState>,
                private alertActions: AlertActions
    ) {}

    sendSuccess(message: string, delay?: number) {
        this.sendAlert({message: message, type: AlertType.success}, delay);
    }

    sendInfo(message: string, delay?: number) {
        this.sendAlert({message: message, type: AlertType.info}, delay);
    }

    sendWarning(message: string, delay?: number) {
        this.sendAlert({message: message, type: AlertType.warning}, delay);
    }

    sendError(message: string, delay?: number) {
        this.sendAlert({message: message, type: AlertType.error}, delay);
    }

    private sendAlert(alert: Alert, delay = 3000) {
        this.store.dispatch(this.alertActions.Add(alert));
        Observable.of(true)
            .delay(delay)
            .subscribe(
                () => this.store.dispatch(this.alertActions.Delete(alert))
            );
    }
}
