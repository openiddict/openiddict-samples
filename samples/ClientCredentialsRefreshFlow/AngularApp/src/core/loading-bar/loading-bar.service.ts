import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {AppState} from '../../app/app-store';
import {Store} from '@ngrx/store';
import { LoadingBarActions } from './loading-bar.actions';

@Injectable()
export class LoadingBarService {
    constructor(private store: Store<AppState>,
                private loadingBarActions: LoadingBarActions,
    ) {}

    load() {
        this.store.dispatch(this.loadingBarActions.Start());
    }

    done() {
        this.store.dispatch(this.loadingBarActions.Done());
    }

    doWithLoader<T>(task: Observable<T>): Observable<T> {
        return Observable
            .of(true)
            .do(() => this.load())
            .flatMap(() => task)
            .finally( () => this.done());
    }
}
