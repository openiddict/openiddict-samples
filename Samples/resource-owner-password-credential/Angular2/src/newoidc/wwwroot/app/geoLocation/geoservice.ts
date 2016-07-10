import {Injectable} from '@angular/core';
import { Response, Http} from '@angular/http';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import {geoResult} from './geo.shared';

@Injectable()
export class geoService {
    constructor(private http: Http) { }
    public getPlaces(xml_Latitude: any, xml_Lang: any) {
        return this.http.get(`http://maps.googleapis.com/maps/api/geocode/json?latlng=${xml_Latitude},${xml_Lang}&sensor=true`)
            .map(res => <geoResult>res.json())
            .catch(this.handleError);
    }

    private handleError(error: Response) {
        console.error(error);
        return error.json().error || 'Server error';
    }
}