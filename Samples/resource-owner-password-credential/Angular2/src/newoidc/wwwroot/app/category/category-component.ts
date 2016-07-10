import {Component, OnInit} from '@angular/core';
import { category}              from './category';
import {categoryService}       from './categoryService';
import {Observable} from 'rxjs/Rx';
import {MaterializeDirective} from "angular2-materialize";
import {Control, FORM_DIRECTIVES, CORE_DIRECTIVES, NgFor, NgIf, NgFormControl, NgForm} from '@angular/common';
@Component({
    selector: 'category-list',
    template: `
<div>
<h3>Edit categories</h3>
<form [hidden]="!edit" (ng-submit)="editcategory()" #cart="ngForm" novalidate>
            <div class="form-group">
 <input type="text" [hidden] class="form-control" required
                       [(ngModel)]="editmodel.id"
                       ngControl="CategoryTitle" value="{{editmodel.id}}" #CategoryTitle="ngForm">
                <label for="CategoryTitle">Alter Ego</label>

                <input type="text" class="form-control" required
                       [(ngModel)]="editmodel.CategoryTitle"
                       ngControl="CategoryTitle"  value="{{editmodel.CategoryTitle}}" #CategoryTitle="ngForm">
                <div [hidden]="(CategoryTitle.touched && CategoryTitle.valid) || CategoryTitle.untouched" class="//alert //alert-danger">
                    Country is required
                </div>
            </div>
            <div class="form-group">
                <label for="power">category Power</label>
                <select class="form-control"  materialize="material_select"
                        [(ngModel)]="editmodel.parentId"  ngControl="parentId" #parentId="ngForm">
                    
                    <option *ngFor="let p of categories" [value]="p.id">{{p.CategoryTitle}}</option>
                </select>
            </div>
            <button type="submit" (click)="editcategory()" class="btn btn-default">
                Submit
            </button>
        </form>
</div>
  <h3>categories:</h3>
  <ul>
    <li *ngFor="let category of categories">
      {{ category.CategoryTitle }},
{{ category.id }},
{{ category.parentId }}
<button class="btn btn-danger" (click)="deletecategory(category)"><i class="fa fa-times"></i> X </button>
<button class="btn btn-success" (click)="edit1category(category)"><i class="fa fa-times"></i> Edit </button>
    </li>
  </ul>
  New category:
{{city}}
        <form (ng-submit)="addcategory()" #cart="ngForm" novalidate>
            <div class="form-group">
                <label for="CategoryTitle">Alter Ego</label>
                <input type="text" class="form-control" required
                       [(ngModel)]="model.CategoryTitle"
                       ngControl="CategoryTitle" #CategoryTitle="ngForm">
                <div [hidden]="(CategoryTitle.touched && CategoryTitle.valid) || CategoryTitle.untouched" class="//alert //alert-danger">
                    Country is required
                </div>
            </div>
            <div class="form-group">
                <label for="power">category Power</label>
                <select class="form-control"  materialize="material_select"
                        [(ngModel)]="model.parentId" ngControl="parentId" #parentId="ngForm">
                    
                    <option *ngFor="let p of categories" [value]="p.id">{{p.CategoryTitle}}</option>
                </select>
            </div>
            <button type="submit" (click)="addcategory()" class="btn btn-default"
                    [disabled]="!cart.form.valid">
                Submit
            </button>
        </form>
  <div class="error" *ngIf="errorMessage">{{errorMessage}}</div>
  `,
    directives: [CORE_DIRECTIVES, FORM_DIRECTIVES, MaterializeDirective, NgForm, NgFormControl],
    styles: ['.error {color:red;}']

})
export class categoryComponent{

    constructor(private _categoryService: categoryService) { }

    errorMessage: string;
    public categories;
    public edit: boolean = false;
    ngOnInit() { this.getCategories(); }
    editmodel = new category;

    getCategories() {
        this._categoryService.getCategories()
            .subscribe(
            categories => this.categories = categories,
            error => this.errorMessage = <any>error);
    }
    edit1category(category) {
        this.edit = true;
        this.editmodel = category;
    }
    model = new category;
    addcategory() {

        this._categoryService.addcategory(this.model)
            .subscribe(
            category => this.categories.push(category),
            error => this.errorMessage = <any>error);
    }
    editcategory() {
        //alert(JSON.stringify(this.editmodel));
        this._categoryService.editcategory(this.editmodel)
            .subscribe(
            data => {
                this.getCategories()
            }, err => console.log(err));
    }
    deletecategory(category) {
        //alert("out called" + category.id);
        this._categoryService.deleteCategory(category.id)
            .subscribe(data => {
                this.getCategories()
            }, err => console.log(err));


    }
}

