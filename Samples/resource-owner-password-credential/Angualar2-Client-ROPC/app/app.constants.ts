import { Injectable } from '@angular/core';

@Injectable()
export class Configuration {
    public Server: string = "http://localhost:52606";
    public FileServer: string = "http://localhost:3000";
}