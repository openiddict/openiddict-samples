/*
This class simply takes new token from the server and saves it in localstorage for further usage
*/
import {Component } from '@angular/core'
@Component({
    selector: 'authorize',
    template: '<h3>Successfully authorized </h3><h4>Loading ...</h4>',
    directives: []
})
export class extauthorizeComponent {
    constructor() {
        //get id token from urls part 
        var x =  location.hash;
        var extToken = x.replace("#id_token=", "");
        //save into localstorage
       localStorage.setItem("auth_key", extToken);
    }
   
   
   


   

   

};