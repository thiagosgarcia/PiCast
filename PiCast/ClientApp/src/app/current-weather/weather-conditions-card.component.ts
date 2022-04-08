import { Component, Inject, OnInit, OnDestroy, Input } from '@angular/core';

@Component({
  selector: '[weather-conditions-card]',
  templateUrl: './weather-conditions-card.component.html'
})
export class WeatherConditionsCardComponent implements OnInit, OnDestroy {
  @Input() city: string="";
  @Input() description: string="";
  @Input() feelsLike: number=0;
  @Input() humidity: number=0;
  @Input() sunrise: Date = new Date();
  @Input() sunset: Date = new Date();

  ngOnInit() {
  }

  ngOnDestroy() {
  }
}

