import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../../types/user';
import { Photo } from '../../types/member';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  GetUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  public UpdateUserRoles(userId: string, roles: string[]) {
    return this.http.post<string[]>(
      this.baseUrl + 'admin/edit-roles' + userId + '?roles=' + roles,
      {},
    );
  }

  public GetPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate', {});
  }

  public ApprovePhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }

  public RejectPhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'admin/reject-photo/' + photoId, {});
  }
}
