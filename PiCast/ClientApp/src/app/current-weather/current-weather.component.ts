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


  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.requestWeather();
    this.requestTime();
  }

  private requestWeather() {
    this.http.get<any>(this.baseUrl + 'api/CurrentWeather').subscribe(result => {
      this.city = result.name;
      this.temp = Math.round(result.main.temp * 10) / 10 ;
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
      this.time = result.time;
      this.date = result.readableDate;
    },
      error => console.error(error));
  }

  ngOnInit() {
    this.weatherInterval = setInterval(() => this.requestWeather(), 60000);
    this.timeInterval = setInterval(() => this.requestTime(), 2000);
  }
  ngOnDestroy() {
    clearInterval(this.weatherInterval);
    clearInterval(this.timeInterval);
  }
}

