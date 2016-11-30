import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';

import { AppComponent } from './app.component';
import { CoreModule } from '../core/core.module';
import { NotFoundComponent } from './not-found/not-found.component';
import { appRouting } from './app.routing';
import { StoreModule } from '@ngrx/store';
import { appReducer } from './app-store';
import { HttpModule } from '@angular/http';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
     AppComponent,
     NotFoundComponent,
     LoginComponent,
     RegisterComponent
  ],
  imports: [
    ReactiveFormsModule,
    BrowserModule,
    HttpModule,
    CoreModule,
    appRouting,
    StoreModule.provideStore(appReducer),

   StoreDevtoolsModule.instrumentOnlyWithExtension()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
