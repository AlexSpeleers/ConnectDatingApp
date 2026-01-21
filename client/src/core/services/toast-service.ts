import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private toastSelector: string = 'toast-container';
  constructor() {
    this.CreateToastContainer();
  }

  private CreateToastContainer() {
    if (!document.getElementById(this.toastSelector)) {
      const container = document.createElement('div');
      container.id = this.toastSelector;
      container.className = 'toast';
      document.body.appendChild(container);
    }
  }

  private CreateToastElement(message: string, alertClass: string, duration: number = 5000) {
    const toastContainer = document.getElementById(this.toastSelector);
    if (!toastContainer) return;
    const toast = document.createElement('div');
    toast.classList.add('alert', alertClass, 'shadow-lg');
    toast.innerHTML = `
      <span class="text-white">${message}</span>
      <button class="font-extrabold ml-4 btn btn-sm btn-ghost">X</button>
    `;

    toast.querySelector('button')?.addEventListener('click', () => {
      toastContainer.removeChild(toast);
    });
    toastContainer.append(toast);
    setTimeout(() => {
      if (toastContainer.contains(toast)) toastContainer.removeChild(toast);
    }, duration);
  }

  public Success(message: string, duration?: number) {
    this.CreateToastElement(message, 'alert-success', duration);
  }
  public Error(message: string, duration?: number) {
    this.CreateToastElement(message, 'alert-error', duration);
  }
  public Warning(message: string, duration?: number) {
    this.CreateToastElement(message, 'alert-warning', duration);
  }
  public Info(message: string, duration?: number) {
    this.CreateToastElement(message, 'alert-info', duration);
  }
}
