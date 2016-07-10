
import {Injectable}     from '@angular/core';
import {Http, Response} from '@angular/http';
import {Headers, RequestOptions} from '@angular/http';
import { picture}           from './picture';
import {Observable}     from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class pictureService {
    constructor(private http: Http) { }
    private _heroesUrl = '/api/productpictures';  // URL to web api

    getHeroes() {

        return this.http.get(this._heroesUrl)
            .map(res => <picture[]>res.json())
            .catch(this.handleError);
    }

    addHero(heroType: picture): Observable<picture> {
        let body = JSON.stringify(heroType);
        //alert(JSON.stringify(heroType));
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });

        return this.http.post(this._heroesUrl, body, options)
            .map(res => <picture>res.json())
            .catch(this.handleError)
    }
    editHero(heroType: picture): Observable<picture> {
        //alert(JSON.stringify(heroType));
        let body = JSON.stringify(heroType);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        return this.http.put(`${this._heroesUrl}/` + heroType.id, body, options)
            .map(res => <picture>res.json())
            .catch(this.handleError)
    }
    selectHero(heroType: number): Observable<picture> {
        return this.http.get(`${this._heroesUrl}/heroType`)
           .retry(5)
            .map(res => <picture>res.json())
            .catch(this.handleError)
    }
    deleteHero(heroType: number): Observable<picture> {
        //alert("incalled" + heroType);
        return this.http.delete(this._heroesUrl + "/" + heroType)
            .map(res => <picture>res.json())
            .catch(this.handleError)
    }
    private handleError(error: Response) {
        console.error(error);
        return Observable.throw(error.json().error || 'Server error');
    }
}
