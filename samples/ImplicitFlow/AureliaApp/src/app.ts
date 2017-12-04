import { autoinject } from "aurelia-framework";
import { RouterConfiguration, Router } from "aurelia-router";
import { User, Log } from "oidc-client";
import { OpenIdConnect } from "aurelia-open-id-connect";

@autoinject
export class App {

  private router: Router;
  private user: User;

  constructor(private openIdConnect: OpenIdConnect) {
    this.openIdConnect.observeUser((user: User) => this.user = user);
  }

  public configureRouter(routerConfiguration: RouterConfiguration, router: Router) {

    // switch from hash (#) to slash (/) navigation
    routerConfiguration.options.pushState = true;
    routerConfiguration.title = "OpenID Connect Implicit Flow Demo";

    // configure routes
    routerConfiguration.map([
      {
        moduleId: "home",
        name: "home",
        route: ["", "home"],
        title: "home",
      },
    ]);

    this.openIdConnect.configure(routerConfiguration);
    this.router = router;
  }
}
