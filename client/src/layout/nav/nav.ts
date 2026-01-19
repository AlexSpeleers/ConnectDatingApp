import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService: AccountService = inject(AccountService);
  protected creds: any = {};

  protected Login() {
    this.accountService.Login(this.creds).subscribe({
      next: (result) => {
        this.creds = {};
      },
      error: (error) => alert(error.message),
    });
  }

  protected Logout() {
    this.accountService.Logout();
  }
}
