import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';


export abstract class StorageBackend {
    abstract setItem(key: string, value: string): Observable<any>
    abstract getItem(key: string): Observable<any>
    abstract removeItem(key: string): Observable<any>
}

@Injectable()
export class Storage {
    constructor(private storageBackend: StorageBackend) {}

    setItem(key: string, value: any) {
        return this.storageBackend.setItem(key, JSON.stringify(value));
    }
    getItem(key: string) {
        return this.storageBackend.getItem(key)
            .map(item => JSON.parse(item));
    }
    removeItem(key: string) {
        return this.storageBackend.removeItem(key);
    }
}

@Injectable()
export class LocalStorageBackend implements StorageBackend {
    setItem(key: string, value: string) {
        return Observable.of( localStorage.setItem(key, value));
    }
    getItem(key: string) {
        return Observable.of( localStorage.getItem(key));
    }
    removeItem(key: string) {
        return Observable.of( localStorage.removeItem(key));
    }
}
