import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  imports: [],
  templateUrl: './test-errors.html',
  styleUrl: './test-errors.css',
})
export class TestErrors {
  private http = inject(HttpClient);
  baseUrl = 'https://localhost:5001/api/';
  protected validationErrors = signal<string[]>([]);

  private Reset(): void {
    this.validationErrors.set([]);
  }

  protected Get404Error() {
    this.Reset();
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe({
      next: (response) => console.log(response),
      error: (error) => console.log(error),
    });
  }
  protected Get400Error() {
    this.Reset();
    this.http.get(this.baseUrl + 'buggy/bad-request').subscribe({
      next: (response) => console.log(response),
      error: (error) => console.log(error),
    });
  }
  protected Get500Error() {
    this.Reset();
    this.http.get(this.baseUrl + 'buggy/server-error').subscribe({
      next: (response) => console.log(response),
      error: (error) => console.log(error),
    });
  }
  protected Get401Error() {
    this.Reset();
    this.http.get(this.baseUrl + 'buggy/auth').subscribe({
      next: (response) => console.log(response),
      error: (error) => console.log(error),
    });
  }
  protected Get400ValidationError() {
    this.Reset();
    this.http.post(this.baseUrl + 'account/register', {}).subscribe({
      next: (response) => console.log(response),
      error: (error) => {
        console.log(error);
        this.validationErrors.set(error);
      },
    });
  }
}
