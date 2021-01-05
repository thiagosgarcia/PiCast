import { Component, Inject, OnInit, OnDestroy, Input } from '@angular/core';

@Component({
  selector: '[current-time-card]',
  templateUrl: './current-time-card.component.html'
})
export class CurrentTimeCardComponent implements OnInit, OnDestroy {
  @Input() time: any;
  @Input() date: any;

  ngOnInit() {
  }

  ngOnDestroy() {
  }
}

