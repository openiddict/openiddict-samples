import { Injectable,  EventEmitter} from '@angular/core';
import {Subject} from 'rxjs/Subject';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
@Injectable()
export class AuthStatemanager {
    // Share login logout functions throughout application
    modalstatus = new EventEmitter();
    closemodal = new EventEmitter();
    emitopenmodal() {
        this.modalstatus.emit(null);
    }
    emitclosemodal() {
        this.closemodal.emit(null);
    }
    //share userdetails and logged in status throughout applicatoion
    public defaultshare = new SharedUserDetailsModel();
    public UserStatus: Subject<SharedUserDetailsModel> = new BehaviorSubject<SharedUserDetailsModel>(this.defaultshare);
    broadcastUserStatus(text: SharedUserDetailsModel) {
        this.UserStatus.next(text);
    }
}

export class SharedUserDetailsModel {
    //can add everything else needed to share
    username: string;
    isLoggedIn: boolean;
}