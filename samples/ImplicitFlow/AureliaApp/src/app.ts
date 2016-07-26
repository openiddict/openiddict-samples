import { autoinject } from "aurelia-framework";
import { RouterConfiguration, Router } from "aurelia-router";
import { OpenIdConnect } from "aurelia-open-id-connect";

@autoinject
export class App {

    constructor(private openIdConnect: OpenIdConnect) { }

    public configureRouter(routerConfiguration: RouterConfiguration, router: Router) {

        // switch from hash (#) to slash (/) navigation
        routerConfiguration.options.pushState = true;

        // configure routes
        routerConfiguration.map([
            { moduleId: "login", route: ["", "login"] },
        ]);

        this.openIdConnect.Configure(routerConfiguration);
    }
}
