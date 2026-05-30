import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private router: Router = inject(Router);
  private toastSelector: string = 'toast-container';

  constructor() {
    this.CreateToastContainer();
  }

  private CreateToastContainer() {
    if (!document.getElementById(this.toastSelector)) {
      const container = document.createElement('div');
      container.id = this.toastSelector;
      container.className = 'toast z-50';
      document.body.appendChild(container);
    }
  }

  private CreateToastElement(
    message: string,
    alertClass: string,
    duration: number = 5000,
    avatar?: string,
    route?: string,
  ) {
    const toastContainer = document.getElementById(this.toastSelector);
    if (!toastContainer) return;
    const toast = document.createElement('div');
    toast.classList.add(
      'alert',
      alertClass,
      'shadow-lg',
      'flex',
      'items-center',
      'gap-3',
      'cursor-pointer',
    );

    if (route) {
      toast.addEventListener('click', () => this.router.navigateByUrl(route));
    }

    toast.innerHTML = `
      ${avatar ? `<img src=${avatar || '/user.png'} class='w-10 h-10 rounded'` : ''}
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

  public Success(message: string, duration?: number, avatar?: string, route?: string) {
    this.CreateToastElement(message, 'alert-success', duration, avatar, route);
  }
  public Error(message: string, duration?: number, avatar?: string, route?: string) {
    this.CreateToastElement(message, 'alert-error', duration, avatar, route);
  }
  public Warning(message: string, duration?: number, avatar?: string, route?: string) {
    this.CreateToastElement(message, 'alert-warning', duration, avatar, route);
  }
  public Info(message: string, duration?: number, avatar?: string, route?: string) {
    this.CreateToastElement(message, 'alert-info', duration, avatar, route);
  }
}
