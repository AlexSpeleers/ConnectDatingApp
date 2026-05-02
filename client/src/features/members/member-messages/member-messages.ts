import { Component, effect, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { MessageService } from '../../../core/services/message-service';
import { Message } from '../../../types/message';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css',
})
export class MemberMessages implements OnInit {
  @ViewChild('messageEndRef') messageEndRef!: ElementRef;
  private memberService = inject(MemberService);
  private messageService = inject(MessageService);
  protected messages = signal<Message[]>([]);
  protected messageContent = '';

  constructor() {
    effect(() => {
      const currentMessages = this.messages();
      if (currentMessages.length > 0) {
        this.ScrollToBottom();
      }
    });
  }

  ngOnInit(): void {
    this.LoadMessages();
  }

  LoadMessages() {
    const memberId = this.memberService.member()?.id;
    if (memberId) {
      this.messageService.GetMessageThread(memberId).subscribe({
        next: (messages) =>
          this.messages.set(
            messages.map((message) => ({
              ...message,
              currentUserSender: message.senderId !== memberId,
            })),
          ),
      });
    }
  }

  SendMessage() {
    const recipientId = this.memberService.member()?.id;
    if (!recipientId) return;
    this.messageService.SendMessage(recipientId, this.messageContent).subscribe({
      next: (message) => {
        this.messages.update((messages) => {
          message.currentUserSender = true;
          return [...messages, message];
        });
        this.messageContent = '';
      },
    });
  }

  ScrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behaviour: 'smooth' });
      }
    });
  }
}
