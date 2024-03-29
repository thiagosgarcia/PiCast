import {Component, Inject, OnInit, OnDestroy, Input} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {interval} from 'rxjs';

@Component({
  selector: '[app-forecast-card]',
  templateUrl: './forecast-card.component.html'
})
export class ForecastCardComponent implements OnInit, OnDestroy {
  @Input() forecast: any;

  @Input() dayOfWeekToShow: string = "";
  @Input() currentWeather: boolean = false;

  @Input() time: any;
  @Input() description: string = "";
  @Input() humidity: number = 0;
  @Input() temp: number = 0;
  @Input() feelsLike: number = 0;
  @Input() icon: any;

  public lastDayOfWeek: string = "";
  public iconPrefix: string;

  constructor() {
    this.iconPrefix = "http://openweathermap.org/img/wn/";
  }

  ngOnInit() {
  }

  ngOnDestroy() {
  }
}

