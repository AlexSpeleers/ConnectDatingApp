import { Component, computed, HostListener, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { PresenceService } from '../../../core/services/presence-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  private likeService = inject(LikesService);
  private presenceService = inject(PresenceService);
  public member = input.required<Member>();

  protected hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id));
  protected isOnlune = computed(() =>
    this.presenceService.onlineUsers().includes(this.member().id),
  );

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

  protected ToggleLike(event: Event) {
    event.stopPropagation();
    this.likeService.toggleLike(this.member().id);
  }
}
