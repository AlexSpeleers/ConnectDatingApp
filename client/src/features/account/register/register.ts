import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  protected creds: RegisterCreds = {} as RegisterCreds;
  protected cancelRegister = output<boolean>();
  private accountService: AccountService = inject(AccountService);

  protected Register() {
    this.accountService.Register(this.creds).subscribe({
      next: (response) => {
        console.log(response);
        this.Cancel();
      },
      error: (error) => console.log(error),
    });
  }

  protected Cancel() {
    this.cancelRegister.emit(false);
  }
}
