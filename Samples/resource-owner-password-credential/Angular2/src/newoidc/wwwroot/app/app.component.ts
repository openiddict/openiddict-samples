import {Component, Injectable, provide, Input, Output,EventEmitter,ViewChild} from '@angular/core';
import {RouteConfig, ROUTER_DIRECTIVES, ROUTER_PROVIDERS, } from '@angular/router-deprecated';
import {Http, Headers} from '@angular/http';
import {JwtHelper, AuthHttp, AuthConfig, AUTH_PROVIDERS} from 'angular2-jwt'
import {authorizeComponent} from './authorize/authorize-component'
import {auth_nav} from './authorize/authorize_nav'
import {userComponent} from './User/user-component'
import {welcome} from './welcome-component'
import { MODAL_DIRECTIVES, ModalComponent  } from 'ng2-bs3-modal/ng2-bs3-modal';
import {extauthorizeComponent} from './authorize/externalauth'
import {Router, RouterOutlet, ComponentInstruction} from '@angular/router-deprecated';
import {authervice} from './authorize/authoriza-service'
import {AuthLoginService} from './sharedservice'
import {componentProxyFactory} from './virtual-proxy';
import {geoComponent } from './geoLocation/geoComponent';
import {GeoSharedService } from './geoLocation/geo.shared';
import {geoService} from './geoLocation/geoservice';
import {MaterializeDirective} from "angular2-materialize";
import {categoryService}       from './category/categoryservice';
declare var $:any;
    @RouteConfig([
        { path: '/', name: 'Default', component: welcome, useAsDefault:true  },
        { path: '/login', name: 'Login', component: authorizeComponent},
        { path: '/dashboard', name: 'Dashboard', component: userComponent },
        { path: '/signin-oidc', name: 'Extauth', component: extauthorizeComponent },
        {
        path: '/Category',
        component: componentProxyFactory({
            path: '/app/category/category-component',
            provide: m => m.categoryComponent
        }), useAsDefault: false,
        name: 'Category'
    }
        ,
        {
            path: '/Product',
            component: componentProxyFactory({
                path: '/app/Product/productComponent',
                provide: m => m.ProductComponent
            }),
            name: 'Product'
        },
        {
            path: '/searchProduct',
            component: componentProxyFactory({
                path: '/app/Product/proListComponent',
                provide: m => m.ProductListComponent
            }),
            name: 'SearchProduct'
        }
        
])

        @Component({
            selector: 'body',
            templateUrl: 'app/app.component.html',
            directives: [MaterializeDirective,ROUTER_DIRECTIVES, authorizeComponent,  auth_nav, geoComponent],

            providers: [

            ]
        })

export class AppComponent {
       /* ngOnInit() {
            $('.button-collapse').sideNav();
        }
        shosn() {
            $('.button-collapse').sideNav();
            $('.button-collapse').sideNav('show');
        }
        hidsn() { $('.button-collapse').sideNav('hide'); }*/
    private log: boolean;
    private authodata;
    item: number = 0;
    subscription: any;
    constructor(public jwtHelper: JwtHelper,
        private _http: Http,
        private _parentRouter: Router,
        private Authentication: authervice,
        private od: AuthLoginService,
        private _geoService: geoService,
        private geosharedService: GeoSharedService
        ) {  
        this.subscription = this.od.getLoggedInEmitter()
            .subscribe(item => { this.authcheck(); });
        this.subscription = this.od.getLoggedOutEmitter()
            .subscribe(item => { this.logOut(); });
      //  $(".button-collapse").sideNav();
    }
    

    @ViewChild(authorizeComponent)
    public authorizeComponentRefer: authorizeComponent;

    public logOut() {
        this.authorizeComponentRefer.Logout();
    }
    public authcheck() {
        if (localStorage.getItem('auth_key') && !this.jwtHelper.isTokenExpired(localStorage.getItem('auth_key'))) { //validation for secure routes there are other ways too but i think its simplest
            this._parentRouter.navigate(['/Dashboard']);
            this.authorizeComponentRefer.logstatus();
        }
        else {
            this.authorizeComponentRefer.mopen();
        }
    }
  
}

