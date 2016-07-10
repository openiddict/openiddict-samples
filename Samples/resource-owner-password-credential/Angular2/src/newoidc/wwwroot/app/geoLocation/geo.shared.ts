import {Injectable, EventEmitter}     from '@angular/core';
import {Subject} from 'rxjs/Subject';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';

@Injectable()
export class GeoSharedService {
    public LocationModel = new geoSharedModel();
    public country = new Subject<geoSharedModel>();
    public address: Subject<geoSharedModel> = new BehaviorSubject<geoSharedModel>(this.LocationModel);
    searchTextStream$ = this.country.asObservable();
    broadcastTextChange(text: geoSharedModel) {
        this.address.next(text);
        this.country.next(text);
    }
}
export class geoSharedModel {
    country: string;
    state: string;
    city: string;
    street: string;
}
export class geoResult {
    public status: any;
    public results: any;
}