import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RoleService } from '../roles.service';
import { UserService } from './user.service';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs/Observable';
import { User } from '../models/user';
import { AppState } from '../../app-store';

@Component({
    selector: 'app-user-list',
    templateUrl: 'user-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserListComponent implements OnInit {

    users: Observable<User[]>;

    constructor(private userService: UserService,
                private roleService: RoleService,
                private store: Store<AppState>
    ) { }

    ngOnInit(): void {
        this.users = this.store.select(state => state.users);

        this.getUsers();
    }

    getUsers() {
        this.userService.getUsers()
            .subscribe();
    }

    removeFromRole(userId: string, roleId: string) {
        this.roleService.removeFromRole(userId, roleId)
            .subscribe();
    }

}
