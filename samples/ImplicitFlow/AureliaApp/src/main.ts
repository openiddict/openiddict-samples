import { Aurelia } from 'aurelia-framework'
import { OpenIdConnectConfiguration } from "aurelia-open-id-connect";
import environment from './environment';
import oidcConfig from "./open-id-connect-configuration";
// import oidcConfig from "./open-id-connect-configuration-auth0";
// import oidcConfig from "./open-id-connect-configuration-azure";
// import oidcConfig from "./open-id-connect-configuration-identity-server";

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .feature('resources');

  // Simplified configuration as of v0.19.0.
  aurelia.use.plugin("aurelia-open-id-connect", () => oidcConfig);

  if (environment.debug) {
    aurelia.use.developmentLogging();
  }

  if (environment.testing) {
    aurelia.use.plugin('aurelia-testing');
  }

  aurelia.start().then(() => aurelia.setRoot());
}
