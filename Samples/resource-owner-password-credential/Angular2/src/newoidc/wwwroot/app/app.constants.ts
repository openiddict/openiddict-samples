import { Injectable } from '@angular/core';

@Injectable()
export class Configuration {
    public Server: string = "http://localhost:58056";
    public FileServer: string = "http://localhost:58056/";
}