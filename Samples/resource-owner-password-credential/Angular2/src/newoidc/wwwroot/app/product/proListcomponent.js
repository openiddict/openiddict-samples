System.register(['@angular/core', './productService', '../models/productlist.model', "angular2-materialize"], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
        var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
        if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
        else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
        return c > 3 && r && Object.defineProperty(target, key, r), r;
    };
    var __metadata = (this && this.__metadata) || function (k, v) {
        if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
    };
    var core_1, productService_1, productlist_model_1, angular2_materialize_1;
    var ProductListComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (productService_1_1) {
                productService_1 = productService_1_1;
            },
            function (productlist_model_1_1) {
                productlist_model_1 = productlist_model_1_1;
            },
            function (angular2_materialize_1_1) {
                angular2_materialize_1 = angular2_materialize_1_1;
            }],
        execute: function() {
            ProductListComponent = (function () {
                function ProductListComponent(_heroes) {
                    this._heroes = _heroes;
                    this.isloading = true;
                    this.pad = new productlist_model_1.proview;
                }
                ProductListComponent.prototype.ngOnInit = function () {
                    this.getHeroes();
                };
                ProductListComponent.prototype.getHeroes = function () {
                    var _this = this;
                    this._heroes.getProducts()
                        .subscribe(function (heroes) {
                        _this.heroes = heroes;
                        _this.isloading = false;
                        console.log(heroes);
                    }, function (error) { return _this.errorMessage = error; });
                };
                ProductListComponent.prototype.showPro = function (p) {
                    this.pad = p;
                };
                ProductListComponent = __decorate([
                    core_1.Component({
                        selector: 'productedit',
                        template: "\n     <div class=\"loader\" [hidden]=\"!isloading\" style=\"text-align:center; padding-top:100px\" >\n            <div class=\"preloader-wrapper small active\">\n                <div class=\"spinner-layer spinner-green-only\">\n                    <div class=\"circle-clipper left\">\n                        <div class=\"circle\"></div>\n                    </div>\n                    <div class=\"gap-patch\">\n                        <div class=\"circle\">\n                        </div>\n                    </div>\n                    <div class=\"circle-clipper right\">\n                        <div class=\"circle\"></div>\n                    </div>\n                </div>\n            </div>\n        </div>\n    <div class=\"row\">\n      <div  *ngFor=\"let p of heroes\" class=\"col s4\">\n     <div class=\"card \">\n    <div class=\"card-image waves-effect waves-block waves-light\">\n      <img class=\"activator\" alt=\"{{p.picturefirst}}\" src=\"http://localhost:58056/wallimages/imagepath/{{p.picturefirst}}\">\n    </div>\n    <div class=\"card-content\">\n      <span class=\"card-title activator grey-text text-darken-4\">{{p.ProductName}}<i class=\"material-icons right\">more_vert</i></span>\n      <p><a href=\"#\">This is a link</a></p>\n      <div>\n      <div *ngFor=\"let hero of p.Categories\" class=\"chip\">\n      {{hero.CategoryTitle}}\n      </div>\n      </div>\n    </div>\n    <div class=\"card-reveal\">\n      <span class=\"card-title grey-text text-darken-4\">{{p.ProductName}}<i class=\"material-icons right\">close</i></span>\n      <p>{{p.ProductDescription}}</p>\n        <p>{{p.returnDeal}}</p>\n    </div>\n     <div class=\"card-action\">\n            <a materialize=\"leanModal\" [materializeParams]=\"[{dismissible: true}]\" (click)=\"showPro(p)\" class=\"waves-effect waves-light btn modal-trigger\" href=\"#modal1\">Modal</a>\n                  <a href=\"#\">This is a link</a>\n            </div>\n  </div>\n  </div>\n  </div>\n  \n   <!-- Modal Structure -->\n  <div id=\"modal1\" style=\"min-height:90%\" class=\"modal bottom-sheet\">\n    <div class=\"modal-content\">\n      <h4>{{pad.ProductName}}</h4>\n    </div>\n    \n    <div class=\"row\">\n      <div class=\"card col s5\">\n    <div class=\"card-image waves-effect waves-block waves-light\">\n     <img *ngIf=\"pad.picturefirst\"\n    class=\"activator\"src=\"http://localhost:58056/wallimages/imagepath/{{pad.picturefirst}}\">\n  <img *ngIf=\"!pad.picturefirst\" \n    class=\"activator\"src=\"images/office.jpg\">\n    \n    </div>\n    <div class=\"card-content\">\n      <span class=\"card-title activator grey-text text-darken-4\">{{pad.ProductDescription}}<i class=\"material-icons right\">more_vert</i></span>\n      <p><a href=\"#\">This is a link</a></p>\n      <div>\n      <div *ngFor=\"let hero of pad.Categories\" class=\"chip\">\n      {{hero.CategoryTitle}}\n      </div>\n      </div>\n    </div>\n     <div class=\"card-action\">\n            <a materialize=\"leanModal\" [materializeParams]=\"[{dismissible: true}]\" (click)=\"showPro(p)\" class=\"waves-effect waves-light btn modal-trigger\" href=\"#modal1\">Modal</a>\n                  <a href=\"#\">This is a link</a>\n            </div>\n  </div>\n   \n    <div class=\"col s7\">\n   <h3>Add your products to Swapping cart</h3>\n                           <ul class=\"collection\">\n                            <li  *ngFor=\"let p of heroes\" class=\"collection-item avatar\">\n                               <img *ngIf=\"pad.picturefirst\"\n                                class=\"circle\"src=\"http://localhost:58056/wallimages/imagepath/{{pad.picturefirst}}\">\n                              <img *ngIf=\"!pad.picturefirst\" \n                                class=\"circle\"src=\"images/office.jpg\">\n                         \n                              <span class=\"title\">{{p.ProductName}}</span>\n                              <p>{{p.catName}} <br>\n                               {{p.AddDate}}\n                              </p>\n                              <p class=\"btn btn-primary secondary-content\" onclick=\"Removelink(this);\" data-id=\"p.ProductId\"> <i class=\"material-icons\">shopping_cart</i>    Add to Cart  </p>\n \n                            </li>\n                           </ul>\n                           \n                              <div class=\"form-group\">\n\n\n                        <div class=\"row\">\n                            <div class=\"input-field col s12\">\n                                <textarea class=\"materialize-textarea validate\" \n                                          name=\"returnDeal\"></textarea>\n                                <label for=\"icon_prefix2\" data-error=\"wrong\" data-success=\"\">What you expect in return</label>\n                            </div>\n                        </div>\n                  <button type=\"submit\"  class=\"waves-effect btn waves-light\">Submit</button>\n                    </div>\n    </div>\n     <div class=\"modal-footer\">\n      <button  onClick=\" $('#modal1').closeModal();\" class=\" modal-action modal-close waves-effect waves-green btn-flat\">Close</button>\n    </div>\n    \n    </div>\n  </div>\n    ",
                        directives: [angular2_materialize_1.MaterializeDirective],
                        styles: ['.error {color:red;}'],
                        providers: [productService_1.ProductService]
                    }), 
                    __metadata('design:paramtypes', [productService_1.ProductService])
                ], ProductListComponent);
                return ProductListComponent;
            }());
            exports_1("ProductListComponent", ProductListComponent);
        }
    }
});
//# sourceMappingURL=proListcomponent.js.map