import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';
import { themes } from '../theme';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav implements OnInit {
  protected accountService: AccountService = inject(AccountService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds: any = {};
  protected selectedTheme = signal<string>(localStorage.getItem('theme') || 'dark');
  protected themes = themes;

  ngOnInit(): void {
    document.documentElement.setAttribute('data-theme', this.selectedTheme());
  }

  protected HandleSelectedTheme(theme: string) {
    this.selectedTheme.set(theme);
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
    const element = document.activeElement as HTMLDivElement;
    if (element) element.blur();
  }

  protected Login() {
    this.accountService.Login(this.creds).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
        this.toast.Success('Logged in successfully');
        this.creds = {};
      },
      error: (error) => {
        this.toast.Error(error.error);
      },
    });
  }

  protected Logout() {
    this.accountService.Logout();
    this.router.navigateByUrl('/');
  }
}
