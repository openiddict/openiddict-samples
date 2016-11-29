import {Injectable} from '@angular/core';
import { Observable }           from 'rxjs/Observable';
import { AuthHttp } from '../../core/auth-http/auth-http.service';

@Injectable()
export class RoleService {
    constructor(
                private authHttp: AuthHttp
    ) {}
    removeFromRole(userId: string, roleId: string): Observable<any> {
        return this.authHttp.post('/api/roles/removeFromRole', {userId, roleId});
    }
}
