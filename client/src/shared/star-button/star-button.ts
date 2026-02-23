import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-star-button',
  imports: [],
  templateUrl: './star-button.html',
  styleUrl: './star-button.css',
})
export class StarButton {
  public disabled = input<boolean>();
  public selected = input<boolean>();
  public clickEvent = output<Event>();

  OnClick(event: Event) {
    this.clickEvent.emit(event);
  }
}
