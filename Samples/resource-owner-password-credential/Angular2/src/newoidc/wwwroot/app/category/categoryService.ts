import {Injectable}     from '@angular/core';
import {Http, Response,Headers, RequestOptions} from '@angular/http';
import { category}           from './category';
import {Observable}     from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class categoryService {
    categorymodel: Array<any>;
    constructor(private http: Http) { }
    private _categoriesUrl = 'http://localhost:58056/api/categories';  
    private headers = new Headers({ 'Content-Type': 'application/json' });
    private options = new RequestOptions({ headers: this.headers });

    getCategories() {

        return this.http.get(this._categoriesUrl)
            .map(res => <category[]>res.json())
            .catch(this.handleError);
    }

    addcategory(categoryType: category): Observable<category> {
        let body = JSON.stringify(categoryType);
        return this.http.post(this._categoriesUrl, body, this.options)
            .map(res => <category>res.json())
            .catch(this.handleError)
    }
    editcategory(categoryType: category): Observable<category> {
        let body = JSON.stringify(categoryType);
        return this.http.put(`${this._categoriesUrl}/` + categoryType.id, body, this.options)
            .map(res => <any>res.json())
            .catch(this.handleError)
    }
    selectcategory(categoryType: number): Observable<category> {
        return this.http.get(`${this._categoriesUrl}/categoryType`)
            .map(res => <category>res.json())
            .catch(this.handleError)
    }
    deleteCategory(categoryType: number): Observable<category> {
        return this.http.delete(this._categoriesUrl + "/" + categoryType)
            .map(res => <category>res.json())
            .catch(this.handleError)
    }
    
    private handleError(error: Response) {
         alert(error)    ;
    console.error(error);
        return Observable.throw(error.json().error || 'Server error');
    }
}
