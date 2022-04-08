import {Component, Inject, OnInit, OnDestroy, Input} from '@angular/core';

@Component({
  selector: '[main-weather-card]',
  templateUrl: './main-weather-card.component.html'
})
export class MainWeatherCardComponent implements OnInit, OnDestroy {
  @Input() temp: number = 0;
  @Input()
  icon: string = "";

  ngOnInit() {
  }

  ngOnDestroy() {
  }
}

