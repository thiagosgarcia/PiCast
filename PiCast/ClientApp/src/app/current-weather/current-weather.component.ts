import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval } from 'rxjs';

@Component({
  selector: 'app-currrent-weather',
  templateUrl: './current-weather.component.html'
})
export class CurrentWeatherComponent implements OnInit, OnDestroy {
  public city: string;
  public temp: number;
  public feelsLike: number;
  public tempMin: number;
  public tempMax: number;
  public humidity: string;
  public description: string;
  public weatherInterval: any;
  public baseUrl: string;

  public sunrise: Date;
  public sunset: Date;
  public icon: string;

  public time: string;
  public date: string;
  public timeInterval: any;
  public cleanRun: any;

  public f_time: Date;
  public f_temp: number;
  public f_feelsLike: number;
  public f_humidity: string;
  public f_description: string;
  public f_icon: string;
  public forecastInterval: any;
  public forecastUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.requestForecast();
    this.requestWeather();
    this.requestTime();
    this.cleanRun = true;
  }

  private requestForecast() {
    this.http.get<any>(this.baseUrl + 'api/Forecast').subscribe(result => {
      this.f_temp = Math.round(result.list[1].main.temp * 10) / 10;
      this.f_feelsLike = Math.round(result.list[1].main.feels_like * 10) / 10;
      this.f_humidity = result.list[1].main.humidity;
      this.f_description = result.list[1].weather[0].description;
      this.f_time = new Date(result.list[1].dt * 1000);

      this.f_icon = `http://openweathermap.org/img/wn/${result.list[1].weather[0].icon}.png`;
    },
      error => console.error(error));
  }

  private requestWeather() {
    this.http.get<any>(this.baseUrl + 'api/CurrentWeather').subscribe(result => {
      this.city = result.name;
      this.temp = Math.round(result.main.temp * 10) / 10;
      this.feelsLike = Math.round(result.main.feels_like * 10) / 10;
      this.tempMin = Math.floor(result.main.temp_min);
      this.tempMax = Math.ceil(result.main.temp_max);
      this.humidity = result.main.humidity;
      this.description = result.weather[0].description;

      this.sunrise = new Date(result.sys.sunrise * 1000);
      this.sunset = new Date(result.sys.sunset * 1000);

      this.icon = `http://openweathermap.org/img/wn/${result.weather[0].icon}@2x.png`;
    },
      error => console.error(error));
  }

  private requestTime() {

    this.http.get<any>(this.baseUrl + 'api/CurrentTime').subscribe(result => {
      if (!!this.time && this.time !== result.time && !!this.cleanRun) {
        clearInterval(this.weatherInterval);
        clearInterval(this.timeInterval);
        this.weatherInterval = setInterval(() => this.requestWeather(), 60000);
        this.timeInterval = setInterval(() => this.requestTime(), 60000);

        this.requestWeather();
        this.cleanRun = false;
      }
      this.time = result.time;
      this.date = result.readableDate;
    },
      error => console.error(error));
  }

  ngOnInit() {
    this.forecastInterval = setInterval(() => this.forecastInterval(), 60000);
    this.weatherInterval = setInterval(() => this.requestWeather(), 60000);
    this.timeInterval = setInterval(() => this.requestTime(), 100);
  }
  ngOnDestroy() {
    clearInterval(this.weatherInterval);
    clearInterval(this.timeInterval);
    clearInterval(this.forecastInterval);
  }
}

