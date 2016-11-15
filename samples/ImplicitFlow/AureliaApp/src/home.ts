import { autoinject } from "aurelia-framework";
import { OpenIdConnect, User } from "aurelia-open-id-connect";
import { HttpClient } from "aurelia-fetch-client";

@autoinject
export class Home {

    private accessTokenExpiresIn: number;
    private currentTime: number;
    private resourceServerMessage: Array<string> = new Array<string>();
    private inMemoryUser: User;
    private isLoggedIn: boolean;

    private get userAsJson(): string {
        return JSON.stringify(this.inMemoryUser, null, 4);
    };

    constructor(private openIdConnect: OpenIdConnect, private httpClient: HttpClient) {
        this.setInMemoryUserOnUserLoadedEvent();
        this.setTimeUntilAccessTokenExpiry();
    }

    private attached() {

        // retrieve the user from storage
        this.openIdConnect.userManager.getUser()
            .then((user) => {
                if (typeof user === "undefined" || user === null) {
                    // we do not have a user in storage
                    // so we cannot do loginSilent, because we
                    // do not have an id_token to use as an id_token_hint
                    return;
                }
                if (user.expired) {
                    // we have an expired user in storage
                    this.loginSilent();
                } else {
                    // we have a non-expired user in storage
                    this.inMemoryUser = user;
                }
            });
    }

    private loginSilent() {
        this.openIdConnect.LoginSilent().catch((error) => {
            // if this is a timeout error 
            // then increase the silentRequestTimeout value
            console.log(error);
        });
    }

    private setInMemoryUserOnUserLoadedEvent() {
        this.openIdConnect.userManager.events.addUserLoaded(() =>
            this.openIdConnect.userManager.getUser().then((user: User) =>
                this.inMemoryUser = user));
    }

    private setTimeUntilAccessTokenExpiry() {
        setInterval(() => {
            if (typeof this.inMemoryUser === "undefined" || this.inMemoryUser == null) {
                return;
            }

            this.accessTokenExpiresIn = this.inMemoryUser.expires_in;
            this.currentTime = Math.round((new Date()).getTime() / 1000);
        }, 1000);
    }

    private getFetchInit(): RequestInit {
        let fetchInit = <RequestInit>{
            headers: new Headers()
        };

        if (this.inMemoryUser != null) {
            (<Headers>fetchInit.headers)
                .append("Authorization", `Bearer ${this.inMemoryUser.access_token}`);
        }

        return fetchInit;
    }

    private getResourceServerUrl(serverNum: number, isPrivate: boolean) {
        let leftPart = serverNum === 1
            ? "http://localhost:5001"
            : "http://localhost:5002";

        let path = isPrivate ? "private" : "public";

        return `${leftPart}/api/${path}`;
    }

    private queryResourceServer(serverNum: number, isPrivate: boolean) {
        let fetchUrl = this.getResourceServerUrl(serverNum, isPrivate);
        let fetchInit = this.getFetchInit();

        this.resourceServerMessage.splice(0, 0, `Fetching ${fetchUrl}\n`);
        this.httpClient.fetch(fetchUrl, fetchInit)
            .then((response) => response.ok ? response.text() : response.statusText)
            .then((data) => this.resourceServerMessage.splice(1, 0, `${data}\n\n`))
            .catch((err) => this.resourceServerMessage.splice(1, 0, `${err.message}\n\n`));
    }
}
