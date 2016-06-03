/* global module */
module.exports = function (config) {
    'use strict';
    config.set({

        basePath: '.',
        singleRun: false,
        frameworks: ['jasmine'],
        reporters: ['spec'],
        browsers: ['PhantomJS'],
        //browsers: ['Chrome'],
        files: [
            'node_modules/es6-shim/es6-shim.min.js',
            'node_modules/systemjs/dist/system-polyfills.js',
            'node_modules/systemjs/dist/system.src.js',
            'node_modules/typescript/lib/typescript.js',
            'node_modules/zone.js/dist/zone.js',
            'node_modules/reflect-metadata/Reflect.js',
            'node_modules/jquery/dist/jquery.js',
            'node_modules/bootstrap/dist/js/bootstrap.js',
            'test/test-main.js',
            { pattern: 'node_modules/@angular/**/*.js', included: false, served: true },
            { pattern: 'node_modules/rxjs/**/*.js', included: false, served: true },
            { pattern: 'src/**/*.ts', included: false, served: true },
            { pattern: 'test/**/*.ts', included: false, served: true }
        ]
    });
};