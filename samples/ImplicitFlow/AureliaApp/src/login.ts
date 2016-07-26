import { autoinject } from "aurelia-framework";
import { OpenIdConnect, User } from "aurelia-open-id-connect";
import { HttpClient } from "aurelia-fetch-client";

@autoinject
export class Login {

    private authorizationServerMessage: string;
    private resourceServerMessage: string;
    private isLoggedIn: boolean = false;

    constructor(private openIdConnect: OpenIdConnect, private httpClient: HttpClient) {
        this.openIdConnect.UserManager.getUser().then((user: User) => {

            console.log(user);
            if (user === null || user === undefined) {
                return;
            }

            console.log("logged in");
            this.isLoggedIn = true;
            this.authorizationServerMessage = JSON.stringify(user, null, 4);

            console.log("login constructor done");
        });
    }

    private login() {
        this.openIdConnect.Login();
    }

    private logout() {
        this.openIdConnect.Logout();
    }

    private queryResourceServer(serverNum: number, isPrivate: boolean) {

        this.openIdConnect.UserManager.getUser().then((user: User) => {

            let url = this.getUrl(serverNum, isPrivate);

            this.resourceServerMessage = `Fetching ${url}`;

            let fetchInit = {
                headers: new Headers(),
            };

            if (user !== null && user !== undefined) {
                fetchInit.headers.append("Authorization", `Bearer ${user.access_token}`);
            }

            this.httpClient.fetch(url, fetchInit)
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        return response.statusText;
                    }
                })
                .then((data) => {
                    this.resourceServerMessage = `${serverNum}: ${data}`;
                })
                .catch((err) => {
                    this.resourceServerMessage = `${serverNum}: ${err.message}`;
                });
        });
    }

    private getUrl(serverNum: number, isPrivate: boolean) {

        let leftPart = serverNum === 1
            ? "http://localhost:5001"
            : "http://localhost:5002";

        let path = isPrivate ? "private" : "public";

        return `${leftPart}/api/${path}`;
    }
}
