import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { ConfirmDialogService } from '../../core/services/confirm-dialog-service';

@Component({
  selector: 'app-confirm-dialog',
  imports: [],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  @ViewChild('dialogRef') dialogRef!: ElementRef<HTMLDialogElement>;
  protected message = 'Are you sure?';

  private resolver: ((result: boolean) => void) | null = null;

  constructor() {
    inject(ConfirmDialogService).register(this);
  }

  public Open(message: string): Promise<boolean> {
    this.message = message;
    this.dialogRef.nativeElement.showModal();
    return new Promise((resolve) => (this.resolver = resolve));
  }

  public Confirm() {
    this.dialogRef.nativeElement.close();
    this.resolver?.(true);
    this.resolver = null;
  }

  public Cancel() {
    this.dialogRef.nativeElement.close();
    this.resolver?.(false);
    this.resolver = null;
  }
}
