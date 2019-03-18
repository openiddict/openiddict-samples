import { autoinject } from "aurelia-framework";
import { RouterConfiguration, Router, NavigationInstruction } from "aurelia-router";
import { User, Log } from "oidc-client";
import { OpenIdConnect, OpenIdConnectRoles } from "aurelia-open-id-connect";

@autoinject
export class App {

  private router: Router;
  private user: User;

  constructor(private openIdConnect: OpenIdConnect) {
    this.openIdConnect.userManager.getUser().then((user) => {
      this.user = user;
    });
  }

  public configureRouter(routerConfiguration: RouterConfiguration, router: Router) {

    // switch from hash (#) to slash (/) navigation
    routerConfiguration.options.pushState = true;

    routerConfiguration.title = "OpenID Connect Implicit Flow Demo";

    // configure routes
    routerConfiguration.map([
      {
        moduleId: "home", name: "home", nav: true, route: [""],
        settings: { roles: [OpenIdConnectRoles.Everyone] }, title: "home",
      },
      // OpenId
      {
        name: "login", nav: false,
        navigationStrategy: (instruction: NavigationInstruction) => {

          this.openIdConnect.login(instruction);
        },
        route: "login",
        settings: { roles: [OpenIdConnectRoles.Anonymous] },
      },
      {
        name: "logout", nav: false,
        navigationStrategy: (instruction: NavigationInstruction) => {
          this.openIdConnect.logout(instruction);
        },
        route: "logout",
        settings: { roles: [OpenIdConnectRoles.Authenticated] },
      },
      // todo: Make user profile functional.
      // {
      //   moduleId: "user-profile", name: "profile", nav: true, route: "profile",
      //   settings: { roles: [OpenIdConnectRoles.Authorized] }, title: "profile",
      // },
      // todo: Make admin functional.
      // {
      //   moduleId: "admin", name: "admin", nav: true, route: "admin",
      //   settings: { roles: [OpenIdConnectRoles.Administrator] }, title: "admin",
      // },
    ]);

    this.openIdConnect.configure(routerConfiguration);
    this.router = router;
  }
}
