import {Server as Karma} from 'karma';
import {CLIOptions} from 'aurelia-cli';
import * as path from 'path';

let karma = done => {
  new Karma({
    configFile: path.join(__dirname, '/../../test/karma.conf.js'),
    singleRun: !CLIOptions.hasFlag('watch'),
    autoWatch: CLIOptions.hasFlag('watch')
  }, function(exitCode) {
    console.log('Karma has exited with ' + exitCode)
    process.exit(exitCode)
  }).start();
};

export { karma as default };
