import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http: HttpClient = inject(HttpClient);
  protected currentUser = signal<User | null>(null);
  private baseUrl: string = environment.apiUrl;

  public get CurrentUser(): User | null {
    return this.currentUser();
  }

  public GetUserFromStorage(): User | null {
    const userString = localStorage.getItem('user');
    if (!userString) return null;

    const user: User = JSON.parse(userString);
    if (!user) return null;

    this.currentUser.set(user);
    return this.currentUser();
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
    this.currentUser.set(null);
  }

  public Register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds).pipe(
      tap((user) => {
        if (user) this.SetCurrentUser(user);
      }),
    );
  }

  private SetCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }
}
