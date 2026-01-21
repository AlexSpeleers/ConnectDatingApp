import { HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error) => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors) {
              const modelStateErrors = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modelStateErrors.push(error.error.errors[key]);
                }
              }
              throw modelStateErrors.flat();
            } else {
              toast.Error(error.error);
            }
            break;
          case 401:
            toast.Error('Unauthorized.');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExras: NavigationExtras = { state: { error: error.error } };
            router.navigateByUrl('/server-error', navigationExras);
            toast.Error('Server error.');
            break;
          default:
            toast.Error('Something went wrong.');
            break;
        }
      }
      throw error;
    }),
  );
};
