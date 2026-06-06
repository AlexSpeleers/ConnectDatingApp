import { Component, inject, OnInit, signal } from '@angular/core';
import { Photo } from '../../../types/member';
import { AdminService } from '../../../core/services/admin-service';

@Component({
  selector: 'app-photo-management',
  imports: [],
  templateUrl: './photo-management.html',
  styleUrl: './photo-management.css',
})
export class PhotoManagement implements OnInit {
  protected photos = signal<Photo[]>([]);
  private adminService = inject(AdminService);

  ngOnInit(): void {
    this.GetPhotosForApproval();
  }

  protected GetPhotosForApproval() {
    this.adminService.GetPhotosForApproval().subscribe({
      next: (photos) => this.photos.set(photos),
    });
  }

  protected ApprovePhoto(photoId: number) {
    this.adminService.ApprovePhoto(photoId).subscribe({
      next: () =>
        this.photos.update((photos) => {
          return photos.filter((x) => x.id !== photoId);
        }),
    });
  }

  protected RejectPhoto(photoId: number) {
    this.adminService.RejectPhoto(photoId).subscribe({
      next: () =>
        this.photos.update((photos) => {
          return photos.filter((x) => x.id !== photoId);
        }),
    });
  }
}
