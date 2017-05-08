/**
 * System configuration for Angular 2 samples
 * Adjust as necessary for your application needs.
 */
(function (global) {
    // map tells the System loader where to look for things
    var map = {
        'app': 'app', // 'dist',
        'rxjs': 'node_modules/rxjs',
        'rxjs/add/operator/*': 'node_modules/rxjs/add/operator/*',
        '@angular': 'node_modules/@angular',
        'angular2-jwt': 'node_modules/angular2-jwt/angular2-jwt.js',
        'ng2-bs3-modal': 'node_modules/ng2-bs3-modal',
     
    };
    // packages tells the System loader how to load when no filename and/or no extension
    var packages = {
        'app': { main: 'boot.js', defaultExtension: 'js' },
        'rxjs': { defaultExtension: 'js' },
        'angular2-jwt': { defaultExtension: 'js' },
        'ng2-bs3-modal': { defaultExtension: 'js' }
      
    };
    var ngPackageNames = [
      'common',
      'compiler',
      'core',
      'forms',
      'http',
      'platform-browser',
      'platform-browser-dynamic',
      'router',
      'router-deprecated',
      'upgrade',
    ];
    // Individual files (~300 requests):
    function packIndex(pkgName) {
        if (pkgName == "router") {
            packages['@angular/' + pkgName] = { main: 'index.js', defaultExtension: 'js' };
        } else {
            packages['@angular/' + pkgName] = { main: 'index.js', defaultExtension: 'js' };
        }
    }
    // Bundled (~40 requests):
    function packUmd(pkgName) {
        if (pkgName == "router") {
            packages['@angular/' + pkgName] = { main: 'index.js', defaultExtension: 'js' };
        } else {
            packages['@angular/' + pkgName] = { main: '/bundles/' + pkgName + '.umd.js', defaultExtension: 'js' };
        }
    }
    // Most environments should use UMD; some (Karma) need the individual index files
    var setPackageConfig = System.packageWithIndex ? packIndex : packUmd;
    // Add package entries for angular packages
    ngPackageNames.forEach(setPackageConfig);
    var config = {
        map: map,
        packages: packages
    };
    System.config(config);
})(this);
