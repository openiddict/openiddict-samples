import { Component } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { MdSnackBar } from '@angular/material';
import { AriaLivePoliteness } from '@angular/material';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    providers: [MdSnackBar]
})
export class HomeComponent {
    constructor(private snackbar: MdSnackBar) { }

    failedAttempt() {
        var bar = this.snackbar.open('It didn\'t quite work!', 'close');
    }

}
