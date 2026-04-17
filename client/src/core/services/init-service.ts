import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { of } from 'rxjs';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  private likeService = inject(LikesService);

  public Init() {
    this.accountService.GetUserFromStorage();
    this.likeService.GetLikeIds();
    return of(null);
  }
}
