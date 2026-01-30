import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member, Photo } from '../../types/member';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  public GetMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'members');
  }
  public GetMember(id: string) {
    return this.http.get<Member>(this.baseUrl + 'members/' + id);
  }
  public GetMemberPhotos(id: string) {
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos');
  }
}
