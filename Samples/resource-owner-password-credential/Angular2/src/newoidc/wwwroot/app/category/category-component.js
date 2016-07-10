System.register(['@angular/core', './category', './categoryService', "angular2-materialize", '@angular/common'], function(exports_1, context_1) {
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
    var core_1, category_1, categoryService_1, angular2_materialize_1, common_1;
    var categoryComponent;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (category_1_1) {
                category_1 = category_1_1;
            },
            function (categoryService_1_1) {
                categoryService_1 = categoryService_1_1;
            },
            function (angular2_materialize_1_1) {
                angular2_materialize_1 = angular2_materialize_1_1;
            },
            function (common_1_1) {
                common_1 = common_1_1;
            }],
        execute: function() {
            categoryComponent = (function () {
                function categoryComponent(_categoryService) {
                    this._categoryService = _categoryService;
                    this.edit = false;
                    this.editmodel = new category_1.category;
                    this.model = new category_1.category;
                }
                categoryComponent.prototype.ngOnInit = function () { this.getCategories(); };
                categoryComponent.prototype.getCategories = function () {
                    var _this = this;
                    this._categoryService.getCategories()
                        .subscribe(function (categories) { return _this.categories = categories; }, function (error) { return _this.errorMessage = error; });
                };
                categoryComponent.prototype.edit1category = function (category) {
                    this.edit = true;
                    this.editmodel = category;
                };
                categoryComponent.prototype.addcategory = function () {
                    var _this = this;
                    this._categoryService.addcategory(this.model)
                        .subscribe(function (category) { return _this.categories.push(category); }, function (error) { return _this.errorMessage = error; });
                };
                categoryComponent.prototype.editcategory = function () {
                    var _this = this;
                    this._categoryService.editcategory(this.editmodel)
                        .subscribe(function (data) {
                        _this.getCategories();
                    }, function (err) { return console.log(err); });
                };
                categoryComponent.prototype.deletecategory = function (category) {
                    var _this = this;
                    this._categoryService.deleteCategory(category.id)
                        .subscribe(function (data) {
                        _this.getCategories();
                    }, function (err) { return console.log(err); });
                };
                categoryComponent = __decorate([
                    core_1.Component({
                        selector: 'category-list',
                        template: "\n<div>\n<h3>Edit categories</h3>\n<form [hidden]=\"!edit\" (ng-submit)=\"editcategory()\" #cart=\"ngForm\" novalidate>\n            <div class=\"form-group\">\n <input type=\"text\" [hidden] class=\"form-control\" required\n                       [(ngModel)]=\"editmodel.id\"\n                       ngControl=\"CategoryTitle\" value=\"{{editmodel.id}}\" #CategoryTitle=\"ngForm\">\n                <label for=\"CategoryTitle\">Alter Ego</label>\n\n                <input type=\"text\" class=\"form-control\" required\n                       [(ngModel)]=\"editmodel.CategoryTitle\"\n                       ngControl=\"CategoryTitle\"  value=\"{{editmodel.CategoryTitle}}\" #CategoryTitle=\"ngForm\">\n                <div [hidden]=\"(CategoryTitle.touched && CategoryTitle.valid) || CategoryTitle.untouched\" class=\"//alert //alert-danger\">\n                    Country is required\n                </div>\n            </div>\n            <div class=\"form-group\">\n                <label for=\"power\">category Power</label>\n                <select class=\"form-control\"  materialize=\"material_select\"\n                        [(ngModel)]=\"editmodel.parentId\"  ngControl=\"parentId\" #parentId=\"ngForm\">\n                    \n                    <option *ngFor=\"let p of categories\" [value]=\"p.id\">{{p.CategoryTitle}}</option>\n                </select>\n            </div>\n            <button type=\"submit\" (click)=\"editcategory()\" class=\"btn btn-default\">\n                Submit\n            </button>\n        </form>\n</div>\n  <h3>categories:</h3>\n  <ul>\n    <li *ngFor=\"let category of categories\">\n      {{ category.CategoryTitle }},\n{{ category.id }},\n{{ category.parentId }}\n<button class=\"btn btn-danger\" (click)=\"deletecategory(category)\"><i class=\"fa fa-times\"></i> X </button>\n<button class=\"btn btn-success\" (click)=\"edit1category(category)\"><i class=\"fa fa-times\"></i> Edit </button>\n    </li>\n  </ul>\n  New category:\n{{city}}\n        <form (ng-submit)=\"addcategory()\" #cart=\"ngForm\" novalidate>\n            <div class=\"form-group\">\n                <label for=\"CategoryTitle\">Alter Ego</label>\n                <input type=\"text\" class=\"form-control\" required\n                       [(ngModel)]=\"model.CategoryTitle\"\n                       ngControl=\"CategoryTitle\" #CategoryTitle=\"ngForm\">\n                <div [hidden]=\"(CategoryTitle.touched && CategoryTitle.valid) || CategoryTitle.untouched\" class=\"//alert //alert-danger\">\n                    Country is required\n                </div>\n            </div>\n            <div class=\"form-group\">\n                <label for=\"power\">category Power</label>\n                <select class=\"form-control\"  materialize=\"material_select\"\n                        [(ngModel)]=\"model.parentId\" ngControl=\"parentId\" #parentId=\"ngForm\">\n                    \n                    <option *ngFor=\"let p of categories\" [value]=\"p.id\">{{p.CategoryTitle}}</option>\n                </select>\n            </div>\n            <button type=\"submit\" (click)=\"addcategory()\" class=\"btn btn-default\"\n                    [disabled]=\"!cart.form.valid\">\n                Submit\n            </button>\n        </form>\n  <div class=\"error\" *ngIf=\"errorMessage\">{{errorMessage}}</div>\n  ",
                        directives: [common_1.CORE_DIRECTIVES, common_1.FORM_DIRECTIVES, angular2_materialize_1.MaterializeDirective, common_1.NgForm, common_1.NgFormControl],
                        styles: ['.error {color:red;}']
                    }), 
                    __metadata('design:paramtypes', [categoryService_1.categoryService])
                ], categoryComponent);
                return categoryComponent;
            }());
            exports_1("categoryComponent", categoryComponent);
        }
    }
});
//# sourceMappingURL=category-component.js.map