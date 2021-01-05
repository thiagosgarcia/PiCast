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
  public humidity: number;
  public description: string;
  public weatherInterval: any;
  public baseUrl: string;

  public sunrise: Date;
  public sunset: Date;
  public icon: string;
  public iconMini: string;

  public time: string;
  public date: string;
  public timeInterval: any;
  public cleanRun: any;

  public f_time: Date;
  public f_temp: number;
  public f_feelsLike: number;
  public f_humidity: number;
  public f_description: string;
  public f_icon: string;
  public forecastUrl: string;
  public forecastList: any;

  public showForecast: boolean;
  public showInterval: any;
  public iconPrefix: string;

  public dayOfWeek: any;
  public dayIndex: any;
  public lastDayOfWeek: string;

  public lastUpdate: number;
  public tickInterval: any;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.requestForecast();
    this.requestWeather();
    this.requestTime();
    this.cleanRun = true;
    this.showForecast = false;
    this.iconPrefix = "http://openweathermap.org/img/wn/";

    this.dayIndex = (date: any) => {
      var dayIndex = new Date(date).getDay();
      var todayIndex = new Date().getDay();
      return (dayIndex - todayIndex) % 2 === 0;
    }

    this.dayOfWeek = (date: any, threadSafe: boolean) => {
      if (!date) {
        date = new Date().getTime();
        this.lastDayOfWeek = null;
      }

      const weekday = new Array(7);
      weekday[0] = "dom";
      weekday[1] = "seg";
      weekday[2] = "ter";
      weekday[3] = "qua";
      weekday[4] = "qui";
      weekday[5] = "sex";
      weekday[6] = "sáb";

      var n = weekday[new Date(date).getDay()];
      if (n === this.lastDayOfWeek)
        return "";

      if (!threadSafe)
        this.lastDayOfWeek = n;

      return n;
    };
  }

  private requestForecast() {
    this.http.get<any>(this.baseUrl + 'api/Forecast').subscribe(result => {
      this.f_temp = result.list[1].main.temp;
      this.f_feelsLike = result.list[1].main.feels_like;
      this.f_humidity = result.list[1].main.humidity;
      this.f_description = result.list[1].weather[0].description;
      this.f_time = new Date(result.list[1].dt * 1000);

      this.f_icon = `${this.iconPrefix}${result.list[1].weather[0].icon}.png`;

      this.forecastList = result.list;
    },
      error => console.error(error));
  }

  private requestWeather() {
    this.http.get<any>(this.baseUrl + 'api/CurrentWeather').subscribe(result => {
      this.city = result.name;
      this.temp = result.main.temp;
      this.feelsLike = result.main.feels_like;
      this.tempMin = Math.floor(result.main.temp_min);
      this.tempMax = Math.ceil(result.main.temp_max);
      this.humidity = result.main.humidity;
      this.description = result.weather[0].description;

      this.sunrise = new Date(result.sys.sunrise * 1000);
      this.sunset = new Date(result.sys.sunset * 1000);

      this.icon = `${this.iconPrefix}${result.weather[0].icon}@2x.png`;
      this.iconMini = result.weather[0].icon;
      this.lastUpdate = new Date().getTime();
    },
      error => console.error(error));
    this.requestForecast();
  }

  private requestTime() {

    this.http.get<any>(this.baseUrl + 'api/CurrentTime').subscribe(result => {
      if (!!this.time && this.time !== result.time && !!this.cleanRun) {
        clearInterval(this.weatherInterval);
        clearInterval(this.timeInterval);
        clearInterval(this.showInterval);
        this.weatherInterval = setInterval(() => this.requestWeather(), 60000);
        this.timeInterval = setInterval(() => this.requestTime(), 60000);
        this.showInterval = setInterval(() => this.showForecast = !this.showForecast, 15000);

        this.requestWeather();
        this.cleanRun = false;
      }
      this.time = result.time;
      this.date = result.readableDate;
    },
      error => console.error(error));
  }

  ngOnInit() {
    this.timeInterval = setInterval(() => this.requestTime(), 500);

    this.tickInterval = setInterval(() => {
        if (!this.time)
          return;
        if (this.time.indexOf(":") > 0)
          this.time = this.time.replace(":", " "); //colon => no-break space
        else
          this.time = this.time.replace(" ", ":"); //no-break space => colon
      },
      1000);
  }

  ngOnDestroy() {
    clearInterval(this.weatherInterval);
    clearInterval(this.timeInterval);
    clearInterval(this.showInterval);
    clearInterval(this.tickInterval);
  }
}

