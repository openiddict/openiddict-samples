import {Component, Output, Input, OnInit, OnChanges, Injector, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { product}              from './product';
import {ProductService}       from './productService';
import {MaterializeDirective} from "angular2-materialize";
import {categoryService}       from '../category/categoryService';
import {geoService} from '../geoLocation/geoservice';
import {Observable} from 'rxjs/Rx';
import * as Materialize from "angular2-materialize/dist/index";
import {Subscription}   from 'rxjs/Subscription';
import {GeoSharedService, geoSharedModel, geoResult} from '../geoLocation/geo.shared';
import {Control, FORM_DIRECTIVES, CORE_DIRECTIVES, Validators, ControlGroup, NgFormModel, FormBuilder, NgFor, NgIf, NgFormControl, NgForm} from '@angular/common';
declare var google: any;
declare var $: any;
declare var System: any;
@Component({
    selector: 'productedit',
    templateUrl: "/app/product/proedit.html",
    directives: [MaterializeDirective],
    styles: ['.error {color:red;}'],
    providers: [ProductService]

})
export class ProductComponent {
    subscription: Subscription;
    public maps: any;
    public genom: geoService;
    constructor(private _heroes: categoryService, private _injector: Injector,
        private _heroService: ProductService, private fb: FormBuilder, private shared: GeoSharedService, private cdr: ChangeDetectorRef) {
    
        var count;
        var maxImageWidth = 500,
            maxImageHeight = 500;


    }

    initDropzone(d: any) {

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
                    
                    // Do the dimension checks you want to do
                    if (file.width < 500 || file.height < 500) {
                        this.removeFile(file);
                        Materialize.toast("Image should be larger than 500px x 500px");
                    }

                });
                this.on("success", function (file, response) {
                    if (response.code == "403") {
                        this.removeFile(file);
                        file.previewElement.classList.add("dz-error");
                    } else {
                        response.Message;
                        $(file.previewTemplate).find(".dz-filename > span").text(response.Message);
                        // $(file.previewTemplate).find(".dz-remove").attr("onclick", "fr(this,'" + response.Message + "');");

                    }

                });
            }
        });
    }

    public instance = this;
    public country: string;
    public state: string;
    public city: string;
    public street = new geoSharedModel;
    errorMessage: string;
    public heroes;
    public model = new product;
    public isloading: boolean = true;
    public input: any;
    public autocomplete: any;
    private _dropzone: Dropzone;


    form: ControlGroup;


    ngOnInit() {
        this.getHeroes();
        this.getNewProduct(0);
        this.getlocation();
        this.model.cat = 1;
        this.form = this.fb.group({
            ProductName: ['', Validators.required],
            location: ['', Validators.required],
            ProductDescription: ['', Validators.required],
            cat: ['', Validators.required],
            returnDeal: ['', Validators.required]
        });
  
    }

    getlocation() {
        this.shared.address.subscribe((val) => {
            console.log("behavi" + val.country + "," + val.city + "," + val.state);
            this.street = val;
            this.country = this.street.country;
            this.state = this.street.state;
            this.city = this.street.city
            console.log(this.country, this.state, this.city);
        });

    }
    editmodel = new product;
    getHeroes() {
        this._heroes.getCategories()
            .subscribe(
            heroes => this.heroes = heroes,
            error => this.errorMessage = <any>error);
    }


    addProduct() {
        this.cdr.detectChanges();
        console.log(this.country, this.state, this.city);
    }

    getNewProduct(vals: number) {
        this._heroService.selectHero(vals)
            .subscribe(
            hero => {
                this.model = hero;
                this.model.location = this.city;
                this.cdr.detectChanges();
                this.isloading = false;
                $('#New').material_select();
                $('#New').val(this.model.dealCategories);
                  System.import('/libs/dropzone/dist/dropzone.js').then((dz) => this.initDropzone(dz));
                //alert(this.model.dealCategories);
            },
            error => this.errorMessage = <any>error);
    }
    getProduct(vals: number) {
        this._heroService.selectHero(vals)
            .subscribe(
            hero => {
                this.model = hero;
                this.isloading = false;

                $('#New').material_select();
                var bval: Array<any> = [] = this.model.dealCategories;
                //alert(bval);
                $('#New').val(JSON.parse(this.model.dealCategories));
                $('#New').material_select();

            },
            error => this.errorMessage = <any>error);
    }



    editHero() {
        this.model.City = this.street.city;
        this.model.State = this.street.state;
        this.model.Country = this.street.country;
        this.model.location = this.street.city;
        this.model.active=1;
        this.model.New = $('input[name="group1"]:checked').val();
        this.model.dealCategories = JSON.stringify($('#New').val());
        //alert(JSON.stringify(this.model));
        this._heroService.editHero(this.model.Id, this.model)
            .subscribe(
            data => {
                this.getProduct(this.model.Id);
                Materialize.toast("Product added successfully");
            }, error => this.errorMessage = <any>error);
    }

    deleteHero(hero) {
        //alert("out called" + hero.id);
        this._heroService.deleteHero(hero.id)
            .subscribe(data => {
                this.getHeroes()
            }, error => this.errorMessage = <any>error);


    }


}

