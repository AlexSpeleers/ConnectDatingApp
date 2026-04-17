import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http: HttpClient = inject(HttpClient);
  private likesService = inject(LikesService);
  public currentUser = signal<User | null>(null);
  private baseUrl: string = environment.apiUrl;

  public GetUserFromStorage() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user: User = JSON.parse(userString);
    this.currentUser.set(user);
  }

  public Login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds).pipe(
      tap((user) => {
        if (user) this.SetCurrentUser(user);
      }),
    );
  }

  public Logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('filters');
    this.currentUser.set(null);
    this.likesService.ClearLikeIds();
  }

  public Register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds).pipe(
      tap((user) => {
        if (user) this.SetCurrentUser(user);
      }),
    );
  }

  public SetCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likesService.GetLikeIds();
  }
}
