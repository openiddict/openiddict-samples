System.register(['@angular/core', '@angular/http', 'rxjs/Observable', 'rxjs/add/operator/map', 'rxjs/add/operator/catch'], function(exports_1, context_1) {
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
    var core_1, http_1, Observable_1;
    var categoryService;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (Observable_1_1) {
                Observable_1 = Observable_1_1;
            },
            function (_1) {},
            function (_2) {}],
        execute: function() {
            categoryService = (function () {
                function categoryService(http) {
                    this.http = http;
                    this._categoriesUrl = 'http://localhost:58056/api/categories';
                    this.headers = new http_1.Headers({ 'Content-Type': 'application/json' });
                    this.options = new http_1.RequestOptions({ headers: this.headers });
                }
                categoryService.prototype.getCategories = function () {
                    return this.http.get(this._categoriesUrl)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                categoryService.prototype.addcategory = function (categoryType) {
                    var body = JSON.stringify(categoryType);
                    return this.http.post(this._categoriesUrl, body, this.options)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                categoryService.prototype.editcategory = function (categoryType) {
                    var body = JSON.stringify(categoryType);
                    return this.http.put((this._categoriesUrl + "/") + categoryType.id, body, this.options)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                categoryService.prototype.selectcategory = function (categoryType) {
                    return this.http.get(this._categoriesUrl + "/categoryType")
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                categoryService.prototype.deleteCategory = function (categoryType) {
                    return this.http.delete(this._categoriesUrl + "/" + categoryType)
                        .map(function (res) { return res.json(); })
                        .catch(this.handleError);
                };
                categoryService.prototype.handleError = function (error) {
                    alert(error);
                    console.error(error);
                    return Observable_1.Observable.throw(error.json().error || 'Server error');
                };
                categoryService = __decorate([
                    core_1.Injectable(), 
                    __metadata('design:paramtypes', [http_1.Http])
                ], categoryService);
                return categoryService;
            }());
            exports_1("categoryService", categoryService);
        }
    }
});
//# sourceMappingURL=categoryservice.js.map