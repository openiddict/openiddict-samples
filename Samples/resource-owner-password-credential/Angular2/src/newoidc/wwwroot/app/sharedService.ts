import {Component, Injectable, provide, Input, Output, EventEmitter, ViewChild} from '@angular/core';
import {Subject} from 'rxjs/Subject';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
@Injectable()
export class AuthLoginService {
    // Share login logout functions throughout application
    cehckStatus = new EventEmitter();
    logout = new EventEmitter();
    constructor() { }
    emitNotLoggedIn() {
        this.cehckStatus.emit(null);
    }
    emitLogOut() {
        this.logout.emit(null);
    }
    getLoggedInEmitter() {
        return this.cehckStatus;
    }
    getLoggedOutEmitter() {
        return this.logout;
    }

    //share userdetails and logged in status throughout applicatoion
    public defaultshare = new SharedUserDetailsModel();
    public UserStatus = new Subject<SharedUserDetailsModel>();
    public space: Subject<SharedUserDetailsModel> = new BehaviorSubject<SharedUserDetailsModel>(this.defaultshare);
    searchTextStream$ = this.UserStatus.asObservable();

    broadcastTextChange(text: SharedUserDetailsModel) {
        this.space.next(text);
        this.UserStatus.next(text);
    }
}

export class SharedUserDetailsModel {
    //can add everything else needed to share
    username: string;
    isLoggedIn: boolean;
}