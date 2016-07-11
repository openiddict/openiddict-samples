import {Injectable}     from '@angular/core';
import {Http, Response, Headers, RequestOptions} from '@angular/http';
import {logModel, registerModel, extprovider,token,Regresult} from './authorize-component'
import {Configuration} from '../app.constants'
import {Observable} from 'rxjs/Rx';
import 'rxjs/add/operator/map';
@Injectable()
export class authervice {
    constructor(private http: Http, private app: Configuration) { }
  
    private _authUrl = this.app.Server;  // URL to web api
    private headers = new Headers({ 'Content-Type': 'application/X-www-form-urlencoded' });
    private jheaders = new Headers({ 'Content-Type': 'application/json' });
    private authheaders;
    private options = new RequestOptions({ headers: this.headers });
    private joptions = new RequestOptions({ headers: this.jheaders });
    private Authoptions;// = new RequestOptions({ headers: this.authheaders });
    private tokenParams = "grant_type=password" +
                          "&client_id=localApp" +// password type reuqets with credentials read more on grant_types
                          "&resource="+ this.app.Server + // audience url . read more on docs /blog
                          "&responseType=token" + // get token 
                          "&scope=offline_access profile email roles"; // offline_access for refresh_token read more on docs / blog

    private refreshParams;
                           

    getUserInfo() {
        if (localStorage.getItem("auth_key")) {
            this.authheaders = new Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
              this.Authoptions = new RequestOptions({ headers: this.authheaders });
              return this.http.get(this._authUrl + "/api/Resource", this.Authoptions)
                .map(res => res.json())
                .catch(this.handleError);
        }
    }

    logout() {
        if (localStorage.getItem("auth_key")) {
            this.authheaders = new Headers({ "Authorization": "Bearer " + localStorage.getItem("auth_key") });
              this.Authoptions = new RequestOptions({ headers: this.authheaders });
            return this.http.get(this._authUrl + "/connect/logout", this.Authoptions)
                .map(res => res)
                .catch(this.handleError);
        }
    }

    Login(inputType: logModel): Observable<token> {
        return this.http.post(this._authUrl + "/connect/token" , this.tokenParams + "&username=" + inputType.username + "&password=" + inputType.password, this.options)
            .map(res => <token>res.json())
            .catch(this.handleError)
    }

    refreshLogin(): Observable<token> {
        this.refreshParams == "grant_type=refresh_token" +
            "&client_id=localApp" +// password type reuqets with credentials read more on grant_types
                         // refresh tokens when access_tokens are expired simply renew em!
            "&resource=" + this.app.Server  +
            "&refresh_token=" + localStorage.getItem("refresh_key"); // get refresh token stored when logged in 
        return this.http.post(this._authUrl + "/connect/token", this.refreshParams, this.options)
            .map(res => <token>res.json())
            .catch(this.handleError)
    }

    Register(inputType: registerModel): Observable<Regresult> {
        let body = JSON.stringify(inputType);
        return this.http.post(this._authUrl + "/api/account/register", body, this.joptions)
            .map(res => <Regresult>res.json())
            .catch(this.handleError)
    }
   
    private handleError(error: Response) {
        return Observable.throw(error || 'Server error');
    }
}
