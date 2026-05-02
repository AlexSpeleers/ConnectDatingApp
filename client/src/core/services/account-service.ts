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

  public Register(creds: RegisterCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/register', creds, { withCredentials: true })
      .pipe(
        tap((user) => {
          if (user) {
            this.SetCurrentUser(user);
            this.StartTokenRefreshInterval();
          }
        }),
      );
  }

  public Login(creds: LoginCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/login', creds, { withCredentials: true })
      .pipe(
        tap((user) => {
          if (user) {
            this.SetCurrentUser(user);
            this.StartTokenRefreshInterval();
          }
        }),
      );
  }

  public Logout() {
    localStorage.removeItem('filters');
    this.likesService.ClearLikeIds();
    this.currentUser.set(null);
  }

  public RefreshToken() {
    return this.http.post<User>(
      this.baseUrl + 'account/refresh-token',
      {},
      { withCredentials: true },
    );
  }

  StartTokenRefreshInterval() {
    setInterval(
      () => {
        this.http
          .post<User>(this.baseUrl + 'account/refresh-token', {}, { withCredentials: true })
          .subscribe({
            next: (user) => {
              this.SetCurrentUser(user);
            },
            error: () => {
              this.Logout();
            },
          });
      },
      5 * 60 * 1000,
    );
  }

  public SetCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user);
    this.currentUser.set(user);
    this.likesService.GetLikeIds();
  }

  private getRolesFromToken(user: User): string[] {
    console.log(user.token);
    const payload = user.token.split('.')[1];
    const decoded = atob(payload);
    const jsonPayload = JSON.parse(decoded);
    return Array.isArray(jsonPayload.role) ? jsonPayload.role : [jsonPayload.role];
  }
}
