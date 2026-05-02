import { Component, inject, OnInit, signal } from '@angular/core';
import { MessageService } from '../../core/services/message-service';
import { PaginatedResult } from '../../types/pagination';
import { Message } from '../../types/message';
import { Paginator } from '../../shared/paginator/paginator';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-messages',
  imports: [Paginator, RouterLink, DatePipe],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages implements OnInit {
  private messageService = inject(MessageService);
  protected container = 'Inbox';
  protected fetchedContainer = 'Inbox';
  protected pageNumber = 1;
  protected pageSize = 10;
  protected paginatedMessages = signal<PaginatedResult<Message> | null>(null);

  tabs = [
    { label: 'Inbox', value: 'Inbox' },
    { label: 'Outbox', value: 'Outbox' },
  ];

  ngOnInit(): void {
    this.LoadMessages();
  }

  private LoadMessages() {
    this.messageService.GetMessages(this.container, this.pageNumber, this.pageSize).subscribe({
      next: (response) => {
        this.paginatedMessages.set(response);
        this.fetchedContainer = this.container;
      },
    });
  }

  protected DeleteMessage(event: Event, id: string) {
    event.stopPropagation();
    this.messageService.DeleteMessage(id).subscribe({
      next: () => {
        const current = this.paginatedMessages();
        if (current?.items) {
          this.paginatedMessages.update((prev) => {
            if (!prev) return null;
            const newItems = prev.items.filter((x) => x.id !== id) || [];
            const newMetadata = prev.metadata
              ? {
                  ...prev.metadata,
                  totalCount: prev.metadata.totalCount - 1,
                  totalPages: Math.max(
                    1,
                    Math.ceil((prev.metadata.totalCount - 1) / prev.metadata.pageSize),
                  ),
                  currentPage: Math.min(
                    prev.metadata.currentPage,
                    Math.max(1, Math.ceil((prev.metadata.totalCount - 1) / prev.metadata.pageSize)),
                  ),
                }
              : undefined;
            return {
              items: newItems,
              metadata: prev.metadata,
            };
          });
        }
      },
    });
  }

  get isInbox() {
    return this.fetchedContainer === 'Inbox';
  }

  setContainer(container: string) {
    this.container = container;
    this.pageNumber = 1;
    this.LoadMessages();
  }

  protected OnPageChange(event: { pageNumber: number; pageSize: number }) {
    this.pageSize = event.pageSize;
    this.pageNumber = event.pageNumber;
    this.LoadMessages();
  }
}
