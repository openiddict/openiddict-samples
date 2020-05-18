import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { CoreModule } from '../core/core.module';
import { appRouting } from './app.routing';
import { HttpModule } from '@angular/http';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
     AppComponent,
     LoginComponent,
     RegisterComponent
  ],
  imports: [
    ReactiveFormsModule,
    BrowserModule,
    HttpModule,
    CoreModule,
    appRouting,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
