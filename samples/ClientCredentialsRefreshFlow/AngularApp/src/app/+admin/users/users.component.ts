import {Component, Input} from '@angular/core';
import {User} from '../models/user';

@Component({
    selector: 'app-user',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.css']
})
export class UsersComponent {
    @Input() user: User;
}
