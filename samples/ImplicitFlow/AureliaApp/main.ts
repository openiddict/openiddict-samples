import { Aurelia } from "aurelia-framework";
import oidcConfig from "./open-id-connect-configuration";
import environment from "./environment";

// Configure Bluebird Promises.
// Note: You may want to use environment-specific configuration.
(<any>Promise).config({
  warnings: {
    wForgottenReturn: false,
  },
});

export function configure(aurelia: Aurelia) {

  if (environment.useHttps && location.protocol !== "https:") {
    // location.protocol is buggy in Firefox
    // see also http://stackoverflow.com/a/10036029
    location.href = "https:" + window.location.href.substring(window.location.protocol.length);
  }

  aurelia.use
    .standardConfiguration()
    .plugin("aurelia-open-id-connect", (callback) => callback(oidcConfig));

  aurelia.use.globalResources("./navbar.html");

  aurelia.use.developmentLogging();

  aurelia.start().then(() => aurelia.setRoot());
}
