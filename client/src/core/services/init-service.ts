import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  public Init() {
    return this.accountService.RefreshToken().pipe(
      tap((user) => {
        if (user) {
          this.accountService.SetCurrentUser(user);
          this.accountService.StartTokenRefreshInterval();
        }
      }),
    );
  }
}
