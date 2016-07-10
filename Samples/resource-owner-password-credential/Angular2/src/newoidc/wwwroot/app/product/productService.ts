
import {Injectable}     from '@angular/core';
import {Http, Response} from '@angular/http';
import {Headers, RequestOptions} from '@angular/http';
import { product}           from './product';
import {Observable}     from 'rxjs/Observable';
import{proview} from '../models/productlist.model';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw'
//import 'rxjs/add/operator/throw';

@Injectable()
export class ProductService {
    constructor(private http: Http) { }
    private _heroesUrl = 'http://localhost:58056/api/products';  // URL to web api
    public instance: any = this;
    getHeroes() {

        return this.http.get(this._heroesUrl)
            .map(res => <product[]>res.json())
            .catch(this.handleError);
    }

    addProduct(): Observable<product> {
        let body = "{tp:'1'}";
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        return this.http.post(this._heroesUrl, body, options)
            .map(res => <product>res.json())
            .catch(this.handleError)
    }
    editHero(id:number,heroType: product): Observable<product> {
 
        //alert(id);
        let body = JSON.stringify(heroType);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        //alert(JSON.stringify(`${this._heroesUrl}/${id}`+ body+ options));
        return this.http.put(`${this._heroesUrl}/${id}`, body, options)
            .map(res => <product>res.json())
            .catch(this.handleError)
    }
    selectHero(heroType: number): Observable<product> {
        return this.http.get(`${this._heroesUrl}/${heroType}`)
            .map(res => <product>res.json())
            .catch(this.handleError)
    }
    
    getProducts(): Observable<product> {
        return this.http.get(this._heroesUrl)
            .map(res => <proview[]>res.json())
            .catch(this.handleError)
    }
    deleteHero(heroType: number): Observable<product> {
        //alert("incalled" + heroType);
        return this.http.delete(this._heroesUrl + "/" + heroType)
            .map(res => <product>res.json())
            .catch(this.handleError)
    }
    private handleError(error: Response) {
        var instance = this;
        console.error(error);
        var err = error.json();
        var sVerror = "";
        for (var key in err) {
            sVerror = sVerror + " " + err[key] +" | ";
        }
        console.log(sVerror);
        return Observable.throw(err.error || sVerror);
    }
   
}
