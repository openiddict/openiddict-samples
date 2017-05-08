import {Injectable, EventEmitter} from '@angular/core';
import {Http, Response, Headers, RequestOptions} from '@angular/http';
import {LogModel, RegisterModel, TokenResult, Regresult} from './authorize-component'
import {Configuration} from '../app.constants'
import {Observable} from 'rxjs/Rx';
import {JwtHelper, tokenNotExpired} from 'angular2-jwt'
import 'rxjs/add/operator/map';
@Injectable()
export class Authservice {
    constructor(public jwtHelper: JwtHelper, private http: Http, private app: Configuration) { }

    //event emitter to open login box
    loginEmitter = new EventEmitter();
    emitNotLoggedIn() {
        this.loginEmitter.emit(null);
    }
    getLoggedInEmitter() {
        return this.loginEmitter;
    }

    private authUrl = this.app.Server;  // URL to web api
    private headers = new Headers({ 'Content-Type': 'application/X-www-form-urlencoded' });
    private jheaders = new Headers({ 'Content-Type': 'application/json' });
    private authheaders;
    private options = new RequestOptions({ headers: this.headers });
    private joptions = new RequestOptions({ headers: this.jheaders });
    private authoptions;// = new RequestOptions({ headers: this.authheaders });
    private tokenParams = "grant_type=password" +// password type reuqets with credentials read more on grant_types
    "&client_id=localApp" +
    "&resource=" + this.app.FileServer + // audience url . read more on docs /blog
    "&responseType=token" + // get token 
    "&scope=offline_access profile email roles"; // offline_access for refresh_token read more on docs / blog

    //common method to check auth status
    authenticated() {
        if (tokenNotExpired("auth_key")) {
            return true;
        } else {
            this.emitNotLoggedIn();
            return false;
        }
    }

    logout() {
        if (localStorage.getItem("auth_key")) {
            this.authheaders = new Headers({ 'Content-Type': 'application/x-www-form-urlencoded' });
            this.authoptions = new RequestOptions({ headers: this.authheaders });
            return this.http.post(this.authUrl + "/connect/revoke",
                "token=" + localStorage.getItem("refresh_key") + "&token_type=refresh_token", this.authoptions)
                .map(res => res)
                .catch(this.handleError);
        }
    }

    login(inputType: LogModel): Observable<TokenResult> {
        return this.http.post(this.authUrl + "/connect/token",
            this.tokenParams + "&username=" + inputType.username + "&password=" + inputType.password, this.options)
            .map(res => <TokenResult>res.json())
            .catch(this.handleError)
    }

    refreshLogin(): Observable<TokenResult> {
        let refreshParams = "grant_type=refresh_token" +
            "&client_id=localApp" +
            "&resource=" + this.app.FileServer +
            "&refresh_token=" + localStorage.getItem("refresh_key"); // get refresh token stored when logged in 
        return this.http.post(this.authUrl + "/connect/token", refreshParams, this.options)
            .map(res => <TokenResult>res.json())
            .catch(this.handleError)
    }

    register(inputType: RegisterModel): Observable<Regresult> {
        let body = JSON.stringify(inputType);
        return this.http.post(this.authUrl + "/api/account/register", body, this.joptions)
            .map(res => <Regresult>res.json())
            .catch(this.handleError)
    }

    private handleError(error: Response) {
        return Observable.throw(error || 'Server error');
    }
}
