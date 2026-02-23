import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-delete-button',
  imports: [],
  templateUrl: './delete-button.html',
  styleUrl: './delete-button.css',
})
export class DeleteButton {
  public disabled = input<boolean>();
  public clickEvent = output<Event>();

  public OnClick(event: Event) {
    this.clickEvent.emit(event);
  }
}
