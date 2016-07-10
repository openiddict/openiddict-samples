import {Component} from '@angular/core'
import {ProductService}       from './productService';
import{proview} from '../models/productlist.model';
import {MaterializeDirective} from "angular2-materialize";
@Component({
    selector: 'productedit',
    template: `
     <div class="loader" [hidden]="!isloading" style="text-align:center; padding-top:100px" >
            <div class="preloader-wrapper small active">
                <div class="spinner-layer spinner-green-only">
                    <div class="circle-clipper left">
                        <div class="circle"></div>
                    </div>
                    <div class="gap-patch">
                        <div class="circle">
                        </div>
                    </div>
                    <div class="circle-clipper right">
                        <div class="circle"></div>
                    </div>
                </div>
            </div>
        </div>
    <div class="row">
      <div  *ngFor="let p of heroes" class="col s4">
     <div class="card ">
    <div class="card-image waves-effect waves-block waves-light">
      <img class="activator" alt="{{p.picturefirst}}" src="http://localhost:58056/wallimages/imagepath/{{p.picturefirst}}">
    </div>
    <div class="card-content">
      <span class="card-title activator grey-text text-darken-4">{{p.ProductName}}<i class="material-icons right">more_vert</i></span>
      <p><a href="#">This is a link</a></p>
      <div>
      <div *ngFor="let hero of p.Categories" class="chip">
      {{hero.CategoryTitle}}
      </div>
      </div>
    </div>
    <div class="card-reveal">
      <span class="card-title grey-text text-darken-4">{{p.ProductName}}<i class="material-icons right">close</i></span>
      <p>{{p.ProductDescription}}</p>
        <p>{{p.returnDeal}}</p>
    </div>
     <div class="card-action">
            <a materialize="leanModal" [materializeParams]="[{dismissible: true}]" (click)="showPro(p)" class="waves-effect waves-light btn modal-trigger" href="#modal1">Modal</a>
                  <a href="#">This is a link</a>
            </div>
  </div>
  </div>
  </div>
  
   <!-- Modal Structure -->
  <div id="modal1" style="min-height:90%" class="modal bottom-sheet">
    <div class="modal-content">
      <h4>{{pad.ProductName}}</h4>
    </div>
    
    <div class="row">
      <div class="card col s5">
    <div class="card-image waves-effect waves-block waves-light">
     <img *ngIf="pad.picturefirst"
    class="activator"src="http://localhost:58056/wallimages/imagepath/{{pad.picturefirst}}">
  <img *ngIf="!pad.picturefirst" 
    class="activator"src="images/office.jpg">
    
    </div>
    <div class="card-content">
      <span class="card-title activator grey-text text-darken-4">{{pad.ProductDescription}}<i class="material-icons right">more_vert</i></span>
      <p><a href="#">This is a link</a></p>
      <div>
      <div *ngFor="let hero of pad.Categories" class="chip">
      {{hero.CategoryTitle}}
      </div>
      </div>
    </div>
     <div class="card-action">
            <a materialize="leanModal" [materializeParams]="[{dismissible: true}]" (click)="showPro(p)" class="waves-effect waves-light btn modal-trigger" href="#modal1">Modal</a>
                  <a href="#">This is a link</a>
            </div>
  </div>
   
    <div class="col s7">
   <h3>Add your products to Swapping cart</h3>
                           <ul class="collection">
                            <li  *ngFor="let p of heroes" class="collection-item avatar">
                               <img *ngIf="pad.picturefirst"
                                class="circle"src="http://localhost:58056/wallimages/imagepath/{{pad.picturefirst}}">
                              <img *ngIf="!pad.picturefirst" 
                                class="circle"src="images/office.jpg">
                         
                              <span class="title">{{p.ProductName}}</span>
                              <p>{{p.catName}} <br>
                               {{p.AddDate}}
                              </p>
                              <p class="btn btn-primary secondary-content" onclick="Removelink(this);" data-id="p.ProductId"> <i class="material-icons">shopping_cart</i>    Add to Cart  </p>
 
                            </li>
                           </ul>
                           
                              <div class="form-group">


                        <div class="row">
                            <div class="input-field col s12">
                                <textarea class="materialize-textarea validate" 
                                          name="returnDeal"></textarea>
                                <label for="icon_prefix2" data-error="wrong" data-success="">What you expect in return</label>
                            </div>
                        </div>
                  <button type="submit"  class="waves-effect btn waves-light">Submit</button>
                    </div>
    </div>
     <div class="modal-footer">
      <button  onClick=" $('#modal1').closeModal();" class=" modal-action modal-close waves-effect waves-green btn-flat">Close</button>
    </div>
    
    </div>
  </div>
    `,  
    directives: [MaterializeDirective],
    styles: ['.error {color:red;}'],
   providers: [ ProductService]

})
export class ProductListComponent {
   constructor(private _heroes: ProductService) { }
   public heroes;
   public errorMessage:string;
   public isloading:boolean=true;
   public pad= new proview;
   ngOnInit(){
      this.getHeroes();
     
   }
  getHeroes() {
        this._heroes.getProducts()
            .subscribe(
            heroes => {this.heroes = heroes;
              this.isloading=false;
          //  $('#modal1').openModal();
            console.log(heroes);  
            },
            error => this.errorMessage = <any>error);
    }
    showPro(p){
      this.pad=p;
    }
   
}