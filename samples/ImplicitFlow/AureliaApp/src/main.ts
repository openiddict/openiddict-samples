import { Aurelia } from "aurelia-framework";
import oidcConfig from "./open-id-connect-configuration";

// Configure Bluebird Promises.
// Note: You may want to use environment-specific configuration.
(<any>Promise).config({
  warnings: {
    wForgottenReturn: false,
  },
});

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .plugin("aurelia-open-id-connect", (callback) => callback(oidcConfig));

  aurelia.use.developmentLogging();

  aurelia.start().then(() => aurelia.setRoot());
}
