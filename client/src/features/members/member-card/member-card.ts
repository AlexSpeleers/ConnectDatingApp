import { Component, HostListener, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  public member = input.required<Member>();

  @HostListener('mousemove', ['$event'])
  mousemove(event: MouseEvent) {
    this.OnMouseMove(event);
  }

  private OnMouseMove(event: MouseEvent) {
    if (event.target instanceof Element) {
      // const rect = event.target?.getBoundingClientRect();
      // const width = rect.width;
      // const height = rect.height;
      // const mouseX = event.clientX - rect.left;
      // const mouseY = event.clientY - rect.top;
      // const xPct = mouseX / width - 0.5;
      // const yPct = mouseY / height - 0.5;
    }
  }
}
