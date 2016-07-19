import {Injectable} from '@angular/core';
import {Http, Response, Headers, RequestOptions} from '@angular/http';
import {Configuration} from '../app.constants'
import {Observable} from 'rxjs/Rx';
import 'rxjs/add/operator/map';
@Injectable()
export class ResourceService {
    constructor(private http: Http, private app: Configuration) { }
  
    private _authUrl = this.app.Server;  // URL to web api
  
    getUserInfo() {
        if (localStorage.getItem("auth_key")) {
            let authheaders = new Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
            let Authoptions = new RequestOptions({ headers: authheaders });
            return this.http.get(this._authUrl + "/api/Resource", Authoptions)
            .map(res => res.json())
            .catch(this.handleError);
        }
    }
   
    private handleError(error: Response) {
        return Observable.throw(error || 'Server error');
    }
}
