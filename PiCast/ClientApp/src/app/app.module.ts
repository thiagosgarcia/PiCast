import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import {CurrentWeatherComponent} from "./current-weather/current-weather.component";
import {MainWeatherCardComponent} from "./current-weather/main-weather-card.component";
import {WeatherConditionsCardComponent} from "./current-weather/weather-conditions-card.component";
import {CurrentTimeCardComponent} from "./current-weather/current-time-card.component";
import {ForecastCardComponent} from "./forecast-card/forecast-card.component";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    CurrentWeatherComponent,
    MainWeatherCardComponent,
    WeatherConditionsCardComponent,
    CurrentTimeCardComponent,
    ForecastCardComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'current-weather', component: CurrentWeatherComponent },
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
