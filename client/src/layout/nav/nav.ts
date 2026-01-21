import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService: AccountService = inject(AccountService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds: any = {};

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
