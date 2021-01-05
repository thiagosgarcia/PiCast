import { Component, Inject, OnInit, OnDestroy, Input } from '@angular/core';

@Component({
  selector: '[weather-conditions-card]',
  templateUrl: './weather-conditions-card.component.html'
})
export class WeatherConditionsCardComponent implements OnInit, OnDestroy {
  @Input() city: string;
  @Input() description: string;
  @Input() feelsLike: number;
  @Input() humidity: number;
  @Input() sunrise: Date;
  @Input() sunset: Date;

  ngOnInit() {
  }

  ngOnDestroy() {
  }
}

