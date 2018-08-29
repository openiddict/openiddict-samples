import { autoinject } from "aurelia-framework";
import { OpenIdConnect } from "aurelia-open-id-connect";
import { User } from "oidc-client";
import { HttpClient } from "aurelia-fetch-client";
import environment from "./environment";

@autoinject
export class Home {

    public isLoggedIn: boolean;

    private accessTokenExpiresIn: number;
    private currentTime: number;
    private resourceServerMessage: Array<string> = new Array<string>();
    private inMemoryUser: User;

    private get environmentAsJson(): string {
        return JSON.stringify(environment, null, 4);
    }

    private get userAsJson(): string {
        return JSON.stringify(this.inMemoryUser, null, 4);
    };

    constructor(private openIdConnect: OpenIdConnect, private httpClient: HttpClient) {
        this.setInMemoryUserOnUserLoadedEvent();
        this.setTimeUntilAccessTokenExpiry();
    }

    public attached() {

        // retrieve the user from storage
        this.openIdConnect.userManager.getUser()
            .then((user) => {
                if (typeof user === "undefined" || user === null) {
                    // if we do not have a user in localStorage (or sessionStorage)
                    // then we do not have an id_token to use as an id_token_hint;
                    // as a result, we cannot do a silent login, so we return; 
                    // alternatively, we could call `login` to 
                    // automatically redirect to the authorization server:
                    // this.openIdConnect.Login();
                    return;
                } else {
                    if (user.expired) {
                        // if we have an EXPIRED user in storage,
                        // then we can do a silent login.
                        this.loginSilent();
                    } else {
                        // if we have a non-expired user in storage, 
                        // then our app can use it for access.
                        this.inMemoryUser = user;
                    }

                }
            });
    }

    public queryResourceServer(serverNum: number, isPrivate: boolean) {
        let fetchUrl = this.getResourceServerUrl(serverNum, isPrivate);
        let fetchInit = this.getFetchInit();

        this.resourceServerMessage.splice(0, 0, `Fetching ${fetchUrl}\n`);
        this.resourceServerMessage.splice(1, 0, `\n`);

        this.httpClient.fetch(fetchUrl, fetchInit)
            .then((response) => response.ok ? response.text() : response.statusText)
            .then((data) => this.resourceServerMessage.splice(1, 0, `${data}\n`))
            .catch((err) => this.resourceServerMessage.splice(1, 0, `${err.message}\n`));
    }

    public loginSilent() {
        this.openIdConnect.loginSilent().catch((error) => {
            // if this is a timeout error,
            // then use a text editor to increase the silentRequestTimeout value,
            // that we configure in open-id-connect-configuration.ts
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
            this.currentTime = Math.round((new Date()).getTime() / 1000);

            if (typeof this.inMemoryUser !== "undefined" && this.inMemoryUser !== null) {
                this.accessTokenExpiresIn = this.inMemoryUser.expires_in;
            }

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
            ? environment.urls.resourceServer01
            : environment.urls.resourceServer02;

        let path = isPrivate ? "private" : "public";

        return `${leftPart}/api/${path}`;
    }
}
