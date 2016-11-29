import { Component, OnInit } from '@angular/core';
import { AlertService} from '../../core/alert/alert.service';
import { Observable} from 'rxjs/Observable';
import { Alert} from '../../core/models/alert.model';
import { AppState} from '../../app/app-store';
import { Store} from '@ngrx/store';

@Component({
    selector: 'app-alerts',
    templateUrl: 'alert.component.html',
    styleUrls: ['alert.component.css']
})
export class AlertComponent implements OnInit {
    alerts: Observable<Alert[]>;

    constructor(private alertService: AlertService,
                private store: Store<AppState>
    ) {}

    ngOnInit(): void {
        this.alerts = this.store.select( state => state.alerts);
    }

}
