System.register(['@angular/core', './product', './productService', "angular2-materialize", '../category/categoryService', "angular2-materialize/dist/index", '../geoLocation/geo.shared', '@angular/common'], function(exports_1, context_1) {
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
    var core_1, product_1, productService_1, angular2_materialize_1, categoryService_1, Materialize, geo_shared_1, common_1;
    var ProductComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (product_1_1) {
                product_1 = product_1_1;
            },
            function (productService_1_1) {
                productService_1 = productService_1_1;
            },
            function (angular2_materialize_1_1) {
                angular2_materialize_1 = angular2_materialize_1_1;
            },
            function (categoryService_1_1) {
                categoryService_1 = categoryService_1_1;
            },
            function (Materialize_1) {
                Materialize = Materialize_1;
            },
            function (geo_shared_1_1) {
                geo_shared_1 = geo_shared_1_1;
            },
            function (common_1_1) {
                common_1 = common_1_1;
            }],
        execute: function() {
            ProductComponent = (function () {
                function ProductComponent(_heroes, _injector, _heroService, fb, shared, cdr) {
                    this._heroes = _heroes;
                    this._injector = _injector;
                    this._heroService = _heroService;
                    this.fb = fb;
                    this.shared = shared;
                    this.cdr = cdr;
                    this.instance = this;
                    this.street = new geo_shared_1.geoSharedModel;
                    this.model = new product_1.product;
                    this.isloading = true;
                    this.editmodel = new product_1.product;
                    var count;
                    var maxImageWidth = 500, maxImageHeight = 500;
                }
                ProductComponent.prototype.initDropzone = function (d) {
                    var pid = $('#pname').attr('pid');
                    this._dropzone = new d('div#myId', {
                        url: 'http://localhost:58056/home/SaveUploadedFile',
                        addRemoveLinks: true,
                        uploadMultiple: true,
                        headers: { "X-Hello": pid },
                        acceptedFiles: "image/jpeg,image/png,image/gif",
                        dictRemoveFile: "Remove",
                        maxFiles: 5,
                        init: function () {
                            this.on("maxfilesexceeded", function (file) {
                                Materialize.toast("Uplad Limit Reached");
                                this.removeFile(file);
                            });
                            this.on("thumbnail", function (file) {
                                alert(pid);
                                if (file.width < 500 || file.height < 500) {
                                    this.removeFile(file);
                                    Materialize.toast("Image should be larger than 500px x 500px");
                                }
                            });
                            this.on("success", function (file, response) {
                                if (response.code == "403") {
                                    this.removeFile(file);
                                    file.previewElement.classList.add("dz-error");
                                }
                                else {
                                    response.Message;
                                    $(file.previewTemplate).find(".dz-filename > span").text(response.Message);
                                }
                            });
                        }
                    });
                };
                ProductComponent.prototype.ngOnInit = function () {
                    this.getHeroes();
                    this.getNewProduct(0);
                    this.getlocation();
                    this.model.cat = 1;
                    this.form = this.fb.group({
                        ProductName: ['', common_1.Validators.required],
                        location: ['', common_1.Validators.required],
                        ProductDescription: ['', common_1.Validators.required],
                        cat: ['', common_1.Validators.required],
                        returnDeal: ['', common_1.Validators.required]
                    });
                };
                ProductComponent.prototype.getlocation = function () {
                    var _this = this;
                    this.shared.address.subscribe(function (val) {
                        console.log("behavi" + val.country + "," + val.city + "," + val.state);
                        _this.street = val;
                        _this.country = _this.street.country;
                        _this.state = _this.street.state;
                        _this.city = _this.street.city;
                        console.log(_this.country, _this.state, _this.city);
                    });
                };
                ProductComponent.prototype.getHeroes = function () {
                    var _this = this;
                    this._heroes.getCategories()
                        .subscribe(function (heroes) { return _this.heroes = heroes; }, function (error) { return _this.errorMessage = error; });
                };
                ProductComponent.prototype.addProduct = function () {
                    this.cdr.detectChanges();
                    console.log(this.country, this.state, this.city);
                };
                ProductComponent.prototype.getNewProduct = function (vals) {
                    var _this = this;
                    this._heroService.selectHero(vals)
                        .subscribe(function (hero) {
                        _this.model = hero;
                        _this.model.location = _this.city;
                        _this.cdr.detectChanges();
                        _this.isloading = false;
                        $('#New').material_select();
                        $('#New').val(_this.model.dealCategories);
                        System.import('/libs/dropzone/dist/dropzone.js').then(function (dz) { return _this.initDropzone(dz); });
                    }, function (error) { return _this.errorMessage = error; });
                };
                ProductComponent.prototype.getProduct = function (vals) {
                    var _this = this;
                    this._heroService.selectHero(vals)
                        .subscribe(function (hero) {
                        _this.model = hero;
                        _this.isloading = false;
                        $('#New').material_select();
                        var bval = _this.model.dealCategories;
                        $('#New').val(JSON.parse(_this.model.dealCategories));
                        $('#New').material_select();
                    }, function (error) { return _this.errorMessage = error; });
                };
                ProductComponent.prototype.editHero = function () {
                    var _this = this;
                    this.model.City = this.street.city;
                    this.model.State = this.street.state;
                    this.model.Country = this.street.country;
                    this.model.location = this.street.city;
                    this.model.active = 1;
                    this.model.New = $('input[name="group1"]:checked').val();
                    this.model.dealCategories = JSON.stringify($('#New').val());
                    this._heroService.editHero(this.model.Id, this.model)
                        .subscribe(function (data) {
                        _this.getProduct(_this.model.Id);
                        Materialize.toast("Product added successfully");
                    }, function (error) { return _this.errorMessage = error; });
                };
                ProductComponent.prototype.deleteHero = function (hero) {
                    var _this = this;
                    this._heroService.deleteHero(hero.id)
                        .subscribe(function (data) {
                        _this.getHeroes();
                    }, function (error) { return _this.errorMessage = error; });
                };
                ProductComponent = __decorate([
                    core_1.Component({
                        selector: 'productedit',
                        templateUrl: "/app/product/proedit.html",
                        directives: [angular2_materialize_1.MaterializeDirective],
                        styles: ['.error {color:red;}'],
                        providers: [productService_1.ProductService]
                    }), 
                    __metadata('design:paramtypes', [categoryService_1.categoryService, core_1.Injector, productService_1.ProductService, common_1.FormBuilder, geo_shared_1.GeoSharedService, core_1.ChangeDetectorRef])
                ], ProductComponent);
                return ProductComponent;
            }());
            exports_1("ProductComponent", ProductComponent);
        }
    }
});
//# sourceMappingURL=productComponent.js.map