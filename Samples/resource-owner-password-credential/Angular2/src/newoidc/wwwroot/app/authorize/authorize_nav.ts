//this file is not needed actually its a bug with materialize -csss https://github.com/Dogfalo/materialize/issues/1593 and 
//thats why i am forced to created this file so everything can work smoother
import {Component, ViewChild, Input, ChangeDetectorRef } from '@angular/core'
import {AuthLoginService,SharedUserDetailsModel} from '../sharedservice'
@Component({
    selector: 'authorize-nav',
    template: `<li [hidden]="token.isLoggedIn"><button class="waves-effect waves-teal btn-flat" (click)="mopen()">Login</button></li>


<li [hidden]="!token.isLoggedIn">Welcome {{token.username}}</li>
<li [hidden]="!token.isLoggedIn"><button class="waves-effect waves-teal btn-flat" (click)="mclose()">Logout</button></li>`,
})
export class auth_nav {
    public token :SharedUserDetailsModel;
    
    constructor(private _AuthLoginService: AuthLoginService, private cdr: ChangeDetectorRef) { }
 
    ngOnInit() {
        this._AuthLoginService.space.subscribe((val) => {
            this.token = val;
            console.log(this.token);
          //  this.cdr.detectChanges();
        });
    }

    mopen() {
        this._AuthLoginService.emitNotLoggedIn();
        this.cdr.detectChanges();
    }
    mclose() {
        this._AuthLoginService.emitLogOut();
        this.cdr.detectChanges();
    }
}